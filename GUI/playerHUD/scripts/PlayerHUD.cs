using System;
using System.Collections.Generic;
using Godot;

public class PlayerHUD : CanvasLayer
{
	// private
	private readonly List<HeartGUI> hearts = new List<HeartGUI>();
	private Control gameOver;
	private Button continueButton;
	private Button menuButton;
	private ButtonMenu buttonMenu;
	private AnimationPlayer animationPlayer;
	private AudioStreamPlayer audioStreamPlayer;
	private Control BossHpBar;
	private NotificationBar notificationBar;
	private TextureProgress textureProgress;
	private Label BossNameLabel;

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
		textureProgress = GetNode<TextureProgress>("Control/BossHpBar/TextureProgress");
		BossNameLabel = GetNode<Label>("Control/BossHpBar/Label");

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
		menuButton.Connect("pressed", this, nameof(BackToMenu));
		continueButton.Connect("pressed", this, nameof(LoadGame));
		buttonMenu.ConnectFocus(menuButton, audioStreamPlayer);
		buttonMenu.ConnectFocus(continueButton, audioStreamPlayer);
		GlobalLevelManager.Instance.Connect(nameof(GlobalLevelManager.LevelLoadStarted), this, nameof(HideGameOverScreen));
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
		continueButton.Visible = GlobalSaveManager.Instance.CheckLoad();
		animationPlayer.Connect("animation_finished", this, nameof(ShowGameOverScreen2));
	}

	private void ShowGameOverScreen2(string animName)
	{
		animationPlayer.Disconnect("animation_finished", this, nameof(ShowGameOverScreen2));

		if (!continueButton.Visible)
			menuButton.GrabFocus();
		else
			continueButton.GrabFocus();
	}

	private void LoadGame()
	{
		buttonMenu.PlayPress(audioStreamPlayer);
		GetTree().CreateTimer(FadeToBlack(), false).Connect("timeout", this, nameof(LoadGame2));
	}

	private void LoadGame2()
	{
		GlobalSaveManager.Instance.LoadGame();
	}

	private void BackToMenu()
	{
		buttonMenu.PlayPress(audioStreamPlayer);
		GetTree().CreateTimer(FadeToBlack(), false).Connect("timeout", this, nameof(BackToMenu2));
	}

	private void BackToMenu2()
	{
		GlobalLevelManager.Instance.LoadNewLevel("res://title_screen/TitleScene.tscn", "", Vector2.Zero);
	}

	private float FadeToBlack()
	{
		animationPlayer.Play("fadeToBlack");
		GlobalPlayerManager.Instance.Player.RevivePlayer();
		return animationPlayer.CurrentAnimationLength;
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
}
