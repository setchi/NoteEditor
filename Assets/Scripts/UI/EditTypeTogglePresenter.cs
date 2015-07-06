using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class EditTypeTogglePresenter : MonoBehaviour
{
    [SerializeField]
    Button editTypeToggleButton;
    [SerializeField]
    Sprite iconLongNotes;
    [SerializeField]
    Sprite iconNormalNotes;

    void Awake()
    {
        var model = NotesEditorModel.Instance;

        editTypeToggleButton.OnClickAsObservable()
            .Select(_ => model.EditType.Value == NoteTypes.Normal ? NoteTypes.Long : NoteTypes.Normal)
            .Subscribe(editType => model.EditType.Value = editType);

        var buttonImage = editTypeToggleButton.GetComponent<Image>();
        var defaultButtonColor = buttonImage.color;

        model.EditType.Select(_ => model.EditType.Value == NoteTypes.Long)
            .Subscribe(isLongType => {
                buttonImage.sprite = isLongType ? iconLongNotes : iconNormalNotes;
                buttonImage.color = isLongType ? Color.cyan : defaultButtonColor;
            });
    }
}
