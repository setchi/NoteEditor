using NoteEditor.GLDrawing;
using NoteEditor.Model;
using NoteEditor.Utility;
using System.Linq;
using UniRx;
using UnityEngine;

namespace NoteEditor.Presenter
{
    public class EditSectionPresenter : MonoBehaviour
    {
        [SerializeField]
        RectTransform markerRect;
        [SerializeField]
        EditSectionHandlePresenter point1;
        [SerializeField]
        EditSectionHandlePresenter point2;
        [SerializeField]
        RectTransform playbackPositionSliderRectTransform;
        [SerializeField]
        RectTransform sliderMarker;
        [SerializeField]
        Color markerColor;

        Geometry drawData = new Geometry(Enumerable.Range(0, 4).Select(_ => Vector3.zero).ToArray(), Color.clear);

        void Awake()
        {
            Audio.OnLoad.First().Subscribe(_ => Init());
        }

        void Init()
        {

            var sliderWidth = playbackPositionSliderRectTransform.sizeDelta.x;

            Observable.Merge(
                    point1.Position,
                    point2.Position)
                .Subscribe(_ =>
                {
                    var sortedPoints = new[] { point1, point2 }.OrderBy(p => p.Position.Value);
                    var start = sortedPoints.First();
                    var end = sortedPoints.Last();

                    var scale = start.HandleRectTransform.localScale;
                    scale.x = -1;
                    start.HandleRectTransform.localScale = scale;
                    var scale1 = end.HandleRectTransform.localScale;
                    scale1.x = 1;
                    end.HandleRectTransform.localScale = scale1;

                    var markerCanvasWidth = end.Position.Value - start.Position.Value;
                    var startPos = start.Position.Value / NoteCanvas.ScaleFactor.Value + Screen.width / 2f;
                    var halfScreenHeight = Screen.height / 2f;
                    var halfHeight = markerRect.sizeDelta.y / NoteCanvas.ScaleFactor.Value / 2;

                    var min = new Vector2(startPos, halfScreenHeight - halfHeight);
                    var max = new Vector2(startPos + markerCanvasWidth / NoteCanvas.ScaleFactor.Value, halfScreenHeight + halfHeight);

                    drawData = new Geometry(
                        new[] {
                            new Vector3(min.x, max.y, 0),
                            new Vector3(max.x, max.y, 0),
                            new Vector3(max.x, min.y, 0),
                            new Vector3(min.x, min.y, 0)
                        },
                        markerColor);

                    var sliderMarkerSize = sliderMarker.sizeDelta;
                    sliderMarkerSize.x = sliderWidth * markerCanvasWidth / NoteCanvas.Width.Value;
                    sliderMarker.sizeDelta = sliderMarkerSize;

                    if (NoteCanvas.Width.Value > 0)
                    {
                        var startPer = (start.Position.Value - ConvertUtils.SamplesToCanvasPositionX(0)) / NoteCanvas.Width.Value;
                        var sliderMarkerPos = sliderMarker.localPosition;
                        sliderMarkerPos.x = sliderWidth * startPer - sliderWidth / 2f;
                        sliderMarker.localPosition = sliderMarkerPos;
                    }
                });
        }

        void LateUpdate()
        {
            GLQuadDrawer.Draw(drawData);
        }
    }
}
