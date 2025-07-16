using System.Collections.Generic;
using Godot;

namespace Rpg
{
	public partial class PlayerHUD : CanvasLayer
	{
		// private
		private readonly List<HeartGUI> hearts = new();
		private Control gameOver;
		private Button continueButton;
		private Button menuButton;
		private ButtonMenu buttonMenu;
		private AnimationPlayer animationPlayer;
		private AudioStreamPlayer audioStreamPlayer;
		private Control BossHpBar;
		private NotificationBar notificationBar;
		private TextureProgressBar textureProgress;
		private Label BossNameLabel;
		private Label arrowLabel;
		private Label bombLabel;
		private HBoxContainer abilities;

		// properties
		public static PlayerHUD Instance { get; private set; }

		// methods
		public override void _Ready()
		{
			Instance = this;
			gameOver = GetNode<Control>("Control/GameOver");
			buttonMenu = GetNode<ButtonMenu>("Control/GameOver/ButtonMenu");
			continueButton = buttonMenu.GetNode<Button>("Continue");
			menuButton = buttonMenu.GetNode<Button>("Menu");
			animationPlayer = GetNode<AnimationPlayer>("Control/GameOver/AnimationPlayer");
			audioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
			BossHpBar = GetNode<Control>("Control/BossHpBar");
			notificationBar = GetNode<NotificationBar>("Control/CanvasLayer/NotificationBar");
			textureProgress = GetNode<TextureProgressBar>("Control/BossHpBar/TextureProgressBar");
			BossNameLabel = GetNode<Label>("Control/BossHpBar/Label");
			abilities = GetNode<HBoxContainer>("Control/Abilities");
			arrowLabel = abilities.GetNode<Label>("Panel3/Label");
			bombLabel = abilities.GetNode<Label>("Panel4/Label2");

			foreach (Node child in GetNode<HFlowContainer>("./Control/HFlowContainer").GetChildren())
			{
				if (child is HeartGUI heart)
				{
					heart.Hide();
					hearts.Add(heart);
				}
			}

			HideBossHealthBar();
			HideGameOverScreen();
			menuButton.Connect(BaseButton.SignalName.Pressed, new(this, MethodName.BackToMenu));
			continueButton.Connect(BaseButton.SignalName.Pressed, new(this, MethodName.LoadGame));
			buttonMenu.ConnectFocus(menuButton, audioStreamPlayer);
			buttonMenu.ConnectFocus(continueButton, audioStreamPlayer);
			GlobalLevelManager.Instance.Connect(GlobalLevelManager.SignalName.LevelLoadStarted, new(this, MethodName.HideGameOverScreen));
		}

		public void QueueNotification(string title, string message)
		{
			notificationBar.AddNotificationToQueue(title, message);
		}

		public void HideBossHealthBar()
		{
			BossHpBar.Hide();
		}

		public void ShowBossHealthBar(string bossName)
		{
			BossNameLabel.Text = bossName;
			BossHpBar.Show();
		}

		public void UpdateBossHealthBar(float currentHp, float maxHp)
		{
			textureProgress.Value = Mathf.Clamp(currentHp / maxHp * 100, 0, 100);
		}

		private void HideGameOverScreen()
		{
			gameOver.Hide();
			gameOver.MouseFilter = Control.MouseFilterEnum.Ignore;
		}

		public void ShowGameOverScreen()
		{
			gameOver.Show();
			gameOver.MouseFilter = Control.MouseFilterEnum.Stop;
			animationPlayer.Play("showGameOver");
			continueButton.Visible = GlobalSaveManager.CheckLoad();
			animationPlayer.Connect(AnimationMixer.SignalName.AnimationFinished, new(this, MethodName.ShowGameOverScreen2), (uint)ConnectFlags.OneShot);
		}

		private void ShowGameOverScreen2(string _)
		{
			if (!continueButton.Visible)
				menuButton.GrabFocus();
			else
				continueButton.GrabFocus();
		}

		private void LoadGame()
		{
			ButtonMenu.PlayPress(audioStreamPlayer);
			GetTree().CreateTimer(FadeToBlack(), false).Connect(SceneTreeTimer.SignalName.Timeout, Callable.From(GlobalSaveManager.Instance.LoadGame));
		}

		private void BackToMenu()
		{
			ButtonMenu.PlayPress(audioStreamPlayer);
			GetTree().CreateTimer(FadeToBlack(), false).Connect(SceneTreeTimer.SignalName.Timeout, Callable.From(() => GlobalLevelManager.Instance.LoadNewLevel("res://title_screen/TitleScene.tscn", "", Vector2.Zero)));
		}

		private float FadeToBlack()
		{
			animationPlayer.Play("fadeToBlack");
			GlobalPlayerManager.Instance.Player.RevivePlayer();
			return (float)animationPlayer.CurrentAnimationLength;
		}

		public void UpdateHP(int hp, int maxHp)
		{
			UpdateMaxHP(maxHp);

			for (int i = 0; i <= hp; i++)
				UpdateHeart(i, hp);
		}

		private void UpdateHeart(int index, int hp)
		{
			hearts[index].FrameNumber = Mathf.Clamp(hp - index * 2, 0, 2);
		}

		private void UpdateMaxHP(int maxHp)
		{
			for (int i = 0; i < hearts.Count; i++)
				hearts[i].Visible = i < Mathf.Round(maxHp / 2);
		}

		public void UpdateArrows(int count)
		{
			arrowLabel.Text = count.ToString();
		}

		public void UpdateBombs(int count)
		{
			bombLabel.Text = count.ToString();
		}

		public void UpdateAbilityUI(int index)
		{
			for (int i = 0; i < abilities.GetChildCount(); i++)
			{
				if (i != index)
					abilities.GetChild<Panel>(i).SelfModulate = new(1, 1, 1, 0);
				else
					abilities.GetChild<Panel>(i).SelfModulate = new(1, 1, 1, 1);
			}

			ButtonMenu.PlayFocus(audioStreamPlayer);
		}
	}
}
