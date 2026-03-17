namespace RougelikeGame.Core.Systems;

/// <summary>
/// 環境音システム基盤 - 環境音切替のフックポイント（実音源はVer.β）
/// </summary>
public static class AmbientSoundSystem
{
    /// <summary>環境音イベント定義</summary>
    public record AmbientSoundEvent(
        AmbientSoundType Type,
        float Volume,
        bool ShouldLoop
    );

    /// <summary>領地に応じた環境音を取得</summary>
    public static AmbientSoundType GetAmbientForTerritory(TerritoryId territory) => territory switch
    {
        TerritoryId.Capital => AmbientSoundType.Town,
        TerritoryId.Forest => AmbientSoundType.Forest,
        TerritoryId.Mountain => AmbientSoundType.Mountain,
        TerritoryId.Coast => AmbientSoundType.Coast,
        TerritoryId.Southern => AmbientSoundType.Desert,
        TerritoryId.Frontier => AmbientSoundType.Frontier,
        _ => AmbientSoundType.Silence
    };

    /// <summary>ダンジョン階層に応じた環境音を取得</summary>
    public static AmbientSoundType GetAmbientForDungeon(int floor, bool isBossFloor) =>
        isBossFloor ? AmbientSoundType.BossBattle : AmbientSoundType.Dungeon;

    /// <summary>環境音のデフォルトボリュームを取得</summary>
    public static float GetDefaultVolume(AmbientSoundType type) => type switch
    {
        AmbientSoundType.Dungeon => 0.3f,
        AmbientSoundType.Forest => 0.5f,
        AmbientSoundType.Mountain => 0.4f,
        AmbientSoundType.Coast => 0.5f,
        AmbientSoundType.Desert => 0.3f,
        AmbientSoundType.Frontier => 0.2f,
        AmbientSoundType.Town => 0.6f,
        AmbientSoundType.BossBattle => 0.7f,
        AmbientSoundType.Silence => 0.0f,
        _ => 0.3f
    };

    /// <summary>環境音名を取得</summary>
    public static string GetSoundName(AmbientSoundType type) => type switch
    {
        AmbientSoundType.Dungeon => "ダンジョン（洞窟の水滴音）",
        AmbientSoundType.Forest => "森（鳥の声と風音）",
        AmbientSoundType.Mountain => "山岳（強風）",
        AmbientSoundType.Coast => "沿岸（波音）",
        AmbientSoundType.Desert => "砂漠（乾いた風）",
        AmbientSoundType.Frontier => "辺境（不気味な風）",
        AmbientSoundType.Town => "街（人々の声）",
        AmbientSoundType.BossBattle => "ボス戦（緊迫）",
        AmbientSoundType.Silence => "静寂",
        _ => "不明"
    };

    /// <summary>環境音イベントを生成（実音源はVer.βで実装）</summary>
    public static AmbientSoundEvent CreateEvent(AmbientSoundType type)
    {
        return new AmbientSoundEvent(type, GetDefaultVolume(type), type != AmbientSoundType.Silence);
    }
}
