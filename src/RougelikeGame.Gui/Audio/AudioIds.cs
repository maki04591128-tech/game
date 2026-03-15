namespace RougelikeGame.Gui.Audio;

/// <summary>
/// BGMのシーンID定数
/// </summary>
public static class BgmIds
{
    public const string Title = "BGM_001";
    public const string DungeonNormal = "BGM_002";
    public const string BattleNormal = "BGM_003";
    public const string BattleBoss = "BGM_004";
    public const string DeathReturn = "BGM_005";
    public const string GameOver = "BGM_006";
    public const string DungeonDeep = "BGM_007";
    public const string Town = "BGM_008";
    public const string Shop = "BGM_009";
    public const string Event = "BGM_010";
}

/// <summary>
/// SEのID定数
/// </summary>
public static class SeIds
{
    // 戦闘系
    public const string AttackHit = "SE_001";
    public const string AttackMiss = "SE_002";
    public const string CriticalHit = "SE_003";
    public const string MagicCast = "SE_004";
    public const string TakeDamage = "SE_005";
    public const string EnemyDefeat = "SE_006";
    public const string PlayerDeath = "SE_007";

    // 探索系
    public const string Footstep = "SE_010";
    public const string DoorOpen = "SE_011";
    public const string StairsMove = "SE_012";
    public const string TrapTrigger = "SE_013";
    public const string SecretFound = "SE_014";

    // アイテム系
    public const string ItemPickup = "SE_020";
    public const string PotionUse = "SE_021";
    public const string ScrollUse = "SE_022";
    public const string EquipChange = "SE_023";
    public const string Eat = "SE_024";

    // UI系
    public const string MenuOpen = "SE_030";
    public const string LevelUp = "SE_031";
    public const string SanityChange = "SE_032";
    public const string HungerChange = "SE_033";
}
