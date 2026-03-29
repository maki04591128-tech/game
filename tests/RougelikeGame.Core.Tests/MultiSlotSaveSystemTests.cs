using Xunit;
using RougelikeGame.Core;
using RougelikeGame.Core.Systems;

namespace RougelikeGame.Core.Tests;

/// <summary>
/// マルチスロットセーブシステム 専用テスト
/// テスト数: 18件
/// </summary>
public class MultiSlotSaveSystemTests
{
    // ============================================================
    // 初期状態テスト
    // ============================================================

    [Fact]
    public void Constructor_Creates5EmptySlots()
    {
        // 初期化時に5つの空スロットが作成される
        var system = new MultiSlotSaveSystem();
        var slots = system.GetAllSlots();
        Assert.Equal(5, slots.Count);
        Assert.All(slots, s => Assert.True(s.IsEmpty));
    }

    [Fact]
    public void MaxSlots_Is5()
    {
        // 最大スロット数は5
        Assert.Equal(5, MultiSlotSaveSystem.MaxSlots);
    }

    [Fact]
    public void GetEmptySlotCount_Initial_Returns5()
    {
        // 初期状態で空きスロット数は5
        var system = new MultiSlotSaveSystem();
        Assert.Equal(5, system.GetEmptySlotCount());
    }

    // ============================================================
    // GetSlot テスト
    // ============================================================

    [Fact]
    public void GetSlot_ValidSlot_ReturnsEmptySlotInfo()
    {
        // 有効なスロット番号で空のスロット情報を取得
        var system = new MultiSlotSaveSystem();
        var slot = system.GetSlot(1);
        Assert.Equal(1, slot.SlotNumber);
        Assert.True(slot.IsEmpty);
        Assert.Null(slot.CharacterName);
        Assert.Null(slot.Level);
        Assert.Null(slot.Location);
        Assert.Null(slot.SaveTime);
    }

    [Fact]
    public void GetSlot_InvalidSlot_Zero_ReturnsEmpty()
    {
        // スロット番号0は無効で空のスロット情報を返す
        var system = new MultiSlotSaveSystem();
        var slot = system.GetSlot(0);
        Assert.True(slot.IsEmpty);
    }

    [Fact]
    public void GetSlot_InvalidSlot_ExceedsMax_ReturnsEmpty()
    {
        // 最大スロット数を超えるスロット番号は空を返す
        var system = new MultiSlotSaveSystem();
        var slot = system.GetSlot(6);
        Assert.True(slot.IsEmpty);
    }

    // ============================================================
    // SaveToSlot テスト
    // ============================================================

    [Fact]
    public void SaveToSlot_ValidSlot_ReturnsTrue()
    {
        // 有効なスロットへのセーブは成功する
        var system = new MultiSlotSaveSystem();
        var result = system.SaveToSlot(1, "勇者", 10, "王都");
        Assert.True(result);
    }

    [Fact]
    public void SaveToSlot_InvalidSlot_ReturnsFalse()
    {
        // 無効なスロット番号へのセーブは失敗する
        var system = new MultiSlotSaveSystem();
        Assert.False(system.SaveToSlot(0, "勇者", 10, "王都"));
        Assert.False(system.SaveToSlot(6, "勇者", 10, "王都"));
    }

    [Fact]
    public void SaveToSlot_UpdatesSlotInfo()
    {
        // セーブ後にスロット情報が更新される
        var system = new MultiSlotSaveSystem();
        system.SaveToSlot(2, "魔法使い", 15, "森林");

        var slot = system.GetSlot(2);
        Assert.False(slot.IsEmpty);
        Assert.Equal("魔法使い", slot.CharacterName);
        Assert.Equal(15, slot.Level);
        Assert.Equal("森林", slot.Location);
        Assert.NotNull(slot.SaveTime);
        Assert.Equal(2, slot.SlotNumber);
    }

