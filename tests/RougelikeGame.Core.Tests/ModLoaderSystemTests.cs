using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// ModLoaderSystem（MOD・カスタムコンテンツ対応基盤）のテスト
/// </summary>
public class ModLoaderSystemTests
{
    // --- コンストラクタ ---

    [Fact]
    public void Constructor_EmptyState()
    {
        var system = new ModLoaderSystem();
        Assert.Empty(system.LoadedMods);
        Assert.Equal(0, system.LoadedCount);
    }

    // --- ParseManifest ---

    [Fact]
    public void ParseManifest_ValidInput_ReturnsManifest()
    {
        var system = new ModLoaderSystem();
        var manifest = system.ParseManifest("mod_1", "テストMOD", "作者", "1.0.0", "説明文");
        Assert.NotNull(manifest);
        Assert.Equal("mod_1", manifest.ModId);
        Assert.True(manifest.IsValid);
    }

    [Fact]
    public void ParseManifest_WithContents_IncludesContents()
    {
        var system = new ModLoaderSystem();
        var contents = new List<ModLoaderSystem.ModContentEntry>
        {
            new(ModContentType.Item, "items.json", "アイテムデータ"),
            new(ModContentType.Enemy, "enemies.json", "敵データ")
        };
        var manifest = system.ParseManifest("mod_1", "テストMOD", "作者", "1.0.0", "説明文", contents);
        Assert.NotNull(manifest);
        Assert.Equal(2, manifest.Contents.Count);
    }

    [Fact]
    public void ParseManifest_NullContents_DefaultsToEmpty()
    {
        var system = new ModLoaderSystem();
        var manifest = system.ParseManifest("mod_1", "テストMOD", "作者", "1.0.0", "説明文", null);
        Assert.NotNull(manifest);
        Assert.Empty(manifest.Contents);
    }

    // --- Validate ---

    [Fact]
    public void Validate_ValidManifest_ReturnsValid()
    {
        var system = new ModLoaderSystem();
        var contents = new List<ModLoaderSystem.ModContentEntry>
        {
            new(ModContentType.Item, "items.json", "アイテムデータ")
        };
        var manifest = system.ParseManifest("mod_1", "テストMOD", "作者", "1.0.0", "説明文", contents)!;
        var result = system.Validate(manifest);
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_EmptyModId_ReturnsError()
    {
        var system = new ModLoaderSystem();
        // DI-3/DI-4: ParseManifestが空modId/nameでnullを返す仕様に変更
        var manifest = system.ParseManifest("", "テストMOD", "作者", "1.0.0", "説明文");
        Assert.Null(manifest);
    }

    [Fact]
    public void Validate_EmptyName_ReturnsError()
    {
        var system = new ModLoaderSystem();
        // DI-3/DI-4: ParseManifestが空nameでnullを返す仕様に変更
        var manifest = system.ParseManifest("mod_1", "", "作者", "1.0.0", "説明文");
        Assert.Null(manifest);
    }

    [Fact]
    public void Validate_EmptyVersion_ReturnsError()
    {
        var system = new ModLoaderSystem();
        var manifest = system.ParseManifest("mod_1", "テストMOD", "作者", "", "説明文")!;
        var result = system.Validate(manifest);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("バージョン"));
    }

    [Fact]
    public void Validate_EmptyAuthor_AddsWarning()
    {
        var system = new ModLoaderSystem();
        var contents = new List<ModLoaderSystem.ModContentEntry>
        {
            new(ModContentType.Item, "items.json", "アイテム")
        };
        var manifest = system.ParseManifest("mod_1", "テストMOD", "", "1.0.0", "説明文", contents)!;
        var result = system.Validate(manifest);
        Assert.True(result.IsValid); // 作者名は警告のみ
        Assert.Contains(result.Warnings, w => w.Contains("作者名"));
    }

