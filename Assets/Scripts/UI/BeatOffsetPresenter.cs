using System;
using System.Text.RegularExpressions;
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

        var isUndoRedoAction = false;

        beatOffsetInputField.OnValueChangeAsObservable()
            .Where(x => Regex.IsMatch(x, @"^[0-9]+$"))
            .Select(x => int.Parse(x))
            .Merge(buttonOperateObservable)
            .Select(x => Mathf.Clamp(x, 0, int.MaxValue))
            .DistinctUntilChanged()
            .Where(_ => isUndoRedoAction ? (isUndoRedoAction = false) : true)
            .Select(x => new { current = x, prev = model.BeatOffsetSamples.Value })
            .Subscribe(x => UndoRedoManager.Do(
                new Command(
                    () => model.BeatOffsetSamples.Value = x.current,
                    () => { isUndoRedoAction = true; model.BeatOffsetSamples.Value = x.prev; },
                    () => { isUndoRedoAction = true; model.BeatOffsetSamples.Value = x.current; })));

        model.BeatOffsetSamples.DistinctUntilChanged()
            .Subscribe(x => beatOffsetInputField.text = x.ToString());
    }

    public void IncreaseButtonOnMouseDown() { ChangeButtonsOnMouseDownObservable.OnNext(100); }
    public void IncreaseButtonOnMouseUp() { ChangeButtosOnMouseUpObservable.OnNext(0); }
    public void DecreaseButtonOnMouseDown() { ChangeButtonsOnMouseDownObservable.OnNext(-100); }
    public void DecreaseButtonOnMouseUp() { ChangeButtosOnMouseUpObservable.OnNext(0); }
}
