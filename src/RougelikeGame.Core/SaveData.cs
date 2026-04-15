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

    /// <summary>コンパニオンID（後方互換性のため維持。実際のデータはCompanionsリストを使用）</summary>
    [Obsolete("Use Companions list property instead. Kept for backward compatibility with old save files.")]
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

    /// <summary>アンロック済み実績（後方互換性のため維持。実際のデータはUnlockedAchievementsを使用）</summary>
    [Obsolete("Use UnlockedAchievements instead. Kept for backward compatibility with old save files.")]
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

    /// <summary>U.3: 撃破した敵の総数</summary>
    public int TotalEnemiesDefeated { get; set; }

    /// <summary>U.3: 到達した最深階層</summary>
    public int DeepestFloorReached { get; set; }

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

    /// <summary>BZ-5: 商人ギルドデータ</summary>
    public MerchantGuildSaveData? MerchantGuild { get; set; }

    /// <summary>BZ-6: 派閥戦争データ</summary>
    public FactionWarSaveData? FactionWar { get; set; }

    // ===== BQ系: サブシステム永続性 =====

    /// <summary>BQ-4: 図鑑エントリ発見レベル・討伐数</summary>
    public Dictionary<string, EncyclopediaSaveEntry> EncyclopediaEntries { get; set; } = new();

    /// <summary>BQ-6: アクティブ誓約</summary>
    public List<string> ActiveOaths { get; set; } = new();

    /// <summary>BQ-9: 投資記録</summary>
    public List<InvestmentSaveData> Investments { get; set; } = new();

    /// <summary>BQ-10: グリッドインベントリ配置</summary>
    public List<GridItemSaveData> GridItems { get; set; } = new();

    /// <summary>BQ-12: NPC関係値</summary>
    public Dictionary<string, int> NpcRelations { get; set; } = new();

    /// <summary>BQ-13: 識別済みアイテムID</summary>
    public List<string> IdentifiedItemIds { get; set; } = new();

    /// <summary>BQ-17: 解読済み碑文ID</summary>
    public List<string> DecodedInscriptionIds { get; set; } = new();

    /// <summary>BQ-19: 領地勢力影響</summary>
    public Dictionary<string, Dictionary<string, float>> TerritoryInfluences { get; set; } = new();

    /// <summary>BQ-21: 訪問済みフロア・解放済みショートカット</summary>
    public List<string> VisitedDungeonFloors { get; set; } = new();
    public List<string> UnlockedShortcuts { get; set; } = new();

    /// <summary>CA-8: プレイヤー向き</summary>
    public string? PlayerFacingDirection { get; set; }

    /// <summary>BQ-5: 死亡ログ</summary>
    public List<DeathLogSaveData> DeathLogs { get; set; } = new();

    /// <summary>BQ-11: NPC記憶</summary>
    public List<NpcMemorySaveData> NpcMemories { get; set; } = new();

    /// <summary>BQ-14: ダンジョン生態系イベント</summary>
    public List<EcosystemEventSaveData> EcosystemEvents { get; set; } = new();

    /// <summary>BU-4: ゲーム時間開始値</summary>
    public int GameTimeStartYear { get; set; } = 1024;
    public int GameTimeStartMonth { get; set; } = 6;
    public int GameTimeStartDay { get; set; } = 15;
    public int GameTimeStartHour { get; set; } = 8;
    public int GameTimeStartMinute { get; set; }

    /// <summary>AS-3/CE-12: マップ探索状態（IsExplored=trueのタイル座標）</summary>
    public List<string> ExploredTiles { get; set; } = new();

    /// <summary>マップタイルデータ（セーブ/ロードでマップ構造を保持するため）</summary>
    public MapSaveData? MapData { get; set; }

    /// <summary>T.2: セーブデータの値の範囲を検証・修正</summary>
    public void Validate()
    {
        CurrentFloor = Math.Max(0, CurrentFloor);
        TurnCount = Math.Max(0, TurnCount);
        TotalDeaths = Math.Max(0, TotalDeaths);
        TotalEnemiesDefeated = Math.Max(0, TotalEnemiesDefeated);
        DeepestFloorReached = Math.Max(0, DeepestFloorReached);
        BankBalance = Math.Max(0, BankBalance);
        GuildPoints = Math.Max(0, GuildPoints);
        InfiniteDungeonKills = Math.Max(0, InfiniteDungeonKills);

        Player ??= new PlayerSaveData();
        Player.Level = Math.Clamp(Player.Level, 1, 99);
        Player.CurrentHp = Math.Max(0, Player.CurrentHp);
        Player.CurrentMp = Math.Max(0, Player.CurrentMp);
        Player.CurrentSp = Math.Max(0, Player.CurrentSp);
        Player.Gold = Math.Max(0, Player.Gold);
        Player.Sanity = Math.Clamp(Player.Sanity, 0, 100);
        Player.Hunger = Math.Clamp(Player.Hunger, 0, 100);
        Player.Thirst = Math.Clamp(Player.Thirst, 0, 100);

        MessageHistory ??= new List<string>();
        ActiveQuests ??= new List<Systems.QuestProgressSaveData>();
        CompletedQuests ??= new List<string>();
        SkillCooldowns ??= new Dictionary<string, int>();
        VisitedTerritories ??= new List<string>();
        NpcStates ??= new Dictionary<string, Systems.NpcStateSaveData>();
        KarmaHistory ??= new List<string>();
        ProficiencyLevels ??= new Dictionary<string, int>();
        ProficiencyExp ??= new Dictionary<string, int>();
        DialogueFlags ??= new List<string>();
    }
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
    /// <summary>疲労（double型: 0.0=快調、100.0=疲労困憊）</summary>
    public double Fatigue { get; set; } = 0.0;  // デフォルト値0.0（快調状態）
    /// <summary>気付け薬による疲労行動制限解除中かどうか</summary>
    public bool HasFatigueRestrictionRelief { get; set; }
    /// <summary>気付け薬の効果残りターン数</summary>
    public int FatigueRestrictionReliefRemainingTurns { get; set; }
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

    /// <summary>ボーナス最大HP（種族・職業ボーナス等）</summary>
    public int BonusMaxHp { get; set; }

    /// <summary>ボーナス最大MP（種族・職業ボーナス等）</summary>
    public int BonusMaxMp { get; set; }

    /// <summary>ボーナスクリティカル率</summary>
    public double BonusCriticalRate { get; set; }

    /// <summary>習得済みルーン</summary>
    public List<string> KnownRunes { get; set; } = new();
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

    /// <summary>表示名</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>スタック数</summary>
    public int StackCount { get; set; } = 1;

    /// <summary>ダメージ属性</summary>
    public string DamageElement { get; set; } = string.Empty;

    /// <summary>最大スタック数</summary>
    public int MaxStack { get; set; } = 1;
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

    /// <summary>最大HP（レベルアップ等で増加した値）</summary>
    public int MaxHp { get; set; }

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
    /// <summary>忠誠度（旧セーブデータ互換: デフォルト50）</summary>
    public int Loyalty { get; set; } = 50;
    /// <summary>雇用コスト（旧セーブデータ互換: デフォルト0）</summary>
    public int HireCost { get; set; }
    /// <summary>AIモード（旧セーブデータ互換: デフォルトDefensive）</summary>
    public string? AIMode { get; set; }
}

