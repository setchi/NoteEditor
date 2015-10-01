using NoteEditor.UI.Model;
using UniRx;

namespace NoteEditor.UI.Presenter
{
    public class SettingMaxBlockSpinBoxPresenter : SpinBoxPresenterBase
    {
        protected override ReactiveProperty<int> GetReactiveProperty()
        {
            return EditData.MaxBlock;
        }
    }
}
