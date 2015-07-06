using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class PlayPositionPresenter : MonoBehaviour
{
    [SerializeField]
    CanvasEvents canvasEvents;
    [SerializeField]
    RectTransform canvasRect;
    [SerializeField]
    Slider playPositionController;
    [SerializeField]
    Text playPositionDisplayText;

    NotesEditorModel model;

    void Awake()
    {
        model = NotesEditorModel.Instance;
        model.OnLoadedMusicObservable.First().Subscribe(_ => Init());
    }

    void Init()
    {
        playPositionController.maxValue = model.Audio.clip.samples;


        // Input -> Audio timesamples -> Model timesamples -> UI

        // Input (scroll pad)
        var operateScrollPadObservable = this.UpdateAsObservable()
            .SkipUntil(canvasEvents.ScrollPadOnMouseDownObservable
                .Where(_ => !Input.GetMouseButtonDown(1))
                .Where(_ => 0 > model.ClosestNotePosition.Value.samples))
            .TakeWhile(_ => !Input.GetMouseButtonUp(0))
            .Select(_ => Input.mousePosition.x)
            .Buffer(2, 1).Where(b => 2 <= b.Count)
            .RepeatSafe()
            .Select(b => (b[0] - b[1])
                / model.CanvasWidth.Value
                * model.CanvasScaleFactor.Value
                * model.Audio.clip.samples)
            .Select(delta => model.Audio.timeSamples + delta);

        model.IsDraggingDuringPlay = canvasEvents.ScrollPadOnMouseDownObservable
            .Where(_ => model.IsPlaying.Value)
            .Select(_ => !(model.IsPlaying.Value = false))
            .Merge(this.UpdateAsObservable()
                .Where(_ => model.IsDraggingDuringPlay.Value)
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
            .Select(delta => model.Audio.clip.samples / 100 * -delta);

        // Input (slider)
        var operatePlayPositionSliderObservable = playPositionController.OnValueChangedAsObservable()
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
        this.UpdateAsObservable()
            .Select(_ => model.Audio.timeSamples)
            .DistinctUntilChanged()
            .Subscribe(timeSamples => model.TimeSamples.Value = timeSamples);


        // Model timesamples -> UI(slider)
        model.TimeSamples.DistinctUntilChanged()
            .Subscribe(timeSamples => playPositionController.value = timeSamples);

        // Model timesamples -> UI(text)
        model.TimeSamples.DistinctUntilChanged()
            .Select(timeSamples => timeSamples / (float)model.Audio.clip.samples)
            .Select(per =>
                TimeSpan.FromSeconds(model.Audio.time).ToString().Substring(3, 5)
                + " / "
                + TimeSpan.FromSeconds(model.Audio.clip.samples / model.Audio.clip.frequency).ToString().Substring(3, 5))
            .SubscribeToText(playPositionDisplayText);

        // Model timesamples -> UI(canvas position)
        model.TimeSamples.DistinctUntilChanged()
            .Merge(model.CanvasWidth.Select(_ => model.TimeSamples.Value)) // Merge width scaling timing
            .Select(timeSamples => timeSamples / (float)model.Audio.clip.samples)
            .Select(per => canvasRect.sizeDelta.x * per)
            .Select(x => x + model.CanvasOffsetX.Value)
            .Subscribe(x => canvasRect.localPosition = Vector3.left * x);
    }
}
