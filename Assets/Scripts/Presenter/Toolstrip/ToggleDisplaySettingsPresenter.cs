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
                .Subscribe(_ => Settings.IsViewing.Value = !Settings.IsViewing.Value);

            Observable.Merge(
                    this.UpdateAsObservable()
                        .Where(_ => Settings.IsViewing.Value)
                        .Where(_ => Input.GetKey(KeyCode.Escape)),
                    this.UpdateAsObservable()
                        .Where(_ => Settings.IsViewing.Value)
                        .Where(_ => !isMouseOverOnSettingsWindow && Input.GetMouseButtonDown(0)))
                .Subscribe(_ => Settings.IsViewing.Value = false);

            Settings.IsViewing.Select(isViewing => isViewing ? Vector3.zero : Vector3.up * 100000)
                .Subscribe(pos => settingsWindowTransform.localPosition = pos);

            Settings.IsViewing.Subscribe(_ => Settings.SelectedBlock.Value = -1);
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
