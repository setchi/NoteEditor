using NoteEditor.Notes;
using NoteEditor.Model;
using NoteEditor.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace NoteEditor.Presenter
{
    public class InputNotesByKeyboardPresenter : MonoBehaviour
    {
        EditNotesPresenter editPresenter;

        void Awake()
        {
            editPresenter = EditNotesPresenter.Instance;
            Audio.OnLoad.First().Subscribe(_ => Init());
        }

        void Init()
        {
            this.UpdateAsObservable()
                .Where(_ => !Settings.IsOpen.Value)
                .Where(_ => !KeyInput.AltKey())
                .Where(_ => !KeyInput.CtrlKey())
                .Where(_ => !KeyInput.ShiftKey())
                .SelectMany(_ => Observable.Range(0, EditData.MaxBlock.Value))
                .Where(block => Input.GetKeyDown(Settings.NoteInputKeyCodes.Value[block]))
                .Subscribe(block => EnterNote(block));
        }

        void EnterNote(int block)
        {
            var offset = -5000;
            var unitBeatSamples = Audio.Source.clip.frequency * 60f / EditData.BPM.Value / EditData.LPB.Value;
            var timeSamples = Audio.Source.timeSamples - EditData.OffsetSamples.Value + (Audio.IsPlaying.Value ? offset : 0);
            var beats = Mathf.RoundToInt(timeSamples / unitBeatSamples);

            editPresenter.RequestForEditNote.OnNext(new Note(new NotePosition(EditData.LPB.Value, beats, block), EditState.NoteType.Value));
        }
    }
}
