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
        model.OnLoadedMusicObservable.First().Subscribe(_ => Init());
    }

    void Init()
    {
        var closestNoteAreaOnMouseDownObservable = canvasEvents.NotesRegionOnMouseDownObservable
            .Where(_ => !Input.GetMouseButtonDown(1))
            .Where(_ => 0 <= model.ClosestNotePosition.Value.num);

        closestNoteAreaOnMouseDownObservable
            .Where(_ => !KeyInput.ShiftKey())
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
            .Do(notePosition => model.EditType.Value = NoteTypes.Long)
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

        var finishEditLongNoteObservable = model.EditType.DistinctUntilChanged()
            .Where(editType => editType == NoteTypes.Normal);

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

        RequestForChangeNoteStatus.Select(note => new { current = note, prev = model.NoteObjects[note.position].ToNote() })
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
                if (!model.NoteObjects.ContainsKey(note.position))
                {
                    RequestForAddNote.OnNext(note);
                    return;
                }
                RequestForRemoveNote.OnNext(note);
            }
            else if (note.type == NoteTypes.Long)
            {
                if (!model.NoteObjects.ContainsKey(note.position))
                {
                    RequestForAddNote.OnNext(note);
                    return;
                }

                var noteObject = model.NoteObjects[note.position];
                (noteObject.noteType.Value == NoteTypes.Long
                    ? RequestForRemoveNote
                    : RequestForChangeNoteStatus)
                .OnNext(noteObject.ToNote());
            }
        });
    }

    public void AddNote(Note note)
    {
        if (model.NoteObjects.ContainsKey(note.position))
        {
            RequestForChangeNoteStatus.OnNext(note);
            return;
        }

        var noteObject = (Instantiate(notePrefab) as GameObject).GetComponent<NoteObject>();
        noteObject.notePosition = note.position;
        noteObject.noteType.Value = note.type;
        noteObject.transform.SetParent(notesRegion.transform);
        noteObject.next = note.next;
        noteObject.prev = note.prev;
        model.NoteObjects.Add(note.position, noteObject);

        if (model.NoteObjects.ContainsKey(note.prev))
        {
            model.NoteObjects[note.prev].next = noteObject.notePosition;
        }

        if (model.NoteObjects.ContainsKey(note.next))
        {
            model.NoteObjects[note.next].prev = noteObject.notePosition;
        }

        if (note.type == NoteTypes.Long)
        {
            UpdateLongNoteTailPosition(noteObject, note);
        }
    }

    void ChangeNoteStates(Note note)
    {
        if (!model.NoteObjects.ContainsKey(note.position) || model.NoteObjects[note.position].ToNote().Equals(note))
            return;

        var noteObject = model.NoteObjects[note.position];
        noteObject.notePosition = note.position;
        noteObject.noteType.Value = note.type;
        noteObject.next = note.next;
        noteObject.prev = note.prev;

        if (model.NoteObjects.ContainsKey(note.prev))
        {
            model.NoteObjects[note.prev].next = noteObject.notePosition;
        }

        if (model.NoteObjects.ContainsKey(note.next))
        {
            model.NoteObjects[note.next].prev = noteObject.notePosition;
        }

        if (note.type == NoteTypes.Long)
        {
            UpdateLongNoteTailPosition(noteObject, note);
        }
    }

    void UpdateLongNoteTailPosition(NoteObject noteObject, Note note)
    {
        model.LongNoteTailPosition.Value = model.LongNoteTailPosition.Value.Equals(note.prev)
            ? note.position
            : NotePosition.None;
    }

    void RemoveNote(Note note)
    {
        if (!model.NoteObjects.ContainsKey(note.position))
            return;

        var noteObject = model.NoteObjects[note.position];
        model.NoteObjects.Remove(noteObject.notePosition);
        DestroyObject(noteObject.gameObject);
    }
}
