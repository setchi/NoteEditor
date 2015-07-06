using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class BPMPresenter : MonoBehaviour
{
    [SerializeField]
    InputField BPMInputField;
    [SerializeField]
    Button BPMUpButton;
    [SerializeField]
    Button BPMDownButton;

    NotesEditorModel model;

    void Awake()
    {
        model = NotesEditorModel.Instance;
        model.OnLoadedMusicObservable.Subscribe(_ => Init());
    }

    void Init()
    {
        model.UnitBeatSamples = model.BPM.DistinctUntilChanged()
            .Select(x => Mathf.FloorToInt(model.Audio.clip.frequency * 60 / x))
            .ToReactiveProperty();

        BPMInputField.OnValueChangeAsObservable()
            .Select(x => string.IsNullOrEmpty(x) ? "1" : x)
            .Select(x => float.Parse(x))
            .Merge(BPMUpButton.OnClickAsObservable().Select(_ => model.BPM.Value + 1))
            .Merge(BPMDownButton.OnClickAsObservable().Select(_ => model.BPM.Value - 1))
            .Select(x => Mathf.Clamp(x, 1, 320))
            .Subscribe(x => model.BPM.Value = x);

        model.BPM.DistinctUntilChanged()
            .Subscribe(x => BPMInputField.text = x.ToString());
    }
}
