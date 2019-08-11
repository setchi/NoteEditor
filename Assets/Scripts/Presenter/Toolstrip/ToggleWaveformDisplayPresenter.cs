using NoteEditor.Model;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.Presenter
{
    public class ToggleWaveformDisplayPresenter : MonoBehaviour
    {
        [SerializeField]
        Toggle toggle = default;

        void Awake()
        {
            toggle.OnValueChangedAsObservable()
                .Subscribe(x => EditorState.WaveformDisplayEnabled.Value = x);
        }
    }
}
