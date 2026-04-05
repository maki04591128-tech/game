namespace RougelikeGame.Core.Systems;

/// <summary>
/// 関係値システム - 種族間/領地間/宗教間/個人の多層関係値管理
/// </summary>
public class RelationshipSystem
{
    /// <summary>関係値エントリ</summary>
    public record RelationEntry(
        RelationshipType Type,
        string EntityA,
        string EntityB,
        int Value
    );

    private readonly Dictionary<(RelationshipType, string, string), int> _relations = new();

    /// <summary>関係値を設定</summary>
    public void SetRelation(RelationshipType type, string entityA, string entityB, int value)
    {
        var key = NormalizeKey(type, entityA, entityB);
        _relations[key] = Math.Clamp(value, -100, 100);
    }

    /// <summary>関係値を取得</summary>
    public int GetRelation(RelationshipType type, string entityA, string entityB)
    {
        var key = NormalizeKey(type, entityA, entityB);
        return _relations.TryGetValue(key, out var v) ? v : 0;
    }

    /// <summary>関係値を変動</summary>
    public void ModifyRelation(RelationshipType type, string entityA, string entityB, int delta)
    {
        var current = GetRelation(type, entityA, entityB);
        SetRelation(type, entityA, entityB, current + delta);
    }

    /// <summary>関係の状態名を取得</summary>
    public static string GetRelationName(int value) => value switch
    {
        >= 80 => "盟友",
        >= 50 => "友好",
        >= 20 => "好意的",
        >= -19 => "中立",
        >= -49 => "警戒",
        >= -79 => "敵対",
        _ => "宿敵"
    };

    /// <summary>ショップ割引率を取得</summary>
    public static float GetShopDiscount(int relationValue)
    {
        if (relationValue >= 80) return 0.2f;
        if (relationValue >= 50) return 0.1f;
        if (relationValue >= 20) return 0.05f;
        if (relationValue <= -50) return -0.1f;
        return 0f;
    }

    /// <summary>
    /// 全関係値をリセットする（死に戻り時に呼び出し）。
    /// 死に戻りは時間巻き戻しであるため、築いた関係は全て消失する。
    /// </summary>
    public void Reset()
    {
        _relations.Clear();
    }

    /// <summary>関係値総数</summary>
    public int TotalRelations => _relations.Count;

    private static (RelationshipType, string, string) NormalizeKey(RelationshipType type, string a, string b)
    {
        return string.Compare(a, b, StringComparison.Ordinal) <= 0 ? (type, a, b) : (type, b, a);
    }

    /// <summary>時間経過による関係値の自然減衰（中立方向に戻る）</summary>
    public void DecayRelationships(int turnsElapsed)
    {
        int decayAmount = Math.Max(1, turnsElapsed / 100);
        var keys = _relations.Keys.ToList();
        foreach (var key in keys)
        {
            int current = _relations[key];
            if (Math.Abs(current) <= decayAmount)
            {
                _relations[key] = 0;
            }
            else if (current > 0)
            {
                _relations[key] = current - decayAmount;
            }
            else
            {
                _relations[key] = current + decayAmount;
            }
        }
    }


    /// <summary>全関係値をRelationEntry形式で取得</summary>
    public IReadOnlyList<RelationEntry> GetAllRelationEntries()
    {
        return _relations.Select(kvp => new RelationEntry(kvp.Key.Item1, kvp.Key.Item2, kvp.Key.Item3, kvp.Value)).ToList();
    }

    /// <summary>BQ-12: セーブデータから関係値を復元</summary>
    public void RestoreRelations(Dictionary<string, int> savedRelations)
    {
        foreach (var (key, value) in savedRelations)
        {
            // key format: "type:entityA:entityB"
            var parts = key.Split(':');
            if (parts.Length == 3 && Enum.TryParse<RelationshipType>(parts[0], out var relType))
            {
                _relations[(relType, parts[1], parts[2])] = value;
            }
        }
    }
}