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
    public NoteObject next;
    [HideInInspector]
    public NoteObject prev;
    [HideInInspector]
    public ReactiveProperty<NoteTypes> noteType = new ReactiveProperty<NoteTypes>();
    [HideInInspector]
    public ReactiveProperty<bool> isSelected = new ReactiveProperty<bool>();
    [HideInInspector]
    public RectTransform rectTransform;

    NotesEditorModel model;
    Subject<NoteTypes> onMouseDownObservable = new Subject<NoteTypes>();

    void Start()
    {
        model = NotesEditorModel.Instance;
        rectTransform = GetComponent<RectTransform>();
        rectTransform.localPosition = model.NoteToScreenPosition(notePosition);


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


        var mouseDownObservable = onMouseDownObservable
            .Where(_ => model.ClosestNotePosition.Value.Equals(notePosition));

        var editObservable = mouseDownObservable
            .Where(editType => editType == NoteTypes.Normal)
            .Where(editType => noteType.Value == editType)
            .Merge(mouseDownObservable
                .Where(editType => editType == NoteTypes.Long));

        editObservable.Subscribe(_ => model.EditNoteObservable.OnNext(ToNote()));


        var longNoteLateUpdateObservable = this.LateUpdateAsObservable()
            .Where(_ => noteType.Value == NoteTypes.Long);

        longNoteLateUpdateObservable
            .Where(_ => next != null)
            .Select(_ => model.NoteToScreenPosition(next.notePosition))
            .Merge(longNoteLateUpdateObservable
                .Where(_ => next == null)
                .Where(_ => model.EditType.Value == NoteTypes.Long)
                .Where(_ => model.LongNoteTailPosition.Value.Equals(notePosition))
                .Select(_ => model.ScreenToCanvasPosition(Input.mousePosition)))
            .Select(nextPosition => new Line[] { new Line( model.NoteToScreenPosition(notePosition), nextPosition,
                isSelected.Value || next != null && next.isSelected.Value ? selectedStateColor
                    : 0 < nextPosition.x - model.NoteToScreenPosition(notePosition).x ? longStateColor : invalidStateColor) })
            .Subscribe(lines => GLLineRenderer.RenderLines(notePosition.ToString(), lines));
    }

    public void OnMouseDown()
    {
        onMouseDownObservable.OnNext(model.EditType.Value);
    }

    public Note ToNote()
    {
        var note = new Note(notePosition, noteType.Value);
        note.next = next == null || next.notePosition.num < 0 ? NotePosition.None : next.notePosition;
        note.prev = prev == null || prev.notePosition.num < 0 ? NotePosition.None : prev.notePosition;
        return note;
    }
}
