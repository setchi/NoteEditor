using NoteEditor.UI.Model;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.UI.Presenter
{
    public class SoundEffectEnabledPresenter : MonoBehaviour
    {
        [SerializeField]
        Toggle soundEffectEnabledToggle;

        void Awake()
        {
            var model = NotesEditorModel.Instance;
            soundEffectEnabledToggle.OnValueChangedAsObservable()
                .Subscribe(isEnabled => model.PlaySoundEffectEnabled.Value = isEnabled);
        }
    }
}
