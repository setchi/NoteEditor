using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class NoteObject : MonoBehaviour
{
    [SerializeField]
    Color selectedStateColor;
    [SerializeField]
    Color normalStateColor;
    [SerializeField]
    Color longStateColor;
    [SerializeField]
    Color invalidStateColor;

    [HideInInspector]
    public Note note = new Note();
    [HideInInspector]
    public ReactiveProperty<bool> isSelected = new ReactiveProperty<bool>();
    [HideInInspector]
    public RectTransform rectTransform;

    Subject<NoteTypes> onMouseDownObservable = new Subject<NoteTypes>();
    ReactiveProperty<NoteTypes> noteType = new ReactiveProperty<NoteTypes>();

    void Start()
    {
        var model = NotesEditorModel.Instance;
        rectTransform = GetComponent<RectTransform>();
        rectTransform.localPosition = model.NoteToCanvasPosition(note.position);

        var editPresenter = EditNotesPresenter.Instance;

        noteType = this.ObserveEveryValueChanged(_ => note.type).ToReactiveProperty();

        var image = GetComponent<Image>();
        noteType.Where(_ => !isSelected.Value)
            .Merge(isSelected.Select(_ => noteType.Value))
            .Select(type => type == NoteTypes.Long)
            .Subscribe(isLongNote => image.color = isLongNote ? longStateColor : normalStateColor);

        isSelected.Where(selected => selected)
            .Subscribe(_ => image.color = selectedStateColor);


        this.UpdateAsObservable()
            .Select(_ => model.NoteToCanvasPosition(note.position))
            .DistinctUntilChanged()
            .Subscribe(pos => rectTransform.localPosition = pos);


        var mouseDownObservable = onMouseDownObservable
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


        var longNoteLateUpdateObservable = this.LateUpdateAsObservable()
            .Where(_ => noteType.Value == NoteTypes.Long);

        longNoteLateUpdateObservable
            .Where(_ => model.NoteObjects.ContainsKey(note.next))
            .Select(_ => model.NoteToCanvasPosition(note.next))
            .Merge(longNoteLateUpdateObservable
                .Where(_ => model.EditType.Value == NoteTypes.Long)
                .Where(_ => model.LongNoteTailPosition.Value.Equals(note.position))
                .Select(_ => model.ScreenToCanvasPosition(Input.mousePosition)))
            .Select(nextPosition => new Line[] { new Line(
                model.CanvasToScreenPosition(model.NoteToCanvasPosition(note.position)),
                model.CanvasToScreenPosition(nextPosition),
                isSelected.Value || model.NoteObjects.ContainsKey(note.next) && model.NoteObjects[note.next].isSelected.Value ? selectedStateColor
                    : 0 < nextPosition.x - model.NoteToCanvasPosition(note.position).x ? longStateColor : invalidStateColor) })
            .Subscribe(lines => GLLineRenderer.Render(note.position.ToString(), lines));
    }

    public void OnMouseDown()
    {
        onMouseDownObservable.OnNext(NotesEditorModel.Instance.EditType.Value);
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
