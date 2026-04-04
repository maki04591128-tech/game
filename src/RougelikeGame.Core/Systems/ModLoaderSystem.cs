namespace RougelikeGame.Core.Systems;

/// <summary>
/// MOD・カスタムコンテンツ対応基盤 - 外部JSON読み込み、データバリデーション
/// 参考: Caves of Qud（MOD対応）、RimWorld（Steam Workshop）
/// </summary>
public class ModLoaderSystem
{
    /// <summary>MODマニフェスト定義</summary>
    public record ModManifest(
        string ModId,
        string Name,
        string Author,
        string Version,
        string Description,
        IReadOnlyList<ModContentEntry> Contents,
        bool IsValid
    );

    /// <summary>MODコンテンツエントリ</summary>
    public record ModContentEntry(
        ModContentType Type,
        string FileName,
        string Description
    );

    /// <summary>バリデーション結果</summary>
    public record ValidationResult(
        bool IsValid,
        string ModId,
        IReadOnlyList<string> Errors,
        IReadOnlyList<string> Warnings
    );

    /// <summary>ロード結果</summary>
    public record LoadResult(
        bool Success,
        string ModId,
        int LoadedContentCount,
        string Message
    );

    private readonly Dictionary<string, ModManifest> _loadedMods = new();

    /// <summary>ロード済みMOD一覧</summary>
    public IReadOnlyDictionary<string, ModManifest> LoadedMods => _loadedMods;

    /// <summary>ロード済みMOD数</summary>
    public int LoadedCount => _loadedMods.Count;

    /// <summary>MODマニフェストをパースする（JSON文字列から）</summary>
    public ModManifest? ParseManifest(string modId, string name, string author, string version,
        string description, List<ModContentEntry>? contents = null)
    {
        // DI-3/DI-4: null/空文字バリデーション
        if (string.IsNullOrWhiteSpace(modId) || string.IsNullOrWhiteSpace(name))
            return null;
        contents ??= new List<ModContentEntry>();
        bool isValid = !string.IsNullOrWhiteSpace(author) && !string.IsNullOrWhiteSpace(version);
        var manifest = new ModManifest(modId, name, author ?? "", version ?? "0.0.0", description ?? "", contents, isValid);
        return manifest;
    }

    /// <summary>MODマニフェストをバリデーション</summary>
    public ValidationResult Validate(ModManifest manifest)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(manifest.ModId))
            errors.Add("MOD IDが未設定です");
        if (string.IsNullOrWhiteSpace(manifest.Name))
            errors.Add("MOD名が未設定です");
        if (string.IsNullOrWhiteSpace(manifest.Version))
            errors.Add("バージョンが未設定です");
        if (string.IsNullOrWhiteSpace(manifest.Author))
            warnings.Add("作者名が未設定です");
        if (manifest.Contents.Count == 0)
            warnings.Add("コンテンツが含まれていません");

        foreach (var content in manifest.Contents)
        {
            if (string.IsNullOrWhiteSpace(content.FileName))
                errors.Add($"コンテンツ '{content.Description}' のファイル名が未設定です");
        }

        if (_loadedMods.ContainsKey(manifest.ModId))
            errors.Add($"MOD '{manifest.ModId}' は既にロード済みです");

        return new ValidationResult(errors.Count == 0, manifest.ModId, errors, warnings);
    }

    /// <summary>MODをロードする</summary>
    public LoadResult LoadMod(ModManifest manifest)
    {
        var validation = Validate(manifest);
        if (!validation.IsValid)
            return new LoadResult(false, manifest.ModId, 0,
                $"バリデーションエラー: {string.Join(", ", validation.Errors)}");

        _loadedMods[manifest.ModId] = manifest;
        return new LoadResult(true, manifest.ModId, manifest.Contents.Count,
            $"MOD '{manifest.Name}' v{manifest.Version} をロードしました（{manifest.Contents.Count}件のコンテンツ）");
    }

    /// <summary>MODをアンロードする</summary>
    public bool UnloadMod(string modId)
    {
        return _loadedMods.Remove(modId);
    }

    /// <summary>指定コンテンツタイプのMODエントリを全取得</summary>
    public IReadOnlyList<ModContentEntry> GetContentsByType(ModContentType type)
    {
        return _loadedMods.Values
            .SelectMany(m => m.Contents)
            .Where(c => c.Type == type)
            .ToList();
    }

    /// <summary>コンテンツ種別名を取得</summary>
    public static string GetContentTypeName(ModContentType type) => type switch
    {
        ModContentType.Enemy => "敵データ",
        ModContentType.Item => "アイテムデータ",
        ModContentType.Map => "マップデータ",
        ModContentType.Spell => "呪文データ",
        ModContentType.Quest => "クエストデータ",
        ModContentType.System => "システム拡張",
        _ => "不明"
    };
}
