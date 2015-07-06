using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class EditTypeTogglePresenter : MonoBehaviour
{
    [SerializeField]
    Button editTypeToggleButton;

    void Awake()
    {
        var model = NotesEditorModel.Instance;

        editTypeToggleButton.OnClickAsObservable()
            .Select(_ => model.EditType.Value == NoteTypes.Normal ? NoteTypes.Long : NoteTypes.Normal)
            .Subscribe(editType => model.EditType.Value = editType);

        var editTypeToggleButtonDefaultColor = editTypeToggleButton.GetComponent<Image>().color;
        model.EditType.Select(_ => model.EditType.Value == NoteTypes.Long)
            .Subscribe(isLongType => {
                editTypeToggleButton.GetComponentInChildren<Text>().text = (isLongType ? "Long" : "Normal") + " Notes";
                editTypeToggleButton.GetComponent<Image>().color = isLongType ? Color.cyan : editTypeToggleButtonDefaultColor;
            });
    }
}
