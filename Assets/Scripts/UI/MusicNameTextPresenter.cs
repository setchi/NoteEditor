using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MusicNameTextPresenter : MonoBehaviour
{
    [SerializeField]
    Text musicNameText;

    void Awake()
    {
        var model = NotesEditorModel.Instance;
        model.MusicName.SubscribeToText(musicNameText);
    }
}
