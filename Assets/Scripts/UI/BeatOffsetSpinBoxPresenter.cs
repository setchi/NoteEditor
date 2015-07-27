using UniRx;

public class BeatOffsetSpinBoxPresenter : SpinBoxPresenterBase
{
    protected override ReactiveProperty<int> GetReactiveProperty()
    {
        return NotesEditorModel.Instance.BeatOffsetSamples;
    }
}
