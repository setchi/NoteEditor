using NoteEditor.Utility;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace NoteEditor.Model
{
    public class Settings : SingletonMonoBehaviour<Settings>
    {
        ReactiveProperty<string> workSpaceDirectoryPath_ = new ReactiveProperty<string>();
        ReactiveProperty<List<KeyCode>> noteInputKeyCodes_ = new ReactiveProperty<List<KeyCode>>();
        ReactiveProperty<int> selectedBlock_ = new ReactiveProperty<int>();
        ReactiveProperty<bool> isOpen_ = new ReactiveProperty<bool>(false);
        Subject<Unit> requestForChangeInputNoteKeyCode_ = new Subject<Unit>();

        public static ReactiveProperty<string> WorkSpaceDirectoryPath { get { return Instance.workSpaceDirectoryPath_; } }
        public static ReactiveProperty<List<KeyCode>> NoteInputKeyCodes { get { return Instance.noteInputKeyCodes_; } }
        public static ReactiveProperty<int> SelectedBlock { get { return Instance.selectedBlock_; } }
        public static ReactiveProperty<bool> IsOpen { get { return Instance.isOpen_; } }
        public static Subject<Unit> RequestForChangeInputNoteKeyCode { get { return Instance.requestForChangeInputNoteKeyCode_; } }
        public static int MaxBlock = 0;
    }
}
