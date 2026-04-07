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

    [Fact]
    public void StatusEffectSaveData_RoundTrip_PreservesAllFields()
    {
        // Arrange
        var original = new SaveData
        {
            Player = new PlayerSaveData
            {
                Name = "テスト",
                StatusEffects = new List<StatusEffectSaveData>
                {
                    new StatusEffectSaveData
                    {
                        Type = "Poison",
                        RemainingTurns = 15,
                        Potency = 7,
                        Name = "毒"
                    },
                    new StatusEffectSaveData
                    {
                        Type = "Burn",
                        RemainingTurns = 5,
                        Potency = 4,
                        Name = "火傷"
                    },
                    new StatusEffectSaveData
                    {
                        Type = "Blessing",
                        RemainingTurns = 50,
                        Potency = 0,
                        Name = "祝福"
                    }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(original, JsonOptions);
        var restored = JsonSerializer.Deserialize<SaveData>(json, JsonOptions);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(3, restored!.Player.StatusEffects.Count);

        var poison = restored.Player.StatusEffects[0];
        Assert.Equal("Poison", poison.Type);
        Assert.Equal(15, poison.RemainingTurns);
        Assert.Equal(7, poison.Potency);
        Assert.Equal("毒", poison.Name);

        var burn = restored.Player.StatusEffects[1];
        Assert.Equal("Burn", burn.Type);
        Assert.Equal(5, burn.RemainingTurns);
        Assert.Equal(4, burn.Potency);
        Assert.Equal("火傷", burn.Name);

        var blessing = restored.Player.StatusEffects[2];
        Assert.Equal("Blessing", blessing.Type);
        Assert.Equal(50, blessing.RemainingTurns);
        Assert.Equal(0, blessing.Potency);
        Assert.Equal("祝福", blessing.Name);
    }

    [Fact]
    public void StatusEffectSaveData_BackwardCompatibility_EmptyNameDefaultsToEmpty()
    {
        // Arrange: 旧形式のセーブデータ（Nameフィールドなし）
        var json = """
        {
            "type": "Curse",
            "remainingTurns": 30,
            "potency": 1
        }
        """;

        // Act
        var restored = JsonSerializer.Deserialize<StatusEffectSaveData>(json, JsonOptions);

        // Assert: Nameが空文字列のデフォルト値になる（後方互換性）
        Assert.NotNull(restored);
        Assert.Equal("Curse", restored!.Type);
        Assert.Equal(30, restored.RemainingTurns);
        Assert.Equal(1, restored.Potency);
        Assert.Equal(string.Empty, restored.Name);
    }

    [Fact]
    public void StatusEffect_RestoredFromSaveData_HasCorrectNameAndDamage()
    {
        // Arrange: セーブデータから復元シミュレーション
        var saveData = new StatusEffectSaveData
        {
            Type = "Poison",
            RemainingTurns = 10,
            Potency = 12,
            Name = "毒"
        };

        // Act: ロード処理と同じロジック
        Enum.TryParse<StatusEffectType>(saveData.Type, out var effectType);
        var restoredName = !string.IsNullOrEmpty(saveData.Name) ? saveData.Name : effectType.ToString();
        var effect = new StatusEffect(effectType, saveData.RemainingTurns)
        {
            Name = restoredName,
            DamagePerTick = saveData.Potency
        };

        // Assert
        Assert.Equal(StatusEffectType.Poison, effect.Type);
        Assert.Equal(10, effect.Duration);
        Assert.Equal(12, effect.DamagePerTick);  // セーブされた値が復元される（コンストラクタの3ではなく）
        Assert.Equal("毒", effect.Name);          // 日本語名が復元される（"Poison"ではなく）
    }

    [Fact]
    public void StatusEffect_RestoredFromOldSaveData_FallsBackToTypeName()
    {
        // Arrange: 旧形式セーブデータ（Nameなし）からの復元
        var saveData = new StatusEffectSaveData
        {
            Type = "Burn",
            RemainingTurns = 5,
            Potency = 4,
            Name = ""  // 旧形式ではNameが空
        };

        // Act
        Enum.TryParse<StatusEffectType>(saveData.Type, out var effectType);
        var restoredName = !string.IsNullOrEmpty(saveData.Name) ? saveData.Name : effectType.ToString();
        var effect = new StatusEffect(effectType, saveData.RemainingTurns)
        {
            Name = restoredName,
            DamagePerTick = saveData.Potency
        };

        // Assert: Nameが空の場合はType名にフォールバック
        Assert.Equal("Burn", effect.Name);
        Assert.Equal(4, effect.DamagePerTick);
    }
}
