using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class InputNoteKeyCodeSettingsItem : MonoBehaviour
{
    [SerializeField]
    Color selectedStateBackgroundColor;
    [SerializeField]
    Color defaultBackgroundColor;
    [SerializeField]
    Color selectedTextColor;
    [SerializeField]
    Color defaultTextColor;

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
            .Do(selected => image.color = selected ? selectedStateBackgroundColor : defaultBackgroundColor)
            .Subscribe(selected => text.color = selected ? selectedTextColor : defaultTextColor)
            .AddTo(this);

        this.UpdateAsObservable()
            .Where(_ => model.IsViewing.Value)
            .Where(_ => model.SelectedBlock.Value == block)
            .Where(_ => Input.anyKeyDown)
            .Select(_ => KeyInput.FetchKey())
            .Where(keyCode => keyCode != KeyCode.None)
            .Do(keyCode => this.keyCode.Value = keyCode)
            .Do(keyCode => model.NoteInputKeyCodes.Value[block] = keyCode)
            .Subscribe(_ => model.RequestForChangeInputNoteKeyCode.OnNext(Unit.Default))
            .AddTo(this);

        this.keyCode.Select(keyCode => block + ": " + keyCode)
            .SubscribeToText(GetComponentInChildren<Text>())
            .AddTo(this);
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
