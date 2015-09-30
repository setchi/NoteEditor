using System.IO;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
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

    [SerializeField]
    GameObject saveDialog;
    [SerializeField]
    Button dialogSaveButton;
    [SerializeField]
    Button dialogDoNotSaveButton;
    [SerializeField]
    Button dialogCancelButton;
    [SerializeField]
    Text dialogMessageText;

    NotesEditorModel model;
    ReactiveProperty<bool> mustBeSaved = new ReactiveProperty<bool>();

    void Awake()
    {
        model = NotesEditorModel.Instance;
        var editPresenter = EditNotesPresenter.Instance;

        this.UpdateAsObservable()
            .Where(_ => Input.GetKeyDown(KeyCode.Escape))
            .Subscribe(_ => Application.Quit());

        var saveActionObservable = this.UpdateAsObservable()
            .Where(_ => KeyInput.CtrlPlus(KeyCode.S))
            .Merge(saveButton.OnClickAsObservable());

        mustBeSaved = Observable.Merge(
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
            .ToReactiveProperty();

        mustBeSaved.SubscribeToText(messageText, unsaved => unsaved ? "保存が必要な状態" : "");

        saveActionObservable.Subscribe(_ => Save());

        dialogSaveButton.AddListener(
            EventTriggerType.PointerClick,
            (e) => {
                mustBeSaved.Value = false;
                saveDialog.SetActive(false);
                Save();
                Application.Quit();
            });

        dialogDoNotSaveButton.AddListener(
            EventTriggerType.PointerClick,
            (e) => {
                mustBeSaved.Value = false;
                saveDialog.SetActive(false);
                Application.Quit();
            });

        dialogCancelButton.AddListener(
            EventTriggerType.PointerClick,
            (e) => {
                saveDialog.SetActive(false);
            });

    }

    void OnApplicationQuit()
    {
        if (mustBeSaved.Value)
        {
            dialogMessageText.text = "Do you want to save the changes you made in the note '" + model.MusicName.Value + "' ?"
                + System.Environment.NewLine + "Your changes will be lost if you don't save them.";
            saveDialog.SetActive(true);
            Application.CancelQuit();
        }
    }

    public void Save()
    {
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
    }
}
