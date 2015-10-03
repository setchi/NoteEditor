using NoteEditor.Utility;
using UniRx;
using UnityEngine;

namespace NoteEditor.Model
{
    public class Audio : SingletonMonoBehaviour<Audio>
    {
        AudioSource source_;
        Subject<Unit> onLoad = new Subject<Unit>();
        ReactiveProperty<float> volume_ = new ReactiveProperty<float>(1);
        ReactiveProperty<bool> isPlaying_ = new ReactiveProperty<bool>(false);
        ReactiveProperty<int> timeSamples_ = new ReactiveProperty<int>(0);
        ReactiveProperty<float> smoothedTimeSamples_ = new ReactiveProperty<float>(0);

        public static AudioSource Source {
            get { return Instance.source_ ?? (Instance.source_ = Instance.gameObject.AddComponent<AudioSource>()); }
        }
        public static Subject<Unit> OnLoad { get { return Instance.onLoad; } }
        public static ReactiveProperty<float> Volume { get { return Instance.volume_; } }
        public static ReactiveProperty<bool> IsPlaying { get { return Instance.isPlaying_; } }
        public static ReactiveProperty<int> TimeSamples { get { return Instance.timeSamples_; } }
        public static ReactiveProperty<float> SmoothedTimeSamples { get { return Instance.smoothedTimeSamples_; } }
    }
}
