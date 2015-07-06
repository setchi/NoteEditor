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
        model.WaveformDisplayEnabled = waveformDisplayToggle.OnValueChangedAsObservable().ToReactiveProperty();
    }
}
