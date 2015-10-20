using NoteEditor.Common;
using NoteEditor.Model;
using NoteEditor.Utility;
using System;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.Presenter
{
    public class PlaybackPositionPresenter : MonoBehaviour
    {
        [SerializeField]
        CanvasEvents canvasEvents;
        [SerializeField]
        Slider playbackPositionController;
        [SerializeField]
        Text playbackTimeDisplayText;

        void Awake()
        {
            Audio.OnLoad.First().Subscribe(_ => Init());
        }

        void Init()
        {
            this.UpdateAsObservable()
                .Where(_ => Audio.Source.clip != null)
                .Select(_ => Audio.Source.clip.samples)
                .Subscribe(samples => playbackPositionController.maxValue = samples);

            // Input -> Audio timesamples -> Model timesamples -> UI

            // Input (arrow key)
            var operateArrowKeyObservable = Observable.Merge(
                    this.UpdateAsObservable().Where(_ => Input.GetKey(KeyCode.RightArrow)).Select(_ => 7),
                    this.UpdateAsObservable().Where(_ => Input.GetKey(KeyCode.LeftArrow)).Select(_ => -7))
                .Select(delta => delta * (KeyInput.CtrlKey() ? 5 : 1))
                .Select(delta => delta
                    / NoteCanvas.Width.Value
                    * NoteCanvas.ScaleFactor.Value
                    * Audio.Source.clip.samples)
                .Select(delta => Audio.Source.timeSamples + delta);

            operateArrowKeyObservable.Where(_ => Audio.IsPlaying.Value)
                .Do(_ => Audio.IsPlaying.Value = false)
                .Subscribe(_ => EditState.IsOperatingPlaybackPositionDuringPlay.Value = true);

            operateArrowKeyObservable.Where(_ => EditState.IsOperatingPlaybackPositionDuringPlay.Value)
                .Throttle(TimeSpan.FromMilliseconds(50))
                .Do(_ => Audio.IsPlaying.Value = true)
                .Subscribe(_ => EditState.IsOperatingPlaybackPositionDuringPlay.Value = false);

            // Input (scroll pad)
            var operateScrollPadObservable = this.UpdateAsObservable()
                .SkipUntil(canvasEvents.WaveformRegionOnMouseDownObservable
                    .Where(_ => !Input.GetMouseButtonDown(1)))
                .TakeWhile(_ => !Input.GetMouseButtonUp(0))
                .Select(_ => Input.mousePosition.x)
                .Buffer(2, 1).Where(b => 2 <= b.Count)
                .RepeatSafe()
                .Select(b => (b[0] - b[1])
                    / NoteCanvas.Width.Value
                    * NoteCanvas.ScaleFactor.Value
                    * Audio.Source.clip.samples)
                .Select(delta => Audio.Source.timeSamples + delta);

            canvasEvents.WaveformRegionOnMouseDownObservable
                .Where(_ => Audio.IsPlaying.Value)
                .Do(_ => Audio.IsPlaying.Value = false)
                .Subscribe(_ => EditState.IsOperatingPlaybackPositionDuringPlay.Value = true);

            this.UpdateAsObservable()
                .Where(_ => EditState.IsOperatingPlaybackPositionDuringPlay.Value)
                .Where(_ => Input.GetMouseButtonUp(0))
                .Do(_ => Audio.IsPlaying.Value = true)
                .Subscribe(_ => EditState.IsOperatingPlaybackPositionDuringPlay.Value = false);

            // Input (mouse scroll wheel)
            var operateMouseScrollWheelObservable = canvasEvents.MouseScrollWheelObservable
                .Where(_ => !KeyInput.CtrlKey())
                .Select(delta => Audio.Source.clip.samples / 100f * -delta)
                .Select(deltaSamples => Audio.Source.timeSamples + deltaSamples);

            operateMouseScrollWheelObservable.Where(_ => Audio.IsPlaying.Value)
                .Do(_ => EditState.IsOperatingPlaybackPositionDuringPlay.Value = true)
                .Subscribe(_ => Audio.IsPlaying.Value = false);

            operateMouseScrollWheelObservable.Throttle(TimeSpan.FromMilliseconds(350))
                .Where(_ => EditState.IsOperatingPlaybackPositionDuringPlay.Value)
                .Do(_ => EditState.IsOperatingPlaybackPositionDuringPlay.Value = false)
                .Subscribe(_ => Audio.IsPlaying.Value = true);

            var isRedoUndoAction = false;

            // Input (slider)
            var operatePlayPositionSliderObservable = playbackPositionController.OnValueChangedAsObservable()
                .DistinctUntilChanged();


            // Input -> Audio timesamples
            var operatePlaybackPositionObservable = Observable.Merge(
                    operateArrowKeyObservable,
                    operateScrollPadObservable,
                    operateMouseScrollWheelObservable,
                    operatePlayPositionSliderObservable)
                .Select(timeSamples => Mathf.FloorToInt(timeSamples))
                .Select(timeSamples => Mathf.Clamp(timeSamples, 0, Audio.Source.clip.samples - 1));

            operatePlaybackPositionObservable.Subscribe(timeSamples => Audio.Source.timeSamples = timeSamples);

            operatePlaybackPositionObservable.Buffer(operatePlaybackPositionObservable.ThrottleFrame(10))
                .Where(_ => isRedoUndoAction ? (isRedoUndoAction = false) : true)
                .Where(b => 2 <= b.Count)
                .Select(x => new { current = x.Last(), prev = x.First() })
                .Subscribe(x => EditCommandManager.Do(
                    new Command(
                        () => Audio.TimeSamples.Value = x.current,
                        () => { isRedoUndoAction = true; Audio.TimeSamples.Value = x.prev; },
                        () => { isRedoUndoAction = true; Audio.TimeSamples.Value = x.current; })));


            // Audio timesamples -> Model timesamples
            Audio.Source.ObserveEveryValueChanged(audio => audio.timeSamples)
                .DistinctUntilChanged()
                .Subscribe(timeSamples => Audio.TimeSamples.Value = timeSamples);

            this.UpdateAsObservable()
                .Where(_ => Audio.Source.clip != null)
                .Where(_ => Audio.Source.timeSamples > Audio.Source.clip.samples - 1)
                .Subscribe(_ => Audio.IsPlaying.Value = false);


            // Model timesamples -> UI(slider)
            Audio.TimeSamples.Subscribe(timeSamples => playbackPositionController.value = timeSamples);

            // Model timesamples -> UI(text)
            Audio.TimeSamples.Select(_ => TimeSpan.FromSeconds(Audio.Source.time).ToString().Substring(3, 5))
                .DistinctUntilChanged()
                .Select(elapsedTime =>
                    elapsedTime + " / "
                    + TimeSpan.FromSeconds(Audio.Source.clip.samples / (float)Audio.Source.clip.frequency).ToString().Substring(3, 5))
                .SubscribeToText(playbackTimeDisplayText);
        }

        public void PlaybackPositionControllerOnMouseDown()
        {
            if (Audio.IsPlaying.Value)
            {
                EditState.IsOperatingPlaybackPositionDuringPlay.Value = true;
                Audio.IsPlaying.Value = false;
            }
        }

        public void PlaybackPositionControllerOnMouseUp()
        {
            if (EditState.IsOperatingPlaybackPositionDuringPlay.Value)
            {
                Audio.IsPlaying.Value = true;
                EditState.IsOperatingPlaybackPositionDuringPlay.Value = false;
            }
        }
    }
}
