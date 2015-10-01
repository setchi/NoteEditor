using LitJson;
using NoteEditor.JSONModel;
using NoteEditor.Utility;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace NoteEditor.UI.Model
{
    public class NoteEditorSettingsModel : SingletonMonoBehaviour<NoteEditorSettingsModel>
    {
        public readonly ReactiveProperty<string> WorkSpaceDirectoryPath = new ReactiveProperty<string>();
        public readonly ReactiveProperty<List<KeyCode>> NoteInputKeyCodes = new ReactiveProperty<List<KeyCode>>();
        public readonly ReactiveProperty<int> SelectedBlock = new ReactiveProperty<int>();
        public readonly ReactiveProperty<bool> IsViewing = new ReactiveProperty<bool>(false);
        public readonly Subject<Unit> RequestForChangeInputNoteKeyCode = new Subject<Unit>();

        [HideInInspector]
        public int MaxBlock = 0;

        public void Apply(SettingsModel data)
        {
            NoteInputKeyCodes.Value = data.noteInputKeyCodes
                .Select(keyCodeNum => (KeyCode)keyCodeNum)
                .ToList();

            MaxBlock = data.maxBlock;

            WorkSpaceDirectoryPath.Value = string.IsNullOrEmpty(data.workSpaceDirectoryPath)
                ? Application.persistentDataPath
                : data.workSpaceDirectoryPath;
        }

        public string SerializeSettings()
        {
            var data = new SettingsModel();

            data.workSpaceDirectoryPath = WorkSpaceDirectoryPath.Value;
            data.maxBlock = EditData.MaxBlock.Value;
            data.noteInputKeyCodes = NoteInputKeyCodes.Value
                .Take(EditData.MaxBlock.Value)
                .Select(keyCode => (int)keyCode)
                .ToList();

            return JsonMapper.ToJson(data);
        }
    }
}
