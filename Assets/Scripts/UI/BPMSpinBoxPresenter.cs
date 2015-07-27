using UniRx;

public class BPMSpinBoxPresenter : SpinBoxPresenterBase
{
    protected override ReactiveProperty<int> GetReactiveProperty()
    {
        return NotesEditorModel.Instance.BPM;
    }
}
