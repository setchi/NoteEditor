using UniRx;

public class LPBSpinBoxPresenter : SpinBoxPresenterBase
{
    protected override ReactiveProperty<int> GetProperty()
    {
        return NotesEditorModel.Instance.LPB;
    }
}
