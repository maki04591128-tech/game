namespace RougelikeGame.Core.Systems;

/// <summary>
/// 環境利用戦闘システム - 地表面×属性魔法の相互作用
/// </summary>
public static class EnvironmentalCombatSystem
{
    /// <summary>地表面タイプ</summary>
    public enum SurfaceType
    {
        Normal,
        Water,
        Oil,
        Ice,
        Poison,
        Fire,
        Electrified
    }

    /// <summary>地表面と属性の相互作用結果</summary>
    public record SurfaceInteraction(SurfaceType ResultSurface, float DamageMultiplier, string Description);

    /// <summary>属性魔法を地表面に当てた際の相互作用を取得</summary>
    public static SurfaceInteraction? GetInteraction(SurfaceType surface, Element element) => (surface, element) switch
    {
        (SurfaceType.Water, Element.Lightning) => new(SurfaceType.Electrified, 1.5f, "水面が帯電！範囲にいる全員に雷ダメージ"),
        (SurfaceType.Water, Element.Ice) => new(SurfaceType.Ice, 1.0f, "水面が凍結！移動困難地形に変化"),
        (SurfaceType.Water, Element.Fire) => new(SurfaceType.Normal, 0.5f, "水が蒸発。火のダメージ半減"),
        (SurfaceType.Oil, Element.Fire) => new(SurfaceType.Fire, 2.0f, "油面が炎上！広範囲の火炎ダメージ"),
        (SurfaceType.Ice, Element.Fire) => new(SurfaceType.Water, 1.0f, "氷が溶けて水面に変化"),
        (SurfaceType.Poison, Element.Fire) => new(SurfaceType.Fire, 1.3f, "毒が燃え上がり有毒煙発生"),
        (SurfaceType.Fire, Element.Water) => new(SurfaceType.Normal, 0.0f, "火が消火された"),
        (SurfaceType.Fire, Element.Ice) => new(SurfaceType.Water, 0.5f, "火と氷がぶつかり水面に"),
        _ => null
    };

    /// <summary>地表面による移動コスト補正</summary>
    public static float GetMovementModifier(SurfaceType surface) => surface switch
    {
        SurfaceType.Ice => 1.5f,
        SurfaceType.Water => 1.2f,
        SurfaceType.Oil => 1.3f,
        SurfaceType.Fire => 1.0f,
        SurfaceType.Poison => 1.1f,
        _ => 1.0f
    };

    /// <summary>地表面による毎ターンダメージ</summary>
    public static int GetSurfaceDamage(SurfaceType surface) => surface switch
    {
        SurfaceType.Fire => 5,
        SurfaceType.Poison => 3,
        SurfaceType.Electrified => 4,
        _ => 0
    };

    /// <summary>地表面の持続ターン数</summary>
    public static int GetSurfaceDuration(SurfaceType surface) => surface switch
    {
        SurfaceType.Water => 999,  // 永続
        SurfaceType.Oil => 30,
        SurfaceType.Ice => 20,
        SurfaceType.Poison => 15,
        SurfaceType.Fire => 8,
        SurfaceType.Electrified => 5,
        _ => 0
    };
}
