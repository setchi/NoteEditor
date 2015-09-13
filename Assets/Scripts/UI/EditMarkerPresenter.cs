using System.Linq;
using UniRx;
using UnityEngine;

public class EditMarkerPresenter : MonoBehaviour
{
    [SerializeField]
    RectTransform markerRect;
    [SerializeField]
    EditMarkerHandlerPresenter point1;
    [SerializeField]
    EditMarkerHandlerPresenter point2;
    [SerializeField]
    RectTransform playbackPositionSliderRectTransform;
    [SerializeField]
    RectTransform sliderMarker;
    [SerializeField]
    Color markerColor;

    NotesEditorModel model;

    void Awake()
    {
        model = NotesEditorModel.Instance;
        model.OnLoadMusicObservable.Subscribe(_ => Init());
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
                var startPos = start.Position.Value / model.CanvasScaleFactor.Value + Screen.width / 2f;
                var halfScreenHeight = Screen.height / 2f;
                var halfHeight = markerRect.sizeDelta.y / model.CanvasScaleFactor.Value / 2;

                GLRectRenderer.Render(
                    "EditMarker",
                    new[] { new ColoringRect(
                        new Vector2(startPos, halfScreenHeight - halfHeight),
                        new Vector2(startPos + markerCanvasWidth / model.CanvasScaleFactor.Value, halfScreenHeight + halfHeight),
                        markerColor) });

                var sliderMarkerSize = sliderMarker.sizeDelta;
                sliderMarkerSize.x = sliderWidth * markerCanvasWidth / model.CanvasWidth.Value;
                sliderMarker.sizeDelta = sliderMarkerSize;

                if (model.CanvasWidth.Value > 0)
                {
                    var startPer = (start.Position.Value - model.SamplesToCanvasPositionX(0)) / model.CanvasWidth.Value;
                    var sliderMarkerPos = sliderMarker.localPosition;
                    sliderMarkerPos.x = sliderWidth * startPer - sliderWidth / 2f;
                    sliderMarker.localPosition = sliderMarkerPos;
                }
            });
    }
}
