using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class InputNotesByKeyboardPresenter : MonoBehaviour
{
    NotesEditorModel model;
    EditNotesPresenter editPresenter;

    void Awake()
    {
        model = NotesEditorModel.Instance;
        editPresenter = EditNotesPresenter.Instance;
        model.OnLoadedMusicObservable.First().Subscribe(_ => Init());
    }

    void Init()
    {
        var settingsModel = NotesEditorSettingsModel.Instance;

        this.UpdateAsObservable()
            .Where(_ => !settingsModel.IsViewing.Value)
            .Where(_ => !KeyInput.AltKey())
            .Where(_ => !KeyInput.CtrlKey())
            .Where(_ => !KeyInput.ShiftKey())
            .SelectMany(_ => Observable.Range(0, model.MaxBlock.Value))
            .Where(num => Input.GetKeyDown(settingsModel.NoteInputKeyCodes.Value[num]))
            .Subscribe(num => EnterNote(num));
    }

    void EnterNote(int block)
    {
        var offset = -5000;
        var unitBeatSamples = model.Audio.clip.frequency * 60f / model.BPM.Value / model.LPB.Value;
        var timeSamples = model.Audio.timeSamples - model.BeatOffsetSamples.Value + (model.IsPlaying.Value ? offset : 0);
        var beats = Mathf.RoundToInt(timeSamples / unitBeatSamples);

        editPresenter.RequestForEditNote.OnNext(new Note(new NotePosition(model.LPB.Value, beats, block), model.EditType.Value));
    }
}
