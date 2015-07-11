using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class SoundEffectPlayer : MonoBehaviour
{
    [SerializeField]
    AudioSource clapAudioSource;
    [SerializeField]
    AudioSource clickAudioSource;

    void Awake()
    {
        var model = NotesEditorModel.Instance;
        var clapOffsetSamples = 1800;

        var editedDuringPlaybackObservable = Observable.Merge(
                model.BeatOffsetSamples.Select(_ => false),
                model.LongNoteObservable.Select(_ => false),
                model.LongNoteObservable.Select(_ => false))
            .Where(_ => model.IsPlaying.Value);

        model.IsPlaying.Where(isPlaying => isPlaying)
            .Merge(editedDuringPlaybackObservable)
            .Select(_ =>
                new Queue<int>(
                    model.NoteObjects.Values
                        .Select(noteObject => noteObject.notePosition.ToSamples(model.Audio.clip.frequency))
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
