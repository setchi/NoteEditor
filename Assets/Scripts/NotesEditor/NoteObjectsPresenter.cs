using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class NoteObjectsPresenter : MonoBehaviour
{
    [SerializeField]
    GameObject notesRegion;
    [SerializeField]
    CanvasEvents canvasEvents;
    [SerializeField]
    GameObject notePrefab;

    NotesEditorModel model;

    void Awake()
    {
        model = NotesEditorModel.Instance;


        var closestNoteAreaOnMouseDownObservable = canvasEvents.ScrollPadOnMouseDownObservable
            .Where(_ => !Input.GetMouseButtonDown(1))
            .Where(_ => 0 <= model.ClosestNotePosition.Value.samples);

        var closestNoteAreaOnMouseDownPosition = closestNoteAreaOnMouseDownObservable
            .Select(_ => Input.mousePosition)
            .ToReactiveProperty();

        var longNoteStartPosition = model.NormalNoteObservable
            .ToReactiveProperty();


        // Start editing of long note
        /*
        this.UpdateAsObservable()
            .SkipUntil(closestNoteAreaOnMouseDownObservable)
            .TakeWhile(_ => !Input.GetMouseButtonUp(0))
            .RepeatSafe()
            .Select(_ => Input.mousePosition)
            .Select(pos => (closestNoteAreaOnMouseDownPosition.Value - pos).magnitude)
            .Where(magnitude => 50 <= magnitude)
            .Select(_ => longNoteStartPosition.Value)
            .DistinctUntilChanged()
            .Do(_ => model.EditType.Value = NoteTypes.Long)
            .Subscribe(notePosition => model.LongNoteObservable.OnNext(notePosition));
            // */
        closestNoteAreaOnMouseDownObservable
            .Where(_ => model.EditType.Value == NoteTypes.Normal)
            .Where(_ => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            .Do(_ => model.EditType.Value = NoteTypes.Long)
            .Select(_ => model.ClosestNotePosition.Value)
            .Do(_ => longNoteStartPosition.Value = model.ClosestNotePosition.Value)
            .Do(notePosition => model.LongNoteObservable.OnNext(notePosition))
            .Subscribe(notePosition => model.LongNoteObservable.OnNext(notePosition));


        // Return to the normal notes edit mode
        var endLongNoteObservable = this.UpdateAsObservable()
            .Where(_ => model.EditType.Value == NoteTypes.Long)
            .Where(_ => Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            .Do(_ => model.EditType.Value = NoteTypes.Normal);

        model.AddedLongNoteObjectObservable.Where(obj => obj != null)
            .TakeUntil(endLongNoteObservable)
            .Buffer(2, 1).Where(b => 2 <= b.Count)
            .RepeatSafe()
            .Subscribe(b => {
                b[0].next = b[1];
                b[1].prev = b[0];
            });


        closestNoteAreaOnMouseDownObservable
            .Where(_ => model.EditType.Value == NoteTypes.Normal)
            .Subscribe(_ => model.NormalNoteObservable.OnNext(model.ClosestNotePosition.Value));

        closestNoteAreaOnMouseDownObservable
            .Where(_ => model.EditType.Value == NoteTypes.Long)
            .Subscribe(_ => model.LongNoteObservable.OnNext(model.ClosestNotePosition.Value));


        model.NormalNoteObservable.Subscribe(notePosition =>
        {
            if (model.NoteObjects.ContainsKey(notePosition))
            {
                RemoveNote(notePosition);
            }
            else
            {
                var noteObject = (Instantiate(notePrefab) as GameObject).GetComponent<NoteObject>();
                noteObject.notePosition = notePosition;
                noteObject.noteType.Value = NoteTypes.Normal;
                noteObject.transform.SetParent(notesRegion.transform);

                model.NoteObjects.Add(notePosition, noteObject);
            }
        });


        model.LongNoteObservable.Subscribe(notePosition =>
        {
            if (model.NoteObjects.ContainsKey(notePosition))
            {
                var noteObject = model.NoteObjects[notePosition];

                if (noteObject.noteType.Value == NoteTypes.Long)
                {

                    if (noteObject.prev != null)
                        noteObject.prev.next = noteObject.next;

                    if (noteObject.next != null)
                        noteObject.next.prev = noteObject.prev;

                    RemoveNote(notePosition);
                }
                else
                {
                    noteObject.noteType.Value = NoteTypes.Long;
                    model.AddedLongNoteObjectObservable.OnNext(noteObject);
                }
            }
            else
            {
                var noteObject = (Instantiate(notePrefab) as GameObject).GetComponent<NoteObject>();
                noteObject.notePosition = notePosition;
                noteObject.noteType.Value = NoteTypes.Long;
                noteObject.transform.SetParent(notesRegion.transform);

                model.NoteObjects.Add(notePosition, noteObject);
                model.AddedLongNoteObjectObservable.OnNext(noteObject);
            }
        });
    }

    void RemoveNote(NotePosition notePosition)
    {
        if (model.NoteObjects.ContainsKey(notePosition))
        {
            var noteObject = model.NoteObjects[notePosition];
            model.NoteObjects.Remove(notePosition);
            DestroyObject(noteObject.gameObject);
        }
    }
}
