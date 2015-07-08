using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class CanvasEvents : MonoBehaviour
{
    public Subject<Vector3> NotesRegionOnMouseUpObservable = new Subject<Vector3>();
    public Subject<Vector3> NotesRegionOnMouseExitObservable = new Subject<Vector3>();
    public Subject<Vector3> NotesRegionOnMouseDownObservable = new Subject<Vector3>();
    public Subject<Vector3> NotesRegionOnMouseEnterObservable = new Subject<Vector3>();
    public Subject<Vector3> VerticalLineOnMouseDownObservable = new Subject<Vector3>();
    public Subject<Vector3> WaveformRegionOnMouseDownObservable = new Subject<Vector3>();
    public Subject<float> MouseScrollWheelObservable = new Subject<float>();

    void Awake()
    {
        this.UpdateAsObservable()
            .Select(_ => Input.GetAxis("Mouse ScrollWheel"))
            .Where(delta => delta != 0)
            .Subscribe(MouseScrollWheelObservable.OnNext);

        var model = NotesEditorModel.Instance;
        model.IsMouseOverNotesRegion = NotesRegionOnMouseExitObservable.Select(_ => false)
            .Merge(NotesRegionOnMouseEnterObservable.Select(_ => true))
            .ToReactiveProperty();
    }

    public void NotesRegionOnMouseUp() { NotesRegionOnMouseUpObservable.OnNext(Input.mousePosition); }
    public void NotesRegionOnMouseExit() { NotesRegionOnMouseExitObservable.OnNext(Input.mousePosition); }
    public void NotesRegionOnMouseDown() { NotesRegionOnMouseDownObservable.OnNext(Input.mousePosition); }
    public void NotesRegionOnMouseEnter() { NotesRegionOnMouseEnterObservable.OnNext(Input.mousePosition); }
    public void VerticalLineOnMouseDown() { VerticalLineOnMouseDownObservable.OnNext(Input.mousePosition); }
    public void WaveformRegionOnMouseDown() { WaveformRegionOnMouseDownObservable.OnNext(Input.mousePosition); }
}
