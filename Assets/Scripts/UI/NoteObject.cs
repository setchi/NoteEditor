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
    NotesEditorModel model;
    RectTransform rectTransform;
    Subject<NoteTypes> onMouseDownObservable = new Subject<NoteTypes>();

    void Awake()
    {
        model = NotesEditorModel.Instance;
        rectTransform = GetComponent<RectTransform>();
        rectTransform.localPosition = CalcPosition(notePosition);


        var image = GetComponent<Image>();
        noteType.DistinctUntilChanged()
            .Select(type => type == NoteTypes.Long)
            .Subscribe(isLongNote => image.color = isLongNote ? Color.cyan : new Color(175 / 255f, 1, 78 / 255f));


        this.LateUpdateAsObservable()
            .Select(_ => CalcPosition(notePosition))
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
            .Select(_ => CalcPosition(next.notePosition))
            .Merge(longNoteLateUpdateObservable
                .Where(_ => next == null)
                .Where(_ => model.EditType.Value == NoteTypes.Long)
                .Where(_ => model.LongNoteTailPosition.Value.Equals(notePosition))
                .Select(_ => model.ScreenToCanvasPosition(Input.mousePosition)))
            .Select(nextPosition => new Line[] { new Line(CalcPosition(notePosition), nextPosition, 0 < nextPosition.x - CalcPosition(notePosition).x ? Color.cyan : Color.red) })
            .Subscribe(lines => GLLineRenderer.RenderLines(notePosition.blockNum + "-" + notePosition.samples, lines));
    }

    Vector3 CalcPosition(NotePosition notePosition)
    {
        return new Vector3(
            model.SamplesToScreenPositionX(notePosition.samples),
            model.BlockNumToScreenPositionY(notePosition.blockNum) * model.CanvasScaleFactor.Value,
            0);
    }

    public void OnMouseEnter()
    {
        model.IsMouseOverCanvas.Value = true;
    }

    public void OnMouseDown()
    {
        onMouseDownObservable.OnNext(model.EditType.Value);
    }
}
