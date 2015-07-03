using UnityEngine;

public class NoteObject : MonoBehaviour {
    public NotePosition notePosition;
    public int noteType;
    NotesEditorModel model;
    RectTransform rectTransform;

	void Awake () {
        model = NotesEditorModel.Instance;
        rectTransform = GetComponent<RectTransform>();
        rectTransform.localPosition = CalcPosition(notePosition);
    }
	
	void Update () {
        rectTransform.localPosition = CalcPosition(notePosition);
    }

    Vector3 CalcPosition(NotePosition notePosition) {
        return (model.NotePositionToScreenPosition(notePosition));
    }
}
