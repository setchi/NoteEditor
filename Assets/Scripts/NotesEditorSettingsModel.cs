using LitJson;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class NotesEditorSettingsModel : SingletonGameObject<NotesEditorSettingsModel>
{
    public ReactiveProperty<string> WorkSpaceDirectoryPath = new ReactiveProperty<string>();
    public ReactiveProperty<List<KeyCode>> NoteInputKeyCodes = new ReactiveProperty<List<KeyCode>>();
    public ReactiveProperty<int> SelectedBlock = new ReactiveProperty<int>();
    public ReactiveProperty<bool> IsViewing = new ReactiveProperty<bool>(false);

    public int MaxBlock = 0;

    public Subject<Unit> ChangeInputKeyCodesObservable = new Subject<Unit>();

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
