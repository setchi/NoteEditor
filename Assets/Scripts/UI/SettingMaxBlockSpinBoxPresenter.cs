using UniRx;

public class SettingMaxBlockSpinBoxPresenter : SpinBoxPresenterBase
{
    protected override ReactiveProperty<int> GetReactiveProperty()
    {
        return NotesEditorModel.Instance.MaxBlock;
    }
}
