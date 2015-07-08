using UnityEngine;

public class KeyInput
{
    public static bool ShiftPlus(KeyCode keyCode)
    {
        return ShiftKey() && Input.GetKeyDown(keyCode);
    }

    public static bool ShiftKey()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    public static bool AltPlus(KeyCode keyCode)
    {
        return AltKey() && Input.GetKeyDown(keyCode);
    }

    public static bool AltKey()
    {
        return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
    }

    public static bool CtrlPlus(KeyCode keyCode)
    {
        return CtrlKey() && Input.GetKeyDown(keyCode);
    }

    public static bool CtrlKey()
    {
        return Input.GetKey(KeyCode.LeftControl) ||
            Input.GetKey(KeyCode.LeftCommand) ||
            Input.GetKey(KeyCode.RightControl) ||
            Input.GetKey(KeyCode.RightCommand);
    }
}
