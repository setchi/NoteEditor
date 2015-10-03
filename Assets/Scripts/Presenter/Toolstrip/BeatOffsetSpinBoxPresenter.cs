using NoteEditor.Model;
using UniRx;

namespace NoteEditor.Presenter
{
    public class BeatOffsetSpinBoxPresenter : SpinBoxPresenterBase
    {
        protected override ReactiveProperty<int> GetReactiveProperty()
        {
            return EditData.OffsetSamples;
        }
    }
}
