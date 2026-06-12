using UnityEngine;

/// <summary>
/// Global gate that tells gameplay scripts (movement, raycast interaction) to
/// ignore input while a blocking UI (dialogue, mission mini-game, info popup) is
/// open, and shows/hides the mouse cursor to match.
///
/// A UI calls Push() when it opens and Pop() when it closes. It's a counter, so
/// overlapping UIs are handled correctly: the cursor appears on the first Push and
/// is hidden again on the last Pop. GameFlowManager calls Reset() on every scene
/// load so a half-finished state can never leave the game stuck or cursor-locked.
/// </summary>
public static class UIBlocker
{
    private static int blockCount = 0;

    /// <summary>True while at least one blocking UI is open.</summary>
    public static bool IsBlocked => blockCount > 0;

    public static void Push()
    {
        blockCount++;
        if (blockCount == 1) ShowCursor(true);
    }

    public static void Pop()
    {
        blockCount = Mathf.Max(0, blockCount - 1);
        if (blockCount == 0) ShowCursor(false);
    }

    public static void Reset()
    {
        blockCount = 0;
        ShowCursor(false);
    }

    private static void ShowCursor(bool visible)
    {
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = visible;
    }
}
