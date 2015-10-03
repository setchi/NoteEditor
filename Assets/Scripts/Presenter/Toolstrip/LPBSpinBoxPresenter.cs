using NoteEditor.Model;
using UniRx;

namespace NoteEditor.Presenter
{
    public class LPBSpinBoxPresenter : SpinBoxPresenterBase
    {
        protected override ReactiveProperty<int> GetReactiveProperty()
        {
            return EditData.LPB;
        }
    }
}
