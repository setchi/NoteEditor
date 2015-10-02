using NoteEditor.Utility;
using System.Collections.Generic;
using UniRx;

namespace NoteEditor.UI.Model
{
    public class MusicSelector : SingletonMonoBehaviour<MusicSelector>
    {
        public readonly ReactiveProperty<string> DirectoryPath = new ReactiveProperty<string>();
        public readonly ReactiveProperty<List<string>> FilePathList = new ReactiveProperty<List<string>>(new List<string>());
        public readonly ReactiveProperty<string> SelectedFileName = new ReactiveProperty<string>();
    }
}
