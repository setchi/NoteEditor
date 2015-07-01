using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;


public class NotesEditorPresenter : MonoBehaviour
{
    [SerializeField]
    CanvasScaler canvasScaler;
    [SerializeField]
    RectTransform canvasRect;
    [SerializeField]
    RectTransform verticalLineRect;
    [SerializeField]
    AudioSource audioSource;
    [SerializeField]
    Button playButton;
    [SerializeField]
    Text titleText;
    [SerializeField]
    DrawLineTest drawLineTest;
    [SerializeField]
    Slider _scaleSliderTest;
    [SerializeField]
    Slider divisionNumOfOneMeasureSlider;
    [SerializeField]
    InputField BPMInputField;
    [SerializeField]
    InputField beatOffsetInputField;

    Subject<Vector3> ScrollPadOnMouseDownStream = new Subject<Vector3>();
    Subject<Vector3> VerticalLineOnMouseDownStream = new Subject<Vector3>();

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
        var unitBeatSamples = new ReactiveProperty<int>();


        // Binds canvas scale factor
        this.UpdateAsObservable()
            .Select(_ => Screen.width)
            .DistinctUntilChanged()
            .Subscribe(w => model.CanvasScaleFactor.Value = canvasScaler.referenceResolution.x / w);


        // Binds division number of measure
        divisionNumOfOneMeasureSlider.OnValueChangedAsObservable()
            .Select(x => Mathf.FloorToInt(x))
            .Subscribe(x => model.DivisionNumOfOneMeasure.Value = x);


        // Apply music data
        audioSource.clip = SelectedMusicDataStore.Instance.audioClip;
        titleText.text = SelectedMusicDataStore.Instance.fileName ?? "Test";


        // Initialize canvas width
        {
            var sizeDelta = canvasRect.sizeDelta;
            sizeDelta.x = audioSource.clip.samples / 100f;
            canvasRect.sizeDelta = sizeDelta;
        }


        // Canvas width scaler Test
        var canvasWidth = _scaleSliderTest.OnValueChangedAsObservable()
            .DistinctUntilChanged()
            .Select(x => audioSource.clip.samples / 100f * x)
            .Do(x => {
                var delta = canvasRect.sizeDelta;
                delta.x = x;
                canvasRect.sizeDelta = delta;
            }).ToReactiveProperty();


        // Binds BPM
        unitBeatSamples = model.BPM.DistinctUntilChanged()
            .Select(x => Mathf.FloorToInt(audioSource.clip.frequency * 60 / x))
            .ToReactiveProperty();

        BPMInputField.OnValueChangeAsObservable()
            .Select(x => string.IsNullOrEmpty(x) ? "1" : x)
            .Select(x => int.Parse(x))
            .Select(x => Mathf.Clamp(x, 1, 320))
            .Subscribe(x => model.BPM.Value = x);

        model.BPM.DistinctUntilChanged()
            .Subscribe(x => BPMInputField.text = x.ToString());


        // Binds beat offset samples
        beatOffsetInputField.OnValueChangeAsObservable()
            .Select(x => string.IsNullOrEmpty(x) ? "0" : x)
            .Select(x => int.Parse(x))
            .Subscribe(x => model.BeatOffsetSamples.Value = x);

        model.BeatOffsetSamples.DistinctUntilChanged()
            .Subscribe(x => beatOffsetInputField.text = x.ToString());


        // Binds canvas position from samples
        this.UpdateAsObservable()
            .Select(_ => audioSource.timeSamples)
            .DistinctUntilChanged()
            .Merge(canvasWidth.Select(_ => audioSource.timeSamples)) // Merge resized timing
            .Select(timeSamples => timeSamples / (float)audioSource.clip.samples)
            .Select(per => canvasRect.sizeDelta.x * per)
            .Select(x => x + model.CanvasOffsetX.Value)
            .Subscribe(x => canvasRect.localPosition = Vector3.left * x);


        // Binds samples from dragging canvas
        var canvasDragStream = this.UpdateAsObservable()
            .SkipUntil(ScrollPadOnMouseDownStream)
            .TakeWhile(_ => !Input.GetMouseButtonUp(0))
            .Select(_ => Mathf.FloorToInt(Input.mousePosition.x));

        canvasDragStream.Zip(canvasDragStream.Skip(1), (p, c) => new { p, c })
            .RepeatSafe()
            .Select(b => (b.p - b.c) / canvasWidth.Value)
            .Select(p => p * model.CanvasScaleFactor.Value)
            .Select(p => Mathf.FloorToInt(audioSource.clip.samples * p))
            .Select(deltaSample => audioSource.timeSamples + deltaSample)
            .Select(x => Mathf.Clamp(x, 0, audioSource.clip.samples - 1))
            .Subscribe(x => audioSource.timeSamples = x);

