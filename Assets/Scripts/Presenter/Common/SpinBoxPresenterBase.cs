using NoteEditor.Common;
using NoteEditor.Utility;
using System;
using System.Text.RegularExpressions;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NoteEditor.Presenter
{
    public abstract class SpinBoxPresenterBase : MonoBehaviour
    {
        [SerializeField]
        InputField inputField;
        [SerializeField]
        Button increaseButton;
        [SerializeField]
        Button decreaseButton;
        [SerializeField]
        int valueStep;
        [SerializeField]
        int minValue;
        [SerializeField]
        int maxValue;
        [SerializeField]
        int longPressTriggerMilliseconds;
        [SerializeField]
        int continuousPressIntervalMilliseconds;

        Subject<int> _operateSpinButtonObservable = new Subject<int>();

        protected abstract ReactiveProperty<int> GetReactiveProperty();

        void Awake()
        {
            increaseButton.AddListener(EventTriggerType.PointerUp, e => _operateSpinButtonObservable.OnNext(0));
            decreaseButton.AddListener(EventTriggerType.PointerUp, e => _operateSpinButtonObservable.OnNext(0));
            increaseButton.AddListener(EventTriggerType.PointerDown, e => _operateSpinButtonObservable.OnNext(valueStep));
            decreaseButton.AddListener(EventTriggerType.PointerDown, e => _operateSpinButtonObservable.OnNext(-valueStep));

            var property = GetReactiveProperty();

            property.Subscribe(x => inputField.text = x.ToString());

            var updateValueFromInputFieldStream = inputField.OnValueChangedAsObservable()
                .Where(x => Regex.IsMatch(x, @"^[0-9]+$"))
                .Select(x => int.Parse(x));

            var updateValueFromSpinButtonStream = _operateSpinButtonObservable
                .Throttle(TimeSpan.FromMilliseconds(longPressTriggerMilliseconds))
                .Where(delta => delta != 0)
                .SelectMany(delta => Observable.Interval(TimeSpan.FromMilliseconds(continuousPressIntervalMilliseconds))
                    .TakeUntil(_operateSpinButtonObservable.Where(d => d == 0))
                    .Select(_ => delta))
                .Merge(_operateSpinButtonObservable.Where(d => d != 0))
                .Select(delta => property.Value + delta);

            var isUndoRedoAction = false;

            Observable.Merge(
                    updateValueFromSpinButtonStream,
                    updateValueFromInputFieldStream)
                .Select(x => Mathf.Clamp(x, minValue, maxValue))
                .DistinctUntilChanged()
                .Where(_ => isUndoRedoAction ? (isUndoRedoAction = false) : true)
                .Select(x => new { current = x, prev = property.Value })
                .Subscribe(x => EditCommandManager.Do(
                    new Command(
                        () => property.Value = x.current,
                        () => { isUndoRedoAction = true; property.Value = x.prev; },
                        () => { isUndoRedoAction = true; property.Value = x.current; })))
                .AddTo(this);
        }
    }
}
