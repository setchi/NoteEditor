using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class LPBPresenter : MonoBehaviour
{
    [SerializeField]
    Text LPBDisplayText;

    Subject<int> ButtonsOnMouseUpObservable = new Subject<int>();
    Subject<int> ButtonsOnMouseDownObservable = new Subject<int>();

    void Awake()
    {
        var model = NotesEditorModel.Instance;

        model.LPB.SubscribeToText(LPBDisplayText);

        Observable.Merge(
                ButtonsOnMouseDownObservable,
                ButtonsOnMouseUpObservable)
            .Throttle(TimeSpan.FromMilliseconds(350))
            .Where(delta => delta != 0)
            .SelectMany(delta => Observable.Interval(TimeSpan.FromMilliseconds(50))
                .TakeUntil(ButtonsOnMouseUpObservable)
                .Select(_ => delta))
            .Merge(ButtonsOnMouseDownObservable)
            .Select(delta => model.LPB.Value + delta)
            .Select(LPB => Mathf.Clamp(LPB, 2, 32))
            .DistinctUntilChanged()
            .Select(x => new { current = x, prev = model.LPB.Value })
            .Subscribe(x => UndoRedoManager.Do(
                new Command(
                    () => model.LPB.Value = x.current,
                    () => model.LPB.Value = x.prev)));
    }

    public void IncreaseButtonOnMouseDown() { ButtonsOnMouseDownObservable.OnNext(1); }
    public void IncreaseButtonOnMouseUp() { ButtonsOnMouseUpObservable.OnNext(0); }
    public void DecreaseButtonOnMouseDown() { ButtonsOnMouseDownObservable.OnNext(-1); }
    public void DecreaseButtonOnMouseUp() { ButtonsOnMouseUpObservable.OnNext(0); }
}
