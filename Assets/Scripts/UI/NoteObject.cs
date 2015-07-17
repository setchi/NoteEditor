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
    public NotePosition notePosition;
    [HideInInspector]
    public NotePosition next;
    [HideInInspector]
    public NotePosition prev;
    [HideInInspector]
    public ReactiveProperty<NoteTypes> noteType = new ReactiveProperty<NoteTypes>();
    [HideInInspector]
    public ReactiveProperty<bool> isSelected = new ReactiveProperty<bool>();
    [HideInInspector]
    public RectTransform rectTransform;

    Subject<NoteTypes> onMouseDownObservable = new Subject<NoteTypes>();

    void Start()
    {
        var model = NotesEditorModel.Instance;
        rectTransform = GetComponent<RectTransform>();
        rectTransform.localPosition = model.NoteToScreenPosition(notePosition);

        var editPresenter = EditNotesPresenter.Instance;


        var image = GetComponent<Image>();
        noteType.Where(_ => !isSelected.Value)
            .Merge(isSelected.Select(_ => noteType.Value))
            .Select(type => type == NoteTypes.Long)
            .Subscribe(isLongNote => image.color = isLongNote ? longStateColor : normalStateColor);

        isSelected.Where(selected => selected)
            .Subscribe(_ => image.color = selectedStateColor);


        this.UpdateAsObservable()
            .Select(_ => model.NoteToScreenPosition(notePosition))
            .DistinctUntilChanged()
            .Subscribe(pos => rectTransform.localPosition = pos);

        this.OnDestroyAsObservable()
            .Where(_ => noteType.Value == NoteTypes.Long)
            .Subscribe(_ => RemoveLink());


        var mouseDownObservable = onMouseDownObservable
            .Where(_ => model.ClosestNotePosition.Value.Equals(notePosition));

        mouseDownObservable.Where(editType => editType == NoteTypes.Normal)
            .Where(editType => editType == noteType.Value)
            .Subscribe(_ => editPresenter.RequestForRemoveNote.OnNext(ToNote()));

        mouseDownObservable.Where(editType => editType == NoteTypes.Long)
            .Where(editType => editType == noteType.Value)
            .Subscribe(_ => editPresenter.RequestForRemoveNote.OnNext(
                new Note(notePosition, model.EditType.Value, next, prev)));


        var longNoteLateUpdateObservable = this.LateUpdateAsObservable()
            .Where(_ => noteType.Value == NoteTypes.Long);

        longNoteLateUpdateObservable
            .Where(_ => model.NoteObjects.ContainsKey(next))
            .Select(_ => model.NoteToScreenPosition(next))
            .Merge(longNoteLateUpdateObservable
                .Where(_ => model.EditType.Value == NoteTypes.Long)
                .Where(_ => model.LongNoteTailPosition.Value.Equals(notePosition))
                .Select(_ => model.ScreenToCanvasPosition(Input.mousePosition)))
            .Select(nextPosition => new Line[] { new Line(model.NoteToScreenPosition(notePosition), nextPosition,
                isSelected.Value || model.NoteObjects.ContainsKey(next) && model.NoteObjects[next].isSelected.Value ? selectedStateColor
                    : 0 < nextPosition.x - model.NoteToScreenPosition(notePosition).x ? longStateColor : invalidStateColor) })
            .Subscribe(lines => GLLineRenderer.RenderLines(notePosition.ToString(), lines));
    }

    public void OnMouseDown()
    {
        onMouseDownObservable.OnNext(NotesEditorModel.Instance.EditType.Value);
    }

    public Note ToNote()
    {
        return new Note(notePosition, noteType.Value, next, prev);
    }

    void RemoveLink()
    {
        var model = NotesEditorModel.Instance;

        if (model.NoteObjects.ContainsKey(prev))
            model.NoteObjects[prev].next = next;

        if (model.NoteObjects.ContainsKey(next))
            model.NoteObjects[next].prev = prev;
    }

    void InsertLink(NotePosition position)
    {
        var model = NotesEditorModel.Instance;

        if (model.NoteObjects.ContainsKey(prev))
            model.NoteObjects[prev].next = position;

        if (model.NoteObjects.ContainsKey(next))
            model.NoteObjects[next].prev = position;
    }

    public void SetState(Note note)
    {
        var model = NotesEditorModel.Instance;

        if (note.type == NoteTypes.Normal)
        {
            RemoveLink();
        }

        notePosition = note.position;
        noteType.Value = note.type;
        next = note.next;
        prev = note.prev;

        if (note.type == NoteTypes.Long)
        {
            InsertLink(notePosition);
            model.LongNoteTailPosition.Value = model.LongNoteTailPosition.Value.Equals(note.prev)
                ? note.position
                : NotePosition.None;
        }
    }
}
