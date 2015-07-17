using System.IO;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class SavePresenter : MonoBehaviour
{
    [SerializeField]
    Button saveButton;
    [SerializeField]
    Text messageText;
    [SerializeField]
    Color unsavedStateButtonColor;
    [SerializeField]
    Color savedStateButtonColor = Color.white;

    void Awake()
    {
        var model = NotesEditorModel.Instance;
        var editPresenter = EditNotesPresenter.Instance;

        var saveActionObservable = this.UpdateAsObservable()
            .Where(_ => KeyInput.CtrlPlus(KeyCode.S))
            .Merge(saveButton.OnClickAsObservable());

        Observable.Merge(
                model.BPM.Select(_ => true),
                model.BeatOffsetSamples.Select(_ => true),
                model.MaxBlock.Select(_ => true),
                editPresenter.RequestForEditNote.Select(_ => true),
                editPresenter.RequestForAddNote.Select(_ => true),
                editPresenter.RequestForRemoveNote.Select(_ => true),
                editPresenter.RequestForChangeNoteStatus.Select(_ => true),
                model.OnLoadMusicObservable.Select(_ => false),
                saveActionObservable.Select(_ => false))
            .SkipUntil(model.OnLoadMusicObservable.DelayFrame(1))
            .Do(unsaved => saveButton.GetComponent<Image>().color = unsaved ? unsavedStateButtonColor : savedStateButtonColor)
            .SubscribeToText(messageText, unsaved => unsaved ? "保存が必要な状態" : "");

        saveActionObservable.Subscribe(_ => {
            var fileName = Path.GetFileNameWithoutExtension(model.MusicName.Value) + ".json";
            var directoryPath = NotesEditorSettingsModel.Instance.WorkSpaceDirectoryPath.Value + "/Notes/";
            var filePath = directoryPath + fileName;
            var json = model.SerializeNotesData();

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllText(filePath, json, System.Text.Encoding.UTF8);
            messageText.text = filePath + " に保存しました";
        });
    }
}
