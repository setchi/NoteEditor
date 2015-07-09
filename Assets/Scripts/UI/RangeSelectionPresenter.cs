using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class RangeSelectionPresenter : MonoBehaviour
{
    [SerializeField]
    CanvasEvents canvasEvents;

    NotesEditorModel model;

    void Awake()
    {
        model = NotesEditorModel.Instance;


        // Select by dragging
        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButtonDown(0))
            .Select(_ => model.ScreenToCanvasPosition(Input.mousePosition))
            .SelectMany(startPos => this.UpdateAsObservable()
                .TakeWhile(_ => !Input.GetMouseButtonUp(0))
                .Where(_ => model.IsMouseOverNotesRegion.Value)
                .Select(_ => model.ScreenToCanvasPosition(Input.mousePosition))
                .Select(currentPos => new Rect(startPos, currentPos - startPos)))
            .Do(rect => SelectNotesWithinRect(rect))
            .Subscribe(rect => GLLineRenderer.RenderLines("selectionRect", ToLines(rect, Color.magenta)));


        // Deselect by mousedown
        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButtonDown(0))
            .Subscribe(_ => Deselect());


        // Delete selected notes by delete key
        this.UpdateAsObservable()
            .Where(_ => Input.GetKeyDown(KeyCode.Delete))
            .SelectMany(_ => model.NoteObjects.Values.Where(noteObject => noteObject.isSelected.Value).ToList())
            .Subscribe(selectedNote =>
                (selectedNote.noteType.Value == NoteTypes.Long
                    ? model.LongNoteObservable
                    : model.NormalNoteObservable)
                .OnNext(selectedNote.notePosition));
    }

    void SelectNotesWithinRect(Rect rect)
    {
        if (model.NoteObjects == null) Debug.Log("aaaaaaaaa");
        Deselect();
        var notesWithinRect = model.NoteObjects.Values
            .Where(noteObject => rect.Contains(noteObject.rectTransform.localPosition, true));

        foreach (var note in notesWithinRect)
        {
            note.isSelected.Value = true;
        }
    }

    void Deselect()
    {
        var selectedNotes = model.NoteObjects.Values.Where(noteObject => noteObject.isSelected.Value);

        foreach (var note in selectedNotes)
        {
            note.isSelected.Value = false;
        }
    }

    Line[] ToLines(Rect rect, Color color)
    {
        return new[] {
            new Line(rect.min, rect.min + Vector2.right * rect.size.x, color),
            new Line(rect.min, rect.min + Vector2.up    * rect.size.y, color),
            new Line(rect.max, rect.max + Vector2.left  * rect.size.x, color),
            new Line(rect.max, rect.max + Vector2.down  * rect.size.y, color)
        };
    }
}
