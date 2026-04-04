namespace RougelikeGame.Core.Systems;

/// <summary>
/// C-1: 自動セーブシステム - フロア移動時・一定ターン毎に自動保存
/// </summary>
public class AutoSaveSystem
{
    /// <summary>自動セーブの間隔（ターン数）</summary>
    public int AutoSaveInterval { get; set; } = 300;

    /// <summary>フロア移動時に自動セーブするか</summary>
    public bool SaveOnFloorChange { get; set; } = true;

    /// <summary>戦闘終了時に自動セーブするか</summary>
    public bool SaveAfterCombat { get; set; } = false;

    /// <summary>自動セーブが有効か</summary>
    public bool IsEnabled { get; set; } = true;

    private int _lastSaveTurn;

    /// <summary>ターン経過をチェックして自動セーブが必要か判定</summary>
    public bool ShouldAutoSave(int currentTurn, AutoSaveTrigger trigger)
    {
        if (!IsEnabled) return false;

        return trigger switch
        {
            AutoSaveTrigger.TurnElapsed => currentTurn - _lastSaveTurn >= AutoSaveInterval,
            AutoSaveTrigger.FloorChange => SaveOnFloorChange,
            AutoSaveTrigger.CombatEnd => SaveAfterCombat,
            _ => false
        };
    }

    /// <summary>セーブ実行後に呼ぶ（最終セーブターンを記録）</summary>
    public void MarkSaved(int currentTurn)
    {
        _lastSaveTurn = currentTurn;
    }

    /// <summary>セーブ間隔をリセット（ロード時など）</summary>
    public void Reset(int currentTurn = 0)
    {
        _lastSaveTurn = currentTurn;
    }
}

/// <summary>自動セーブのトリガー種別</summary>
public enum AutoSaveTrigger
{
    /// <summary>ターン経過</summary>
    TurnElapsed,
    /// <summary>フロア移動</summary>
    FloorChange,
    /// <summary>戦闘終了</summary>
    CombatEnd
}
