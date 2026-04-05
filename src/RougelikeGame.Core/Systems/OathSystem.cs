namespace RougelikeGame.Core.Systems;

/// <summary>
/// 誓約・縛りプレイシステム - 自発的デメリットで報酬UP
/// </summary>
public class OathSystem
{
    /// <summary>誓約定義</summary>
    public record OathDefinition(
        OathType Type,
        string Name,
        string Description,
        float ExpBonus,
        float DropBonus,
        string Restriction
    );

    private static readonly Dictionary<OathType, OathDefinition> Oaths = new()
    {
        [OathType.Temperance] = new(OathType.Temperance, "禁酒の誓約", "酒類の使用を禁じる", 0.1f, 0.05f, "酒類使用不可"),
        [OathType.Pacifism] = new(OathType.Pacifism, "不殺の誓約", "敵を倒さずに進む", 0.5f, 0.3f, "敵の直接攻撃不可"),
        [OathType.Solitude] = new(OathType.Solitude, "孤高の誓約", "仲間を連れない", 0.2f, 0.15f, "仲間システム使用不可"),
        [OathType.Austerity] = new(OathType.Austerity, "粗食の誓約", "上質な食料を食べない", 0.15f, 0.1f, "レア食料使用不可"),
        [OathType.Darkness] = new(OathType.Darkness, "暗闇の誓約", "松明・光源を使わない", 0.25f, 0.2f, "光源アイテム使用不可"),
    };

    private readonly HashSet<OathType> _activeOaths = new();

    /// <summary>有効な誓約</summary>
    public IReadOnlyCollection<OathType> ActiveOaths => _activeOaths;

    /// <summary>誓約定義を取得</summary>
    public static OathDefinition? GetDefinition(OathType type)
    {
        return Oaths.TryGetValue(type, out var d) ? d : null;
    }

    /// <summary>誓約を受ける</summary>
    public bool TakeOath(OathType type)
    {
        return _activeOaths.Add(type);
    }

    /// <summary>誓約を解除（ペナルティあり）</summary>
    public bool BreakOath(OathType type)
    {
        return _activeOaths.Remove(type);
    }

    /// <summary>CS-2: 誓約を達成して報酬を獲得</summary>
    public (bool Success, float ExpBonus, float DropBonus) CompleteOath(OathType type)
    {
        if (!_activeOaths.Contains(type)) return (false, 0, 0);
        var def = GetDefinition(type);
        if (def == null) return (false, 0, 0);
        _activeOaths.Remove(type);
        // 達成報酬は誓約ボーナスの2倍
        return (true, def.ExpBonus * 2, def.DropBonus * 2);
    }

    /// <summary>累計経験値ボーナスを計算</summary>
    public float GetTotalExpBonus()
    {
        return _activeOaths.Sum(o => Oaths.TryGetValue(o, out var d) ? d.ExpBonus : 0f);
    }

    /// <summary>累計ドロップボーナスを計算</summary>
    public float GetTotalDropBonus()
    {
        return _activeOaths.Sum(o => Oaths.TryGetValue(o, out var d) ? d.DropBonus : 0f);
    }

    /// <summary>誓約違反かチェック</summary>
    public bool IsViolation(OathType type, string action) => (type, action) switch
    {
        (OathType.Temperance, "use_alcohol") => true,
        (OathType.Pacifism, "attack_enemy") => true,
        (OathType.Solitude, "recruit_companion") => true,
        (OathType.Austerity, "use_rare_food") => true,
        (OathType.Darkness, "use_torch") => true,
        _ => false
    };

    /// <summary>
    /// 全誓約を解除する（死に戻り時に呼び出し）。
    /// 死に戻りは時間巻き戻しであるため、誓約は全て消失する。
    /// </summary>
    public void Reset()
    {
        _activeOaths.Clear();
    }

    /// <summary>BQ-6: セーブデータから誓約を復元</summary>
    public void RestoreOaths(IEnumerable<OathType> oaths)
    {
        foreach (var oath in oaths)
            _activeOaths.Add(oath);
    }
}