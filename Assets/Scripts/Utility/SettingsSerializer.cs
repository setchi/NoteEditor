using LitJson;
using NoteEditor.Model;
using NoteEditor.Model.JSON;
using System.Linq;
using UnityEngine;

namespace NoteEditor.Utility
{
    public class SettingsSerializer
    {
        public static void Deserialize(string json)
        {
            var data = JsonMapper.ToObject<SettingsDataModel>(json);
            Settings.NoteInputKeyCodes.Value = data.noteInputKeyCodes
                .Select(keyCodeNum => (KeyCode)keyCodeNum)
                .ToList();

            Settings.MaxBlock = data.maxBlock;

            Settings.WorkSpaceDirectoryPath.Value = string.IsNullOrEmpty(data.workSpaceDirectoryPath)
                ? Application.persistentDataPath
                : data.workSpaceDirectoryPath;
        }

        public static string Serialize()
        {
            var data = new SettingsDataModel();

            data.workSpaceDirectoryPath = Settings.WorkSpaceDirectoryPath.Value;
            data.maxBlock = EditData.MaxBlock.Value;
            data.noteInputKeyCodes = Settings.NoteInputKeyCodes.Value
                .Take(EditData.MaxBlock.Value)
                .Select(keyCode => (int)keyCode)
                .ToList();

            return JsonMapper.ToJson(data);
        }
    }
}
