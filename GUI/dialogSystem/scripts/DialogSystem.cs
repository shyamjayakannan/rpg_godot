using System;
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

    // private
    private float textSpeed = 0.02f;
    private int textLength;
    private string plainText;
    private List<DialogItem> dialogItems;
    private List<DialogBranch> dialogBranches;
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
    private Vector2[][] uiPositions = new Vector2[][]
    {
        new Vector2[]{new Vector2(88, 176), new Vector2(128, 176)},
        new Vector2[]{new Vector2(376, 152), new Vector2(128, 152)},
        new Vector2[]{new Vector2(481, 216), new Vector2(64, 216)},
        new Vector2[]{new Vector2(24, 256), new Vector2(481, 256)},
    };
    private List<Button> choiceButtons = new List<Button>();

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
        if (++dialogItemIndex == dialogItems.Count)
        {
            EmitSignal(nameof(Finished));
            SetUIState(false);
        }
        else
            StartDialog();
    }

    private void OnChoiceButtonPressed(int index)
    {
        ShowDialog(dialogBranches[index].DialogItems);
        dialogBranches[index].EmitSignal(nameof(DialogBranch.Selected));
    }

    private void SetUIState(bool value)
    {
        Visible = value;
        GlobalPlayerManager.Instance.Player.SetProcessUnhandledInput(!value);
    }

    public void ShowDialog(List<DialogItem> items)
    {
        if (items.Count == 0)
        {
            SetUIState(false);
            return;
        }

        SetUIState(true);
        dialogItems = items;
        dialogItemIndex = 0;
        CallDeferred(nameof(StartDialog));
    }

    private void StartDialog()
    {
        ShowDialogButtonIndicator(false);

        if (dialogItems[dialogItemIndex] is DialogText dialogText)
            SetTextDialog(dialogText);
        else if (dialogItems[dialogItemIndex] is DialogChoice dialogChoice)
            SetChoiceDialog(dialogChoice);

        CommonDisplay(dialogItems[dialogItemIndex].NpcResource);

        int i = dialogItems[dialogItemIndex].NpcResource.Name == "Hero" ? 0 : 1;

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

    private void SetTextDialog(DialogText dialogText)
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

    private void SetChoiceDialog(DialogChoice dialogChoice)
    {
        dialogBranches = dialogChoice.DialogBranches;
        SetChoiceDisplay(dialogChoice.DialogBranches);
        choiceButtons[0].GrabFocus();
    }

    public void SetChoiceDisplay(List<DialogBranch> dialogBranches)
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

        label.Text = dialogItemIndex == dialogItems.Count - 1 ? "END" : "NEXT";
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
