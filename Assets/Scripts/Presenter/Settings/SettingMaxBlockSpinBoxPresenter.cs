using NoteEditor.Model;
using UniRx;

namespace NoteEditor.Presenter
{
    public class SettingMaxBlockSpinBoxPresenter : SpinBoxPresenterBase
    {
        protected override ReactiveProperty<int> GetReactiveProperty()
        {
            return EditData.MaxBlock;
        }
    }
}
