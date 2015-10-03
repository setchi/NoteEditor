using NoteEditor.Notes;
using NoteEditor.Model;
using NoteEditor.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.Presenter
{
    public class ToggleEditTypePresenter : MonoBehaviour
    {
        [SerializeField]
        Button editTypeToggleButton;
        [SerializeField]
        Sprite iconLongNotes;
        [SerializeField]
        Sprite iconSingleNotes;
        [SerializeField]
        Color longTypeStateButtonColor;
        [SerializeField]
        Color singleTypeStateButtonColor;

        void Awake()
        {
            editTypeToggleButton.OnClickAsObservable()
                .Merge(this.UpdateAsObservable().Where(_ => KeyInput.AltKeyDown()))
                .Select(_ => EditState.NoteType.Value == NoteTypes.Single ? NoteTypes.Long : NoteTypes.Single)
                .Subscribe(editType => EditState.NoteType.Value = editType);

            var buttonImage = editTypeToggleButton.GetComponent<Image>();

            EditState.NoteType.Select(_ => EditState.NoteType.Value == NoteTypes.Long)
                .Subscribe(isLongType =>
                {
                    buttonImage.sprite = isLongType ? iconLongNotes : iconSingleNotes;
                    buttonImage.color = isLongType ? longTypeStateButtonColor : singleTypeStateButtonColor;
                });
        }
    }
}
