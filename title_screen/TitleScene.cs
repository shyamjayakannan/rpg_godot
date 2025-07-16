using Godot;

namespace Rpg
{
	public partial class TitleScene : Node2D
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
			GlobalLevelManager.Instance.Connect(GlobalLevelManager.SignalName.LevelLoadStarted, new(this, MethodName.ExitTitleScreen));

			PlayerHUD.Instance.Hide();
			PauseMenu.Instance.ProcessMode = ProcessModeEnum.Disabled;

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

			newButton.Connect(BaseButton.SignalName.Pressed, new(this, MethodName.StartGame));
			continueButton.Connect(BaseButton.SignalName.Pressed, new(this, MethodName.LoadGame));
			quit.Connect(BaseButton.SignalName.Pressed, Callable.From(() => GetTree().Quit()));
			buttonMenu.ConnectFocus(newButton, audioStreamPlayer);
			buttonMenu.ConnectFocus(continueButton, audioStreamPlayer);
			buttonMenu.ConnectFocus(quit, audioStreamPlayer);
			continueButton.Visible = GlobalSaveManager.CheckLoad();

			GlobalAudioManager.Instance.PlayAudio(music);
		}

		private void LoadGame()
		{
			ButtonMenu.PlayPress(audioStreamPlayer);
			GlobalSaveManager.Instance.LoadGame();
		}

		private void StartGame()
		{
			ButtonMenu.PlayPress(audioStreamPlayer);
			GlobalLevelManager.Instance.LoadNewLevel(START_LEVEL, "", Vector2.Zero);
		}

		private void ExitTitleScreen()
		{
			GlobalPlayerManager.Instance.Player.Show();
			PlayerHUD.Instance.Show();
			PauseMenu.Instance.ProcessMode = ProcessModeEnum.Always;
			QueueFree();
		}
	}
}
