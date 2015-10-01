using NoteEditor.UI.Model;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace NoteEditor.UI.Presenter
{
    public class SmoothedTimeSamplesPresenter : MonoBehaviour
    {
        NoteEditorModel model;

        void Awake()
        {
            model = NoteEditorModel.Instance;
            Audio.OnLoad.First().Subscribe(_ => Init());
        }

        void Init()
        {
            var prevFrameSamples = 0f;
            var counter = 0;

            this.UpdateAsObservable()
                .Where(_ => Audio.Source.clip != null)
                .Where(_ => Audio.IsPlaying.Value)
                .Subscribe(_ =>
                {
                    var deltaSamples = counter == 0
                        ? (Audio.Source.timeSamples - prevFrameSamples)
                        : Audio.Source.clip.frequency * Time.deltaTime;

                    Audio.SmoothedTimeSamples.Value += deltaSamples;
                    prevFrameSamples = Audio.SmoothedTimeSamples.Value;

                    counter = ++counter % 180;
                });

            Audio.TimeSamples
                .Where(_ => Audio.Source.clip != null)
                .Where(_ => !Audio.IsPlaying.Value)
                .Subscribe(timeSamples =>
                {
                    counter = 0;
                    Audio.SmoothedTimeSamples.Value = timeSamples;
                    prevFrameSamples = timeSamples;
                });
        }
    }
}
