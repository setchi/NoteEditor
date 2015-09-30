using NoteEditor.Notes;
using NoteEditor.UI.Model;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace NoteEditor.UI.Presenter
{
    public class InputNotesByKeyboardPresenter : MonoBehaviour
    {
        NoteEditorModel model;
        EditNotesPresenter editPresenter;

        void Awake()
        {
            model = NoteEditorModel.Instance;
            editPresenter = EditNotesPresenter.Instance;
            model.OnLoadMusicObservable.First().Subscribe(_ => Init());
        }

        void Init()
        {
            var settingsModel = NoteEditorSettingsModel.Instance;

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
}
