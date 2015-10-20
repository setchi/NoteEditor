using NoteEditor.Model;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.Presenter
{
    public class ToggleDisplaySettingsPresenter : MonoBehaviour
    {
        [SerializeField]
        Button toggleDisplaySettingsButton;
        [SerializeField]
        GameObject settingsWindow;

        bool isMouseOverSettingsWindow = false;

        void Awake()
        {
            toggleDisplaySettingsButton.OnClickAsObservable()
                .Subscribe(_ => Settings.IsOpen.Value = !Settings.IsOpen.Value);

            Observable.Merge(
                    this.UpdateAsObservable()
                        .Where(_ => Settings.IsOpen.Value)
                        .Where(_ => Input.GetKey(KeyCode.Escape)),
                    this.UpdateAsObservable()
                        .Where(_ => Settings.IsOpen.Value)
                        .Where(_ => !isMouseOverSettingsWindow)
                        .Where(_ => Input.GetMouseButtonDown(0)))
                .Subscribe(_ => Settings.IsOpen.Value = false);

            Settings.IsOpen.Subscribe(_ => Settings.SelectedBlock.Value = -1);
            Settings.IsOpen.Subscribe(isOpen => settingsWindow.SetActive(isOpen));
        }

        public void OnMouseEnterSettingsWindow()
        {
            isMouseOverSettingsWindow = true;
        }

        public void OnMouseExitSettingsWindow()
        {
            isMouseOverSettingsWindow = false;
        }
    }
}
