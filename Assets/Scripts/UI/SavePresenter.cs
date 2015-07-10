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

    NotesEditorModel model;

    void Awake()
    {
        model = NotesEditorModel.Instance;

        var saveActionObservable = this.UpdateAsObservable()
            .Where(_ => KeyInput.CtrlPlus(KeyCode.S))
            .Merge(saveButton.OnClickAsObservable());

        Observable.Merge(
                model.BPM.Select(_ => true),
                model.BeatOffsetSamples.Select(_ => true),
                model.NormalNoteObservable.Select(_ => true),
                model.LongNoteObservable.Select(_ => true),
                model.OnLoadedMusicObservable.Select(_ => false),
                saveActionObservable.Select(_ => false))
            .SkipUntil(model.OnLoadedMusicObservable.DelayFrame(1))
            .Do(unsaved => saveButton.GetComponent<Image>().color = unsaved ? unsavedStateButtonColor : savedStateButtonColor)
            .SubscribeToText(messageText, unsaved => unsaved ? "保存が必要な状態" : "");

        saveActionObservable.Subscribe(_ => {
            var fileName = Path.GetFileNameWithoutExtension(model.MusicName.Value) + ".json";
            var directoryPath = Application.persistentDataPath + "/Notes/";
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
