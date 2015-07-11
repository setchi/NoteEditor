using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class KeyCodeSettingsItem : MonoBehaviour
{
    [SerializeField]
    Color selectedStateBackgroundColor;
    [SerializeField]
    Color normalBackgroundColor;
    [SerializeField]
    Color selectedTextColor;
    [SerializeField]
    Color normalTextColor;

    ReactiveProperty<KeyCode> keyCode = new ReactiveProperty<KeyCode>();
    int block;
    NotesEditorSettingsModel model;

    void Start()
    {
        GetComponent<RectTransform>().localScale = Vector3.one;
        model = NotesEditorSettingsModel.Instance;

        var text = GetComponentInChildren<Text>();
        var image = GetComponent<Image>();

        model.SelectedBlock
            .Select(selectedBlock => block == selectedBlock)
            .Do(selected => image.color = selected ? selectedStateBackgroundColor : normalBackgroundColor)
            .Subscribe(selected => text.color = selected ? selectedTextColor : normalTextColor)
            .AddTo(gameObject);

        this.UpdateAsObservable()
            .Where(_ => model.IsViewing.Value)
            .Where(_ => model.SelectedBlock.Value == block)
            .Where(_ => Input.anyKeyDown)
            .Select(_ => KeyInput.FetchKey())
            .Where(keyCode => keyCode != KeyCode.None)
            .Do(keyCode => this.keyCode.Value = keyCode)
            .Do(keyCode => model.NoteInputKeyCodes.Value[block] = keyCode)
            .Subscribe(_ => model.ChangeInputKeyCodesObservable.OnNext(Unit.Default))
            .AddTo(gameObject);

        this.keyCode.Select(keyCode => block + ": " + keyCode)
            .SubscribeToText(GetComponentInChildren<Text>());
    }

    public void SetData(int block, KeyCode keyCode)
    {
        this.block = block;
        this.keyCode.Value = keyCode;
    }

    public void OnMouseDown()
    {
        model.SelectedBlock.Value = block;
    }
}
