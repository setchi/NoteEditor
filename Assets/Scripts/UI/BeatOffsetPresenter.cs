using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class BeatOffsetPresenter : MonoBehaviour
{
    [SerializeField]
    InputField beatOffsetInputField;

    Subject<int> ChangeButtosOnMouseUpObservable = new Subject<int>();
    Subject<int> ChangeButtonsOnMouseDownObservable = new Subject<int>();

    void Awake()
    {
        var model = NotesEditorModel.Instance;

        var buttonOperateObservable = Observable.Merge(
                ChangeButtonsOnMouseDownObservable,
                ChangeButtosOnMouseUpObservable)
            .Throttle(TimeSpan.FromMilliseconds(350))
            .Where(delta => delta != 0)
            .SelectMany(delta => Observable.Interval(TimeSpan.FromMilliseconds(50))
                .TakeUntil(ChangeButtosOnMouseUpObservable)
                .Select(_ => delta))
            .Merge(ChangeButtonsOnMouseDownObservable)
            .Select(delta => model.BeatOffsetSamples.Value + delta);

        beatOffsetInputField.OnValueChangeAsObservable()
            .Select(x => string.IsNullOrEmpty(x) ? "0" : x)
            .Select(x => int.Parse(x))
            .Merge(buttonOperateObservable)
            .Select(x => Mathf.Clamp(x, 0, int.MaxValue))
            .Subscribe(x => model.BeatOffsetSamples.Value = x);

        model.BeatOffsetSamples.DistinctUntilChanged()
            .Subscribe(x => beatOffsetInputField.text = x.ToString());
    }

    public void IncreaseButtonOnMouseDown() { ChangeButtonsOnMouseDownObservable.OnNext(100); }
    public void IncreaseUpButtonOnMouseUp() { ChangeButtosOnMouseUpObservable.OnNext(0); }
    public void DecreaseButtonOnMouseDown() { ChangeButtonsOnMouseDownObservable.OnNext(-100); }
    public void DecreaseDownButtonOnMouseUp() { ChangeButtosOnMouseUpObservable.OnNext(0); }
}
