using UniRx;

public class BPMSpinBoxPresenter : SpinBoxPresenterBase
{
    protected override ReactiveProperty<int> GetProperty()
    {
        return NotesEditorModel.Instance.BPM;
    }
}
