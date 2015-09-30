using NoteEditor.UI.Model;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.UI.Presenter
{
    public class WaveformDisplayTogglePresenter : MonoBehaviour
    {
        [SerializeField]
        Toggle waveformDisplayToggle;

        void Awake()
        {
            var model = NoteEditorModel.Instance;
            waveformDisplayToggle.OnValueChangedAsObservable()
                .Subscribe(x => model.WaveformDisplayEnabled.Value = x);
        }
    }
}
