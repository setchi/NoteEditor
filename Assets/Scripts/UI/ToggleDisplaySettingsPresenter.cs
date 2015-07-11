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

    void Awake()
    {
        var model = NotesEditorSettingsModel.Instance;

        toggleDisplaySettingsButton.OnClickAsObservable()
            .Subscribe(_ => model.IsViewing.Value = !model.IsViewing.Value);

        this.UpdateAsObservable()
            .Where(_ => model.IsViewing.Value)
            .Where(_ => Input.GetKey(KeyCode.Escape))
            .Subscribe(_ => model.IsViewing.Value = false);

        model.IsViewing.Select(isViewing => isViewing ? Vector3.up * 258 : Vector3.up * 100000)
            .Subscribe(pos => settingsWindowTransform.localPosition = pos);

        model.IsViewing.Subscribe(_ => model.SelectedBlock.Value = -1);
    }
}
