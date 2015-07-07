using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class BPMPresenter : MonoBehaviour
{
    [SerializeField]
    InputField BPMInputField;

    NotesEditorModel model;
    Subject<int> ChangeButtonsOnMouseUpObservable = new Subject<int>();
    Subject<int> ChangeButtonsOnMouseDownObservable = new Subject<int>();

    void Awake()
    {
        model = NotesEditorModel.Instance;
        model.OnLoadedMusicObservable.First().Subscribe(_ => Init());
    }

    void Init()
    {
        model.UnitBeatSamples = model.BPM.DistinctUntilChanged()
            .Select(x => Mathf.FloorToInt(model.Audio.clip.frequency * 60f / x))
            .ToReactiveProperty();

        var buttonOperateObservable = Observable.Merge(
                ChangeButtonsOnMouseDownObservable,
                ChangeButtonsOnMouseUpObservable)
            .Throttle(TimeSpan.FromMilliseconds(350))
            .Where(delta => delta != 0)
            .SelectMany(delta => Observable.Interval(TimeSpan.FromMilliseconds(50))
                .TakeUntil(ChangeButtonsOnMouseUpObservable)
                .Select(_ => delta))
            .Merge(ChangeButtonsOnMouseDownObservable)
            .Select(delta => model.BPM.Value + delta);

        BPMInputField.OnValueChangeAsObservable()
            .Select(x => string.IsNullOrEmpty(x) ? "1" : x)
            .Select(x => int.Parse(x))
            .Merge(buttonOperateObservable)
            .Select(x => Mathf.Clamp(x, 1, 320))
            .Subscribe(x => model.BPM.Value = x);

        model.BPM.DistinctUntilChanged()
            .Subscribe(x => BPMInputField.text = x.ToString());
    }

    public void UpButtonOnMouseDown() { ChangeButtonsOnMouseDownObservable.OnNext(1); }
    public void UpButtonOnMouseUp() { ChangeButtonsOnMouseUpObservable.OnNext(0); }
    public void DownButtonOnMouseDown() { ChangeButtonsOnMouseDownObservable.OnNext(-1); }
    public void DownButtonOnMouseUp() { ChangeButtonsOnMouseUpObservable.OnNext(0); }
}
