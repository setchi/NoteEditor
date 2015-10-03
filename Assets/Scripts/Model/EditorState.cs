using NoteEditor.Utility;
using UniRx;

namespace NoteEditor.Model
{
    public class EditorState : SingletonMonoBehaviour<EditorState>
    {
        ReactiveProperty<bool> waveformDisplayEnabled_ = new ReactiveProperty<bool>(true);
        ReactiveProperty<bool> clapSoundEffectEnabled_ = new ReactiveProperty<bool>(true);

        public static ReactiveProperty<bool> WaveformDisplayEnabled { get { return Instance.waveformDisplayEnabled_; } }
        public static ReactiveProperty<bool> ClapSoundEffectEnabled { get { return Instance.clapSoundEffectEnabled_; } }
    }
}
