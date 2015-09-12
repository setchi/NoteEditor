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

    ReactiveProperty<float> position = new ReactiveProperty<float>(0);
    NotesEditorModel model;

    void Start()
    {
        model = NotesEditorModel.Instance;
        model.OnLoadMusicObservable.Subscribe(_ => Init());
    }

    void Init()
    {
        var handlerOnMouseDownObservable = new Subject<Vector3>();
        handler.AddListener(EventTriggerType.PointerDown, (e) => handlerOnMouseDownObservable.OnNext(Input.mousePosition));

        var operateXObservable = this.UpdateAsObservable()
            .SkipUntil(handlerOnMouseDownObservable)
            .TakeWhile(_ => !Input.GetMouseButtonUp(0))
            .Select(_ => Input.mousePosition.x)
            .Buffer(2, 1).Where(b => 2 <= b.Count)
            .RepeatSafe()
            .Select(b => (b[1] - b[0]) * model.CanvasScaleFactor.Value)
            .Select(x => x + position.Value)
            .DistinctUntilChanged();

        operateXObservable.Subscribe(canvasPos => position.Value = canvasPos);

        operateXObservable.Buffer(this.UpdateAsObservable().Where(_ => Input.GetMouseButtonUp(0)))
            .Where(b => 2 <= b.Count)
            .Select(x => new { current = x.Last(), prev = x.First() })
            .Subscribe(x => UndoRedoManager.Do(
                new Command(
                    () => position.Value = x.current,
                    () => position.Value = x.prev)));

        Observable.Merge(
                position,
                model.CanvasOffsetX.Select(_ => position.Value),
                model.TimeSamples.Select(_ => position.Value))
            .Subscribe(x =>
            {
                var pos = lineRect.localPosition;
                pos.x = model.SamplesToScreenPositionX(0) + x;
                lineRect.localPosition = pos;
            });
    }
}
