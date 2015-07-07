using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class InputNotesByKeyboardPresenter : MonoBehaviour
{
    NotesEditorModel model;

    void Awake()
    {
        model = NotesEditorModel.Instance;

        var targetFrameObservable = this.UpdateAsObservable();

        targetFrameObservable
            .Where(_ => Input.GetKeyDown(KeyCode.R)).Subscribe(_ => EnterNote(0));

        targetFrameObservable
            .Where(_ => Input.GetKeyDown(KeyCode.F)).Subscribe(_ => EnterNote(1));

        targetFrameObservable
            .Where(_ => Input.GetKeyDown(KeyCode.H)).Subscribe(_ => EnterNote(2));

        targetFrameObservable
            .Where(_ => Input.GetKeyDown(KeyCode.I)).Subscribe(_ => EnterNote(3));

        targetFrameObservable
            .Where(_ => Input.GetKeyDown(KeyCode.J)).Subscribe(_ => EnterNote(4));
    }

    void EnterNote(int block)
    {
        var unitBeatSamples = model.Audio.clip.frequency * 60f / model.BPM.Value / model.LPB.Value;
        var timeSamples = model.Audio.timeSamples - model.BeatOffsetSamples.Value;
        var beats = Mathf.RoundToInt(timeSamples / unitBeatSamples);

        var observable = model.EditType.Value == NoteTypes.Long
                ? model.LongNoteObservable
                : model.NormalNoteObservable;

        observable.OnNext(new NotePosition(model.BPM.Value, model.LPB.Value, beats, block));
    }
}
