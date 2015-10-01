using NoteEditor.UI.Model;
using UniRx;

namespace NoteEditor.UI.Presenter
{
    public class LPBSpinBoxPresenter : SpinBoxPresenterBase
    {
        protected override ReactiveProperty<int> GetReactiveProperty()
        {
            return EditData.LPB;
        }
    }
}
