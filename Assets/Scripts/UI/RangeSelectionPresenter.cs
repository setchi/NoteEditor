using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class RangeSelectionPresenter : MonoBehaviour
{
    [SerializeField]
    CanvasEvents canvasEvents;
    [SerializeField]
    Color selectionRectColor;

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
            .Do(rect => SelectNotesWithin(rect))
            .Subscribe(rect => GLLineRenderer.RenderLines("selectionRect", ToLines(rect, selectionRectColor)));


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

    void SelectNotesWithin(Rect rect)
    {
        foreach (var note in model.NoteObjects.Values)
        {
            note.isSelected.Value = rect.Contains(note.rectTransform.localPosition, true);
        }
    }

    void Deselect()
    {
        foreach (var note in model.NoteObjects.Values)
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
