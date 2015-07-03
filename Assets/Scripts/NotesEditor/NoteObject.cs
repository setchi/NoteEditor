using UnityEngine;

public class NoteObject : MonoBehaviour
{
    public NotePosition notePosition;
    public int noteType;
    NotesEditorModel model;
    RectTransform rectTransform;

    void Awake()
    {
        model = NotesEditorModel.Instance;
        rectTransform = GetComponent<RectTransform>();
        rectTransform.localPosition = CalcPosition(notePosition);
    }

    void LateUpdate()
    {
        rectTransform.localPosition = CalcPosition(notePosition);
    }

    Vector3 CalcPosition(NotePosition notePosition)
    {
        return model.NotePositionToScreenPosition(notePosition);
    }

    public void OnMouseEnter()
    {
        model.IsMouseOverCanvas.Value = true;
    }

    public void OnMouseDown()
    {
        model.NormalNoteObservable.OnNext(notePosition);
    }
}
