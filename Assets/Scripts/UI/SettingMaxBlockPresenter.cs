using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class SettingMaxBlockPresenter : MonoBehaviour
{
    [SerializeField]
    Text MaxBlockDisplaytext;

    Subject<int> ChangeButtonsOnMouseUpObservable = new Subject<int>();
    Subject<int> ChangeButtonsOnMouseDownObservable = new Subject<int>();

    void Awake()
    {
        var model = NotesEditorSettingsModel.Instance;

        model.MaxBlock.DistinctUntilChanged().SubscribeToText(MaxBlockDisplaytext);

        Observable.Merge(
                ChangeButtonsOnMouseDownObservable,
                ChangeButtonsOnMouseUpObservable)
            .Throttle(TimeSpan.FromMilliseconds(350))
            .Where(delta => delta != 0)
            .SelectMany(delta => Observable.Interval(TimeSpan.FromMilliseconds(50))
                .TakeUntil(ChangeButtonsOnMouseUpObservable)
                .Select(_ => delta))
            .Merge(ChangeButtonsOnMouseDownObservable)
            .Select(delta => model.MaxBlock.Value + delta)
            .Select(maxBlock => Mathf.Clamp(maxBlock, 1, 200))
            .Subscribe(maxBlock => model.MaxBlock.Value = maxBlock);
    }

    public void IncreaseButtonOnMouseDown() { ChangeButtonsOnMouseDownObservable.OnNext(1); }
    public void IncreaseUpButtonOnMouseUp() { ChangeButtonsOnMouseUpObservable.OnNext(0); }
    public void DecreaseButtonOnMouseDown() { ChangeButtonsOnMouseDownObservable.OnNext(-1); }
    public void DecreaseDownButtonOnMouseUp() { ChangeButtonsOnMouseUpObservable.OnNext(0); }
}
