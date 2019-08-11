using NoteEditor.Model;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.Presenter
{
    public class MusicNameTextPresenter : MonoBehaviour
    {
        [SerializeField]
        Text musicNameText = default;

        void Awake()
        {
            EditData.Name.SubscribeToText(musicNameText);
        }
    }
}
