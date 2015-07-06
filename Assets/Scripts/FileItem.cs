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
        this.UpdateAsObservable()
            .Select(_ => fileName == model.SelectedFileName.Value)
            .Select(selected => selected ? Color.red : Color.green)
            .Subscribe(color => GetComponentInChildren<Text>().color = color);
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
