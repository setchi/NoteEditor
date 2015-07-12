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
        model.OnLoadedMusicObservable.First().Subscribe(_ => Init());
    }

    void Init()
    {
        var closestNoteAreaOnMouseDownObservable = canvasEvents.NotesRegionOnMouseDownObservable
            .Where(_ => !Input.GetMouseButtonDown(1))
            .Where(_ => 0 <= model.ClosestNotePosition.Value.num);

        closestNoteAreaOnMouseDownObservable
            .Select(_ => model.EditType.Value == NoteTypes.Long
                ? model.LongNoteObservable
                : model.NormalNoteObservable)
            .Subscribe(observable => observable.OnNext(model.ClosestNotePosition.Value));


        // Start editing of long note
        closestNoteAreaOnMouseDownObservable
            .Where(_ => model.EditType.Value == NoteTypes.Normal)
            .Where(_ => KeyInput.ShiftKey())
            .Do(notePosition => model.EditType.Value = NoteTypes.Long)
            .Subscribe(_ => model.LongNoteObservable.OnNext(model.ClosestNotePosition.Value));


        // Finish editing long note by press-escape or right-click
        this.UpdateAsObservable()
            .Where(_ => model.EditType.Value == NoteTypes.Long)
            .Where(_ => Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            .Subscribe(_ => model.EditType.Value = NoteTypes.Normal);

        var finishEditLongNoteObservable = model.EditType.DistinctUntilChanged()
            .Where(editType => editType == NoteTypes.Normal)
            .Skip(1);

        finishEditLongNoteObservable.Subscribe(_ => model.LongNoteTailPosition.Value = new NotePosition(-1, -1, -1));


        // Update long note link and tail position
        model.AddedLongNoteObjectObservable
            .Subscribe(obj =>
            {
                if (model.NoteObjects.ContainsKey(model.LongNoteTailPosition.Value))
                {
                    var tailObj = model.NoteObjects[model.LongNoteTailPosition.Value];
                    tailObj.next = obj;
                    obj.prev = tailObj;
                }

                model.LongNoteTailPosition.Value = obj.notePosition;
            });


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
                    else
                    {
                        model.LongNoteTailPosition.Value = noteObject.prev == null ? new NotePosition(-1, -1, -1) : noteObject.prev.notePosition;
                    }

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
