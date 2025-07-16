using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace Rpg
{
    [Tool]
    [GlobalClass, Icon("res://GUI/dialogSystem/icons/star_bubble.png")]
    public partial class DialogSystem : CanvasLayer
    {
        // Signals
        [Signal]
        public delegate void FinishedEventHandler();
        [Signal]
        public delegate void LetterAddedEventHandler(string letter);
        [Signal]
        public delegate void BranchSelectedEventHandler(int index);

        // private
        private float textSpeed = 0.02f;
        private int textLength;
        private string plainText;
        private Array<DialogItemResource> dialogItemResources;
        private Array<DialogBranchResource> dialogBranchResources;
        private int dialogItemIndex;
        private RichTextLabel richTextLabel;
        private VBoxContainer choiceContainer;
        private PortraitSprite portraitSprite;
        private Label nameLabel;
        private Label label;
        private PanelContainer panelContainer;
        private float timer = 0;
        private bool timerStarted = false;
        private AudioStreamPlayer audioStreamPlayer;
        private readonly Vector2[][] uiPositions = new Vector2[4][]
        {
            new Vector2[2] { new(88, 176), new(128, 176) },
            new Vector2[2] { new(376, 152), new(128, 152) },
            new Vector2[2] { new(481, 216), new(64, 216) },
            new Vector2[2] { new(24, 256), new(481, 256) },
        };
        private readonly List<Button> choiceButtons = new();
        private DialogInteraction dialogInteraction;

        // properties
        public Button DialogProgressIndicator { get; private set; }
        public static DialogSystem Instance { get; private set; }

        // methods
        public override void _Ready()
        {
            Instance = this;

            richTextLabel = GetNode<RichTextLabel>("DialogUI/PanelContainer/RichTextLabel");
            choiceContainer = GetNode<VBoxContainer>("DialogUI/PanelContainer/VBoxContainer");
            nameLabel = GetNode<Label>("DialogUI/NameLabel");
            DialogProgressIndicator = GetNode<Button>("DialogUI/DialogProgressIndicator");
            panelContainer = GetNode<PanelContainer>("DialogUI/PanelContainer");
            label = GetNode<Label>("DialogUI/DialogProgressIndicator/Label");
            audioStreamPlayer = GetNode<AudioStreamPlayer>("DialogUI/AudioStreamPlayer");
            portraitSprite = (PortraitSprite)GetNode<Sprite2D>("DialogUI/PortraitSprite");

            if (Engine.IsEditorHint())
            {
                Node parent = GetParent();

                if (parent is not DialogItem)
                    parent.RemoveChild(this);
                else
                    InitializeButtons();

                return;
            }

            SetUIState(false);
            InitializeButtons();
            DialogProgressIndicator.Connect(BaseButton.SignalName.Pressed, new(this, MethodName.OnDialogProgressIndicatorPressed));

            // cannot connect in portrait sprite because its onready runs before this
            Connect(SignalName.LetterAdded, new(portraitSprite, PortraitSprite.MethodName.OnLetterAdded));
        }

        public override void _Process(double delta)
        {
            if (!timerStarted)
                return;

            timer += (float)delta;

            if (timer > textSpeed)
            {
                timer = 0;
                timerStarted = false;
                OnTimerTimeout();
            }
        }

        private void InitializeButtons()
        {
            Array<Node> children = choiceContainer.GetChildren();

            for (int i = 0; i < children.Count; i++)
            {
                Button button = (Button)children[i];
                button.Connect(BaseButton.SignalName.Pressed, Callable.From(() => OnChoiceButtonPressed(i)));
                choiceButtons.Add(button);
            }
        }

        private void OnDialogProgressIndicatorPressed()
        {
            if (++dialogItemIndex == dialogItemResources.Count)
            {
                EmitSignal(SignalName.Finished);
                SetUIState(false);
            }
            else
                StartDialog();
        }

        private void OnChoiceButtonPressed(int index)
        {
            ShowDialog(dialogBranchResources[index].DialogItemResources, dialogInteraction);
            dialogBranchResources[index].QuestAdvanceResource?.AdvanceQuest();
            EmitSignal(SignalName.BranchSelected, index);
        }

        private void SetUIState(bool value)
        {
            Visible = value;
            GlobalPlayerManager.Instance.Player.SetProcessUnhandledInput(!value);
            GlobalPlayerManager.Instance.Player.ChangeStateToIdle();
        }

        public void ShowDialog(Array<DialogItemResource> items, DialogInteraction _dialogInteraction)
        {
            if (items.Count == 0)
            {
                SetUIState(false);
                return;
            }

            SetUIState(true);
            dialogItemResources = items;
            dialogItemIndex = 0;
            CallDeferred(MethodName.StartDialog);
            dialogInteraction = _dialogInteraction;
        }

        private void StartDialog()
        {
            // need to check separately for dialogbranch
            if (dialogItemResources[dialogItemIndex].QuestConditionResource != null && !dialogItemResources[dialogItemIndex].QuestConditionResource.CheckIsActivated())
            {
                OnDialogProgressIndicatorPressed();
                return;
            }

            ShowDialogButtonIndicator(false);

            if (dialogItemResources[dialogItemIndex] is DialogTextResource dialogText)
                SetTextDialog(dialogText);
            else if (dialogItemResources[dialogItemIndex] is DialogChoiceResource dialogChoice)
                SetChoiceDialog(dialogChoice);

            CommonDisplay(dialogItemResources[dialogItemIndex].NpcResource);
            dialogItemResources[dialogItemIndex].QuestAdvanceResource?.AdvanceQuest();

            int i = dialogItemResources[dialogItemIndex].NpcResource.Name == "Hero" ? 0 : 1;

            panelContainer.Position = uiPositions[0][i];
            nameLabel.Position = uiPositions[1][i];
            portraitSprite.Position = uiPositions[2][i];
            DialogProgressIndicator.Position = uiPositions[3][i];
            portraitSprite.Scale = new(i == 0 ? -1 : 1, 1);
        }

        public void CommonDisplay(NpcResource npcResource)
        {
            nameLabel.Text = npcResource.Name;
            portraitSprite.Texture = npcResource.Portrait;
        }

        private void SetTextDialog(DialogTextResource dialogText)
        {
            SetTextDisplay(dialogText.Text);
            portraitSprite.BasePitch = dialogText.NpcResource.DialoguePitch;
            textLength = richTextLabel.Text.Length;
            richTextLabel.VisibleCharacters = 0;
            plainText = richTextLabel.Text;
            timerStarted = true;
        }

        public void SetTextDisplay(string text)
        {
            choiceContainer.Hide();
            richTextLabel.Show();
            richTextLabel.Text = text;
        }

        private void SetChoiceDialog(DialogChoiceResource dialogChoice)
        {
            dialogBranchResources.Clear();

            foreach (DialogBranchResource dialogBranch in dialogChoice.DialogBranchResources)
                if (dialogBranch.QuestConditionResource == null || dialogBranch.QuestConditionResource.CheckIsActivated())
                    dialogBranchResources.Add(dialogBranch);

            SetChoiceDisplay(dialogBranchResources);
            choiceButtons[0].GrabFocus();
        }

        public void SetChoiceDisplay(Array<DialogBranchResource> dialogBranches)
        {
            choiceContainer.Show();
            richTextLabel.Hide();

            int i = 0;

            for (; i < dialogBranches.Count; i++)
            {
                choiceButtons[i].Text = dialogBranches[i].Text;
                choiceButtons[i].Disabled = false;
                choiceButtons[i].FocusMode = Control.FocusModeEnum.All;
            }

            for (int j = i; j < choiceButtons.Count; j++)
            {
                choiceButtons[j].Disabled = true;
                choiceButtons[j].FocusMode = Control.FocusModeEnum.None;
            }
        }

        private void ShowDialogButtonIndicator(bool isVisible)
        {
            DialogProgressIndicator.Visible = isVisible;

            if (isVisible)
                DialogProgressIndicator.GrabFocus();

            label.Text = dialogItemIndex == dialogItemResources.Count - 1 ? "END" : "NEXT";
        }

        private void OnTimerTimeout()
        {
            if (richTextLabel.VisibleCharacters == textLength)
            {
                ShowDialogButtonIndicator(true);
                return;
            }

            richTextLabel.VisibleCharacters += 1;
            EmitSignal(SignalName.LetterAdded, plainText.Substr(richTextLabel.VisibleCharacters - 1, 1));
            timerStarted = true;
        }
    }
}
