using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EditMarkerHandlePresenter : MonoBehaviour
{
    [SerializeField]
    Image handleImage;
    [SerializeField]
    RectTransform lineRectTransform;

    NotesEditorModel model;
    ReactiveProperty<int> CurrentSamples = new ReactiveProperty<int>(0);
    ReactiveProperty<float> position_ = new ReactiveProperty<float>();

    public ReactiveProperty<float> Position
    {
        get { return position_; }
    }
    public RectTransform HandleRectTransform
    {
        get { return handleRectTransform_ ?? (handleRectTransform_ = handleImage.GetComponent<RectTransform>()); }
    }
    RectTransform handleRectTransform_;

    void Start()
    {
        model = NotesEditorModel.Instance;
        model.OnLoadMusicObservable.Subscribe(_ => Init());

        position_ = lineRectTransform.ObserveEveryValueChanged(rect => rect.localPosition.x).ToReactiveProperty();
    }

    void Init()
    {
        var handlerOnMouseDownObservable = new Subject<Vector3>();

        handleImage.AddListener(
            EventTriggerType.PointerDown,
            (e) => {
                handlerOnMouseDownObservable.OnNext(Vector3.right * model.SamplesToCanvasPositionX(CurrentSamples.Value));
            });

        var operateXObservable = this.UpdateAsObservable()
            .SkipUntil(handlerOnMouseDownObservable)
            .TakeWhile(_ => !Input.GetMouseButtonUp(0))
            .RepeatSafe()
            .Select(_ => model.ScreenToCanvasPosition(Input.mousePosition))
            .Select(canvasPos => model.CanvasPositionXToSamples(canvasPos.x))
            .Select(samples => Mathf.Clamp(samples, 0, model.Audio.clip.samples))
            .DistinctUntilChanged();

        operateXObservable.Subscribe(samples => CurrentSamples.Value = samples);

        operateXObservable.Buffer(this.UpdateAsObservable().Where(_ => Input.GetMouseButtonUp(0)))
            .Where(b => 2 <= b.Count)
            .Select(x => new { current = x.Last(), prev = x.First() })
            .Subscribe(x => UndoRedoManager.Do(
                new Command(
                    () => CurrentSamples.Value = x.current,
                    () => CurrentSamples.Value = x.prev)));

        Observable.Merge(
                CurrentSamples.Select(_ => Unit.Default),
                model.CanvasOffsetX.Select(_ => Unit.Default),
                model.TimeSamples.Select(_ => Unit.Default),
                model.CanvasWidth.Select(_ => Unit.Default),
                model.BeatOffsetSamples.Select(_ => Unit.Default))
            .Select(_ => CurrentSamples.Value)
            .Subscribe(x =>
            {
                var pos = lineRectTransform.localPosition;
                pos.x = model.SamplesToCanvasPositionX(x);
                lineRectTransform.localPosition = pos;
            });
    }
}
