using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class SoundEffectEnabledPresenter : MonoBehaviour
{
    [SerializeField]
    Toggle soundEffectEnabledToggle;

    void Awake()
    {
        var model = NotesEditorModel.Instance;
        model.PlaySoundEffectEnabled = soundEffectEnabledToggle.OnValueChangedAsObservable().ToReactiveProperty();
    }
}
