using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class InputNotesByKeyboardPresenter : MonoBehaviour
{
    NotesEditorModel model;

    void Awake()
    {
        model = NotesEditorModel.Instance;

        var settingsModel = NotesEditorSettingsModel.Instance;

        this.UpdateAsObservable()
            .Where(_ => !settingsModel.IsViewing.Value)
            .SelectMany(_ => Enumerable.Range(0, settingsModel.MaxBlock.Value))
            .Where(num => Input.GetKeyDown(settingsModel.NoteInputKeyCodes.Value[num]))
            .Subscribe(num => EnterNote(num));
    }

    void EnterNote(int block)
    {
        if (block >= 5) return; // Provisional



        var offset = -5000;
        var unitBeatSamples = model.Audio.clip.frequency * 60f / model.BPM.Value / model.LPB.Value;
        var timeSamples = model.Audio.timeSamples - model.BeatOffsetSamples.Value + (model.IsPlaying.Value ? offset : 0);
        var beats = Mathf.RoundToInt(timeSamples / unitBeatSamples);

        var observable = model.EditType.Value == NoteTypes.Long
                ? model.LongNoteObservable
                : model.NormalNoteObservable;

        observable.OnNext(new NotePosition(model.BPM.Value, model.LPB.Value, beats, block));
    }
}
