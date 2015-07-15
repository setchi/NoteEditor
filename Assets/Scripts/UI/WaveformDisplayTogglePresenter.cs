using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class WaveformDisplayTogglePresenter : MonoBehaviour
{
    [SerializeField]
    Toggle waveformDisplayToggle;

    void Awake()
    {
        var model = NotesEditorModel.Instance;
        waveformDisplayToggle.OnValueChangedAsObservable()
            .Subscribe(x => model.WaveformDisplayEnabled.Value = x);
    }
}
