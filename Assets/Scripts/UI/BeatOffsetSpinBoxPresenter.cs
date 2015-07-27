using UniRx;

public class BeatOffsetSpinBoxPresenter : SpinBoxPresenterBase
{
    protected override ReactiveProperty<int> GetProperty()
    {
        return NotesEditorModel.Instance.BeatOffsetSamples;
    }
}
