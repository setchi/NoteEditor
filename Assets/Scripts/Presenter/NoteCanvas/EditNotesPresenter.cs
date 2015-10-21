using NoteEditor.Common;
using NoteEditor.Notes;
using NoteEditor.Model;
using NoteEditor.Utility;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace NoteEditor.Presenter
{
    public class EditNotesPresenter : SingletonMonoBehaviour<EditNotesPresenter>
    {
        [SerializeField]
        CanvasEvents canvasEvents;

        public readonly Subject<Note> RequestForEditNote = new Subject<Note>();
        public readonly Subject<Note> RequestForRemoveNote = new Subject<Note>();
        public readonly Subject<Note> RequestForAddNote = new Subject<Note>();
        public readonly Subject<Note> RequestForChangeNoteStatus = new Subject<Note>();

        void Awake()
        {
            Audio.OnLoad.First().Subscribe(_ => Init());
        }

        void Init()
        {
            var closestNoteAreaOnMouseDownObservable = canvasEvents.NotesRegionOnMouseDownObservable
                .Where(_ => !KeyInput.CtrlKey())
                .Where(_ => !Input.GetMouseButtonDown(1))
                .Where(_ => 0 <= NoteCanvas.ClosestNotePosition.Value.num);

            closestNoteAreaOnMouseDownObservable
                .Where(_ => EditState.NoteType.Value == NoteTypes.Single)
                .Where(_ => !KeyInput.ShiftKey())
                .Merge(closestNoteAreaOnMouseDownObservable
                    .Where(_ => EditState.NoteType.Value == NoteTypes.Long))
                .Subscribe(_ =>
                {
                    if (EditData.Notes.ContainsKey(NoteCanvas.ClosestNotePosition.Value))
                    {
                        EditData.Notes[NoteCanvas.ClosestNotePosition.Value].OnClickObservable.OnNext(Unit.Default);
                    }
                    else
                    {
                        RequestForEditNote.OnNext(
                           new Note(
                               NoteCanvas.ClosestNotePosition.Value,
                               EditState.NoteType.Value,
                               NotePosition.None,
                               EditState.LongNoteTailPosition.Value));
                    }
                });


            // Start editing of long note
            closestNoteAreaOnMouseDownObservable
                .Where(_ => EditState.NoteType.Value == NoteTypes.Single)
                .Where(_ => KeyInput.ShiftKey())
                .Do(_ => EditState.NoteType.Value = NoteTypes.Long)
                .Subscribe(_ => RequestForAddNote.OnNext(
                    new Note(
                        NoteCanvas.ClosestNotePosition.Value,
                        NoteTypes.Long,
                        NotePosition.None,
                        NotePosition.None)));


            // Finish editing long note by press-escape or right-click
            this.UpdateAsObservable()
                .Where(_ => EditState.NoteType.Value == NoteTypes.Long)
                .Where(_ => Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
                .Subscribe(_ => EditState.NoteType.Value = NoteTypes.Single);

            var finishEditLongNoteObservable = EditState.NoteType.Where(editType => editType == NoteTypes.Single);

            finishEditLongNoteObservable.Subscribe(_ => EditState.LongNoteTailPosition.Value = NotePosition.None);


            RequestForRemoveNote.Buffer(RequestForRemoveNote.ThrottleFrame(1))
                .Select(b => b.OrderBy(note => note.position.ToSamples(Audio.Source.clip.frequency, EditData.BPM.Value)).ToList())
                .Subscribe(notes => EditCommandManager.Do(
                    new Command(
                        () => notes.ForEach(RemoveNote),
                        () => notes.ForEach(AddNote))));

            RequestForAddNote.Buffer(RequestForAddNote.ThrottleFrame(1))
                .Select(b => b.OrderBy(note => note.position.ToSamples(Audio.Source.clip.frequency, EditData.BPM.Value)).ToList())
                .Subscribe(notes => EditCommandManager.Do(
                    new Command(
                        () => notes.ForEach(AddNote),
                        () => notes.ForEach(RemoveNote))));

            RequestForChangeNoteStatus.Select(note => new { current = note, prev = EditData.Notes[note.position].note })
                .Buffer(RequestForChangeNoteStatus.ThrottleFrame(1))
                .Select(b => b.OrderBy(note => note.current.position.ToSamples(Audio.Source.clip.frequency, EditData.BPM.Value)).ToList())
                .Subscribe(notes => EditCommandManager.Do(
                    new Command(
                        () => notes.ForEach(x => ChangeNoteStates(x.current)),
                        () => notes.ForEach(x => ChangeNoteStates(x.prev)))));


            RequestForEditNote.Subscribe(note =>
            {
                if (note.type == NoteTypes.Single)
                {
                    (EditData.Notes.ContainsKey(note.position)
                        ? RequestForRemoveNote
                        : RequestForAddNote)
                    .OnNext(note);
                }
                else if (note.type == NoteTypes.Long)
                {
                    if (!EditData.Notes.ContainsKey(note.position))
                    {
                        RequestForAddNote.OnNext(note);
                        return;
                    }

                    var noteObject = EditData.Notes[note.position];
                    (noteObject.note.type == NoteTypes.Long
                        ? RequestForRemoveNote
                        : RequestForChangeNoteStatus)
                    .OnNext(noteObject.note);
                }
            });
        }

        public void AddNote(Note note)
        {
            if (EditData.Notes.ContainsKey(note.position))
            {
                if (!EditData.Notes[note.position].note.Equals(note))
                    RequestForChangeNoteStatus.OnNext(note);

                return;
            }

            var noteObject = new NoteObject();
            noteObject.SetState(note);
            noteObject.Init();
            EditData.Notes.Add(noteObject.note.position, noteObject);
        }

        void ChangeNoteStates(Note note)
        {
            if (!EditData.Notes.ContainsKey(note.position))
                return;

            EditData.Notes[note.position].SetState(note);
        }

        void RemoveNote(Note note)
        {
            if (!EditData.Notes.ContainsKey(note.position))
                return;

            var noteObject = EditData.Notes[note.position];
            noteObject.Dispose();
            EditData.Notes.Remove(noteObject.note.position);
        }
    }
}
