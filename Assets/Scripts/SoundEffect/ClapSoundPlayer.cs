using NoteEditor.Model;
using NoteEditor.Presenter;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace NoteEditor.SoundEffect
{
    public class ClapSoundPlayer : MonoBehaviour
    {
        [SerializeField]
        AudioSource clapAudioSource;

        void Awake()
        {
            var editPresenter = EditNotesPresenter.Instance;
            var clapOffsetSamples = 1800;

            var editedDuringPlaybackObservable = Observable.Merge(
                    EditData.OffsetSamples.Select(_ => false),
                    editPresenter.RequestForEditNote.Select(_ => false),
                    editPresenter.RequestForRemoveNote.Select(_ => false),
                    editPresenter.RequestForAddNote.Select(_ => false))
                .Where(_ => Audio.IsPlaying.Value);

            Audio.IsPlaying.Where(isPlaying => isPlaying)
                .Merge(editedDuringPlaybackObservable)
                .Select(_ =>
                    new Queue<int>(
                        EditData.Notes.Values
                            .Select(noteObject => noteObject.note.position.ToSamples(Audio.Source.clip.frequency, EditData.BPM.Value))
                            .Distinct()
                            .Select(samples => samples + EditData.OffsetSamples.Value)
                            .Where(samples => Audio.Source.timeSamples <= samples)
                            .OrderBy(samples => samples)
                            .Select(samples => samples - clapOffsetSamples)))
                .SelectMany(samplesQueue =>
                    this.LateUpdateAsObservable()
                        .TakeWhile(_ => Audio.IsPlaying.Value)
                        .TakeUntil(editedDuringPlaybackObservable.Skip(1))
                        .Select(_ => samplesQueue))
                .Where(samplesQueue => samplesQueue.Count > 0)
                .Where(samplesQueue => samplesQueue.Peek() <= Audio.Source.timeSamples)
                .Do(samplesQueue => samplesQueue.Dequeue())
                .Where(_ => EditorState.ClapSoundEffectEnabled.Value)
                .Subscribe(_ => clapAudioSource.PlayOneShot(clapAudioSource.clip, 1));
        }
    }
}
