using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Entities;
using RougelikeGame.Core.Systems;
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

    [Fact]
    public void StatusEffectSaveData_RoundTrip_PreservesStackAndElement()
    {
        // Arrange: スタック済み毒のセーブデータ
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
                        RemainingTurns = 10,
                        Potency = 5,
                        Name = "毒",
                        StackCount = 3,
                        DamageElement = "Poison",
                        MaxStack = 3
                    }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(original, JsonOptions);
        var restored = JsonSerializer.Deserialize<SaveData>(json, JsonOptions);

        // Assert
        Assert.NotNull(restored);
        var effect = restored!.Player.StatusEffects[0];
        Assert.Equal(3, effect.StackCount);
        Assert.Equal("Poison", effect.DamageElement);
        Assert.Equal(3, effect.MaxStack);
    }

    [Fact]
    public void StatusEffect_RestoredWithStackCount_HasCorrectTickDamage()
    {
        // Arrange: スタック3の毒を復元
        var saveData = new StatusEffectSaveData
        {
            Type = "Poison",
            RemainingTurns = 10,
            Potency = 5,
            Name = "毒",
            StackCount = 3,
            DamageElement = "Poison",
            MaxStack = 3
        };

        // Act: ロード処理と同じロジック
        Enum.TryParse<StatusEffectType>(saveData.Type, out var effectType);
        var restoredElement = Enum.TryParse<Element>(saveData.DamageElement, out var elem) ? elem : Element.None;
        var effect = new StatusEffect(effectType, saveData.RemainingTurns)
        {
            Name = saveData.Name,
            DamagePerTick = saveData.Potency,
            DamageElement = restoredElement,
            MaxStack = saveData.MaxStack
        };
        if (saveData.StackCount > 1)
        {
            effect.RestoreStackCount(saveData.StackCount);
        }

        // Assert
        Assert.Equal(3, effect.StackCount);
        Assert.Equal(Element.Poison, effect.DamageElement);
        Assert.Equal(3, effect.MaxStack);
        Assert.True(effect.IsStackable);
        var damage = effect.GetTickDamage();
        Assert.NotNull(damage);
        Assert.Equal(15, damage.Value.Amount);  // 5 * 3 stacks = 15
    }

    [Fact]
    public void StatusEffect_BackwardCompat_OldSaveDataStackDefaults()
    {
        // Arrange: 旧形式セーブデータ（StackCount/DamageElement/MaxStackなし）
        var json = """
        {
            "type": "Poison",
            "remainingTurns": 10,
            "potency": 3,
            "name": "毒"
        }
        """;

        // Act
        var restored = JsonSerializer.Deserialize<StatusEffectSaveData>(json, JsonOptions);

        // Assert: デフォルト値にフォールバック
        Assert.NotNull(restored);
        Assert.Equal(1, restored!.StackCount);    // デフォルト1
        Assert.Equal(string.Empty, restored.DamageElement);  // デフォルト空
        Assert.Equal(1, restored.MaxStack);        // デフォルト1
    }

    [Fact]
    public void StatusEffect_Constructor_SetsDefaultDamageElement()
    {
        // Arrange & Act: コンストラクタでのDamageElementデフォルト値確認
        var poison = new StatusEffect(StatusEffectType.Poison, 10);
        var instantDeath = new StatusEffect(StatusEffectType.InstantDeath, 1);
        var burn = new StatusEffect(StatusEffectType.Burn, 5);

        // Assert
        Assert.Equal(Element.Poison, poison.DamageElement);      // 毒は毒属性
        Assert.Equal(Element.Dark, instantDeath.DamageElement);   // 即死は闇属性
        Assert.Equal(Element.Fire, burn.DamageElement);            // 火傷は火属性
    }

    [Fact]
    public void StatusEffect_RestoreStackCount_ClampsToMaxStack()
    {
        // Arrange
        var effect = new StatusEffect(StatusEffectType.Poison, 10) { MaxStack = 3 };

        // Act: MaxStackを超えるStackCountを復元しようとする
        effect.RestoreStackCount(5);

        // Assert: MaxStack=3にクランプされる
        Assert.Equal(3, effect.StackCount);
    }

    [Fact]
    public void StatusEffect_RestoreStackCount_ClampsToMin1()
    {
        // Arrange
        var effect = new StatusEffect(StatusEffectType.Poison, 10) { MaxStack = 3 };

        // Act: 0以下のStackCountを復元しようとする
        effect.RestoreStackCount(0);

        // Assert: 最低1にクランプされる
        Assert.Equal(1, effect.StackCount);
    }

    [Fact]
    public void PlayerSaveData_BonusStats_RoundTrip()
    {
        // Arrange
        var saveData = new PlayerSaveData
        {
            Name = "テスト",
            BonusMaxHp = 50,
            BonusMaxMp = 30,
            BonusCriticalRate = 0.15,
            KnownRunes = new List<string> { "fire_rune", "ice_rune" }
        };

        // Act
        var json = JsonSerializer.Serialize(saveData, JsonOptions);
        var restored = JsonSerializer.Deserialize<PlayerSaveData>(json, JsonOptions);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(50, restored!.BonusMaxHp);
        Assert.Equal(30, restored.BonusMaxMp);
        Assert.Equal(0.15, restored.BonusCriticalRate);
        Assert.Equal(2, restored.KnownRunes.Count);
        Assert.Contains("fire_rune", restored.KnownRunes);
        Assert.Contains("ice_rune", restored.KnownRunes);
    }

    [Fact]
    public void PlayerSaveData_BackwardCompat_OldSaveDataBonusDefaults()
    {
        // Arrange: 旧形式セーブデータ（BonusStats/KnownRunesなし）
        var json = """
        {
            "name": "テスト",
            "level": 5,
            "experience": 100
        }
        """;

        // Act
        var restored = JsonSerializer.Deserialize<PlayerSaveData>(json, JsonOptions);

        // Assert: デフォルト値にフォールバック
        Assert.NotNull(restored);
        Assert.Equal(0, restored!.BonusMaxHp);
        Assert.Equal(0, restored.BonusMaxMp);
        Assert.Equal(0.0, restored.BonusCriticalRate);
        Assert.Empty(restored.KnownRunes);
    }

    [Fact]
    public void PetSaveData_MaxHp_RoundTrip()
    {
        // Arrange
        var saveData = new PetSaveData
        {
            PetId = "pet_001",
            Name = "ポチ",
            PetType = "Wolf",
            Level = 5,
            CurrentHp = 80,
            MaxHp = 120
        };

        // Act
        var json = JsonSerializer.Serialize(saveData, JsonOptions);
        var restored = JsonSerializer.Deserialize<PetSaveData>(json, JsonOptions);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(120, restored!.MaxHp);
        Assert.Equal(80, restored.CurrentHp);
    }

    [Fact]
    public void PetSaveData_BackwardCompat_OldSaveDataMaxHpDefault()
    {
        // Arrange: 旧形式セーブデータ（MaxHpなし）
        var json = """
        {
            "petId": "pet_001",
            "name": "ポチ",
            "petType": "Wolf",
            "currentHp": 50
        }
        """;

        // Act
        var restored = JsonSerializer.Deserialize<PetSaveData>(json, JsonOptions);

        // Assert: MaxHpがデフォルト0（RestorePetStateで処理）
        Assert.NotNull(restored);
        Assert.Equal(0, restored!.MaxHp);
    }

    [Fact]
    public void PetSystem_RestorePetState_WithMaxHp_RestoresCorrectly()
    {
        // Arrange
        var petSystem = new PetSystem();
        petSystem.AddPet("pet_001", "ポチ", PetType.Wolf);

        // Act: MaxHp=120を復元（レベルアップ後の値）
        petSystem.RestorePetState("pet_001", 5, 100, 80, 70, 100, 120);

        // Assert
        var pets = petSystem.Pets;
        var pet = pets["pet_001"];
        Assert.Equal(120, pet.MaxHp);
        Assert.Equal(100, pet.CurrentHp);
        Assert.Equal(5, pet.Level);
    }

    [Fact]
    public void PetSystem_RestorePetState_WithZeroMaxHp_UsesBaseMaxHp()
    {
        // Arrange: 旧セーブデータではMaxHp=0
        var petSystem = new PetSystem();
        petSystem.AddPet("pet_001", "ポチ", PetType.Wolf);
        var baseMaxHp = petSystem.Pets["pet_001"].MaxHp;

        // Act: MaxHp=0（旧形式、デフォルト値でフォールバック）
        petSystem.RestorePetState("pet_001", 1, 0, 80, 50, 30, 0);

        // Assert: BaseHpを維持
        var pet = petSystem.Pets["pet_001"];
        Assert.Equal(baseMaxHp, pet.MaxHp);
    }

    [Fact]
    public void PetSystem_RestorePetState_RestoresIsRiding()
    {
        // Arrange
        var petSystem = new PetSystem();
        petSystem.AddPet("pet_001", "ポチ", PetType.Wolf);

        // Act: IsRiding=trueを復元
        petSystem.RestorePetState("pet_001", 3, 50, 80, 70, 60, 100, isRiding: true);

        // Assert
        var pet = petSystem.Pets["pet_001"];
        Assert.True(pet.IsRiding);
        Assert.Equal(3, pet.Level);
    }

    [Fact]
    public void PetSystem_RestorePetState_DefaultIsRidingIsFalse()
    {
        // Arrange: 旧セーブデータではIsRiding省略（デフォルトfalse）
        var petSystem = new PetSystem();
        petSystem.AddPet("pet_001", "ポチ", PetType.Wolf);

        // Act: IsRiding未指定
        petSystem.RestorePetState("pet_001", 1, 0, 80, 50, 30, 50);

        // Assert
        var pet = petSystem.Pets["pet_001"];
        Assert.False(pet.IsRiding);
    }

    [Fact]
    public void Player_RestorePreviousReligion_RestoresValue()
    {
        // Arrange
        var player = new Player { Name = "TestPlayer" };

        // Act
        player.RestorePreviousReligion("sun_god");

        // Assert
        Assert.Equal("sun_god", player.PreviousReligion);
    }

    [Fact]
    public void Player_RestorePreviousReligion_NullWhenNotSet()
    {
        // Arrange
        var player = new Player { Name = "TestPlayer" };

        // Act
        player.RestorePreviousReligion(null);

        // Assert
        Assert.Null(player.PreviousReligion);
    }

    [Fact]
    public void KarmaSystem_RestoreHistory_ParsesCorrectly()
    {
        // Arrange
        var karmaSystem = new KarmaSystem();
        var history = new List<string>
        {
            "0->3:クエスト完了",
            "3->-2:盗み発覚",
            "-2->-7:密輸"
        };

        // Act
        karmaSystem.RestoreHistory(history);

        // Assert
        Assert.Equal(3, karmaSystem.KarmaHistory.Count);
        Assert.Equal(0, karmaSystem.KarmaHistory[0].OldValue);
        Assert.Equal(3, karmaSystem.KarmaHistory[0].NewValue);
        Assert.Equal("クエスト完了", karmaSystem.KarmaHistory[0].Reason);
        Assert.Equal(3, karmaSystem.KarmaHistory[1].OldValue);
        Assert.Equal(-2, karmaSystem.KarmaHistory[1].NewValue);
        Assert.Equal("盗み発覚", karmaSystem.KarmaHistory[1].Reason);
    }

    [Fact]
    public void KarmaSystem_RestoreHistory_SkipsInvalidEntries()
    {
        // Arrange
        var karmaSystem = new KarmaSystem();
        var history = new List<string>
        {
            "invalid_entry",
            "0->5:有効なエントリ",
            "not->a:number"
        };

        // Act
        karmaSystem.RestoreHistory(history);

        // Assert: 有効なエントリのみ復元
        Assert.Equal(1, karmaSystem.KarmaHistory.Count);
        Assert.Equal("有効なエントリ", karmaSystem.KarmaHistory[0].Reason);
    }
}
