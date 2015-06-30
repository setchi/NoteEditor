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

        {   // ScaleX initialize
            var scale = transform.localScale;
            scale.x = audioSource.clip.samples / audioSource.clip.frequency;
            transform.localScale = scale;
        }


        // Resized canvas stream
        var canvasResizedStream = transform.ObserveEveryValueChanged(t => t.localScale);
        var canvasScreenWidth = canvasResizedStream
            .Select(scale => Camera.main.WorldToScreenPoint(Vector3.right * scale.x).x - Camera.main.WorldToScreenPoint(Vector3.zero).x)
            .ToReactiveProperty();


        // Binds canvas position from samples
        this.UpdateAsObservable()
            .Select(_ => audioSource.timeSamples)
            .DistinctUntilChanged() // Add merge resizing?
            .Select(x => x / (float)audioSource.clip.samples)
            .Subscribe(x => transform.position = Vector3.left * transform.localScale.x * x);


        // Binds samples from dragging canvas
        var canvasDragStream = this.UpdateAsObservable()
            .SkipUntil(this.OnMouseDownAsObservable())
            .TakeWhile(_ => !Input.GetMouseButtonUp(0))
            .Select(_ => Mathf.FloorToInt(Input.mousePosition.x));

        canvasDragStream.Zip(canvasDragStream.Skip(1), (p, c) => new { p, c })
            .RepeatSafe()
            .Select(b => (b.p - b.c) / canvasScreenWidth.Value)
            .Select(p => Mathf.FloorToInt(audioSource.clip.samples * p))
            .Select(deltaSample => audioSource.timeSamples + deltaSample)
            .Select(x => Mathf.Clamp(x, 0, audioSource.clip.samples - 1))
            .Subscribe(x => audioSource.timeSamples = x);

        this.UpdateAsObservable()
            .CombineLatest(this.OnMouseDownAsObservable().Select(_ => model.IsPlaying.Value), (_, p) => p)
            .Where(p => p)
            .Do(_ => model.IsPlaying.Value = false)
            .Where(_ => Input.GetMouseButtonUp(0))
            .Subscribe(_ => model.IsPlaying.Value = true);


        // Binds play pause toggle
        var playToggleStream = playButton.OnClickAsObservable()
            .Select(_ => false)
            .Scan((p, c) => !p)
            .Subscribe(playing =>
            {
                var playButtonText = playButton.GetComponentInChildren<Text>();

                if (playing)
                {
                    audioSource.Pause();
                    playButtonText.text = "Play";

                }
                else
                {
                    audioSource.Play();
                    playButtonText.text = "Pause";
                }
            });


        // Draw lines
        this.UpdateAsObservable()
            .Select(_ => Enumerable.Range(1, audioSource.clip.samples / audioSource.clip.frequency))
            .Select(x => x.Select(i => i * audioSource.clip.frequency / (float)audioSource.clip.samples))
            .Select(x => x.Select(i => new Line(
                new Vector3(canvasScreenWidth.Value * i, 100, 0),
                new Vector3(canvasScreenWidth.Value * i, -100, 0),
                Color.white)))
            .Subscribe(x => drawLineTest.DrawLines(x.ToArray()));
    }
}
