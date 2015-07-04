using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class ControlPanelPresenter : MonoBehaviour
{
    [SerializeField]
    CanvasEvents canvasEvents;
    [SerializeField]
    CanvasScaler canvasScaler;
    [SerializeField]
    RectTransform canvasRect;
    [SerializeField]
    RectTransform verticalLineRect;
    [SerializeField]
    Button playButton;
    [SerializeField]
    Text titleText;
    [SerializeField]
    Slider canvasWidthScaleController;
    [SerializeField]
    Slider divisionNumOfOneMeasureController;
    [SerializeField]
    Slider volumeController;
    [SerializeField]
    InputField BPMInputField;
    [SerializeField]
    InputField beatOffsetInputField;
    [SerializeField]
    Toggle waveformDisplayEnabled;

    void Awake()
    {
        if (SelectedMusicDataStore.Instance.audioClip == null)
        {
            ObservableWWW.GetWWW("file:///" + Application.persistentDataPath + "/Musics/test.wav").Subscribe(www =>
            {
                SelectedMusicDataStore.Instance.audioClip = www.audioClip;
                Init();
            });

            return;
        }

        Init();
    }

    void Init()
    {
        var model = NotesEditorModel.Instance;
        model.Audio = gameObject.AddComponent<AudioSource>();


        // Binds canvas scale factor
        model.CanvasScaleFactor.Value = canvasScaler.referenceResolution.x / Screen.width;
        this.UpdateAsObservable()
            .Select(_ => Screen.width)
            .DistinctUntilChanged()
            .Subscribe(w => model.CanvasScaleFactor.Value = canvasScaler.referenceResolution.x / w);


        // Binds mouseover on canvas
        model.IsMouseOverCanvas = canvasEvents.ScrollPadOnMouseExitObservable.Select(_ => false)
            .Merge(canvasEvents.ScrollPadOnMouseEnterObservable.Select(_ => true))
            .ToReactiveProperty();


        // Binds division number of measure
        divisionNumOfOneMeasureController.OnValueChangedAsObservable()
            .Select(x => Mathf.FloorToInt(x))
            .Subscribe(x => model.DivisionNumOfOneMeasure.Value = x);


        // Apply music data
        model.Audio.clip = SelectedMusicDataStore.Instance.audioClip;
        titleText.text = SelectedMusicDataStore.Instance.fileName ?? "Test";


        // Initialize canvas offset x
        model.CanvasOffsetX.Value = -Screen.width * 0.45f * model.CanvasScaleFactor.Value;


        // Binds waveform display enabled
        model.WaveformDisplayEnabled = waveformDisplayEnabled.OnValueChangedAsObservable().ToReactiveProperty();


        // Binds audio volume
        model.Volume = volumeController.OnValueChangedAsObservable().ToReactiveProperty();
        model.Volume.DistinctUntilChanged().Subscribe(x => model.Audio.volume = x);


        // Binds canvas width with mouse scroll wheel and slider
        model.CanvasWidth = canvasEvents.MouseScrollWheelObservable
            .Where(_ => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            .Select(delta => model.CanvasWidth.Value * (1 + delta))
            .Select(x => x / (model.Audio.clip.samples / 100f))
            .Select(x => Mathf.Clamp(x, 0.1f, 2f))
            .Merge(canvasWidthScaleController.OnValueChangedAsObservable()
                .DistinctUntilChanged())
            .Select(x => model.Audio.clip.samples / 100f * x)
            .ToReactiveProperty();

        model.CanvasWidth.DistinctUntilChanged()
            .Do(x => canvasWidthScaleController.value = x / (model.Audio.clip.samples / 100f))
            .Subscribe(x =>
            {
                var delta = canvasRect.sizeDelta;
                delta.x = x;
                canvasRect.sizeDelta = delta;
            });


        // Binds BPM
        model.UnitBeatSamples = model.BPM.DistinctUntilChanged()
            .Select(x => Mathf.FloorToInt(model.Audio.clip.frequency * 60 / x))
            .ToReactiveProperty();

        BPMInputField.OnValueChangeAsObservable()
            .Select(x => string.IsNullOrEmpty(x) ? "1" : x)
            .Select(x => int.Parse(x))
            .Select(x => Mathf.Clamp(x, 1, 320))
            .Subscribe(x => model.BPM.Value = x);

        model.BPM.DistinctUntilChanged()
            .Subscribe(x => BPMInputField.text = x.ToString());


        // Binds beat offset samples with input
        beatOffsetInputField.OnValueChangeAsObservable()
            .Select(x => string.IsNullOrEmpty(x) ? "0" : x)
            .Select(x => int.Parse(x))
            .Subscribe(x => model.BeatOffsetSamples.Value = x);

        model.BeatOffsetSamples.DistinctUntilChanged()
            .Subscribe(x => beatOffsetInputField.text = x.ToString());


        // Binds canvas position with samples
        this.UpdateAsObservable()
            .Select(_ => model.Audio.timeSamples)
            .DistinctUntilChanged()
            .Merge(model.CanvasWidth.Select(_ => model.Audio.timeSamples)) // Merge resized timing
            .Select(timeSamples => timeSamples / (float)model.Audio.clip.samples)
            .Select(per => canvasRect.sizeDelta.x * per)
            .Select(x => x + model.CanvasOffsetX.Value)
            .Subscribe(x => canvasRect.localPosition = Vector3.left * x);


        // Binds samples with dragging canvas and mouse scroll wheel
        this.UpdateAsObservable()
            .SkipUntil(canvasEvents.ScrollPadOnMouseDownObservable
                .Where(_ => 0 > model.ClosestNotePosition.Value.samples))
            .TakeWhile(_ => !Input.GetMouseButtonUp(0))
            .Select(_ => Input.mousePosition.x)
            .Buffer(2, 1).Where(b => 2 <= b.Count)
            .RepeatSafe()
            .Select(b => (b[0] - b[1])
                / model.CanvasWidth.Value
                * model.CanvasScaleFactor.Value
                * model.Audio.clip.samples)
            .Merge(canvasEvents.MouseScrollWheelObservable // Merge mouse scroll wheel
                .Where(_ => !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
                .Select(delta => model.Audio.clip.samples / 100 * -delta))
            .Select(deltaSamples => model.Audio.timeSamples + Mathf.RoundToInt(deltaSamples))
            .Select(timeSamples => Mathf.Clamp(timeSamples, 0, model.Audio.clip.samples - 1))
            .Subscribe(timeSamples => model.Audio.timeSamples = timeSamples);

        model.IsDraggingDuringPlay = canvasEvents.ScrollPadOnMouseDownObservable
            .Where(_ => model.IsPlaying.Value)
            .Select(_ => !(model.IsPlaying.Value = false))
            .Merge(this.UpdateAsObservable()
                .Where(_ => model.IsDraggingDuringPlay.Value)
                .Where(_ => Input.GetMouseButtonUp(0))
                .Select(_ => !(model.IsPlaying.Value = true)))
            .ToReactiveProperty();


        // Binds offset x of canvas
        this.UpdateAsObservable()
            .SkipUntil(canvasEvents.VerticalLineOnMouseDownObservable)
            .TakeWhile(_ => !Input.GetMouseButtonUp(0))
            .Select(_ => Input.mousePosition.x)
            .Buffer(2, 1).Where(b => 2 <= b.Count)
            .RepeatSafe()
            .Select(b => (b[1] - b[0]) * model.CanvasScaleFactor.Value)
            .Select(x => x + model.CanvasOffsetX.Value)
            .Select(x => new { x, max = Screen.width * 0.5f * 0.95f * model.CanvasScaleFactor.Value })
            .Select(v => Mathf.Clamp(v.x, -v.max, v.max))
            .Subscribe(x => model.CanvasOffsetX.Value = x);

        model.CanvasOffsetX.DistinctUntilChanged().Subscribe(x =>
        {
            var pos = verticalLineRect.localPosition;
            pos.x = x;
            verticalLineRect.localPosition = pos;
        });


        // Binds play pause toggle
        playButton.OnClickAsObservable()
            .Subscribe(_ => model.IsPlaying.Value = !model.IsPlaying.Value);

        model.IsPlaying.DistinctUntilChanged().Subscribe(playing =>
        {
            var playButtonText = playButton.GetComponentInChildren<Text>();

            if (playing)
            {
                model.Audio.Play();
                playButtonText.text = "Pause";
            }
            else
            {
                model.Audio.Pause();
                playButtonText.text = "Play";
            }
        });

    }

}
