using System.Collections.Generic;
using Godot;
using System.Text.Json;

namespace Rpg
{
	public partial class GlobalSaveManager : Node
	{
		// Signals
		[Signal]
		private delegate void GameSavedEventHandler();
		[Signal]
		public delegate void GameLoadedEventHandler();

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
			public List<(string, int)> InCompleteSteps { get; set; }
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
			public int YSortOrigin { get; set; }
		}
		private struct SaveData
		{
			public Player Player { get; set; }
			public string ScenePath { get; set; }
			public List<ItemData> Items { get; set; }
			public List<ItemData> Equipment { get; set; }
			public List<string> Persistence { get; set; }
			public List<QuestData> Quests { get; set; }
			public Dictionary<string, List<(ItemData, Vector2, int)>> DroppedItems { get; set; }
		}
		private SaveData currentSaveData = new()
		{
			Player = new()
			{
				Hp = 1,
				MaxHp = 1,
				PosX = 0,
				PosY = 0,
				Level = 1,
				Xp = 0,
				Attack = 1,
				Defence = 1,
				YSortOrigin = 0
			},
			ScenePath = "",
			Items = new(),
			Equipment = new(),
			Persistence = new(),
			Quests = new(),
			DroppedItems = new()
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

			FileAccess file = FileAccess.Open(SAVEPATH + "savegame.sav", FileAccess.ModeFlags.Write);
			file.StoreLine(JsonSerializer.Serialize(currentSaveData));
			file.Close();
			EmitSignal(SignalName.GameSaved);
		}

		public static bool CheckLoad()
		{
			return FileAccess.FileExists(SAVEPATH + "savegame.sav");
		}

		public void LoadGame()
		{
			FileAccess file = FileAccess.Open(SAVEPATH + "savegame.sav", FileAccess.ModeFlags.Read);
			currentSaveData = JsonSerializer.Deserialize<SaveData>(file.GetLine());
			file.Close();

			GlobalLevelManager.Instance.LoadNewLevel(
				currentSaveData.ScenePath,
				"",
				Vector2.Zero
			);

			SetPlayer();
			SetPlayerInventory();
			SetQuests();
			SetDroppedItems();

			GlobalLevelManager.Instance.Connect(GlobalLevelManager.SignalName.LevelLoaded, Callable.From(() => EmitSignal(SignalName.GameLoaded)), (uint)ConnectFlags.OneShot);
		}

		private void UpdatePlayer()
		{
			currentSaveData.Player = new()
			{
				Hp = GlobalPlayerManager.Instance.Player.Hp,
				MaxHp = GlobalPlayerManager.Instance.Player.MaxHp,
				PosX = GlobalPlayerManager.Instance.Player.GlobalPosition.X,
				PosY = GlobalPlayerManager.Instance.Player.GlobalPosition.Y,
				Level = GlobalPlayerManager.Instance.Player.Level,
				Xp = GlobalPlayerManager.Instance.Player.Xp,
				Attack = GlobalPlayerManager.Instance.Player.Attack,
				Defence = GlobalPlayerManager.Instance.Player.Defence,
				YSortOrigin = GlobalPlayerManager.Instance.PlayerYSortHandler.YSortOrigin
			};
		}

		private void UpdateScenePath()
		{
			currentSaveData.ScenePath = GetTree().Root.GetNode<Node2D>("Level").SceneFilePath;
		}

		private void UpdateItems()
		{
			currentSaveData.Items = GlobalPlayerManager.Instance.PlayerInventory.GetSaveData();
			currentSaveData.Equipment = GlobalPlayerManager.Instance.PlayerEquipmentInventory.GetSaveData();
			currentSaveData.DroppedItems = GlobalLevelManager.Instance.GetSaveData();
		}

		private void UpdateQuests()
		{
			currentSaveData.Quests = GlobalQuestManager.Instance.CurrentQuests;
		}

		private void SetPlayer()
		{
			GlobalPlayerManager.Instance.SetPlayerPosition(
				new(
					currentSaveData.Player.PosX,
					currentSaveData.Player.PosY
				),
				currentSaveData.Player.YSortOrigin
			);
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

		private void SetDroppedItems()
		{
			GlobalLevelManager.Instance.SetSaveData(currentSaveData.DroppedItems);
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
}
