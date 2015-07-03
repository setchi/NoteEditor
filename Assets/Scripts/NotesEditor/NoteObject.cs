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
        return new Vector3(
            model.SamplesToScreenPositionX(notePosition.samples),
            model.BlockNumToScreenPositionY(notePosition.blockNum) * model.CanvasScaleFactor.Value,
            0);
    }

    public void OnMouseEnter()
    {
        model.IsMouseOverCanvas.Value = true;
    }

    public void OnMouseDown()
    {
        if (model.ClosestNotePosition.Value.Equals(notePosition))
        {
            model.NormalNoteObservable.OnNext(notePosition);
        }
    }
}
