using NoteEditor.UI.Model;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.UI.Presenter
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
            var model = NoteEditorModel.Instance;

            editTypeToggleButton.OnClickAsObservable()
                .Merge(this.UpdateAsObservable().Where(_ => KeyInput.AltKeyDown()))
                .Select(_ => model.EditType.Value == NoteTypes.Single ? NoteTypes.Long : NoteTypes.Single)
                .Subscribe(editType => model.EditType.Value = editType);

            var buttonImage = editTypeToggleButton.GetComponent<Image>();

            model.EditType.Select(_ => model.EditType.Value == NoteTypes.Long)
                .Subscribe(isLongType =>
                {
                    buttonImage.sprite = isLongType ? iconLongNotes : iconSingleNotes;
                    buttonImage.color = isLongType ? longTypeStateButtonColor : singleTypeStateButtonColor;
                });
        }
    }
}