    [Fact]
    public void Validate_NoContents_AddsWarning()
    {
        var system = new ModLoaderSystem();
        var manifest = system.ParseManifest("mod_1", "テストMOD", "作者", "1.0.0", "説明文")!;
        var result = system.Validate(manifest);
        Assert.True(result.IsValid); // コンテンツなしは警告のみ
        Assert.Contains(result.Warnings, w => w.Contains("コンテンツ"));
    }

    [Fact]
    public void Validate_AlreadyLoaded_ReturnsError()
    {
        var system = new ModLoaderSystem();
        var contents = new List<ModLoaderSystem.ModContentEntry>
        {
            new(ModContentType.Item, "items.json", "アイテム")
        };
        var manifest = system.ParseManifest("mod_1", "テストMOD", "作者", "1.0.0", "説明文", contents)!;
        system.LoadMod(manifest);
        var result = system.Validate(manifest);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("既にロード済み"));
    }

    // --- LoadMod ---

    [Fact]
    public void LoadMod_ValidManifest_Succeeds()
    {
        var system = new ModLoaderSystem();
        var contents = new List<ModLoaderSystem.ModContentEntry>
        {
            new(ModContentType.Map, "map.json", "マップデータ")
        };
        var manifest = system.ParseManifest("mod_1", "テストMOD", "作者", "1.0.0", "説明文", contents)!;
        var result = system.LoadMod(manifest);
        Assert.True(result.Success);
        Assert.Equal(1, system.LoadedCount);
        Assert.Equal(1, result.LoadedContentCount);
    }

    [Fact]
    public void LoadMod_InvalidManifest_Fails()
    {
        var system = new ModLoaderSystem();
        // DI-3/DI-4: ParseManifestは空modIdでnullを返す
        var manifest = system.ParseManifest("", "テストMOD", "作者", "1.0.0", "説明文");
        Assert.Null(manifest);
    }

    // --- UnloadMod ---

    [Fact]
    public void UnloadMod_Loaded_ReturnsTrue()
    {
        var system = new ModLoaderSystem();
        var contents = new List<ModLoaderSystem.ModContentEntry>
        {
            new(ModContentType.Item, "items.json", "アイテム")
        };
        var manifest = system.ParseManifest("mod_1", "テストMOD", "作者", "1.0.0", "説明文", contents)!;
        system.LoadMod(manifest);
        Assert.True(system.UnloadMod("mod_1"));
        Assert.Equal(0, system.LoadedCount);
    }

    [Fact]
    public void UnloadMod_NotLoaded_ReturnsFalse()
    {
        var system = new ModLoaderSystem();
        Assert.False(system.UnloadMod("missing_mod"));
    }

    // --- GetContentsByType ---

    [Fact]
    public void GetContentsByType_ReturnsMatchingEntries()
    {
        var system = new ModLoaderSystem();
        var contents = new List<ModLoaderSystem.ModContentEntry>
        {
            new(ModContentType.Item, "items.json", "アイテム"),
            new(ModContentType.Enemy, "enemies.json", "敵"),
            new(ModContentType.Item, "items2.json", "追加アイテム")
        };
        var manifest = system.ParseManifest("mod_1", "テストMOD", "作者", "1.0.0", "説明文", contents)!;
        system.LoadMod(manifest);
        var items = system.GetContentsByType(ModContentType.Item);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public void GetContentsByType_NoMatch_ReturnsEmpty()
    {
        var system = new ModLoaderSystem();
        Assert.Empty(system.GetContentsByType(ModContentType.Quest));
    }

    // --- GetContentTypeName ---

    [Theory]
    [InlineData(ModContentType.Enemy, "敵データ")]
    [InlineData(ModContentType.Item, "アイテムデータ")]
    [InlineData(ModContentType.Map, "マップデータ")]
    [InlineData(ModContentType.Spell, "呪文データ")]
    [InlineData(ModContentType.Quest, "クエストデータ")]
    [InlineData(ModContentType.System, "システム拡張")]
    public void GetContentTypeName_ReturnsJapaneseName(ModContentType type, string expected)
    {
        Assert.Equal(expected, ModLoaderSystem.GetContentTypeName(type));
    }
}
