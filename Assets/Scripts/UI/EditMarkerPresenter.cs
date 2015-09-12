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

    NotesEditorModel model;

    void Awake()
    {
        model = NotesEditorModel.Instance;
        model.OnLoadMusicObservable.Subscribe(_ => Init());
    }

    void Init()
    {
        Observable.Merge(
                point1.Position,
                point2.Position)
            .Subscribe(p =>
            {
                var start = Mathf.Min(point1.Position.Value, point2.Position.Value) / model.CanvasScaleFactor.Value;
                var end = Mathf.Max(point1.Position.Value, point2.Position.Value) / model.CanvasScaleFactor.Value;
                var width = end - start;
                var startPos = start + Screen.width / 2f;
                var halfScreenHeight = Screen.height / 2f;
                var halfHeight = markerRect.sizeDelta.y / model.CanvasScaleFactor.Value / 2;

                GLRectRenderer.Render(
                    "EditMarker",
                    new[] { new ColoringRect(
                        new Vector2(startPos, halfScreenHeight - halfHeight),
                        new Vector2(startPos + width, halfScreenHeight + halfHeight),
                        new Color(1, 1, 1, 0.1f)) });
            });
    }
}
