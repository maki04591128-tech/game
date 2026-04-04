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

    /// <summary>難易度</summary>
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Normal;

    /// <summary>現在のマップ名（種族・素性に応じた開始マップ名等）</summary>
    public string CurrentMapName { get; set; } = "capital_guild";

    /// <summary>素性クリア条件フラグ</summary>
    public Systems.ClearFlagSaveData? ClearFlags { get; set; }

    /// <summary>スキルクールダウン状態</summary>
    public Dictionary<string, int> SkillCooldowns { get; set; } = new();

    /// <summary>現在の領地</summary>
    public string CurrentTerritory { get; set; } = nameof(TerritoryId.Capital);

    /// <summary>訪問済み領地</summary>
    public List<string> VisitedTerritories { get; set; } = new() { nameof(TerritoryId.Capital) };

    /// <summary>地上にいるか</summary>
    public bool IsOnSurface { get; set; } = true;

    /// <summary>銀行残高</summary>
    public int BankBalance { get; set; }

    /// <summary>NPC好感度・状態</summary>
    public Dictionary<string, Systems.NpcStateSaveData> NpcStates { get; set; } = new();

    /// <summary>アクティブクエスト</summary>
    public List<Systems.QuestProgressSaveData> ActiveQuests { get; set; } = new();

    /// <summary>完了済みクエストID</summary>
    public List<string> CompletedQuests { get; set; } = new();

    /// <summary>ギルドランク</summary>
    public string GuildRank { get; set; } = nameof(RougelikeGame.Core.GuildRank.None);

    /// <summary>ギルドポイント</summary>
    public int GuildPoints { get; set; }

    /// <summary>会話フラグ</summary>
    public List<string> DialogueFlags { get; set; } = new();

    /// <summary>ペットデータ</summary>
    public PetSaveData? PetData { get; set; }

    /// <summary>コンパニオンID</summary>
    public List<string> CompanionIds { get; set; } = new();

    /// <summary>カルマ値</summary>
    public int KarmaValue { get; set; }

    /// <summary>カルマ履歴</summary>
    public List<string> KarmaHistory { get; set; } = new();

    /// <summary>習熟レベル</summary>
    public Dictionary<string, int> ProficiencyLevels { get; set; } = new();

    /// <summary>習熟経験値</summary>
    public Dictionary<string, int> ProficiencyExp { get; set; } = new();

    /// <summary>現在の病気</summary>
    public string? CurrentDisease { get; set; }

    /// <summary>病気残りターン</summary>
    public int DiseaseRemainingTurns { get; set; }

    /// <summary>アンロック済み実績</summary>
    public List<string> Achievements { get; set; } = new();

    /// <summary>現在の天候</summary>
    public string? WeatherState { get; set; }

    /// <summary>現在の季節</summary>
    public string? SeasonState { get; set; }

    /// <summary>CM-1: NG+ティア</summary>
    public int? NgPlusTier { get; set; }

    /// <summary>CM-2: クリア済みフラグ</summary>
    public bool HasCleared { get; set; }

    /// <summary>CM-2: クリアランク</summary>
    public string? ClearRank { get; set; }

    /// <summary>CM-3: 無限ダンジョンモード</summary>
    public bool InfiniteDungeonMode { get; set; }

    /// <summary>CM-3: 無限ダンジョン撃破数</summary>
    public int InfiniteDungeonKills { get; set; }

    /// <summary>累計死亡回数</summary>
    public int TotalDeaths { get; set; }

    /// <summary>AS-2: 地面のアイテム</summary>
    public List<GroundItemSaveData> GroundItems { get; set; } = new();

    /// <summary>戦闘スタンス</summary>
    public string? CombatStance { get; set; }

    /// <summary>コンパニオンデータ</summary>
    public List<CompanionSaveData> Companions { get; set; } = new();

    /// <summary>BQ-7: スキルツリー習得済みスキルID</summary>
    public List<string> SkillTreeLearnedSkills { get; set; } = new();

    /// <summary>BQ-8: 建設済み施設カテゴリ</summary>
    public List<string> BuiltFacilities { get; set; } = new();

    /// <summary>BQ-2: 領地別評判値</summary>
    public Dictionary<string, int> ReputationValues { get; set; } = new();

    /// <summary>BQ-24/BU-12: チュートリアル完了済みステップ</summary>
    public List<string> CompletedTutorialSteps { get; set; } = new();

    /// <summary>BU-11: 解除済み実績ID</summary>
    public List<string> UnlockedAchievements { get; set; } = new();

    /// <summary>BR-5: 現在のダンジョン特性</summary>
    public string? CurrentDungeonFeature { get; set; }
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
    /// <summary>渇き</summary>
    public int Thirst { get; set; } = 100;
    /// <summary>疲労</summary>
    public int Fatigue { get; set; } = 100;  // デフォルト値100（疲労なし）
    /// <summary>衛生</summary>
    public int Hygiene { get; set; } = 100;

    /// <summary>アクティブ状態異常</summary>
    public List<StatusEffectSaveData> StatusEffects { get; set; } = new();

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

    /// <summary>前の宗教</summary>
    public string? PreviousReligion { get; set; }

    /// <summary>過去に信仰した宗教</summary>
    public List<string> PreviousReligions { get; set; } = new();

    /// <summary>背教者の呪いフラグ</summary>
    public bool HasApostasyCurse { get; set; }

    /// <summary>背教者の呪い残日数</summary>
    public int ApostasyCurseRemainingDays { get; set; }

    /// <summary>最終祈祷からの経過日数</summary>
    public int DaysSinceLastPrayer { get; set; }

    /// <summary>AB-7/M-3: 今日祈ったかフラグ</summary>
    public bool HasPrayedToday { get; set; }

    /// <summary>信仰度上限</summary>
    public int FaithCap { get; set; } = 100;

    /// <summary>所持金</summary>
    public int Gold { get; set; }

    /// <summary>種族</summary>
    public Race Race { get; set; } = Race.Human;

    /// <summary>職業</summary>
    public CharacterClass CharacterClass { get; set; } = CharacterClass.Fighter;

    /// <summary>素性</summary>
    public Background Background { get; set; } = Background.Adventurer;

    /// <summary>累計死亡回数</summary>
    public int TotalDeaths { get; set; }

    /// <summary>引き継ぎデータ</summary>
    public TransferDataSaveData? TransferData { get; set; }
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

    /// <summary>AS-1: アイテム品質</summary>
    public string Grade { get; set; } = nameof(ItemGrade.Standard);

    /// <summary>AN-3: 適用済みエンチャントIDリスト</summary>
    public List<string> AppliedEnchantments { get; set; } = new();
}

