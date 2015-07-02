using UniRx;
using UnityEngine;

public class CanvasEvents : MonoBehaviour
{
    public Subject<Vector3> VerticalLineOnMouseDownObservable = new Subject<Vector3>();
    public Subject<Vector3> ScrollPadOnMouseEnterObservable = new Subject<Vector3>();
    public Subject<Vector3> ScrollPadOnMouseDownObservable = new Subject<Vector3>();
    public Subject<Vector3> ScrollPadOnMouseExitObservable = new Subject<Vector3>();

    public void ScrollPadOnMouseDown()
    {
        ScrollPadOnMouseDownObservable.OnNext(Input.mousePosition);
    }

    public void ScrollPadOnMouseEnter()
    {
        ScrollPadOnMouseEnterObservable.OnNext(Input.mousePosition);
    }

    public void ScrollPadOnMouseExit()
    {
        ScrollPadOnMouseExitObservable.OnNext(Input.mousePosition);
    }

    public void VerticalLineOnMouseDown()
    {
        VerticalLineOnMouseDownObservable.OnNext(Input.mousePosition);
    }
}
