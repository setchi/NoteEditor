using NoteEditor.UI.Model;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.UI.Presenter
{
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
}
