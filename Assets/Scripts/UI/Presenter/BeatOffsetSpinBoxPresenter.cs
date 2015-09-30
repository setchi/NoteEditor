using NoteEditor.UI.Model;
using UniRx;

namespace NoteEditor.UI.Presenter
{
    public class BeatOffsetSpinBoxPresenter : SpinBoxPresenterBase
    {
        protected override ReactiveProperty<int> GetReactiveProperty()
        {
            return NotesEditorModel.Instance.BeatOffsetSamples;
        }
    }
}
