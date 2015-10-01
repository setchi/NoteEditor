using NoteEditor.UI.Model;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.UI.Presenter
{
    public class ToggleClapSoundEffectEnablePresenter : MonoBehaviour
    {
        [SerializeField]
        Toggle toggle;

        void Awake()
        {
            var model = NoteEditorModel.Instance;
            toggle.OnValueChangedAsObservable()
                .Subscribe(isEnabled => EditorState.ClapSoundEffectEnabled.Value = isEnabled);
        }
    }
}
