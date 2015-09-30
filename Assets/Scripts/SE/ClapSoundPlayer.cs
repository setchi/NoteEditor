using NoteEditor.UI.Model;
using NoteEditor.UI.Presenter;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace NoteEditor.SE
{
    public class ClapSoundPlayer : MonoBehaviour
    {
        [SerializeField]
        AudioSource clapAudioSource;

        void Awake()
        {
            var model = NoteEditorModel.Instance;
            var editPresenter = EditNotesPresenter.Instance;
            var clapOffsetSamples = 1800;

            var editedDuringPlaybackObservable = Observable.Merge(
                    model.BeatOffsetSamples.Select(_ => false),
                    editPresenter.RequestForEditNote.Select(_ => false),
                    editPresenter.RequestForRemoveNote.Select(_ => false),
                    editPresenter.RequestForAddNote.Select(_ => false))
                .Where(_ => model.IsPlaying.Value);

            model.IsPlaying.Where(isPlaying => isPlaying)
                .Merge(editedDuringPlaybackObservable)
                .Select(_ =>
                    new Queue<int>(
                        model.NoteObjects.Values
                            .Select(noteObject => noteObject.note.position.ToSamples(model.Audio.clip.frequency, model.BPM.Value))
                            .Distinct()
                            .Select(samples => samples + model.BeatOffsetSamples.Value)
                            .Where(samples => model.Audio.timeSamples <= samples)
                            .OrderBy(samples => samples)
                            .Select(samples => samples - clapOffsetSamples)))
                .SelectMany(samplesQueue =>
                    this.LateUpdateAsObservable()
                        .TakeWhile(_ => model.IsPlaying.Value)
                        .TakeUntil(editedDuringPlaybackObservable.Skip(1))
                        .Select(_ => samplesQueue))
            .Where(samplesQueue => samplesQueue.Count > 0)
            .Where(samplesQueue => samplesQueue.Peek() <= model.Audio.timeSamples)
            .Do(samplesQueue => samplesQueue.Dequeue())
            .Where(_ => model.PlaySoundEffectEnabled.Value)
            .Subscribe(_ => clapAudioSource.PlayOneShot(clapAudioSource.clip, 1));
        }
    }
}
