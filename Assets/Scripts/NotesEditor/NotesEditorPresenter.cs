using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;


public class NotesEditorPresenter : MonoBehaviour
{
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

    Subject<Vector3> OnMouseDownStream = new Subject<Vector3>();

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


        // Apply music data
        audioSource.clip = SelectedMusicDataStore.Instance.audioClip;
        titleText.text = SelectedMusicDataStore.Instance.fileName ?? "Test";


        var rectTransform = GetComponent<RectTransform>();

        {   // Initialize canvas width
            var sizeDelta = rectTransform.sizeDelta;
            sizeDelta.x = audioSource.clip.samples / 1000f;
            rectTransform.sizeDelta = sizeDelta;
        }


        _scaleSliderTest.OnValueChangedAsObservable()
            .DistinctUntilChanged()
            .Select(x => audioSource.clip.samples / 1000f * x)
            .Subscribe(x => {
                var delta = rectTransform.sizeDelta;
                delta.x = x;
                rectTransform.sizeDelta = delta;
            });


        // Resized canvas stream
        var canvasResizedStream = rectTransform.ObserveEveryValueChanged(t => t.sizeDelta.x);
        var canvasScreenWidth = canvasResizedStream
            .ToReactiveProperty();


        // Binds canvas position from samples
        this.UpdateAsObservable()
            .Select(_ => audioSource.timeSamples)
            .DistinctUntilChanged()
            .Merge(canvasResizedStream.Select(_ => audioSource.timeSamples))
            .Select(timeSamples => timeSamples / (float)audioSource.clip.samples)
            .Select(per => rectTransform.sizeDelta.x * per)
            .Subscribe(x => rectTransform.localPosition = Vector3.left * x);


        // Binds samples from dragging canvas
        var canvasDragStream = this.UpdateAsObservable()
            .SkipUntil(OnMouseDownStream)
            .TakeWhile(_ => !Input.GetMouseButtonUp(0))
            .Select(_ => Mathf.FloorToInt(Input.mousePosition.x));

        canvasDragStream.Zip(canvasDragStream.Skip(1), (p, c) => new { p, c })
            .RepeatSafe()
            .Select(b => (b.p - b.c) / canvasScreenWidth.Value)
            .Select(p => Mathf.FloorToInt(audioSource.clip.samples * p))
            .Select(deltaSample => audioSource.timeSamples + deltaSample)
            .Select(x => Mathf.Clamp(x, 0, audioSource.clip.samples - 1))
            .Subscribe(x => audioSource.timeSamples = x);

        var isPlaying = false;
        OnMouseDownStream.Where(_ => model.IsPlaying.Value)
            .Do(_ => model.IsPlaying.Value = false)
            .Subscribe(_ => isPlaying = true);

        this.UpdateAsObservable()
            .Where(_ => isPlaying)
            .Where(_ => Input.GetMouseButtonUp(0))
            .Do(_ => model.IsPlaying.Value = true)
            .Subscribe(_ => isPlaying = false);


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


        // Draw lines
        this.UpdateAsObservable()
            .Select(_ => Enumerable.Range(0, audioSource.clip.samples / audioSource.clip.frequency)
                .Select(i => i * audioSource.clip.frequency / (float)audioSource.clip.samples)
                .Select(per => per * canvasScreenWidth.Value)
                .Select(x => x - canvasScreenWidth.Value * (audioSource.timeSamples / (float)audioSource.clip.samples))
                .Select(x => new Line(new Vector3(x, 200, 0), new Vector3(x, -200, 0), Color.white)))
            .Subscribe(lines => drawLineTest.DrawLines(lines.ToArray()));

        OnMouseDownStream
            .Subscribe(_ => Debug.Log(_));
    }

    public void OnMouseDown()
    {
        OnMouseDownStream.OnNext(Input.mousePosition);
    }
}