    [Fact]
    public void SaveToSlot_DecreasesEmptyCount()
    {
        // セーブすると空きスロット数が減少する
        var system = new MultiSlotSaveSystem();
        system.SaveToSlot(1, "戦士", 5, "洞窟");
        Assert.Equal(4, system.GetEmptySlotCount());
    }

    [Fact]
    public void SaveToSlot_OverwriteExistingSlot()
    {
        // 既存セーブデータの上書きが可能
        var system = new MultiSlotSaveSystem();
        system.SaveToSlot(1, "戦士", 5, "洞窟");
        system.SaveToSlot(1, "勇者", 20, "王都");

        var slot = system.GetSlot(1);
        Assert.Equal("勇者", slot.CharacterName);
        Assert.Equal(20, slot.Level);
        Assert.Equal("王都", slot.Location);
    }

    // ============================================================
    // ClearSlot テスト
    // ============================================================

    [Fact]
    public void ClearSlot_ValidSlot_ReturnsTrue()
    {
        // 有効なスロットのクリアは成功する
        var system = new MultiSlotSaveSystem();
        system.SaveToSlot(1, "勇者", 10, "王都");
        Assert.True(system.ClearSlot(1));
    }

    [Fact]
    public void ClearSlot_InvalidSlot_ReturnsFalse()
    {
        // 無効なスロット番号のクリアは失敗する
        var system = new MultiSlotSaveSystem();
        Assert.False(system.ClearSlot(0));
        Assert.False(system.ClearSlot(6));
    }

    [Fact]
    public void ClearSlot_ResetsToEmpty()
    {
        // クリア後にスロットが空に戻る
        var system = new MultiSlotSaveSystem();
        system.SaveToSlot(3, "盗賊", 8, "港町");
        system.ClearSlot(3);

        var slot = system.GetSlot(3);
        Assert.True(slot.IsEmpty);
        Assert.Null(slot.CharacterName);
        Assert.Equal(5, system.GetEmptySlotCount());
    }

    // ============================================================
    // GetOldestSlot テスト
    // ============================================================

    [Fact]
    public void GetOldestSlot_AllEmpty_Returns1()
    {
        // 全スロットが空の場合は1を返す
        var system = new MultiSlotSaveSystem();
        Assert.Equal(1, system.GetOldestSlot());
    }

    [Fact]
    public void GetOldestSlot_SingleSave_ReturnsThatSlot()
    {
        // セーブが1つだけの場合、そのスロット番号を返す
        var system = new MultiSlotSaveSystem();
        system.SaveToSlot(3, "戦士", 5, "洞窟");
        Assert.Equal(3, system.GetOldestSlot());
    }

    // ============================================================
    // 複合テスト
    // ============================================================

    [Fact]
    public void SaveAndClear_MultipleOperations()
    {
        // 複数のセーブ・クリア操作が正しく動作する
        var system = new MultiSlotSaveSystem();
        system.SaveToSlot(1, "勇者A", 10, "王都");
        system.SaveToSlot(2, "勇者B", 20, "森林");
        system.SaveToSlot(3, "勇者C", 30, "山岳");
        Assert.Equal(2, system.GetEmptySlotCount());

        system.ClearSlot(2);
        Assert.Equal(3, system.GetEmptySlotCount());
        Assert.True(system.GetSlot(2).IsEmpty);
        Assert.False(system.GetSlot(1).IsEmpty);
        Assert.False(system.GetSlot(3).IsEmpty);
    }

    [Fact]
    public void SaveSlotInfo_RecordEquality()
    {
        // SaveSlotInfoレコードの等値比較
        var time = DateTime.Now;
        var a = new MultiSlotSaveSystem.SaveSlotInfo(1, "勇者", 10, "王都", time, false);
        var b = new MultiSlotSaveSystem.SaveSlotInfo(1, "勇者", 10, "王都", time, false);
        Assert.Equal(a, b);
    }
}
