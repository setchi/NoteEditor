using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class PlaybackPositionPresenter : MonoBehaviour
{
    [SerializeField]
    CanvasEvents canvasEvents;
    [SerializeField]
    RectTransform canvasRect;
    [SerializeField]
    Slider playbackPositionController;
    [SerializeField]
    Text playbackTimeDisplayText;

    NotesEditorModel model;

    void Awake()
    {
        model = NotesEditorModel.Instance;
        model.OnLoadedMusicObservable.First().Subscribe(_ => Init());
    }

    void Init()
    {
        model.Audio.ObserveEveryValueChanged(audio => audio.clip.samples)
            .Subscribe(samples => playbackPositionController.maxValue = samples);


        // Input -> Audio timesamples -> Model timesamples -> UI

        // Input (scroll pad)
        var operateScrollPadObservable = this.UpdateAsObservable()
            .SkipUntil(canvasEvents.ScrollPadOnMouseDownObservable
                .Where(_ => !Input.GetMouseButtonDown(1))
                .Where(_ => 0 > model.ClosestNotePosition.Value.ToSamples(model.Audio.clip.frequency)))
            .TakeWhile(_ => !Input.GetMouseButtonUp(0))
            .Select(_ => Input.mousePosition.x)
            .Buffer(2, 1).Where(b => 2 <= b.Count)
            .RepeatSafe()
            .Select(b => (b[0] - b[1])
                / model.CanvasWidth.Value
                * model.CanvasScaleFactor.Value
                * model.Audio.clip.samples)
            .Select(delta => model.Audio.timeSamples + delta);

        model.IsOperatingPlaybackPositionDuringPlay = canvasEvents.ScrollPadOnMouseDownObservable
            .Where(_ => model.IsPlaying.Value)
            .Select(_ => !(model.IsPlaying.Value = false))
            .Merge(this.UpdateAsObservable()
                .Where(_ => model.IsOperatingPlaybackPositionDuringPlay.Value)
                .Where(_ => Input.GetMouseButtonUp(0))
                .Select(_ => !(model.IsPlaying.Value = true)))
            .ToReactiveProperty();

        // Input (mouse scroll wheel)
        var operateMouseScrollWheelObservable = canvasEvents.MouseScrollWheelObservable
            .Where(_ =>
                // Ctrl key and Command key is not pressed
                !Input.GetKey(KeyCode.LeftControl) &&
                !Input.GetKey(KeyCode.LeftCommand) &&
                !Input.GetKey(KeyCode.RightControl) &&
                !Input.GetKey(KeyCode.RightCommand))
            .Select(delta => model.Audio.clip.samples / 100f * -delta)
            .Select(deltaSamples => model.Audio.timeSamples + deltaSamples);

        operateMouseScrollWheelObservable.Where(_ => model.IsPlaying.Value)
            .Do(_ => model.IsOperatingPlaybackPositionDuringPlay.Value = true)
            .Subscribe(_ => model.IsPlaying.Value = false);

        operateMouseScrollWheelObservable.Throttle(TimeSpan.FromMilliseconds(350))
            .Where(_ => model.IsOperatingPlaybackPositionDuringPlay.Value)
            .Do(_ => model.IsOperatingPlaybackPositionDuringPlay.Value = false)
            .Subscribe(_ => model.IsPlaying.Value = true);

        // Input (slider)
        var operatePlayPositionSliderObservable = playbackPositionController.OnValueChangedAsObservable()
            .DistinctUntilChanged();


        // Input -> Audio timesamples
        Observable.Merge(
                operateScrollPadObservable,
                operateMouseScrollWheelObservable,
                operatePlayPositionSliderObservable)
            .Select(timeSamples => Mathf.FloorToInt(timeSamples))
            .Select(timeSamples => Mathf.Clamp(timeSamples, 0, model.Audio.clip.samples - 1))
            .Subscribe(timeSamples => model.Audio.timeSamples = timeSamples);


        // Audio timesamples -> Model timesamples
        model.Audio.ObserveEveryValueChanged(audio => audio.timeSamples)
            .DistinctUntilChanged()
            .Subscribe(timeSamples => model.TimeSamples.Value = timeSamples);


        // Model timesamples -> UI(slider)
        model.TimeSamples.DistinctUntilChanged()
            .Subscribe(timeSamples => playbackPositionController.value = timeSamples);

        // Model timesamples -> UI(text)
        model.TimeSamples.DistinctUntilChanged()
            .Select(timeSamples => timeSamples / (float)model.Audio.clip.samples)
            .Select(per =>
                TimeSpan.FromSeconds(model.Audio.time).ToString().Substring(3, 5)
                + " / "
                + TimeSpan.FromSeconds(model.Audio.clip.samples / (float)model.Audio.clip.frequency).ToString().Substring(3, 5))
            .SubscribeToText(playbackTimeDisplayText);

        // Model timesamples -> UI(canvas position)
        model.TimeSamples.DistinctUntilChanged()
            .Merge(model.CanvasWidth.Select(_ => model.TimeSamples.Value)) // Merge width scaling timing
            .Select(timeSamples => timeSamples / (float)model.Audio.clip.samples)
            .Select(per => model.CanvasWidth.Value * per)
            .Select(x => x + model.CanvasOffsetX.Value)
            .Subscribe(x => canvasRect.localPosition = Vector3.left * x);
    }

    public void PlaybackPositionControllerOnMouseDown()
    {
        if (model.IsPlaying.Value)
        {
            model.IsOperatingPlaybackPositionDuringPlay.Value = true;
            model.IsPlaying.Value = false;
        }
    }

    public void PlaybackPositionControllerOnMouseUp()
    {
        if (model.IsOperatingPlaybackPositionDuringPlay.Value)
        {
            model.IsPlaying.Value = true;
            model.IsOperatingPlaybackPositionDuringPlay.Value = false;
        }
    }
}
