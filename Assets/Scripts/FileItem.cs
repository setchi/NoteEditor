using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class FileItem : MonoBehaviour
{
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
            .Do(selected => image.color = selected ? new Color(17 / 255f, 19 / 255f, 16 / 255f) : new Color(48 / 255f, 49 / 255f, 47 / 255f))
            .Subscribe(selected => text.color = selected ? new Color(175 / 255f, 255 / 255f, 182 / 255f) : new Color(146 / 255f, 148 / 255f, 143 / 255f));// : new Color(213 / 255f, 215 / 255f, 212 / 255f)); // 
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
