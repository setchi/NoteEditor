using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class NoteObject : MonoBehaviour
{
    public NotePosition notePosition;
    public NoteObject next;
    public NoteObject prev;
    public ReactiveProperty<NoteTypes> noteType = new ReactiveProperty<NoteTypes>();
    public ReactiveProperty<bool> isSelected = new ReactiveProperty<bool>();
    public RectTransform rectTransform;
    NotesEditorModel model;
    Subject<NoteTypes> onMouseDownObservable = new Subject<NoteTypes>();

    void Awake()
    {
        model = NotesEditorModel.Instance;
        rectTransform = GetComponent<RectTransform>();
        rectTransform.localPosition = model.NoteToScreenPosition(notePosition);


        var image = GetComponent<Image>();
        noteType.DistinctUntilChanged()
            .Where(_ => !isSelected.Value)
            .Merge(isSelected.Select(_ => noteType.Value))
            .Select(type => type == NoteTypes.Long)
            .Subscribe(isLongNote => image.color = isLongNote ? Color.cyan : new Color(175 / 255f, 1, 78 / 255f));

        isSelected.Where(selected => selected)
            .Subscribe(_ => image.color = Color.magenta);


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

        editObservable.Where(editType => editType == NoteTypes.Normal)
            .Subscribe(_ => model.NormalNoteObservable.OnNext(notePosition));

        editObservable.Where(editType => editType == NoteTypes.Long)
            .Subscribe(_ => model.LongNoteObservable.OnNext(notePosition));


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
            .Select(nextPosition => new Line[] { new Line(model.NoteToScreenPosition(notePosition), nextPosition, 0 < nextPosition.x - model.NoteToScreenPosition(notePosition).x ? Color.cyan : Color.red) })
            .Subscribe(lines => GLLineRenderer.RenderLines(notePosition.ToString(), lines));
    }

    public void OnMouseDown()
    {
        onMouseDownObservable.OnNext(model.EditType.Value);
    }
}
