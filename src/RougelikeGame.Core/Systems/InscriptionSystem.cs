namespace RougelikeGame.Core.Systems;

/// <summary>
/// 碑文・壁画解読システム - 魔法言語スキルを活用した碑文解読メカニクス
/// 参考: La-Mulana、Return of the Obra Dinn
/// </summary>
public class InscriptionSystem
{
    /// <summary>碑文定義</summary>
    public record Inscription(
        string InscriptionId,
        InscriptionType Type,
        string RawText,
        string DecodedText,
        int RequiredLevel,
        string? RewardInfo,
        bool IsDecoded
    );

    /// <summary>解読結果</summary>
    public record DecodeResult(
        bool Success,
        string InscriptionId,
        string Message,
        string? RewardInfo,
        int PartialProgress
    );

    private readonly Dictionary<string, Inscription> _inscriptions = new();

    /// <summary>全碑文</summary>
    public IReadOnlyDictionary<string, Inscription> Inscriptions => _inscriptions;

    /// <summary>解読済み碑文数</summary>
    public int DecodedCount => _inscriptions.Values.Count(i => i.IsDecoded);

    /// <summary>碑文を登録</summary>
    public void Register(string id, InscriptionType type, string rawText, string decodedText,
        int requiredLevel, string? rewardInfo = null)
    {
        _inscriptions[id] = new Inscription(id, type, rawText, decodedText, requiredLevel, rewardInfo, false);
    }

    /// <summary>碑文の解読を試みる</summary>
    public DecodeResult TryDecode(string inscriptionId, int playerMagicLevel)
    {
        if (!_inscriptions.TryGetValue(inscriptionId, out var inscription))
            return new DecodeResult(false, inscriptionId, "碑文が見つかりません", null, 0);

        if (inscription.IsDecoded)
            return new DecodeResult(true, inscriptionId, inscription.DecodedText, inscription.RewardInfo, 100);

        if (playerMagicLevel < inscription.RequiredLevel)
        {
            int progress = Math.Min(99, playerMagicLevel * 100 / inscription.RequiredLevel);
            string partialText = GetPartialText(inscription.DecodedText, progress);
            return new DecodeResult(false, inscriptionId,
                $"解読中... ({progress}%) - {partialText}", null, progress);
        }

        _inscriptions[inscriptionId] = inscription with { IsDecoded = true };
        return new DecodeResult(true, inscriptionId,
            $"解読成功: {inscription.DecodedText}", inscription.RewardInfo, 100);
    }

    /// <summary>碑文の種別名を取得</summary>
    public static string GetTypeName(InscriptionType type) => type switch
    {
        InscriptionType.Lore => "伝承の碑文",
        InscriptionType.Warning => "警告の碑文",
        InscriptionType.Hint => "手がかりの碑文",
        InscriptionType.Recipe => "秘伝の碑文",
        InscriptionType.Spell => "呪文の碑文",
        InscriptionType.Map => "地図の壁画",
        _ => "不明な碑文"
    };

    /// <summary>碑文の解読報酬種別を取得</summary>
    public static string GetRewardDescription(InscriptionType type) => type switch
    {
        InscriptionType.Lore => "世界観の知識を得た",
        InscriptionType.Warning => "危険な情報を得た",
        InscriptionType.Hint => "攻略のヒントを得た",
        InscriptionType.Recipe => "新しいレシピを習得した",
        InscriptionType.Spell => "新しい呪文を習得した",
        InscriptionType.Map => "隠された部屋の位置を把握した",
        _ => "何かを感じた"
    };

    /// <summary>部分的な解読テキストを生成</summary>
    private static string GetPartialText(string fullText, int progressPercent)
    {
        if (progressPercent <= 0) return "???";
        int revealChars = Math.Max(1, fullText.Length * progressPercent / 100);
        return fullText[..revealChars] + "...";
    }

    /// <summary>
    /// 全碑文の解読状態をリセットする（死に戻り時に呼び出し）。
    /// 死に戻りは時間巻き戻しであるため、解読した碑文は未解読に戻る。
    /// 碑文の登録（マスターデータ）は保持する。
    /// </summary>
    public void Reset()
    {
        var keys = _inscriptions.Keys.ToList();
        foreach (var key in keys)
        {
            var inscription = _inscriptions[key];
            if (inscription.IsDecoded)
            {
                _inscriptions[key] = inscription with { IsDecoded = false };
            }
        }
    }

    /// <summary>種別ごとの碑文一覧を取得</summary>
    public IReadOnlyList<Inscription> GetByType(InscriptionType type)
    {
        return _inscriptions.Values.Where(i => i.Type == type).ToList();
    }
}
