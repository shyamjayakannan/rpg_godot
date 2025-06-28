using Godot;

public class PauseMenu : CanvasLayer
{
	// Signals
	[Signal]
	public delegate void Shown();
	[Signal]
	public delegate void Hidden();
	[Signal]
	public delegate void EquipmentChanged(EquipableItem equipableItem);
	[Signal]
	public delegate void ItemRemoved();

	// private
	private Button save;
	private Button load;
	private Button menu;
	private ButtonMenu buttonMenu;
	private TabContainer tabContainer;
	private bool isPaused = false;
	private Label itemDescription;
	private Control system;

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
		system = GetNode<Control>("Control/TabContainer/System");
		save = buttonMenu.GetNode<Button>("Save");
		load = buttonMenu.GetNode<Button>("Load");
		menu = buttonMenu.GetNode<Button>("Menu");
		itemDescription = GetNode<Label>("Control/TabContainer/Inventory/Description/ItemDescription");
		AudioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
		Stats = GetNode<Stats>("Control/TabContainer/Inventory/Stats");
		InventorySlot.AudioStreamPlayer = AudioStreamPlayer;
		InventorySlot.FocusSound = buttonMenu.ButtonFocusSound;
		InventorySlot.PressSound = buttonMenu.ButtonPressSound;

		load.Connect("pressed", this, nameof(OnLoadPressed));
		menu.Connect("pressed", this, nameof(OnMenuPressed));
		save.Connect("pressed", this, nameof(OnSavePressed));
		buttonMenu.ConnectFocus(menu, AudioStreamPlayer);
		buttonMenu.ConnectFocus(load, AudioStreamPlayer);
		system.Connect("visibility_changed", this, nameof(OnSystemVisibilityChanged));
		itemDescription.Text = "";

		HidePauseMenu();
	}

	private void OnSystemVisibilityChanged()
	{
		if (!system.Visible)
			return;

		load.Visible = GlobalSaveManager.Instance.CheckLoad();
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
		GlobalAudioManager.Instance.PauseMode = PauseModeEnum.Inherit;
		Show();
		isPaused = true;
		GetTree().Paused = true;
		tabContainer.CurrentTab = 0;

		EmitSignal(nameof(Shown));
	}

	private void HidePauseMenu()
	{
		// continue music
		GlobalAudioManager.Instance.PauseMode = PauseModeEnum.Process;
		Hide();
		isPaused = false;
		GetTree().Paused = false;
		EmitSignal(nameof(Hidden));

		if (DialogSystem.Instance != null && DialogSystem.Instance.Visible && DialogSystem.Instance.DialogProgressIndicator.Visible)
			DialogSystem.Instance.DialogProgressIndicator.GrabFocus();
	}

	private void OnSavePressed()
	{
		buttonMenu.PlayPress(AudioStreamPlayer);
		GlobalSaveManager.Instance.SaveGame();
		HidePauseMenu();
	}

	private void OnLoadPressed()
	{
		buttonMenu.PlayPress(AudioStreamPlayer);
		AudioStreamPlayer.Connect("finished", this, nameof(OnLoadPressed2));
		buttonMenu.DisconnectFocus(save);
	}

	private void OnLoadPressed2()
	{
		AudioStreamPlayer.Disconnect("finished", this, nameof(OnLoadPressed2));
		GlobalSaveManager.Instance.LoadGame();
		HidePauseMenu();
	}

	private void OnMenuPressed()
	{
		buttonMenu.PlayPress(AudioStreamPlayer);
		AudioStreamPlayer.Connect("finished", this, nameof(OnMenuPressed2));
		buttonMenu.DisconnectFocus(save);
	}

	private void OnMenuPressed2()
	{
		AudioStreamPlayer.Disconnect("finished", this, nameof(OnMenuPressed2));
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
