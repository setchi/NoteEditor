using LitJson;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class NotesEditorSettingsModel : SingletonGameObject<NotesEditorSettingsModel>
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
        data.maxBlock = NotesEditorModel.Instance.MaxBlock.Value;
        data.noteInputKeyCodes = NoteInputKeyCodes.Value
            .Take(NotesEditorModel.Instance.MaxBlock.Value)
            .Select(keyCode => (int)keyCode)
            .ToList();

        return JsonMapper.ToJson(data);
    }
}
