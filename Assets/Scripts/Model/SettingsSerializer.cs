using LitJson;
using NoteEditor.Model.JSON;
using System.Linq;
using UnityEngine;

namespace NoteEditor.Model
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

            Settings.WorkSpacePath.Value = string.IsNullOrEmpty(data.workSpacePath)
                ? Application.persistentDataPath
                : data.workSpacePath;
        }

        public static string Serialize()
        {
            var data = new SettingsDataModel();

            data.workSpacePath = Settings.WorkSpacePath.Value;
            data.maxBlock = EditData.MaxBlock.Value;
            data.noteInputKeyCodes = Settings.NoteInputKeyCodes.Value
                .Take(EditData.MaxBlock.Value)
                .Select(keyCode => (int)keyCode)
                .ToList();

            return JsonMapper.ToJson(data);
        }
    }
}
