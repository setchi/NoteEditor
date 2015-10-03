using NoteEditor.Notes;
using NoteEditor.Utility;
using UniRx;

namespace NoteEditor.Model
{
    public class EditState : SingletonMonoBehaviour<EditState>
    {
        ReactiveProperty<bool> isOperatingPlaybackPositionDuringPlay_ = new ReactiveProperty<bool>(false);
        ReactiveProperty<NoteTypes> noteType_ = new ReactiveProperty<NoteTypes>(NoteTypes.Single);
        ReactiveProperty<NotePosition> longNoteTailPosition_ = new ReactiveProperty<NotePosition>();

        public static ReactiveProperty<bool> IsOperatingPlaybackPositionDuringPlay { get { return Instance.isOperatingPlaybackPositionDuringPlay_; } }
        public static ReactiveProperty<NoteTypes> NoteType { get { return Instance.noteType_; } }
        public static ReactiveProperty<NotePosition> LongNoteTailPosition { get { return Instance.longNoteTailPosition_; } }
    }
}
