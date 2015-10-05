using NoteEditor.Model;
using NoteEditor.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.Presenter
{
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

        void Start()
        {
            GetComponent<RectTransform>().localScale = Vector3.one;

            var text = GetComponentInChildren<Text>();
            var image = GetComponent<Image>();

            Settings.SelectedBlock
                .Select(selectedBlock => block == selectedBlock)
                .Do(selected => image.color = selected ? selectedStateBackgroundColor : defaultBackgroundColor)
                .Subscribe(selected => text.color = selected ? selectedTextColor : defaultTextColor)
                .AddTo(this);

            this.UpdateAsObservable()
                .Where(_ => Settings.IsOpen.Value)
                .Where(_ => Settings.SelectedBlock.Value == block)
                .Where(_ => Input.anyKeyDown)
                .Select(_ => KeyInput.FetchKey())
                .Where(keyCode => keyCode != KeyCode.None)
                .Do(keyCode => this.keyCode.Value = keyCode)
                .Do(keyCode => Settings.NoteInputKeyCodes.Value[block] = keyCode)
                .Subscribe(_ => Settings.RequestForChangeInputNoteKeyCode.OnNext(Unit.Default))
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
            Settings.SelectedBlock.Value = block;
        }
    }
}
