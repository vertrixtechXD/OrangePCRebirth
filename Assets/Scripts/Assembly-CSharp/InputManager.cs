using System.Collections.Generic;
using UnityEngine;

public static class InputManager
{
    public const bool PcInput = false;

    private static Dictionary<string, float> axis;
    private static Dictionary<string, bool> virtualButtons;

    static InputManager()
    {
        axis = new Dictionary<string, float>();
        virtualButtons = new Dictionary<string, bool>();
    }

    public static void ShowCursor(bool show)
    {
        Cursor.visible = show;
    }

    public static void UnregisterAxis(string axisName)
    {
        if (axisName == null) return;
        axis.Remove(axisName);
    }

    public static void UpdateAxis(string axisName, float value)
    {
        if (axisName == null) return;
        axis[axisName] = value;
    }

    public static float GetAxis(string axisName)
    {
        if (axisName == null) return 0f;
        float val;
        return axis.TryGetValue(axisName, out val) ? val : 0f;
    }

    public static bool GetButton(string name)
    {
        if (name == null) return false;
        bool val;
        return virtualButtons.TryGetValue(name, out val) && val;
    }

    public static void SetButtonDown(string name)
    {
        if (name == null) return;
        virtualButtons[name] = true;
    }

    public static void SetButtonUp(string name)
    {
        if (name == null) return;
        virtualButtons[name] = false;
    }
}
