using NoteEditor.GLDrawing;
using NoteEditor.Model;
using NoteEditor.Presenter;
using NoteEditor.Utility;
using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace NoteEditor.Notes
{
    public class NoteObject : IDisposable
    {
        public Note note = new Note();
        public ReactiveProperty<bool> isSelected = new ReactiveProperty<bool>();
        public Subject<Unit> LateUpdateObservable = new Subject<Unit>();
        public Subject<Unit> OnClickObservable = new Subject<Unit>();
        public Color NoteColor { get { return noteColor_.Value; } }
        ReactiveProperty<Color> noteColor_ = new ReactiveProperty<Color>();

        Color selectedStateColor = new Color(255 / 255f, 0 / 255f, 255 / 255f);
        Color singleNoteColor = new Color(175 / 255f, 255 / 255f, 78 / 255f);
        Color longNoteColor = new Color(0 / 255f, 255 / 255f, 255 / 255f);
        Color invalidStateColor = new Color(255 / 255f, 0 / 255f, 0 / 255f);

        ReactiveProperty<NoteTypes> noteType = new ReactiveProperty<NoteTypes>();
        CompositeDisposable disposable = new CompositeDisposable();

        public void Init()
        {
            disposable = new CompositeDisposable(
                isSelected,
                LateUpdateObservable,
                OnClickObservable,
                noteColor_,
                noteType);

            var editPresenter = EditNotesPresenter.Instance;
            noteType = this.ObserveEveryValueChanged(_ => note.type).ToReactiveProperty();

            disposable.Add(noteType.Where(_ => !isSelected.Value)
                .Merge(isSelected.Select(_ => noteType.Value))
                .Select(type => type == NoteTypes.Long)
                .Subscribe(isLongNote => noteColor_.Value = isLongNote ? longNoteColor : singleNoteColor));

            disposable.Add(isSelected.Where(selected => selected)
                .Subscribe(_ => noteColor_.Value = selectedStateColor));

            var mouseDownObservable = OnClickObservable
                .Select(_ => EditState.NoteType.Value)
                .Where(_ => NoteCanvas.ClosestNotePosition.Value.Equals(note.position));

            disposable.Add(mouseDownObservable.Where(editType => editType == NoteTypes.Single)
                .Where(editType => editType == noteType.Value)
                .Subscribe(_ => editPresenter.RequestForRemoveNote.OnNext(note)));

            disposable.Add(mouseDownObservable.Where(editType => editType == NoteTypes.Long)
                .Where(editType => editType == noteType.Value)
                .Subscribe(_ =>
                {
                    if (EditData.Notes.ContainsKey(EditState.LongNoteTailPosition.Value) && note.prev.Equals(NotePosition.None))
                    {
                        var currentTailNote = new Note(EditData.Notes[EditState.LongNoteTailPosition.Value].note);
                        currentTailNote.next = note.position;
                        editPresenter.RequestForChangeNoteStatus.OnNext(currentTailNote);

                        var selfNote = new Note(note);
                        selfNote.prev = currentTailNote.position;
                        editPresenter.RequestForChangeNoteStatus.OnNext(selfNote);
                    }
                    else
                    {
                        if (EditData.Notes.ContainsKey(note.prev) && !EditData.Notes.ContainsKey(note.next))
                            EditState.LongNoteTailPosition.Value = note.prev;

                        editPresenter.RequestForRemoveNote.OnNext(new Note(note.position, EditState.NoteType.Value, note.next, note.prev));
                        RemoveLink();
                    }
                }));

            var longNoteUpdateObservable = LateUpdateObservable
                .Where(_ => noteType.Value == NoteTypes.Long);

            disposable.Add(longNoteUpdateObservable
                .Where(_ => EditData.Notes.ContainsKey(note.next))
                .Select(_ => ConvertUtils.NoteToCanvasPosition(note.next))
                .Merge(longNoteUpdateObservable
                    .Where(_ => EditState.NoteType.Value == NoteTypes.Long)
                    .Where(_ => EditState.LongNoteTailPosition.Value.Equals(note.position))
                    .Select(_ => ConvertUtils.ScreenToCanvasPosition(Input.mousePosition)))
                .Select(nextPosition => new Line(
                    ConvertUtils.CanvasToScreenPosition(ConvertUtils.NoteToCanvasPosition(note.position)),
                    ConvertUtils.CanvasToScreenPosition(nextPosition),
                    isSelected.Value || EditData.Notes.ContainsKey(note.next) && EditData.Notes[note.next].isSelected.Value ? selectedStateColor
                        : 0 < nextPosition.x - ConvertUtils.NoteToCanvasPosition(note.position).x ? longNoteColor : invalidStateColor))
                .Subscribe(line => GLLineDrawer.Draw(line)));
        }

        void RemoveLink()
        {
            if (EditData.Notes.ContainsKey(note.prev))
                EditData.Notes[note.prev].note.next = note.next;

            if (EditData.Notes.ContainsKey(note.next))
                EditData.Notes[note.next].note.prev = note.prev;
        }

        void InsertLink(NotePosition position)
        {
            if (EditData.Notes.ContainsKey(note.prev))
                EditData.Notes[note.prev].note.next = position;

            if (EditData.Notes.ContainsKey(note.next))
                EditData.Notes[note.next].note.prev = position;
        }

        public void SetState(Note note)
        {
            if (note.type == NoteTypes.Single)
            {
                RemoveLink();
            }

            this.note = note;

            if (note.type == NoteTypes.Long)
            {
                InsertLink(note.position);
                EditState.LongNoteTailPosition.Value = EditState.LongNoteTailPosition.Value.Equals(note.prev)
                    ? note.position
                    : NotePosition.None;
            }
        }

        public void Dispose()
        {
            disposable.Dispose();
        }
    }
}
