using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class CanvasWidthScalePresenter : MonoBehaviour
{
    [SerializeField]
    CanvasEvents canvasEvents;
    [SerializeField]
    Slider canvasWidthScaleController;

    NotesEditorModel model;

    void Awake()
    {
        model = NotesEditorModel.Instance;
        model.OnLoadedMusicObservable.First().Subscribe(_ => Init());
    }

    void Init()
    {
        model.CanvasWidth = canvasEvents.MouseScrollWheelObservable
            .Where(_ => KeyInput.CtrlKey())
            .Merge(this.UpdateAsObservable().Where(_ => Input.GetKey(KeyCode.UpArrow)).Select(_ => 0.1f))
            .Merge(this.UpdateAsObservable().Where(_ => Input.GetKey(KeyCode.DownArrow)).Select(_ => -0.1f))
            .Select(delta => model.CanvasWidth.Value * (1 + delta))
            .Select(x => x / (model.Audio.clip.samples / 100f))
            .Select(x => Mathf.Clamp(x, 0.1f, 2f))
            .Merge(canvasWidthScaleController.OnValueChangedAsObservable()
                .DistinctUntilChanged())
            .Select(x => model.Audio.clip.samples / 100f * x)
            .ToReactiveProperty();
    }
}
