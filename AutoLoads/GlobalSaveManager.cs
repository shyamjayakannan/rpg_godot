using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;

public class GlobalSaveManager : Node
{
	// Signals
	[Signal]
	private delegate void GameSaved();
	[Signal]
	public delegate void GameLoaded();

	// properties
	public static GlobalSaveManager Instance { get; private set; }
	public struct ItemData
	{
		public int Quantity { get; set; }
		public string Path { get; set; }
	}
	public struct QuestData
	{
		public string Title { get; set; }
		public bool IsComplete { get; set; }
		public List<string> CompletedSteps { get; set; }
	}

	// private
	private const string SAVEPATH = "user://";
	private struct Player
	{
		public int Hp { get; set; }
		public int MaxHp { get; set; }
		public int Level { get; set; }
		public int Xp { get; set; }
		public int Attack { get; set; }
		public int Defence { get; set; }
		public float PosX { get; set; }
		public float PosY { get; set; }
	}
	private struct SaveData
	{
		public Player Player { get; set; }
		public string ScenePath { get; set; }
		public List<ItemData> Items { get; set; }
		public List<ItemData> Equipment { get; set; }
		public List<string> Persistence { get; set; }
		public List<QuestData> Quests { get; set; }
	}
	private SaveData currentSaveData = new SaveData()
	{
		Player = new Player
		{
			Hp = 1,
			MaxHp = 1,
			PosX = 0,
			PosY = 0,
			Level = 1,
			Xp = 0,
			Attack = 1,
			Defence = 1
		},
		ScenePath = "",
		Items = new List<ItemData>(),
		Equipment = new List<ItemData>(),
		Persistence = new List<string>(),
		Quests = new List<QuestData>()
	};

	// methods
	public override void _Ready()
	{
		Instance = this;
	}

	public void SaveGame()
	{
		UpdatePlayer();
		UpdateScenePath();
		UpdateItems();
		UpdateQuests();

		File file = new File();
		file.Open(SAVEPATH + "savegame.sav", File.ModeFlags.Write);
		file.StoreLine(JsonConvert.SerializeObject(currentSaveData));
		file.Close();
		EmitSignal(nameof(GameSaved));
	}

	public bool CheckLoad()
	{
		return new File().FileExists(SAVEPATH + "savegame.sav");
	}

	public void LoadGame()
	{
		File file = new File();
		file.Open(SAVEPATH + "savegame.sav", File.ModeFlags.Read);
		currentSaveData = JsonConvert.DeserializeObject<SaveData>(file.GetLine());
		file.Close();

		GlobalLevelManager.Instance.LoadNewLevel(
			currentSaveData.ScenePath,
			"",
			Vector2.Zero
		);

		SetPlayer();
		SetPlayerInventory();
		SetQuests();

		GlobalLevelManager.Instance.Connect(nameof(GlobalLevelManager.LevelLoaded), this, nameof(OnLevelLoaded));
	}

	private void OnLevelLoaded()
	{
		GlobalLevelManager.Instance.Disconnect(nameof(GlobalLevelManager.LevelLoaded), this, nameof(OnLevelLoaded));
		EmitSignal(nameof(GameLoaded));
	}

	private void UpdatePlayer()
	{
		currentSaveData.Player = new Player
		{
			Hp = GlobalPlayerManager.Instance.Player.Hp,
			MaxHp = GlobalPlayerManager.Instance.Player.MaxHp,
			PosX = GlobalPlayerManager.Instance.Player.GlobalPosition.x,
			PosY = GlobalPlayerManager.Instance.Player.GlobalPosition.y,
			Level = GlobalPlayerManager.Instance.Player.Level,
			Xp = GlobalPlayerManager.Instance.Player.Xp,
			Attack = GlobalPlayerManager.Instance.Player.Attack,
			Defence = GlobalPlayerManager.Instance.Player.Defence
		};
	}

	private void UpdateScenePath()
	{
		currentSaveData.ScenePath = GetTree().Root.GetNode<Node2D>("Level").Filename;
	}

	private void UpdateItems()
	{
		currentSaveData.Items = GlobalPlayerManager.Instance.PlayerInventory.GetSaveData();
		currentSaveData.Equipment = GlobalPlayerManager.Instance.PlayerEquipmentInventory.GetSaveData();
	}

	private void UpdateQuests()
	{
		currentSaveData.Quests = GlobalQuestManager.Instance.CurrentQuests;
	}

	private void SetPlayer()
	{
		GlobalPlayerManager.Instance.SetPlayerPosition(new Vector2(
			currentSaveData.Player.PosX,
			currentSaveData.Player.PosY
		));
		GlobalPlayerManager.Instance.Player.SetHP(
			currentSaveData.Player.Hp,
			currentSaveData.Player.MaxHp
		);
		GlobalPlayerManager.Instance.Player.Level = currentSaveData.Player.Level;
		GlobalPlayerManager.Instance.Player.Xp = currentSaveData.Player.Xp;
		GlobalPlayerManager.Instance.Player.Attack = currentSaveData.Player.Attack;
		GlobalPlayerManager.Instance.Player.Defence = currentSaveData.Player.Defence;
	}

	private void SetPlayerInventory()
	{
		GlobalPlayerManager.Instance.PlayerInventory.SetSaveData(currentSaveData.Items);
		GlobalPlayerManager.Instance.PlayerEquipmentInventory.SetSaveData(currentSaveData.Equipment);
	}

	private void SetQuests()
	{
		GlobalQuestManager.Instance.CurrentQuests = currentSaveData.Quests;
		GlobalQuestManager.Instance.LoadQuests();
	}

	public void AddPersistentValue(string value)
	{
		if (!CheckPersistentValue(value))
			currentSaveData.Persistence.Add(value);
	}

	public void RemovePersistentValue(string value)
	{
		if (CheckPersistentValue(value))
			currentSaveData.Persistence.Remove(value);
	}

	public bool CheckPersistentValue(string value)
	{
		return currentSaveData.Persistence.Contains(value);
	}
}
