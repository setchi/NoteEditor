using NoteEditor.GLDrawing;
using NoteEditor.Notes;
using NoteEditor.UI.Model;
using System.Linq;
using UniRx;
using UnityEngine;

namespace NoteEditor.UI.Presenter
{
    public class NoteObject : DisposableHolder
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

        public void Init()
        {
            Disposable(
                isSelected,
                LateUpdateObservable,
                OnClickObservable,
                noteColor_,
                noteType);

            var model = NotesEditorModel.Instance;
            var editPresenter = EditNotesPresenter.Instance;
            noteType = this.ObserveEveryValueChanged(_ => note.type).ToReactiveProperty();

            Disposable(noteType.Where(_ => !isSelected.Value)
                .Merge(isSelected.Select(_ => noteType.Value))
                .Select(type => type == NoteTypes.Long)
                .Subscribe(isLongNote => noteColor_.Value = isLongNote ? longNoteColor : singleNoteColor));

            Disposable(isSelected.Where(selected => selected)
                .Subscribe(_ => noteColor_.Value = selectedStateColor));

            var mouseDownObservable = OnClickObservable
                .Select(_ => model.EditType.Value)
                .Where(_ => model.ClosestNotePosition.Value.Equals(note.position));

            Disposable(mouseDownObservable.Where(editType => editType == NoteTypes.Single)
                .Where(editType => editType == noteType.Value)
                .Subscribe(_ => editPresenter.RequestForRemoveNote.OnNext(note)));

            Disposable(mouseDownObservable.Where(editType => editType == NoteTypes.Long)
                .Where(editType => editType == noteType.Value)
                .Subscribe(_ =>
                {
                    if (model.NoteObjects.ContainsKey(model.LongNoteTailPosition.Value) && note.prev.Equals(NotePosition.None))
                    {
                        var currentTailNote = new Note(model.NoteObjects[model.LongNoteTailPosition.Value].note);
                        currentTailNote.next = note.position;
                        editPresenter.RequestForChangeNoteStatus.OnNext(currentTailNote);

                        var selfNote = new Note(note);
                        selfNote.prev = currentTailNote.position;
                        editPresenter.RequestForChangeNoteStatus.OnNext(selfNote);
                    }
                    else
                    {
                        if (model.NoteObjects.ContainsKey(note.prev) && !model.NoteObjects.ContainsKey(note.next))
                            model.LongNoteTailPosition.Value = note.prev;

                        editPresenter.RequestForRemoveNote.OnNext(new Note(note.position, model.EditType.Value, note.next, note.prev));
                        RemoveLink();
                    }
                }));

            var longNoteUpdateObservable = LateUpdateObservable
                .Where(_ => noteType.Value == NoteTypes.Long);

            Disposable(longNoteUpdateObservable
                .Where(_ => model.NoteObjects.ContainsKey(note.next))
                .Select(_ => ConvertUtils.NoteToCanvasPosition(note.next))
                .Merge(longNoteUpdateObservable
                    .Where(_ => model.EditType.Value == NoteTypes.Long)
                    .Where(_ => model.LongNoteTailPosition.Value.Equals(note.position))
                    .Select(_ => ConvertUtils.ScreenToCanvasPosition(Input.mousePosition)))
                .Select(nextPosition => new Line(
                    ConvertUtils.CanvasToScreenPosition(ConvertUtils.NoteToCanvasPosition(note.position)),
                    ConvertUtils.CanvasToScreenPosition(nextPosition),
                    isSelected.Value || model.NoteObjects.ContainsKey(note.next) && model.NoteObjects[note.next].isSelected.Value ? selectedStateColor
                        : 0 < nextPosition.x - ConvertUtils.NoteToCanvasPosition(note.position).x ? longNoteColor : invalidStateColor))
                .Subscribe(line => GLLineDrawer.Draw(line)));
        }

        void RemoveLink()
        {
            var model = NotesEditorModel.Instance;

            if (model.NoteObjects.ContainsKey(note.prev))
                model.NoteObjects[note.prev].note.next = note.next;

            if (model.NoteObjects.ContainsKey(note.next))
                model.NoteObjects[note.next].note.prev = note.prev;
        }

        void InsertLink(NotePosition position)
        {
            var model = NotesEditorModel.Instance;

            if (model.NoteObjects.ContainsKey(note.prev))
                model.NoteObjects[note.prev].note.next = position;

            if (model.NoteObjects.ContainsKey(note.next))
                model.NoteObjects[note.next].note.prev = position;
        }

        public void SetState(Note note)
        {
            var model = NotesEditorModel.Instance;

            if (note.type == NoteTypes.Single)
            {
                RemoveLink();
            }

            this.note = note;

            if (note.type == NoteTypes.Long)
            {
                InsertLink(note.position);
                model.LongNoteTailPosition.Value = model.LongNoteTailPosition.Value.Equals(note.prev)
                    ? note.position
                    : NotePosition.None;
            }
        }
    }
}
