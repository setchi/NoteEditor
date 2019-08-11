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
        Color selectedStateBackgroundColor = default;
        [SerializeField]
        Color defaultBackgroundColor = default;
        [SerializeField]
        Color selectedTextColor = default;
        [SerializeField]
        Color defaultTextColor = default;
        [SerializeField]
        Image itemTypeIcon = default;
        [SerializeField]
        Sprite directoryIcon = default;
        [SerializeField]
        Sprite musicFileIcon = default;
        [SerializeField]
        Sprite otherFileIcon = default;

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
