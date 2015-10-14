using NoteEditor.Common;
using NoteEditor.Model;
using NoteEditor.Utility;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.Presenter
{
    public class CanvasWidthScalePresenter : MonoBehaviour
    {
        [SerializeField]
        CanvasEvents canvasEvents;
        [SerializeField]
        Slider canvasWidthScaleController;

        void Awake()
        {
            Audio.OnLoad.First().Subscribe(_ => Init());
        }

        void Init()
        {
            var operateCanvasScaleObservable = canvasEvents.MouseScrollWheelObservable
                .Where(_ => KeyInput.CtrlKey())
                .Merge(this.UpdateAsObservable().Where(_ => Input.GetKey(KeyCode.UpArrow)).Select(_ => 0.05f))
                .Merge(this.UpdateAsObservable().Where(_ => Input.GetKey(KeyCode.DownArrow)).Select(_ => -0.05f))
                .Select(delta => NoteCanvas.Width.Value * (1 + delta))
                .Select(x => x / (Audio.Source.clip.samples / 100f))
                .Select(x => Mathf.Clamp(x, 0.1f, 2f))
                .Merge(canvasWidthScaleController.OnValueChangedAsObservable()
                    .DistinctUntilChanged())
                .DistinctUntilChanged()
                .Select(x => Audio.Source.clip.samples / 100f * x);

            operateCanvasScaleObservable.Subscribe(x => NoteCanvas.Width.Value = x);

            operateCanvasScaleObservable.Buffer(operateCanvasScaleObservable.ThrottleFrame(2))
                .Where(b => 2 <= b.Count)
                .Select(x => new { current = x.Last(), prev = x.First() })
                .Subscribe(x => EditCommandManager.Do(
                    new Command(
                        () => NoteCanvas.Width.Value = x.current,
                        () => NoteCanvas.Width.Value = x.prev)));
        }
    }
}
