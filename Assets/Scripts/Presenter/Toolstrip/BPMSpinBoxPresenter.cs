using NoteEditor.Model;
using UniRx;

namespace NoteEditor.Presenter
{
    public class BPMSpinBoxPresenter : SpinBoxPresenterBase
    {
        protected override ReactiveProperty<int> GetReactiveProperty()
        {
            return EditData.BPM;
        }
    }
}
