using NoteEditor.UI.Model;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.UI.Presenter
{
    public class ToggleWaveformDisplayPresenter : MonoBehaviour
    {
        [SerializeField]
        Toggle toggle;

        void Awake()
        {
            var model = NoteEditorModel.Instance;
            toggle.OnValueChangedAsObservable()
                .Subscribe(x => EditorState.WaveformDisplayEnabled.Value = x);
        }
    }
}
