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

        string fileName;

        void Start()
        {
            GetComponent<RectTransform>().localScale = Vector3.one;

            var text = GetComponentInChildren<Text>();
            var image = GetComponent<Image>();

            this.UpdateAsObservable()
                .Select(_ => fileName == MusicSelector.SelectedFileName.Value)
                .DistinctUntilChanged()
                .Do(selected => image.color = selected ? selectedStateBackgroundColor : defaultBackgroundColor)
                .Subscribe(selected => text.color = selected ? selectedTextColor : defaultTextColor)
                .AddTo(this);
        }

        public void SetName(string name)
        {
            fileName = name;
            GetComponentInChildren<Text>().text = name;
        }

        public void OnMouseDown()
        {
            MusicSelector.SelectedFileName.Value = fileName;
        }
    }
}
