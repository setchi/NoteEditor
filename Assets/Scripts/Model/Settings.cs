using LitJson;
using NoteEditor.Model.JSON;
using NoteEditor.Utility;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace NoteEditor.Model
{
    public class Settings : SingletonMonoBehaviour<Settings>
    {
        ReactiveProperty<string> workSpaceDirectoryPath_ = new ReactiveProperty<string>();
        ReactiveProperty<List<KeyCode>> noteInputKeyCodes_ = new ReactiveProperty<List<KeyCode>>();
        ReactiveProperty<int> selectedBlock_ = new ReactiveProperty<int>();
        ReactiveProperty<bool> isViewing_ = new ReactiveProperty<bool>(false);
        Subject<Unit> requestForChangeInputNoteKeyCode_ = new Subject<Unit>();

        public static ReactiveProperty<string> WorkSpaceDirectoryPath { get { return Instance.workSpaceDirectoryPath_; } }
        public static ReactiveProperty<List<KeyCode>> NoteInputKeyCodes { get { return Instance.noteInputKeyCodes_; } }
        public static ReactiveProperty<int> SelectedBlock { get { return Instance.selectedBlock_; } }
        public static ReactiveProperty<bool> IsViewing { get { return Instance.isViewing_; } }
        public static Subject<Unit> RequestForChangeInputNoteKeyCode { get { return Instance.requestForChangeInputNoteKeyCode_; } }
        public static int MaxBlock = 0;

        public static void Apply(SettingsDataModel data)
        {
            NoteInputKeyCodes.Value = data.noteInputKeyCodes
                .Select(keyCodeNum => (KeyCode)keyCodeNum)
                .ToList();

            MaxBlock = data.maxBlock;

            WorkSpaceDirectoryPath.Value = string.IsNullOrEmpty(data.workSpaceDirectoryPath)
                ? Application.persistentDataPath
                : data.workSpaceDirectoryPath;
        }

        public static string SerializeSettings()
        {
            var data = new SettingsDataModel();

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