/// <summary>BZ-5: 商人ギルドのセーブデータ</summary>
public class MerchantGuildSaveData
{
    public bool IsMember { get; set; }
    public string Rank { get; set; } = nameof(GuildRank.None);
    public int GuildPoints { get; set; }
    public int TradeCount { get; set; }
    public int TotalProfit { get; set; }
    public List<TradeRouteSaveData> Routes { get; set; } = new();
}

/// <summary>BZ-5: 交易路セーブデータ</summary>
public class TradeRouteSaveData
{
    public string RouteId { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ProfitMultiplier { get; set; }
    public int EstablishmentCost { get; set; }
}

/// <summary>BZ-6: 派閥戦争のセーブデータ</summary>
public class FactionWarSaveData
{
    public List<WarEventSaveData> ActiveWars { get; set; } = new();
    public List<WarOutcomeSaveData> WarHistory { get; set; } = new();
}

/// <summary>BZ-6: 戦争イベントセーブデータ</summary>
public class WarEventSaveData
{
    public string WarId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Attacker { get; set; } = string.Empty;
    public string Defender { get; set; } = string.Empty;
    public string Phase { get; set; } = string.Empty;
    public int TurnStarted { get; set; }
    public int Duration { get; set; }
    public string PlayerAlignment { get; set; } = string.Empty;
}

/// <summary>BZ-6: 戦争結果セーブデータ</summary>
public class WarOutcomeSaveData
{
    public string WarId { get; set; } = string.Empty;
    public string Winner { get; set; } = string.Empty;
    public string Loser { get; set; } = string.Empty;
    public int TerritoryInfluenceChange { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>BQ-4: 図鑑エントリのセーブデータ</summary>
public class EncyclopediaSaveEntry
{
    public int DiscoveryLevel { get; set; }
    public int KillCount { get; set; }
}

/// <summary>BQ-9: 投資記録のセーブデータ</summary>
public class InvestmentSaveData
{
    public string Type { get; set; } = string.Empty;
    public string TargetName { get; set; } = string.Empty;
    public int Amount { get; set; }
    public int ExpectedReturn { get; set; }
    public int InvestedTurn { get; set; }
    public bool IsCompleted { get; set; }
}

/// <summary>BQ-10: グリッドアイテムのセーブデータ</summary>
public class GridItemSaveData
{
    public string ItemId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public int GridX { get; set; }
    public int GridY { get; set; }
    public bool IsRotated { get; set; }
}

/// <summary>BQ-5: 死亡ログのセーブデータ</summary>
public class DeathLogSaveData
{
    public int RunNumber { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public string Race { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Cause { get; set; } = string.Empty;
    public string CauseDetail { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Floor { get; set; }
    public int TotalTurns { get; set; }
    public string Timestamp { get; set; } = string.Empty;
}

/// <summary>BQ-11: NPC記憶のセーブデータ</summary>
public class NpcMemorySaveData
{
    public string NpcId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public int Impact { get; set; }
    public int TurnRecorded { get; set; }
}

/// <summary>BQ-14: 生態系イベントのセーブデータ</summary>
public class EcosystemEventSaveData
{
    public string Type { get; set; } = string.Empty;
    public string PredatorId { get; set; } = string.Empty;
    public string PreyId { get; set; } = string.Empty;
    public int Floor { get; set; }
    public int Turn { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// マップのセーブデータ（タイル構造・部屋情報・地面アイテムを保持）
/// </summary>
public class MapSaveData
{
    /// <summary>マップ幅</summary>
    public int Width { get; set; }

    /// <summary>マップ高さ</summary>
    public int Height { get; set; }

    /// <summary>フロアキャッシュ作成時のターン数（24時間再生成判定に使用）</summary>
    public int CreatedAtTurn { get; set; }

    /// <summary>タイルタイプの一次元配列（行優先順序: index = y * Width + x）</summary>
    public List<int> TileTypes { get; set; } = new();

    /// <summary>デフォルトと異なる状態を持つタイルの詳細データ</summary>
    public List<TileStateSaveData> TileStates { get; set; } = new();

    /// <summary>部屋情報</summary>
    public List<RoomSaveData> Rooms { get; set; } = new();

    /// <summary>地面のアイテム</summary>
    public List<GroundItemSaveData> GroundItems { get; set; } = new();
}

/// <summary>
/// デフォルトと異なる状態を持つタイルのセーブデータ
/// </summary>
public class TileStateSaveData
{
    public int X { get; set; }
    public int Y { get; set; }
    public int RoomId { get; set; } = -1;
    public bool IsLocked { get; set; }
    public int LockDifficulty { get; set; }
    public string? TrapId { get; set; }
    public string? ItemId { get; set; }
    public string? BuildingId { get; set; }
    public bool ChestOpened { get; set; }
    public int ChestLockDifficulty { get; set; }
    public List<string>? ChestItems { get; set; }
    public string? InscriptionWordId { get; set; }
    public bool InscriptionRead { get; set; }
    public int? GatheringNodeType { get; set; }
}

/// <summary>
/// 部屋のセーブデータ
/// </summary>
public class RoomSaveData
{
    public int Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Type { get; set; } = "Normal";
    public List<int> ConnectedRooms { get; set; } = new();
}

/// <summary>
/// 地面に落ちているアイテムのセーブデータ（マップ復元用）
/// ※ GroundItemSaveData は既存クラスを再利用
/// </summary>
