using NoteEditor.Common;
using NoteEditor.UI.Model;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NoteEditor.UI.Presenter
{
    public class EditSectionHandlePresenter : MonoBehaviour
    {
        [SerializeField]
        Image handleImage;
        [SerializeField]
        RectTransform lineRectTransform;

        NoteEditorModel model;
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
            model = NoteEditorModel.Instance;
            model.OnLoadMusicObservable.First().Subscribe(_ => Init());

            position_ = lineRectTransform.ObserveEveryValueChanged(rect => rect.localPosition.x).ToReactiveProperty();
        }

        void Init()
        {
            var handlerOnMouseDownObservable = new Subject<Vector3>();

            handleImage.AddListener(
                EventTriggerType.PointerDown,
                (e) =>
                {
                    handlerOnMouseDownObservable.OnNext(Vector3.right * ConvertUtils.SamplesToCanvasPositionX(CurrentSamples.Value));
                });

            var operateHandleObservable = this.UpdateAsObservable()
                .SkipUntil(handlerOnMouseDownObservable)
                .TakeWhile(_ => !Input.GetMouseButtonUp(0))
                .RepeatSafe()
                .Select(_ => ConvertUtils.ScreenToCanvasPosition(Input.mousePosition))
                .Select(canvasPos => ConvertUtils.CanvasPositionXToSamples(canvasPos.x))
                .Select(samples => Mathf.Clamp(samples, 0, model.Audio.clip.samples))
                .DistinctUntilChanged();

            operateHandleObservable.Subscribe(samples => CurrentSamples.Value = samples);

            operateHandleObservable.Buffer(this.UpdateAsObservable().Where(_ => Input.GetMouseButtonUp(0)))
                .Where(b => 2 <= b.Count)
                .Select(x => new { current = x.Last(), prev = x.First() })
                .Subscribe(x => UndoRedoManager.Do(
                    new Command(
                        () => CurrentSamples.Value = x.current,
                        () => CurrentSamples.Value = x.prev)));

            Observable.Merge(
                    CurrentSamples.Select(_ => Unit.Default),
                    model.CanvasOffsetX.Select(_ => Unit.Default),
                    model.SmoothedTimeSamples.Select(_ => Unit.Default),
                    model.CanvasWidth.Select(_ => Unit.Default),
                    model.BeatOffsetSamples.Select(_ => Unit.Default))
                .Select(_ => CurrentSamples.Value)
                .Subscribe(x =>
                {
                    var pos = lineRectTransform.localPosition;
                    pos.x = ConvertUtils.SamplesToCanvasPositionX(x);
                    lineRectTransform.localPosition = pos;
                });
        }
    }
}