/// <summary>
/// ゲーム内時間のセーブデータ
/// </summary>
public class GameTimeSaveData
{
    public int TotalTurns { get; set; }
}

/// <summary>
/// 引き継ぎデータのセーブ形式
/// </summary>
public class TransferDataSaveData
{
    public Dictionary<string, int> LearnedWords { get; set; } = new();
    public List<string> LearnedSkills { get; set; } = new();
    public string? Religion { get; set; }
    public int FaithPoints { get; set; }
    public string? PreviousReligion { get; set; }
    public List<string> PreviousReligions { get; set; } = new();
    public int TotalDeaths { get; set; }
    public int RescueCountRemaining { get; set; } = GameConstants.MaxRescueCount;
    public int Sanity { get; set; } = GameConstants.InitialSanity;
    /// <summary>BW-5: 引き継ぎレベル</summary>
    public int Level { get; set; }
    /// <summary>BW-6: 引き継ぎゴールド</summary>
    public int Gold { get; set; }
}

/// <summary>
/// 状態異常のセーブデータ
/// </summary>
public class StatusEffectSaveData
{
    /// <summary>状態異常タイプ</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>残りターン数</summary>
    public int RemainingTurns { get; set; }

    /// <summary>効力</summary>
    public int Potency { get; set; }
}

/// <summary>
/// ペットのセーブデータ
/// </summary>
public class PetSaveData
{
    /// <summary>ペットID</summary>
    public string PetId { get; set; } = string.Empty;

    /// <summary>名前</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>ペット種別</summary>
    public string PetType { get; set; } = string.Empty;

    /// <summary>レベル</summary>
    public int Level { get; set; }

    /// <summary>経験値</summary>
    public int Experience { get; set; }

    /// <summary>空腹度</summary>
    public int Hunger { get; set; } = 100;

    /// <summary>忠誠度</summary>
    public int Loyalty { get; set; } = 50;

    /// <summary>現在HP</summary>
    public int CurrentHp { get; set; }

    /// <summary>騎乗中か</summary>
    public bool IsRiding { get; set; }
}

/// <summary>
/// AS-2: 地面アイテムのセーブデータ
/// </summary>
public class GroundItemSaveData
{
    public ItemSaveData Item { get; set; } = new();
    public int X { get; set; }
    public int Y { get; set; }
}

/// <summary>
/// コンパニオンのセーブデータ
/// </summary>
public class CompanionSaveData
{
    public string Name { get; set; } = string.Empty;
    public string? CompanionType { get; set; }
    public int Level { get; set; }
    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public bool IsAlive { get; set; }
}
