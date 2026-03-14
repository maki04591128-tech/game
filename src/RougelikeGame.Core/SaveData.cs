using System.Text.Json.Serialization;

namespace RougelikeGame.Core;

/// <summary>
/// ゲーム状態のセーブデータ
/// </summary>
public class SaveData
{
    /// <summary>セーブデータバージョン</summary>
    public int Version { get; set; } = 1;

    /// <summary>セーブ日時</summary>
    public DateTime SavedAt { get; set; }

    /// <summary>プレイヤーデータ</summary>
    public PlayerSaveData Player { get; set; } = new();

    /// <summary>現在の階層</summary>
    public int CurrentFloor { get; set; }

    /// <summary>累計ターン数</summary>
    public int TurnCount { get; set; }

    /// <summary>ゲーム内時間</summary>
    public GameTimeSaveData GameTime { get; set; } = new();

    /// <summary>メッセージ履歴</summary>
    public List<string> MessageHistory { get; set; } = new();

    /// <summary>ターン制限延長フラグ</summary>
    public bool TurnLimitExtended { get; set; }

    /// <summary>ターン制限撤廃フラグ</summary>
    public bool TurnLimitRemoved { get; set; }
}

/// <summary>
/// プレイヤーのセーブデータ
/// </summary>
public class PlayerSaveData
{
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Experience { get; set; }

    /// <summary>基礎ステータス</summary>
    public StatsSaveData BaseStats { get; set; } = new();

    /// <summary>現在HP</summary>
    public int CurrentHp { get; set; }
    /// <summary>現在MP</summary>
    public int CurrentMp { get; set; }
    /// <summary>現在SP</summary>
    public int CurrentSp { get; set; }

    /// <summary>正気度</summary>
    public int Sanity { get; set; }
    /// <summary>満腹度</summary>
    public int Hunger { get; set; }

    /// <summary>残り救出回数</summary>
    public int RescueCountRemaining { get; set; }

    /// <summary>位置</summary>
    public PositionSaveData Position { get; set; } = new();

    /// <summary>インベントリ（アイテムID + 強化値リスト）</summary>
    public List<ItemSaveData> InventoryItems { get; set; } = new();

    /// <summary>装備中（スロット→アイテムID）</summary>
    public Dictionary<string, ItemSaveData> EquippedItems { get; set; } = new();

    /// <summary>習得済み魔法言語</summary>
    public Dictionary<string, int> LearnedWords { get; set; } = new();

    /// <summary>習得済みスキル</summary>
    public List<string> LearnedSkills { get; set; } = new();

    /// <summary>信仰中の宗教</summary>
    public string? CurrentReligion { get; set; }

    /// <summary>信仰ポイント</summary>
    public int FaithPoints { get; set; }
}

/// <summary>
/// ステータスのセーブデータ
/// </summary>
public class StatsSaveData
{
    public int Strength { get; set; }
    public int Vitality { get; set; }
    public int Agility { get; set; }
    public int Dexterity { get; set; }
    public int Intelligence { get; set; }
    public int Mind { get; set; }
    public int Perception { get; set; }
    public int Charisma { get; set; }
    public int Luck { get; set; }

    public static StatsSaveData FromStats(Stats stats) => new()
    {
        Strength = stats.Strength,
        Vitality = stats.Vitality,
        Agility = stats.Agility,
        Dexterity = stats.Dexterity,
        Intelligence = stats.Intelligence,
        Mind = stats.Mind,
        Perception = stats.Perception,
        Charisma = stats.Charisma,
        Luck = stats.Luck
    };

    public Stats ToStats() => new(
        Strength, Vitality, Agility, Dexterity,
        Intelligence, Mind, Perception, Charisma, Luck
    );
}

/// <summary>
/// 位置のセーブデータ
/// </summary>
public class PositionSaveData
{
    public int X { get; set; }
    public int Y { get; set; }

    public static PositionSaveData FromPosition(Position pos) => new() { X = pos.X, Y = pos.Y };
    public Position ToPosition() => new(X, Y);
}

/// <summary>
/// アイテムのセーブデータ
/// </summary>
public class ItemSaveData
{
    /// <summary>アイテム定義ID</summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>強化値</summary>
    public int EnhancementLevel { get; set; }

    /// <summary>識別済みか</summary>
    public bool IsIdentified { get; set; } = true;

    /// <summary>呪われているか</summary>
    public bool IsCursed { get; set; }

    /// <summary>祝福されているか</summary>
    public bool IsBlessed { get; set; }

    /// <summary>耐久度</summary>
    public int Durability { get; set; } = -1;

    /// <summary>スタック数（スタック可能アイテムの場合）</summary>
    public int StackCount { get; set; } = 1;
}

/// <summary>
/// ゲーム内時間のセーブデータ
/// </summary>
public class GameTimeSaveData
{
    public int TotalTurns { get; set; }
}
