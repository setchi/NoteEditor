using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class NoteObject : MonoBehaviour
{
    public NotePosition notePosition;
    public NoteObject next;
    public NoteObject prev;
    public ReactiveProperty<NoteTypeEnum> noteType = new ReactiveProperty<NoteTypeEnum>();
    NotesEditorModel model;
    RectTransform rectTransform;
    Subject<NoteTypeEnum> onMouseDownObservable = new Subject<NoteTypeEnum>();

    void Awake()
    {
        model = NotesEditorModel.Instance;
        rectTransform = GetComponent<RectTransform>();
        rectTransform.localPosition = CalcPosition(notePosition);


        var image = GetComponent<Image>();
        noteType.DistinctUntilChanged()
            .Select(type => type == NoteTypeEnum.LongNotes)
            .Subscribe(isLongNote => image.color = isLongNote ? Color.cyan : Color.white);


        this.LateUpdateAsObservable()
            .Select(_ => CalcPosition(notePosition))
            .DistinctUntilChanged()
            .Subscribe(pos => rectTransform.localPosition = pos);


        var mouseDownObservable = onMouseDownObservable
            .Where(_ => model.ClosestNotePosition.Value.Equals(notePosition));

        var editObservable = mouseDownObservable
            .Where(editType => editType == NoteTypeEnum.NormalNotes)
            .Where(editType => noteType.Value == editType)
            .Merge(mouseDownObservable
                .Where(editType => editType == NoteTypeEnum.LongNotes));

        editObservable.Where(editType => editType == NoteTypeEnum.NormalNotes)
            .Subscribe(_ => model.NormalNoteObservable.OnNext(notePosition));

        editObservable.Where(editType => editType == NoteTypeEnum.LongNotes)
            .Subscribe(_ => model.LongNoteObservable.OnNext(notePosition));


        var drawLineObservable = this.LateUpdateAsObservable()
            .Where(_ => noteType.Value == NoteTypeEnum.LongNotes);


        var lastAddLongNote = model.AddLongNoteObjectObservable.ToReactiveProperty();
        drawLineObservable
            .Where(_ => next == null)
            .Where(_ => model.EditType.Value == NoteTypeEnum.LongNotes)
            .Where(_ => lastAddLongNote.Value.notePosition.Equals(notePosition))
            .Select(_ => model.ScreenToCanvasPosition(Input.mousePosition))
            .Where(nextPosition => 0 < nextPosition.x - CalcPosition(notePosition).x)
            .Merge(drawLineObservable
                .Where(_ => next != null)
                .Select(_ => CalcPosition(next.notePosition)))
            .Select(nextPosition => new Line[] { new Line(CalcPosition(notePosition), nextPosition, Color.cyan) })
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
