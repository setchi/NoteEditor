using NoteEditor.Model;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.Presenter
{
    public class FileListItem : MonoBehaviour
    {
        [SerializeField]
        Color selectedStateBackgroundColor;
        [SerializeField]
        Color defaultBackgroundColor;
        [SerializeField]
        Color selectedTextColor;
        [SerializeField]
        Color defaultTextColor;
        [SerializeField]
        Image itemTypeIcon;
        [SerializeField]
        Sprite directoryIcon;
        [SerializeField]
        Sprite musicFileIcon;
        [SerializeField]
        Sprite otherFileIcon;

        string itemName;
        FileItemInfo fileItemInfo;

        void Awake()
        {
            var text = GetComponentInChildren<Text>();
            var image = GetComponent<Image>();

            this.ObserveEveryValueChanged(_ => itemName == MusicSelector.SelectedFileName.Value)
                .Do(selected => image.color = selected ? selectedStateBackgroundColor : defaultBackgroundColor)
                .Subscribe(selected => text.color = selected ? selectedTextColor : defaultTextColor)
                .AddTo(this);
        }

        void Start()
        {
            GetComponent<RectTransform>().localScale = Vector3.one;
        }

        public void SetInfo(FileItemInfo info)
        {
            fileItemInfo = info;
            itemName = System.IO.Path.GetFileName(info.fullName);
            GetComponentInChildren<Text>().text = itemName;

            itemTypeIcon.sprite = fileItemInfo.isDirectory
                ? directoryIcon
                : System.IO.Path.GetExtension(itemName) == ".wav"
                    ? musicFileIcon
                    : otherFileIcon;
        }

        public void OnMouseDown()
        {
            if (fileItemInfo.isDirectory && itemName == MusicSelector.SelectedFileName.Value)
            {
                MusicSelector.DirectoryPath.Value = fileItemInfo.fullName;
                // Scroll top
                return;
            }

            MusicSelector.SelectedFileName.Value = itemName;
        }
    }
}
