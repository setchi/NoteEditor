using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CanvasWidthScalePresenter : MonoBehaviour
{
    [SerializeField]
    CanvasEvents canvasEvents;
    [SerializeField]
    Slider canvasWidthScaleController;
    [SerializeField]
    RectTransform canvasRect;

    NotesEditorModel model;

    void Awake()
    {
        model = NotesEditorModel.Instance;
        model.OnLoadedMusicObservable.Subscribe(_ => Init());
    }

    void Init()
    {
        model.CanvasWidth = canvasEvents.MouseScrollWheelObservable
            .Where(_ => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            .Select(delta => model.CanvasWidth.Value * (1 + delta))
            .Select(x => x / (model.Audio.clip.samples / 100f))
            .Select(x => Mathf.Clamp(x, 0.1f, 2f))
            .Merge(canvasWidthScaleController.OnValueChangedAsObservable()
                .DistinctUntilChanged())
            .Select(x => model.Audio.clip.samples / 100f * x)
            .ToReactiveProperty();

        model.CanvasWidth.DistinctUntilChanged()
            .Do(x => canvasWidthScaleController.value = x / (model.Audio.clip.samples / 100f))
            .Subscribe(x =>
            {
                var delta = canvasRect.sizeDelta;
                delta.x = x;
                canvasRect.sizeDelta = delta;
            });
    }
}
