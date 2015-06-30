using UnityEngine;
using UnityEngine.UI;

public class FileItem : MonoBehaviour
{
    string fileName;

    public void SetName(string name)
    {
        this.fileName = name;
        GetComponentInChildren<Text>().text = name;
    }

    public void OnClick()
    {
        MusicSelectorModel.Instance.SelectedFileName.Value = fileName;
    }

}
