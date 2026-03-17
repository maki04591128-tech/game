namespace RougelikeGame.Core.Systems;

/// <summary>
/// モジュラーHUDシステム - HUDカスタマイズ管理
/// </summary>
public class ModularHudSystem
{
    /// <summary>HUD要素設定</summary>
    public record HudElementConfig(
        HudElement Element,
        bool IsVisible,
        int PositionX,
        int PositionY,
        float Scale
    );

    private readonly Dictionary<HudElement, HudElementConfig> _config = new();

    /// <summary>プリセット数</summary>
    public const int MaxPresets = 3;

    public ModularHudSystem()
    {
        foreach (HudElement elem in Enum.GetValues<HudElement>())
        {
            _config[elem] = new HudElementConfig(elem, true, 0, 0, 1.0f);
        }
    }

    /// <summary>要素設定を取得</summary>
    public HudElementConfig? GetConfig(HudElement element)
    {
        return _config.TryGetValue(element, out var c) ? c : null;
    }

    /// <summary>要素の表示/非表示を切替</summary>
    public void SetVisibility(HudElement element, bool visible)
    {
        if (_config.TryGetValue(element, out var c))
            _config[element] = c with { IsVisible = visible };
    }

    /// <summary>要素の位置を設定</summary>
    public void SetPosition(HudElement element, int x, int y)
    {
        if (_config.TryGetValue(element, out var c))
            _config[element] = c with { PositionX = x, PositionY = y };
    }

    /// <summary>要素のスケールを設定</summary>
    public void SetScale(HudElement element, float scale)
    {
        if (_config.TryGetValue(element, out var c))
            _config[element] = c with { Scale = Math.Clamp(scale, 0.5f, 2.0f) };
    }

    /// <summary>表示中の要素数を取得</summary>
    public int GetVisibleCount() => _config.Values.Count(c => c.IsVisible);

    /// <summary>HUD要素名を取得</summary>
    public static string GetElementName(HudElement element) => element switch
    {
        HudElement.HpBar => "HPバー",
        HudElement.MpBar => "MPバー",
        HudElement.MiniMap => "ミニマップ",
        HudElement.MessageLog => "メッセージログ",
        HudElement.StatusInfo => "ステータス情報",
        _ => "不明"
    };
}
