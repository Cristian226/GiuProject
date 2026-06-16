using UnityEngine;

public static class UIBlocker
{
    private static int blockCount = 0;

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
