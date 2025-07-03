using System.Collections.Generic;
using Godot;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(DialogSystem), "res://GUI/dialogSystem/icons/star_bubble.png", nameof(CanvasLayer))]
public class DialogSystem : CanvasLayer
{
    // Signals
    [Signal]
    public delegate void Finished();
    [Signal]
    public delegate void LetterAdded(string letter);
    [Signal]
    public delegate void BranchSelected(int index);

    // private
    private readonly float textSpeed = 0.02f;
    private int textLength;
    private string plainText;
    private List<DialogItemResource> dialogItemResources;
    private List<DialogBranchResource> dialogBranchResources;
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
    private readonly Vector2[][] uiPositions = new Vector2[][]
    {
        new Vector2[]{new Vector2(88, 176), new Vector2(128, 176)},
        new Vector2[]{new Vector2(376, 152), new Vector2(128, 152)},
        new Vector2[]{new Vector2(481, 216), new Vector2(64, 216)},
        new Vector2[]{new Vector2(24, 256), new Vector2(481, 256)},
    };
    private readonly List<Button> choiceButtons = new List<Button>();
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
        portraitSprite = (PortraitSprite)GetNode<Sprite>("DialogUI/PortraitSprite");

        if (Engine.EditorHint)
        {
            Node parent = GetParent();

            if (!(parent is DialogItem))
                parent.RemoveChild(this);
            else
                InitializeButtons();

            return;
        }

        SetUIState(false);
        InitializeButtons();
        DialogProgressIndicator.Connect("pressed", this, nameof(OnDialogProgressIndicatorPressed));

        // cannot connect in portrait sprite because its onready runs before this
        Connect(nameof(LetterAdded), portraitSprite, nameof(PortraitSprite.OnLetterAdded));
    }

    public override void _Process(float delta)
    {
        if (!timerStarted)
            return;

        timer += delta;

        if (timer > textSpeed)
        {
            timer = 0;
            timerStarted = false;
            OnTimerTimeout();
        }
    }

    private void InitializeButtons()
    {
        Godot.Collections.Array children = choiceContainer.GetChildren();

        for (int i = 0; i < children.Count; i++)
        {
            Button button = (Button)children[i];
            button.Connect("pressed", this, nameof(OnChoiceButtonPressed), new Godot.Collections.Array(i));
            choiceButtons.Add(button);
        }
    }

    private void OnDialogProgressIndicatorPressed()
    {
        if (++dialogItemIndex == dialogItemResources.Count)
        {
            if (dialogItemResources[dialogItemIndex - 1].NewDialogPath != null && dialogItemResources[dialogItemIndex - 1].NewDialogPath != "")
                dialogInteraction.ChangeDialog(dialogItemResources[dialogItemIndex - 1].NewDialogPath);

            EmitSignal(nameof(Finished));
            SetUIState(false);
        }
        else
            StartDialog();
    }

    private void OnChoiceButtonPressed(int index)
    {
        ShowDialog(dialogBranchResources[index].DialogItemResources, dialogInteraction);
        dialogBranchResources[index].QuestAdvanceResource?.AdvanceQuest();
        EmitSignal(nameof(BranchSelected), index);
    }

    private void SetUIState(bool value)
    {
        Visible = value;
        GlobalPlayerManager.Instance.Player.SetProcessUnhandledInput(!value);
        GlobalPlayerManager.Instance.Player.ChangeStateToIdle();
    }

    public void ShowDialog(List<DialogItemResource> items, DialogInteraction _dialogInteraction)
    {
        if (items.Count == 0)
        {
            SetUIState(false);
            return;
        }

        SetUIState(true);
        dialogItemResources = items;
        dialogItemIndex = 0;
        CallDeferred(nameof(StartDialog));
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

        panelContainer.RectPosition = uiPositions[0][i];
        nameLabel.RectPosition = uiPositions[1][i];
        portraitSprite.Position = uiPositions[2][i];
        DialogProgressIndicator.RectPosition = uiPositions[3][i];
        portraitSprite.Scale = new Vector2(i == 0 ? -1 : 1, 1);
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
        dialogBranchResources = dialogChoice.DialogBranchResources.FindAll(dialogBranch => dialogBranch.QuestConditionResource == null || dialogBranch.QuestConditionResource.CheckIsActivated());
        SetChoiceDisplay(dialogBranchResources);
        choiceButtons[0].GrabFocus();
    }

    public void SetChoiceDisplay(List<DialogBranchResource> dialogBranches)
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
        EmitSignal(nameof(LetterAdded), plainText.Substr(richTextLabel.VisibleCharacters - 1, 1));
        timerStarted = true;
    }
}
