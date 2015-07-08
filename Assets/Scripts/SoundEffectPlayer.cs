using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class SoundEffectPlayer : MonoBehaviour
{
    [SerializeField]
    AudioClip clap;
    [SerializeField]
    AudioClip click;

    void Awake()
    {
        var model = NotesEditorModel.Instance;
        var clapOffsetSamples = 1800;

        model.IsPlaying.Where(isPlaying => isPlaying).SelectMany(__ =>
        {
            var notesQueue = new Queue<int>(model.NoteObjects.Values
                .Select(noteObject => noteObject.notePosition.ToSamples(model.Audio.clip.frequency))
                .Where(samples => model.Audio.timeSamples <= samples)
                .OrderBy(samples => samples)
                .Select(samples => samples - clapOffsetSamples));

            return this.LateUpdateAsObservable()
                .TakeWhile(_ => model.IsPlaying.Value)
                .Select(_ => new { timeSamples = model.Audio.timeSamples, queue = notesQueue });
        })
        .Where(obj => 1 < obj.queue.Count)
        .Where(obj => obj.queue.Peek() <= obj.timeSamples)
        .Do(obj => obj.queue.Dequeue())
        .Where(_ => model.PlaySoundEffectEnabled.Value)
        .Subscribe(_ => AudioSource.PlayClipAtPoint(clap, Vector3.zero));
    }
}
