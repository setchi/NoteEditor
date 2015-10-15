using NoteEditor.Common;
using NoteEditor.Model;
using NoteEditor.Notes;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.Presenter
{
    public class MusicSelectorPresenter : MonoBehaviour
    {
        [SerializeField]
        InputField directoryPathInputField;
        [SerializeField]
        GameObject fileItemPrefab;
        [SerializeField]
        GameObject fileItemContainer;
        [SerializeField]
        Transform fileItemContainerTransform;
        [SerializeField]
        Button redoButton;
        [SerializeField]
        Button undoButton;
        [SerializeField]
        Button loadButton;
        [SerializeField]
        GameObject notesRegion;
        [SerializeField]
        GameObject noteObjectPrefab;

        void Start()
        {
            ResetEditor();

            ChangeLocationCommandManager.CanUndo.SubscribeToInteractable(undoButton);
            ChangeLocationCommandManager.CanRedo.SubscribeToInteractable(redoButton);
            undoButton.OnClickAsObservable().Subscribe(_ => ChangeLocationCommandManager.Undo());
            redoButton.OnClickAsObservable().Subscribe(_ => ChangeLocationCommandManager.Redo());

            Settings.WorkSpacePath
                .Subscribe(workSpacePath => directoryPathInputField.text = Path.Combine(workSpacePath, "Musics"));

            directoryPathInputField.OnValueChangeAsObservable()
                .Subscribe(path => MusicSelector.DirectoryPath.Value = path);

            MusicSelector.DirectoryPath
                .Subscribe(path => directoryPathInputField.text = path);

            var isUndoRedoAction = false;

            MusicSelector.DirectoryPath
                .Where(path => Directory.Exists(path))
                .Buffer(2, 1)
                .Where(_ => isUndoRedoAction ? (isUndoRedoAction = false) : true)
                .Select(b => new { prev = b[0], current = b[1] })
                .Subscribe(path => ChangeLocationCommandManager.Do(new Command(
                    () => { },
                    () => { isUndoRedoAction = true; MusicSelector.DirectoryPath.Value = path.prev; },
                    () => { isUndoRedoAction = true; MusicSelector.DirectoryPath.Value = path.current; })));

            if (!Directory.Exists(MusicSelector.DirectoryPath.Value))
            {
                Directory.CreateDirectory(MusicSelector.DirectoryPath.Value);
            }

            Observable.Timer(TimeSpan.FromMilliseconds(300), TimeSpan.Zero)
                    .Where(_ => Directory.Exists(MusicSelector.DirectoryPath.Value))
                    .Select(_ => new DirectoryInfo(MusicSelector.DirectoryPath.Value))
                    .Select(directoryInfo =>
                        directoryInfo.GetDirectories().Select(directory => new FileItemInfo(true, directory.FullName))
                            .Concat(directoryInfo.GetFiles().Select(file => new FileItemInfo(false, file.FullName)))
                            .ToList())
                    .Where(x => !x.Select(item => item.fullName)
                        .SequenceEqual(MusicSelector.FilePathList.Value.Select(item => item.fullName)))
                    .Subscribe(filePathList => MusicSelector.FilePathList.Value = filePathList);

            MusicSelector.FilePathList.AsObservable()
                .Do(_ => Enumerable.Range(0, fileItemContainerTransform.childCount)
                    .Select(i => fileItemContainerTransform.GetChild(i))
                    .ToList()
                    .ForEach(child => DestroyObject(child.gameObject)))
                .SelectMany(fileItemList => fileItemList)
                .Select(fileItemInfo => new { fileItemInfo, obj = Instantiate(fileItemPrefab) as GameObject })
                .Do(elm => elm.obj.transform.SetParent(fileItemContainer.transform))
                .Subscribe(elm => elm.obj.GetComponent<FileListItem>().SetInfo(elm.fileItemInfo));

            loadButton.OnClickAsObservable()
                .Select(_ => MusicSelector.SelectedFileName.Value)
                    .Where(fileName => !string.IsNullOrEmpty(fileName))
                    .Subscribe(fileName => StartCoroutine(LoadMusic(fileName)));
        }

        IEnumerator LoadMusic(string fileName)
        {
            using (var www = new WWW("file:///" + Path.Combine(MusicSelector.DirectoryPath.Value, fileName)))
            {
                yield return www;

                EditCommandManager.Clear();
                ResetEditor();
                Audio.Source.clip = www.audioClip;

                if (Audio.Source.clip == null)
                {
                    // TODO: 読み込み失敗時の処理
                }
                else
                {
                    EditData.Name.Value = fileName;
                    LoadEditData();
                    Audio.OnLoad.OnNext(Unit.Default);
                }
            }
        }

        void LoadEditData()
        {
            var fileName = Path.ChangeExtension(EditData.Name.Value, "json");
            var directoryPath = Path.Combine(Path.GetDirectoryName(MusicSelector.DirectoryPath.Value), "Notes");
            var filePath = Path.Combine(directoryPath, fileName);

            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                EditDataSerializer.Deserialize(json);
            }
        }

        void ResetEditor()
        {
            Audio.TimeSamples.Value = 0;
            Audio.SmoothedTimeSamples.Value = 0;
            Audio.IsPlaying.Value = false;
            Audio.Source.clip = null;
            EditState.NoteType.Value = NoteTypes.Single;
            EditState.LongNoteTailPosition.Value = NotePosition.None;
            EditData.BPM.Value = 120;
            EditData.OffsetSamples.Value = 0;
            EditData.Name.Value = "Note Editor";
            EditData.MaxBlock.Value = Settings.MaxBlock;
            EditData.LPB.Value = 4;

            foreach (var note in EditData.Notes.Values)
            {
                note.Dispose();
            }

            EditData.Notes.Clear();
            Resources.UnloadUnusedAssets();
        }
    }
}
