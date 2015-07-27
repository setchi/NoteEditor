using UniRx;

public class LPBSpinBoxPresenter : SpinBoxPresenterBase
{
    protected override ReactiveProperty<int> GetReactiveProperty()
    {
        return NotesEditorModel.Instance.LPB;
    }
}
