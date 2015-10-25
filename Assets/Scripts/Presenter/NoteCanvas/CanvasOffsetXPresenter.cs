using NoteEditor.Common;
using NoteEditor.Model;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace NoteEditor.Presenter
{
    public class CanvasOffsetXPresenter : MonoBehaviour
    {
        [SerializeField]
        CanvasEvents canvasEvents;
        [SerializeField]
        RectTransform verticalLineRect;
        [SerializeField]
        RectTransform waveformRenderImage;

        void Awake()
        {
            // Initialize canvas offset x
            Audio.OnLoad.Subscribe(_ => NoteCanvas.OffsetX.Value = -Screen.width * 0.45f * NoteCanvas.ScaleFactor.Value);

            var operateCanvasOffsetXObservable = this.UpdateAsObservable()
                .SkipUntil(canvasEvents.VerticalLineOnMouseDownObservable)
                .TakeWhile(_ => !Input.GetMouseButtonUp(0))
                .Select(_ => Input.mousePosition.x)
                .Buffer(2, 1).Where(b => 2 <= b.Count)
                .RepeatSafe()
                .Select(b => (b[1] - b[0]) * NoteCanvas.ScaleFactor.Value)
                .Select(x => x + NoteCanvas.OffsetX.Value)
                .Select(x => new { x, max = Screen.width * 0.5f * 0.95f * NoteCanvas.ScaleFactor.Value })
                .Select(v => Mathf.Clamp(v.x, -v.max, v.max))
                .DistinctUntilChanged();

            operateCanvasOffsetXObservable.Subscribe(x => NoteCanvas.OffsetX.Value = x);

            operateCanvasOffsetXObservable.Buffer(this.UpdateAsObservable().Where(_ => Input.GetMouseButtonUp(0)))
                .Where(b => 2 <= b.Count)
                .Select(x => new { current = x.Last(), prev = x.First() })
                .Subscribe(x => EditCommandManager.Do(
                    new Command(
                        () => NoteCanvas.OffsetX.Value = x.current,
                        () => NoteCanvas.OffsetX.Value = x.prev)));

            NoteCanvas.OffsetX.Subscribe(x =>
            {
                var pos = verticalLineRect.localPosition;
                var pos2 = waveformRenderImage.localPosition;
                pos.x = pos2.x = x;
                verticalLineRect.localPosition = pos;
                waveformRenderImage.localPosition = pos2;
            });
        }
    }
}
