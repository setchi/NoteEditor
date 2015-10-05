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
        Transform settingsWindowTransform;

        bool isMouseOverOnSettingsWindow = false;

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
                        .Where(_ => !isMouseOverOnSettingsWindow && Input.GetMouseButtonDown(0)))
                .Subscribe(_ => Settings.IsOpen.Value = false);

            Settings.IsOpen.Select(isViewing => isViewing ? Vector3.zero : Vector3.up * 100000)
                .Subscribe(pos => settingsWindowTransform.localPosition = pos);

            Settings.IsOpen.Subscribe(_ => Settings.SelectedBlock.Value = -1);
        }

        public void OnMouseEnterSettingsWindow()
        {
            isMouseOverOnSettingsWindow = true;
        }

        public void OnMouseExitSettingsWindow()
        {
            isMouseOverOnSettingsWindow = false;
        }
    }
}
