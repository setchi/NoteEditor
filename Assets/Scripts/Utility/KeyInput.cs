using UnityEngine;

namespace NoteEditor.Utility
{
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

        public static bool ShiftKeyDown()
        {
            return Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
        }

        public static bool AltPlus(KeyCode keyCode)
        {
            return AltKey() && Input.GetKeyDown(keyCode);
        }

        public static bool AltKey()
        {
            return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        }

        public static bool AltKeyDown()
        {
            return Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt);
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

        public static bool CtrlKeyDown()
        {
            return Input.GetKeyDown(KeyCode.LeftControl) ||
                Input.GetKeyDown(KeyCode.LeftCommand) ||
                Input.GetKeyDown(KeyCode.RightControl) ||
                Input.GetKeyDown(KeyCode.RightCommand);
        }

        public static KeyCode FetchKey()
        {
            int e = System.Enum.GetNames(typeof(KeyCode)).Length;

            for (int i = 0; i < e; i++)
            {
                if (Input.GetKey((KeyCode)i))
                {
                    return (KeyCode)i;
                }
            }

            return KeyCode.None;
        }
    }
}
