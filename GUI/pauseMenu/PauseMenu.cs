using Godot;

public partial class PauseMenu : CanvasLayer
{
	// Signals
	[Signal]
	public delegate void ShownEventHandler();
	[Signal]
	public delegate void HiddenEventHandler();
	[Signal]
	public delegate void EquipmentsChangedEventHandler(EquipableItem equipableItem);
	[Signal]
	public delegate void ItemRemovedEventHandler();

	// private
	private Button save;
	private Button load;
	private Button menu;
	private ButtonMenu buttonMenu;
	private TabContainer tabContainer;
	private bool isPaused = false;
	private Label itemDescription;
	private Control system;
	private AbilityButton arrowButton;
	private AbilityButton bombButton;

	// properties
	public AudioStreamPlayer AudioStreamPlayer { get; private set; }
	public Stats Stats { get; private set; }
	public static PauseMenu Instance { get; private set; }

	// methods
	public override void _Ready()
	{
		Instance = this;
		tabContainer = GetNode<TabContainer>("Control/TabContainer");
		buttonMenu = GetNode<ButtonMenu>("Control/TabContainer/System/ButtonMenu");
		arrowButton = GetNode<AbilityButton>("Control/TabContainer/Inventory/Abilities/VBoxContainer/AbilityButton3");
		bombButton = GetNode<AbilityButton>("Control/TabContainer/Inventory/Abilities/VBoxContainer/AbilityButton4");
		system = GetNode<Control>("Control/TabContainer/System");
		save = buttonMenu.GetNode<Button>("Save");
		load = buttonMenu.GetNode<Button>("Load");
		menu = buttonMenu.GetNode<Button>("Menu");
		itemDescription = GetNode<Label>("Control/TabContainer/Inventory/Description/ItemDescription");
		AudioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
		Stats = GetNode<Stats>("Control/TabContainer/Inventory/Stats");
		InventorySlot.AudioStreamPlayer = AudioStreamPlayer;

		load.Connect(BaseButton.SignalName.Pressed, new(this, MethodName.OnLoadPressed));
		menu.Connect(BaseButton.SignalName.Pressed, new(this, MethodName.OnMenuPressed));
		save.Connect(BaseButton.SignalName.Pressed, new(this, MethodName.OnSavePressed));
		buttonMenu.ConnectFocus(menu, AudioStreamPlayer);
		buttonMenu.ConnectFocus(load, AudioStreamPlayer);
		system.Connect(CanvasItem.SignalName.VisibilityChanged, new(this, MethodName.OnSystemVisibilityChanged));
		itemDescription.Text = "";

		HidePauseMenu();
	}

	private void OnSystemVisibilityChanged()
	{
		if (!system.Visible)
			return;

		load.Visible = GlobalSaveManager.CheckLoad();
		save.GrabFocus();

		if (!buttonMenu.IsConnectedFocus(save))
			buttonMenu.ConnectFocus(save, AudioStreamPlayer);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
		{
			if (isPaused)
				HidePauseMenu();
			else
				ShowPauseMenu();
		}

		GetViewport().SetInputAsHandled();
	}

	private void ShowPauseMenu()
	{
		// pause music
		GlobalAudioManager.Instance.ProcessMode = ProcessModeEnum.Inherit;
		Show();
		isPaused = true;
		GetTree().Paused = true;
		tabContainer.CurrentTab = 0;
		arrowButton.UpdateLabel(GlobalPlayerManager.Instance.Player.Arrows);
		bombButton.UpdateLabel(GlobalPlayerManager.Instance.Player.Bombs);

		EmitSignal(SignalName.Shown);
	}

	private void HidePauseMenu()
	{
		// continue music
		GlobalAudioManager.Instance.ProcessMode = ProcessModeEnum.Always;
		Hide();
		isPaused = false;
		GetTree().Paused = false;
		EmitSignal(SignalName.Hidden);

		if (DialogSystem.Instance != null && DialogSystem.Instance.Visible && DialogSystem.Instance.DialogProgressIndicator.Visible)
			DialogSystem.Instance.DialogProgressIndicator.GrabFocus();
	}

	private void OnSavePressed()
	{
		ButtonMenu.PlayPress(AudioStreamPlayer);
		GlobalSaveManager.Instance.SaveGame();
		HidePauseMenu();
	}

	private void OnLoadPressed()
	{
		ButtonMenu.PlayPress(AudioStreamPlayer);
		AudioStreamPlayer.Connect(AudioStreamPlayer.SignalName.Finished, new(this, MethodName.OnLoadPressed2));
		buttonMenu.DisconnectFocus(save);
	}

	private void OnLoadPressed2()
	{
		AudioStreamPlayer.Disconnect(AudioStreamPlayer.SignalName.Finished, new(this, MethodName.OnLoadPressed2));
		GlobalSaveManager.Instance.LoadGame();
		HidePauseMenu();
	}

	private void OnMenuPressed()
	{
		ButtonMenu.PlayPress(AudioStreamPlayer);
		AudioStreamPlayer.Connect(AudioStreamPlayer.SignalName.Finished, new(this, MethodName.OnMenuPressed2));
		buttonMenu.DisconnectFocus(save);
	}

	private void OnMenuPressed2()
	{
		AudioStreamPlayer.Disconnect(AudioStreamPlayer.SignalName.Finished, new(this, MethodName.OnMenuPressed2));
		GlobalLevelManager.Instance.LoadNewLevel("res://title_screen/TitleScene.tscn", "", Vector2.Zero);
		HidePauseMenu();
	}

	public void UpdateDescription(string description)
	{
		itemDescription.Text = description;
	}

	public void PlayAudio(AudioStream audioStream)
	{
		AudioStreamPlayer.Stream = audioStream;
		AudioStreamPlayer.Play();
	}
}
