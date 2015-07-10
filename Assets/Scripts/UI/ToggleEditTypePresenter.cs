using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class ToggleEditTypePresenter : MonoBehaviour
{
    [SerializeField]
    Button editTypeToggleButton;
    [SerializeField]
    Sprite iconLongNotes;
    [SerializeField]
    Sprite iconNormalNotes;
    [SerializeField]
    Color longTypeStateButtonColor;
    [SerializeField]
    Color normalTypeStateButtonColor;

    void Awake()
    {
        var model = NotesEditorModel.Instance;

        editTypeToggleButton.OnClickAsObservable()
            .Merge(this.UpdateAsObservable().Where(_ => KeyInput.AltKeyDown()))
            .Select(_ => model.EditType.Value == NoteTypes.Normal ? NoteTypes.Long : NoteTypes.Normal)
            .Subscribe(editType => model.EditType.Value = editType);

        var buttonImage = editTypeToggleButton.GetComponent<Image>();

        model.EditType.Select(_ => model.EditType.Value == NoteTypes.Long)
            .Subscribe(isLongType => {
                buttonImage.sprite = isLongType ? iconLongNotes : iconNormalNotes;
                buttonImage.color = isLongType ? longTypeStateButtonColor : normalTypeStateButtonColor;
            });
    }
}
