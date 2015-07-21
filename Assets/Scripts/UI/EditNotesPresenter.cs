using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class EditNotesPresenter : SingletonGameObject<EditNotesPresenter>
{
    [SerializeField]
    GameObject notesRegion;
    [SerializeField]
    CanvasEvents canvasEvents;
    [SerializeField]
    GameObject notePrefab;

    public readonly Subject<Note> RequestForEditNote = new Subject<Note>();
    public readonly Subject<Note> RequestForRemoveNote = new Subject<Note>();
    public readonly Subject<Note> RequestForAddNote = new Subject<Note>();
    public readonly Subject<Note> RequestForChangeNoteStatus = new Subject<Note>();

    NotesEditorModel model;

    void Awake()
    {
        model = NotesEditorModel.Instance;
        model.OnLoadMusicObservable.First().Subscribe(_ => Init());
    }

    void Init()
    {
        var closestNoteAreaOnMouseDownObservable = canvasEvents.NotesRegionOnMouseDownObservable
            .Where(_ => !Input.GetMouseButtonDown(1))
            .Where(_ => 0 <= model.ClosestNotePosition.Value.num);

        closestNoteAreaOnMouseDownObservable
            .Where(_ => model.EditType.Value == NoteTypes.Normal)
            .Where(_ => !KeyInput.ShiftKey())
            .Merge(closestNoteAreaOnMouseDownObservable
                .Where(_ => model.EditType.Value == NoteTypes.Long))
            .Subscribe(_ => RequestForEditNote.OnNext(
                new Note(
                    model.ClosestNotePosition.Value,
                    model.EditType.Value,
                    NotePosition.None,
                    model.LongNoteTailPosition.Value)));


        // Start editing of long note
        closestNoteAreaOnMouseDownObservable
            .Where(_ => model.EditType.Value == NoteTypes.Normal)
            .Where(_ => KeyInput.ShiftKey())
            .Do(_ => model.EditType.Value = NoteTypes.Long)
            .Subscribe(_ => RequestForAddNote.OnNext(
                new Note(
                    model.ClosestNotePosition.Value,
                    NoteTypes.Long,
                    NotePosition.None,
                    NotePosition.None)));


        // Finish editing long note by press-escape or right-click
        this.UpdateAsObservable()
            .Where(_ => model.EditType.Value == NoteTypes.Long)
            .Where(_ => Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            .Subscribe(_ => model.EditType.Value = NoteTypes.Normal);

        var finishEditLongNoteObservable = model.EditType.Where(editType => editType == NoteTypes.Normal);

        finishEditLongNoteObservable.Subscribe(_ => model.LongNoteTailPosition.Value = NotePosition.None);


        RequestForRemoveNote.Buffer(RequestForRemoveNote.ThrottleFrame(1))
            .Select(b => b.OrderBy(note => note.position.ToSamples(model.Audio.clip.frequency, model.BPM.Value)).ToList())
            .Subscribe(notes => UndoRedoManager.Do(
                new Command(
                    () => notes.ForEach(RemoveNote),
                    () => notes.ForEach(AddNote))));

        RequestForAddNote.Buffer(RequestForAddNote.ThrottleFrame(1))
            .Select(b => b.OrderBy(note => note.position.ToSamples(model.Audio.clip.frequency, model.BPM.Value)).ToList())
            .Subscribe(notes => UndoRedoManager.Do(
                new Command(
                    () => notes.ForEach(AddNote),
                    () => notes.ForEach(RemoveNote))));

        RequestForChangeNoteStatus.Select(note => new { current = note, prev = model.NoteObjects[note.position].note })
            .Buffer(RequestForChangeNoteStatus.ThrottleFrame(1))
            .Select(b => b.OrderBy(note => note.current.position.ToSamples(model.Audio.clip.frequency, model.BPM.Value)).ToList())
            .Subscribe(notes => UndoRedoManager.Do(
                new Command(
                    () => notes.ForEach(x => ChangeNoteStates(x.current)),
                    () => notes.ForEach(x => ChangeNoteStates(x.prev)))));


        RequestForEditNote.Subscribe(note =>
        {
            if (note.type == NoteTypes.Normal)
            {
                (model.NoteObjects.ContainsKey(note.position)
                    ? RequestForRemoveNote
                    : RequestForAddNote)
                .OnNext(note);
            }
            else if (note.type == NoteTypes.Long)
            {
                if (!model.NoteObjects.ContainsKey(note.position))
                {
                    RequestForAddNote.OnNext(note);
                    return;
                }

                var noteObject = model.NoteObjects[note.position];
                (noteObject.note.type == NoteTypes.Long
                    ? RequestForRemoveNote
                    : RequestForChangeNoteStatus)
                .OnNext(noteObject.note);
            }
        });
    }

    public void AddNote(Note note)
    {
        if (model.NoteObjects.ContainsKey(note.position))
        {
            if (!model.NoteObjects[note.position].note.Equals(note))
                RequestForChangeNoteStatus.OnNext(note);

            return;
        }

        var noteObject = (Instantiate(notePrefab) as GameObject).GetComponent<NoteObject>();
        noteObject.SetState(note);
        noteObject.transform.SetParent(notesRegion.transform);
        model.NoteObjects.Add(note.position, noteObject);
    }

    void ChangeNoteStates(Note note)
    {
        if (!model.NoteObjects.ContainsKey(note.position))
            return;

        model.NoteObjects[note.position].SetState(note);
    }

    void RemoveNote(Note note)
    {
        if (!model.NoteObjects.ContainsKey(note.position))
            return;

        var noteObject = model.NoteObjects[note.position];
        model.NoteObjects.Remove(noteObject.note.position);
        DestroyObject(noteObject.gameObject);
    }
}
