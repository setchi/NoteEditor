using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class FileItem : MonoBehaviour
{
    [SerializeField]
    Color selectedStateBackgroundColor;
    [SerializeField]
    Color normalBackgroundColor;
    [SerializeField]
    Color selectedTextColor;
    [SerializeField]
    Color normalTextColor;

    string fileName;
    MusicSelectorModel model;

    void Start()
    {
        GetComponent<RectTransform>().localScale = Vector3.one;
        model = MusicSelectorModel.Instance;

        var text = GetComponentInChildren<Text>();
        var image = GetComponent<Image>();

        this.UpdateAsObservable()
            .Select(_ => fileName == model.SelectedFileName.Value)
            .Do(selected => image.color = selected ? selectedStateBackgroundColor : normalBackgroundColor)
            .Subscribe(selected => text.color = selected ? selectedTextColor : normalTextColor);
    }

    public void SetName(string name)
    {
        this.fileName = name;
        GetComponentInChildren<Text>().text = name;
    }

    public void OnMouseDown()
    {
        model.SelectedFileName.Value = fileName;
    }
}
