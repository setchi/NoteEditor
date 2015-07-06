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

    NotesEditorModel model;

    void Awake()
    {
        model = NotesEditorModel.Instance;

        var saveActionObservable = this.UpdateAsObservable()
            .Where(_ => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            .Where(_ => Input.GetKey(KeyCode.S))
            .Merge(saveButton.OnClickAsObservable());

        Observable.Merge(
                model.BPM.Select(_ => true),
                model.LPB.Select(_ => true),
                model.BeatOffsetSamples.Select(_ => true),
                model.NormalNoteObservable.Select(_ => true),
                model.LongNoteObservable.Select(_ => true))
            .Merge(saveActionObservable.Select(_ => false))
            .SkipUntil(model.OnLoadedMusicObservable.DelayFrame(1))
            .Do(unsaved => saveButton.GetComponent<Image>().color = unsaved ? Color.yellow : Color.white)
            .SubscribeToText(messageText, _ => "保存が必要な状態");

        saveActionObservable.Subscribe(_ => {
            var fileName = Path.GetFileNameWithoutExtension(SelectedMusicDataStore.Instance.fileName) + ".json";
            var filePath = Application.persistentDataPath + "/Notes/";
            var fileFullPath = filePath + fileName;
            var text = model.SerializeNotesData();

            if (!File.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            File.WriteAllText(fileFullPath, text, System.Text.Encoding.UTF8);
            messageText.text = fileFullPath + " に保存しました";
        });
    }
}
