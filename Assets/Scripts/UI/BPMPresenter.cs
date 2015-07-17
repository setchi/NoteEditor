using System;
using System.Text.RegularExpressions;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class BPMPresenter : MonoBehaviour
{
    [SerializeField]
    InputField BPMInputField;

    NotesEditorModel model;
    Subject<int> ButtonsOnMouseUpObservable = new Subject<int>();
    Subject<int> ButtonsOnMouseDownObservable = new Subject<int>();

    void Awake()
    {
        model = NotesEditorModel.Instance;
        model.OnLoadMusicObservable.First().Subscribe(_ => Init());
    }

    void Init()
    {
        var buttonOperateObservable = Observable.Merge(
                ButtonsOnMouseDownObservable,
                ButtonsOnMouseUpObservable)
            .Throttle(TimeSpan.FromMilliseconds(350))
            .Where(delta => delta != 0)
            .SelectMany(delta => Observable.Interval(TimeSpan.FromMilliseconds(50))
                .TakeUntil(ButtonsOnMouseUpObservable)
                .Select(_ => delta))
            .Merge(ButtonsOnMouseDownObservable)
            .Select(delta => model.BPM.Value + delta);

        bool isRedoUndoAction = false;

        BPMInputField.OnValueChangeAsObservable()
            .Where(x => Regex.IsMatch(x, @"^[0-9]+$"))
            .Select(x => int.Parse(x))
            .Merge(buttonOperateObservable)
            .Select(x => Mathf.Clamp(x, 1, 320))
            .DistinctUntilChanged()
            .Select(x => new { current = x, prev = model.BPM.Value })
            .Where(_ => isRedoUndoAction ? (isRedoUndoAction = false) : true)
            .Subscribe(x => UndoRedoManager.Do(
                new Command(
                    () => model.BPM.Value = x.current,
                    () => { isRedoUndoAction = true; model.BPM.Value = x.prev; },
                    () => { isRedoUndoAction = true; model.BPM.Value = x.current; })));
        
        model.BPM.DistinctUntilChanged()
            .Subscribe(x => BPMInputField.text = x.ToString());
    }

    public void UpButtonOnMouseDown() { ButtonsOnMouseDownObservable.OnNext(1); }
    public void UpButtonOnMouseUp() { ButtonsOnMouseUpObservable.OnNext(0); }
    public void DownButtonOnMouseDown() { ButtonsOnMouseDownObservable.OnNext(-1); }
    public void DownButtonOnMouseUp() { ButtonsOnMouseUpObservable.OnNext(0); }
}
