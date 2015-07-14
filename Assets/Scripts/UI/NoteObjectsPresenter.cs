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
            .Subscribe(_ => model.EditNoteObservable.OnNext(new Note(model.ClosestNotePosition.Value, model.EditType.Value)));


        // Start editing of long note
        closestNoteAreaOnMouseDownObservable
            .Where(_ => model.EditType.Value == NoteTypes.Normal)
            .Where(_ => KeyInput.ShiftKey())
            .Do(notePosition => model.EditType.Value = NoteTypes.Long)
            .Subscribe(_ => model.EditNoteObservable.OnNext(new Note(model.ClosestNotePosition.Value, NoteTypes.Long)));


        // Finish editing long note by press-escape or right-click
        this.UpdateAsObservable()
            .Where(_ => model.EditType.Value == NoteTypes.Long)
            .Where(_ => Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            .Subscribe(_ => model.EditType.Value = NoteTypes.Normal);

        var finishEditLongNoteObservable = model.EditType.DistinctUntilChanged()
            .Where(editType => editType == NoteTypes.Normal)
            .Skip(1);

        finishEditLongNoteObservable.Subscribe(_ => model.LongNoteTailPosition.Value = NotePosition.None);


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


        model.EditNoteObservable.Subscribe(note =>
        {
            if (note.type == NoteTypes.Normal)
            {

                if (model.NoteObjects.ContainsKey(note.position))
                {
                    RemoveNote(note.position);
                }
                else
                {
                    var noteObject = (Instantiate(notePrefab) as GameObject).GetComponent<NoteObject>();
                    noteObject.notePosition = note.position;
                    noteObject.noteType.Value = NoteTypes.Normal;
                    noteObject.transform.SetParent(notesRegion.transform);

                    model.NoteObjects.Add(note.position, noteObject);
                }
            }
            else if (note.type == NoteTypes.Long) {

                if (model.NoteObjects.ContainsKey(note.position))
                {
                    var noteObject = model.NoteObjects[note.position];

                    if (noteObject.noteType.Value == NoteTypes.Long)
                    {

                        if (noteObject.prev != null)
                            noteObject.prev.next = noteObject.next;

                        if (noteObject.next != null)
                            noteObject.next.prev = noteObject.prev;
                        else
                        {
                            model.LongNoteTailPosition.Value = noteObject.prev == null ? NotePosition.None : noteObject.prev.notePosition;
                        }

                        RemoveNote(note.position);
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
                    noteObject.notePosition = note.position;
                    noteObject.noteType.Value = NoteTypes.Long;
                    noteObject.transform.SetParent(notesRegion.transform);

                    model.NoteObjects.Add(note.position, noteObject);
                    model.AddedLongNoteObjectObservable.OnNext(noteObject);
                }
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