        var isDraggingDuringPlay = false;
        ScrollPadOnMouseDownStream.Where(_ => model.IsPlaying.Value)
            .Select(_ => model.IsPlaying.Value = false)
            .Subscribe(_ => isDraggingDuringPlay = true);

        this.UpdateAsObservable().Where(_ => isDraggingDuringPlay)
            .Where(_ => Input.GetMouseButtonUp(0))
            .Select(_ => model.IsPlaying.Value = true)
            .Subscribe(_ => isDraggingDuringPlay = false);


        // Binds offset x of canvas
        var verticalLineDragStream = this.UpdateAsObservable()
            .SkipUntil(VerticalLineOnMouseDownStream)
            .TakeWhile(_ => !Input.GetMouseButtonUp(0))
            .Select(_ => Mathf.FloorToInt(Input.mousePosition.x));

        verticalLineDragStream.Zip(verticalLineDragStream.Skip(1), (p, c) => new { p, c })
            .RepeatSafe()
            .Select(b => (b.c - b.p) * model.CanvasScaleFactor.Value)
            .Select(x => x + model.CanvasOffsetX.Value)
            .Select(x => new { x, max = Screen.width * 0.5f * 0.95f * model.CanvasScaleFactor.Value })
            .Select(v => Mathf.Clamp(v.x, -v.max, v.max))
            .Subscribe(x => model.CanvasOffsetX.Value = x);

        model.CanvasOffsetX.DistinctUntilChanged().Subscribe(x => {
            var pos = verticalLineRect.localPosition;
            pos.x = x;
            verticalLineRect.localPosition = pos;
        });


        // Binds play pause toggle
        playButton.OnClickAsObservable()
            .Subscribe(_ => model.IsPlaying.Value = !model.IsPlaying.Value);

        model.IsPlaying.DistinctUntilChanged().Subscribe(playing => {
            var playButtonText = playButton.GetComponentInChildren<Text>();

            if (playing)
            {
                audioSource.Play();
                playButtonText.text = "Pause";
            }
            else
            {
                audioSource.Pause();
                playButtonText.text = "Play";
            }
        });


        // Draw measure lines
        this.UpdateAsObservable()
            .Select(_ => model.DivisionNumOfOneMeasure.Value * Mathf.CeilToInt(audioSource.clip.samples / (float)unitBeatSamples.Value))
            .Select(max => Enumerable.Range(0, max)
                .Select(i => i * unitBeatSamples.Value / (float)audioSource.clip.samples / model.DivisionNumOfOneMeasure.Value)
                .Select(i => i + model.BeatOffsetSamples.Value / (float)audioSource.clip.samples)
                .Select(per => per * canvasWidth.Value)
                .Select(x => x - canvasWidth.Value * (audioSource.timeSamples / (float)audioSource.clip.samples))
                .Select(x => x + model.CanvasOffsetX.Value)
                .Select((x, i) => new Line(new Vector3(x, 250, 0), new Vector3(x, -250, 0), i % model.DivisionNumOfOneMeasure.Value == 0 ? Color.white : Color.white / 2)))
            .Subscribe(lines => drawLineTest.DrawLines("measures", lines.ToArray()));


        // Draw wave
        {
            var waveData = new float[500000];
            var skipSamples = 50;
            var lineColor = Color.green * 0.5f;
            var lines = Enumerable.Range(0, waveData.Length / skipSamples)
                .Select(_ => new Line(Vector3.zero, Vector3.zero, lineColor))
                .ToArray();

            this.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    audioSource.clip.GetData(waveData, audioSource.timeSamples);
                    var x = (canvasWidth.Value / audioSource.clip.samples) / 2f;
                    var offsetX = model.CanvasOffsetX.Value;

                    for (int li = 0, wi = 0, l = waveData.Length; wi < l; li++, wi += skipSamples)
                    {
                        lines[li].start.x = lines[li].end.x = wi * x + offsetX;
                        lines[li].end.y = -(lines[li].start.y = waveData[wi] * 200);
                    }

                    drawLineTest.DrawLines("wave", lines);
                });
        }
    }

    public void ScrollPadOnMouseDown()
    {
        ScrollPadOnMouseDownStream.OnNext(Input.mousePosition);
    }

    public void VerticalLineOnMouseDown()
    {
        VerticalLineOnMouseDownStream.OnNext(Input.mousePosition);
    }

}
