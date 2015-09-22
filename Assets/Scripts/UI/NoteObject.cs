using System.Linq;
using UniRx;
using UnityEngine;

public class NoteObject
{
    [SerializeField]
    Color selectedStateColor = new Color(255 / 255f, 0 / 255f, 255 / 255f);
    [SerializeField]
    Color normalStateColor = new Color(175 / 255f, 255 / 255f, 78 / 255f);
    [SerializeField]
    Color longStateColor = new Color(0 / 255f, 255 / 255f, 255 / 255f);
    [SerializeField]
    Color invalidStateColor = new Color(255 / 255f, 0 / 255f, 0 / 255f);

    [HideInInspector]
    public Note note = new Note();
    [HideInInspector]
    public ReactiveProperty<bool> isSelected = new ReactiveProperty<bool>();
    ReactiveProperty<Color> noteColor_ = new ReactiveProperty<Color>();

    [HideInInspector]
    public Subject<Unit> LateUpdateObservable = new Subject<Unit>();
    [HideInInspector]
    public Subject<Unit> OnClickObservable = new Subject<Unit>();
    [HideInInspector]
    public Color NoteColor { get { return noteColor_.Value; } }

    public void Init()
    {
        var model = NotesEditorModel.Instance;
        var editPresenter = EditNotesPresenter.Instance;
        var noteType = this.ObserveEveryValueChanged(_ => note.type).ToReactiveProperty();

        noteType.Where(_ => !isSelected.Value)
            .Merge(isSelected.Select(_ => noteType.Value))
            .Select(type => type == NoteTypes.Long)
            .Subscribe(isLongNote => noteColor_.Value = isLongNote ? longStateColor : normalStateColor);

        isSelected.Where(selected => selected)
            .Subscribe(_ => noteColor_.Value = selectedStateColor);


        var mouseDownObservable = OnClickObservable
            .Select(_ => model.EditType.Value)
            .Where(_ => model.ClosestNotePosition.Value.Equals(note.position));

        mouseDownObservable.Where(editType => editType == NoteTypes.Normal)
            .Where(editType => editType == noteType.Value)
            .Subscribe(_ => editPresenter.RequestForRemoveNote.OnNext(note));

        mouseDownObservable.Where(editType => editType == NoteTypes.Long)
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
            });


        var longNoteUpdateObservable = LateUpdateObservable
            .Where(_ => noteType.Value == NoteTypes.Long);

        longNoteUpdateObservable
            .Where(_ => model.NoteObjects.ContainsKey(note.next))
            .Select(_ => model.NoteToCanvasPosition(note.next))
            .Merge(longNoteUpdateObservable
                .Where(_ => model.EditType.Value == NoteTypes.Long)
                .Where(_ => model.LongNoteTailPosition.Value.Equals(note.position))
                .Select(_ => model.ScreenToCanvasPosition(Input.mousePosition)))
            .Select(nextPosition => new Line(
                model.CanvasToScreenPosition(model.NoteToCanvasPosition(note.position)),
                model.CanvasToScreenPosition(nextPosition),
                isSelected.Value || model.NoteObjects.ContainsKey(note.next) && model.NoteObjects[note.next].isSelected.Value ? selectedStateColor
                    : 0 < nextPosition.x - model.NoteToCanvasPosition(note.position).x ? longStateColor : invalidStateColor))
            .Subscribe(line => GLLineRenderer.Render(line));
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

        if (note.type == NoteTypes.Normal)
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
