using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class ToggleDisplaySettingsPresenter : MonoBehaviour
{
    [SerializeField]
    Button toggleDisplaySettingsButton;
    [SerializeField]
    Transform settingsWindowTransform;

    bool isMouseOverOnSettingsWindow = false;

    void Awake()
    {
        var model = NotesEditorSettingsModel.Instance;

        toggleDisplaySettingsButton.OnClickAsObservable()
            .Subscribe(_ => model.IsViewing.Value = !model.IsViewing.Value);

        Observable.Merge(
                this.UpdateAsObservable()
                    .Where(_ => model.IsViewing.Value)
                    .Where(_ => Input.GetKey(KeyCode.Escape)),
                this.UpdateAsObservable()
                    .Where(_ => model.IsViewing.Value)
                    .Where(_ => !isMouseOverOnSettingsWindow && Input.GetMouseButtonDown(0)))
            .Subscribe(_ => model.IsViewing.Value = false);

        model.IsViewing.Select(isViewing => isViewing ? Vector3.zero : Vector3.up * 100000)
            .Subscribe(pos => settingsWindowTransform.localPosition = pos);

        model.IsViewing.Subscribe(_ => model.SelectedBlock.Value = -1);
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
