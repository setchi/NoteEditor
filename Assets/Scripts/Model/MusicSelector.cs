using NoteEditor.Utility;
using System.Collections.Generic;
using UniRx;

namespace NoteEditor.Model
{
    public class MusicSelector : SingletonMonoBehaviour<MusicSelector>
    {
        ReactiveProperty<string> directoryPath_ = new ReactiveProperty<string>();
        ReactiveProperty<List<FileItemInfo>> filePathList_ = new ReactiveProperty<List<FileItemInfo>>(new List<FileItemInfo>());
        ReactiveProperty<string> selectedFileName_ = new ReactiveProperty<string>();

        public static ReactiveProperty<string> DirectoryPath { get { return Instance.directoryPath_; } }
        public static ReactiveProperty<List<FileItemInfo>> FilePathList { get { return Instance.filePathList_; } }
        public static ReactiveProperty<string> SelectedFileName { get { return Instance.selectedFileName_; } }
    }
}
