using NoteEditor.Model;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.Presenter
{
    public class ToggleClapSoundEffectEnablePresenter : MonoBehaviour
    {
        [SerializeField]
        Toggle toggle;

        void Awake()
        {
            toggle.OnValueChangedAsObservable()
                .Subscribe(isEnabled => EditorState.ClapSoundEffectEnabled.Value = isEnabled);
        }
    }
}
