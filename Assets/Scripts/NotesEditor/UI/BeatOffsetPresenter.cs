using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class BeatOffsetPresenter : MonoBehaviour
{
    [SerializeField]
    InputField beatOffsetInputField;
    [SerializeField]
    Button beatOffsetIncreaseButton;
    [SerializeField]
    Button beatOffsetDecreaseButton;

    void Awake()
    {
        var model = NotesEditorModel.Instance;

        beatOffsetInputField.OnValueChangeAsObservable()
            .Select(x => string.IsNullOrEmpty(x) ? "0" : x)
            .Select(x => int.Parse(x))
            .Merge(beatOffsetIncreaseButton.OnClickAsObservable().Select(_ => model.BeatOffsetSamples.Value + 100))
            .Merge(beatOffsetDecreaseButton.OnClickAsObservable().Select(_ => model.BeatOffsetSamples.Value - 100))
            .Select(x => Mathf.Clamp(x, 0, int.MaxValue))
            .Subscribe(x => model.BeatOffsetSamples.Value = x);

        model.BeatOffsetSamples.DistinctUntilChanged()
            .Subscribe(x => beatOffsetInputField.text = x.ToString());
    }
}
