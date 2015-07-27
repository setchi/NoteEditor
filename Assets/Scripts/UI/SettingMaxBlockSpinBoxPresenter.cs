using UniRx;

public class SettingMaxBlockSpinBoxPresenter : SpinBoxPresenterBase
{
    protected override ReactiveProperty<int> GetProperty()
    {
        return NotesEditorModel.Instance.MaxBlock;
    }
}
