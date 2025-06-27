using Godot;

public class TitleScene : Node2D
{
	// Exports
	[Export]
	private AudioStream music;

	// constants
	private const string START_LEVEL = "res://Levels/Area1/2.tscn";

	// private
	private ButtonMenu buttonMenu;
	private Button newButton;
	private Button continueButton;
	private Button quit;
	private AudioStreamPlayer audioStreamPlayer;

	// methods
	public override void _Ready()
	{
		buttonMenu = GetNode<ButtonMenu>("CanvasLayer/Control/ButtonMenu");
		newButton = buttonMenu.GetNode<Button>("NewButton");
		continueButton = buttonMenu.GetNode<Button>("ContinueButton");
		quit = buttonMenu.GetNode<Button>("Quit");
		audioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");

		GlobalPlayerManager.Instance.PlayerSpawned = false;
		GlobalLevelManager.Instance.Connect(nameof(GlobalLevelManager.LevelLoadStarted), this, nameof(ExitTitleScreen));

		PlayerHUD.Instance.Hide();
		PauseMenu.Instance.PauseMode = PauseModeEnum.Stop;

		// make sure that audioplayer's pause mode is set to process
		GetTree().Paused = true;
		GlobalPlayerManager.Instance.Player.Hide();
		SetupTitleScreen();
	}

	private void SetupTitleScreen()
	{
		// do this before connecting the signals so that button focus sound doesn't run the first time
		if (!continueButton.Visible)
			newButton.GrabFocus();
		else
			continueButton.GrabFocus();

		newButton.Connect("pressed", this, nameof(StartGame));
		continueButton.Connect("pressed", this, nameof(LoadGame));
		quit.Connect("pressed", this, nameof(EndGame));
		buttonMenu.ConnectFocus(newButton, audioStreamPlayer);
		buttonMenu.ConnectFocus(continueButton, audioStreamPlayer);
		buttonMenu.ConnectFocus(quit, audioStreamPlayer);
		continueButton.Visible = GlobalSaveManager.Instance.CheckLoad();

		GlobalAudioManager.Instance.PlayAudio(music);
	}

	private void EndGame()
	{
		GetTree().Quit();
	}

	private void LoadGame()
	{
		buttonMenu.PlayPress(audioStreamPlayer);
		GlobalSaveManager.Instance.LoadGame();
	}

	private void StartGame()
	{
		buttonMenu.PlayPress(audioStreamPlayer);
		GlobalLevelManager.Instance.LoadNewLevel(START_LEVEL, "", Vector2.Zero);
	}

	private void ExitTitleScreen()
	{
		GlobalPlayerManager.Instance.Player.Show();
		PlayerHUD.Instance.Show();
		PauseMenu.Instance.PauseMode = PauseModeEnum.Process;
		QueueFree();
	}
}
