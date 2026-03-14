using Xunit;
using RougelikeGame.Core;
using System.Text.Json;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// SaveDataのシリアライズ・デシリアライズテスト
/// </summary>
public class SaveDataTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void SaveData_RoundTrip_PreservesPlayerData()
    {
        // Arrange
        var original = new SaveData
        {
            CurrentFloor = 3,
            TurnCount = 150,
            SavedAt = DateTime.Now,
            GameTime = new GameTimeSaveData { TotalTurns = 150 },
            Player = new PlayerSaveData
            {
                Name = "テスト冒険者",
                Level = 5,
                Experience = 1200,
                CurrentHp = 80,
                CurrentMp = 30,
                CurrentSp = 50,
                Sanity = 90,
                Hunger = 75,
                RescueCountRemaining = 2,
                BaseStats = new StatsSaveData
                {
                    Strength = 14, Vitality = 12, Agility = 12,
                    Dexterity = 10, Intelligence = 10, Mind = 10,
                    Perception = 10, Charisma = 10, Luck = 10
                },
                Position = new PositionSaveData { X = 10, Y = 20 }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(original, JsonOptions);
        var restored = JsonSerializer.Deserialize<SaveData>(json, JsonOptions);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(original.CurrentFloor, restored.CurrentFloor);
        Assert.Equal(original.TurnCount, restored.TurnCount);
        Assert.Equal(original.Player.Name, restored.Player.Name);
        Assert.Equal(original.Player.Level, restored.Player.Level);
        Assert.Equal(original.Player.Experience, restored.Player.Experience);
        Assert.Equal(original.Player.CurrentHp, restored.Player.CurrentHp);
        Assert.Equal(original.Player.Sanity, restored.Player.Sanity);
        Assert.Equal(original.Player.Hunger, restored.Player.Hunger);
        Assert.Equal(original.Player.Position.X, restored.Player.Position.X);
        Assert.Equal(original.Player.Position.Y, restored.Player.Position.Y);
    }

    [Fact]
    public void SaveData_RoundTrip_PreservesInventoryItems()
    {
        // Arrange
        var original = new SaveData
        {
            Player = new PlayerSaveData
            {
                Name = "テスト",
                InventoryItems =
                {
                    new ItemSaveData { ItemId = "iron_sword", EnhancementLevel = 2, IsIdentified = true },
                    new ItemSaveData { ItemId = "healing_potion", StackCount = 3 }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(original, JsonOptions);
        var restored = JsonSerializer.Deserialize<SaveData>(json, JsonOptions);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(2, restored.Player.InventoryItems.Count);
        Assert.Equal("iron_sword", restored.Player.InventoryItems[0].ItemId);
        Assert.Equal(2, restored.Player.InventoryItems[0].EnhancementLevel);
        Assert.Equal("healing_potion", restored.Player.InventoryItems[1].ItemId);
        Assert.Equal(3, restored.Player.InventoryItems[1].StackCount);
    }

    [Fact]
    public void SaveData_RoundTrip_PreservesEquippedItems()
    {
        // Arrange
        var original = new SaveData
        {
            Player = new PlayerSaveData
            {
                Name = "テスト",
                EquippedItems =
                {
                    ["MainHand"] = new ItemSaveData { ItemId = "iron_sword" },
                    ["Body"] = new ItemSaveData { ItemId = "leather_armor" }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(original, JsonOptions);
        var restored = JsonSerializer.Deserialize<SaveData>(json, JsonOptions);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(2, restored.Player.EquippedItems.Count);
        Assert.True(restored.Player.EquippedItems.ContainsKey("MainHand"));
        Assert.True(restored.Player.EquippedItems.ContainsKey("Body"));
    }

    [Fact]
    public void SaveData_RoundTrip_PreservesLearnedWords()
    {
        // Arrange
        var original = new SaveData
        {
            Player = new PlayerSaveData
            {
                Name = "テスト",
                LearnedWords = { ["fire"] = 3, ["heal"] = 5 }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(original, JsonOptions);
        var restored = JsonSerializer.Deserialize<SaveData>(json, JsonOptions);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(2, restored.Player.LearnedWords.Count);
        Assert.Equal(3, restored.Player.LearnedWords["fire"]);
        Assert.Equal(5, restored.Player.LearnedWords["heal"]);
    }

    [Fact]
    public void SaveData_RoundTrip_PreservesMessageHistory()
    {
        // Arrange
        var original = new SaveData
        {
            Player = new PlayerSaveData { Name = "テスト" },
            MessageHistory = { "ダンジョンに入った", "敵を倒した", "アイテムを拾った" }
        };

        // Act
        var json = JsonSerializer.Serialize(original, JsonOptions);
        var restored = JsonSerializer.Deserialize<SaveData>(json, JsonOptions);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(3, restored.MessageHistory.Count);
        Assert.Equal("ダンジョンに入った", restored.MessageHistory[0]);
    }

    [Fact]
    public void StatsSaveData_RoundTrip_WithStats()
    {
        // Arrange
        var stats = new Stats(14, 12, 12, 10, 10, 10, 10, 10, 10);
        var saveData = StatsSaveData.FromStats(stats);

        // Act
        var restored = saveData.ToStats();

        // Assert
        Assert.Equal(stats.Strength, restored.Strength);
        Assert.Equal(stats.Vitality, restored.Vitality);
        Assert.Equal(stats.Agility, restored.Agility);
        Assert.Equal(stats.Intelligence, restored.Intelligence);
    }

    [Fact]
    public void PositionSaveData_RoundTrip()
    {
        // Arrange
        var pos = new Position(15, 25);
        var saveData = PositionSaveData.FromPosition(pos);

        // Act
        var restored = saveData.ToPosition();

        // Assert
        Assert.Equal(pos.X, restored.X);
        Assert.Equal(pos.Y, restored.Y);
    }

    [Fact]
    public void SaveData_DefaultVersion_Is1()
    {
        // Arrange & Act
        var save = new SaveData();

        // Assert
        Assert.Equal(1, save.Version);
    }
}
