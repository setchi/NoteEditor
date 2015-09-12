using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EditMarkerHandlerPresenter : MonoBehaviour
{
    [SerializeField]
    Image handler;
    [SerializeField]
    RectTransform lineRect;

    NotesEditorModel model;
    ReactiveProperty<int> currentSamples = new ReactiveProperty<int>(0);
    ReactiveProperty<float> position_ = new ReactiveProperty<float>();

    public ReactiveProperty<float> Position { get { return position_; } }

    void Start()
    {
        model = NotesEditorModel.Instance;
        model.OnLoadMusicObservable.Subscribe(_ => Init());

        position_ = lineRect.ObserveEveryValueChanged(rect => rect.localPosition.x).ToReactiveProperty();
    }

    void Init()
    {
        var handlerOnMouseDownObservable = new Subject<Vector3>();

        handler.AddListener(
            EventTriggerType.PointerDown,
            (e) => {
                handlerOnMouseDownObservable.OnNext(Vector3.right * model.SamplesToScreenPositionX(currentSamples.Value));
            });

        var operateXObservable = this.UpdateAsObservable()
            .SkipUntil(handlerOnMouseDownObservable)
            .TakeWhile(_ => !Input.GetMouseButtonUp(0))
            .RepeatSafe()
            .Select(_ => model.ScreenToCanvasPosition(Input.mousePosition))
            .Select(canvasPos => model.ScreenPositionXToSamples(canvasPos.x))
            .Select(samples => Mathf.Clamp(samples, 0, model.Audio.clip.samples))
            .DistinctUntilChanged();

        operateXObservable.Subscribe(samples => currentSamples.Value = samples);

        operateXObservable.Buffer(this.UpdateAsObservable().Where(_ => Input.GetMouseButtonUp(0)))
            .Where(b => 2 <= b.Count)
            .Select(x => new { current = x.Last(), prev = x.First() })
            .Subscribe(x => UndoRedoManager.Do(
                new Command(
                    () => currentSamples.Value = x.current,
                    () => currentSamples.Value = x.prev)));

        Observable.Merge(
                currentSamples.Select(_ => Unit.Default),
                model.CanvasOffsetX.Select(_ => Unit.Default),
                model.TimeSamples.Select(_ => Unit.Default),
                model.CanvasWidth.Select(_ => Unit.Default),
                model.BeatOffsetSamples.Select(_ => Unit.Default))
            .Select(_ => currentSamples.Value)
            .Subscribe(x =>
            {
                var pos = lineRect.localPosition;
                pos.x = model.SamplesToScreenPositionX(x);
                lineRect.localPosition = pos;
            });
    }
}
