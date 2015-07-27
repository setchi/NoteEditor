using System;
using System.Text.RegularExpressions;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class SpinBoxPresenterBase : MonoBehaviour
{
    [SerializeField]
    InputField inputField;
    [SerializeField]
    Button increaseButton;
    [SerializeField]
    Button decreaseButton;
    [SerializeField]
    int valueChangeCoefficient;
    [SerializeField]
    int minValue;
    [SerializeField]
    int maxValue;
    [SerializeField]
    int longPressTriggerMilliseconds;
    [SerializeField]
    int continuousPressIntervalMilliseconds;

    Subject<int> _operateButtonStream = new Subject<int>();

    protected abstract ReactiveProperty<int> GetProperty();

    EventTrigger.Entry InstantiateEntry(EventTriggerType eventID, UnityAction<BaseEventData> callback)
    {
        var entry = new EventTrigger.Entry();
        entry.eventID = eventID;
        entry.callback.AddListener(callback);
        return entry;
    }

    void Awake()
    {
        var increaseButtonEventTriggers = (increaseButton.GetComponent<EventTrigger>() ?? increaseButton.gameObject.AddComponent<EventTrigger>()).triggers;
        var decreaseButtonEventTriggers = (decreaseButton.GetComponent<EventTrigger>() ?? decreaseButton.gameObject.AddComponent<EventTrigger>()).triggers;
        increaseButtonEventTriggers.Add(InstantiateEntry(EventTriggerType.PointerDown, (e) => _operateButtonStream.OnNext(valueChangeCoefficient)));
        increaseButtonEventTriggers.Add(InstantiateEntry(EventTriggerType.PointerUp, (e) => _operateButtonStream.OnNext(0)));
        decreaseButtonEventTriggers.Add(InstantiateEntry(EventTriggerType.PointerDown, (e) => _operateButtonStream.OnNext(-valueChangeCoefficient)));
        decreaseButtonEventTriggers.Add(InstantiateEntry(EventTriggerType.PointerUp, (e) => _operateButtonStream.OnNext(0)));

        var property = GetProperty();

        var operateButtonObservable = _operateButtonStream
            .Throttle(TimeSpan.FromMilliseconds(longPressTriggerMilliseconds))
            .Where(delta => delta != 0)
            .SelectMany(delta => Observable.Interval(TimeSpan.FromMilliseconds(continuousPressIntervalMilliseconds))
                .TakeUntil(_operateButtonStream.Where(d => d == 0))
                .Select(_ => delta))
            .Merge(_operateButtonStream.Where(d => d != 0))
            .Select(delta => property.Value + delta);

        var isUndoRedoAction = false;

        inputField.OnValueChangeAsObservable()
            .Where(x => Regex.IsMatch(x, @"^[0-9]+$"))
            .Select(x => int.Parse(x))
            .Merge(operateButtonObservable)
            .Select(x => Mathf.Clamp(x, minValue, maxValue))
            .DistinctUntilChanged()
            .Where(_ => isUndoRedoAction ? (isUndoRedoAction = false) : true)
            .Select(x => new { current = x, prev = property.Value })
            .Subscribe(x => UndoRedoManager.Do(
                new Command(
                    () => property.Value = x.current,
                    () => { isUndoRedoAction = true; property.Value = x.prev; },
                    () => { isUndoRedoAction = true; property.Value = x.current; })));

        property.Subscribe(x => inputField.text = x.ToString());
    }
}
