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
        GameObject fileItem;
        [SerializeField]
        GameObject fileItemContainer;
        [SerializeField]
        Transform fileItemContainerTransform;
        [SerializeField]
        Button loadButton;
        [SerializeField]
        GameObject notesRegion;
        [SerializeField]
        GameObject noteObjectPrefab;

        void Start()
        {
            ResetEditor();

            Settings.WorkSpaceDirectoryPath
                .Subscribe(workSpacePath => directoryPathInputField.text = workSpacePath + "/Musics/");

            directoryPathInputField.OnValueChangeAsObservable()
                .Subscribe(path => MusicSelector.DirectoryPath.Value = path);

            MusicSelector.DirectoryPath
                .Subscribe(path => directoryPathInputField.text = path);


            if (!Directory.Exists(MusicSelector.DirectoryPath.Value))
            {
                Directory.CreateDirectory(MusicSelector.DirectoryPath.Value);
            }

            Observable.Timer(TimeSpan.FromMilliseconds(300), TimeSpan.Zero)
                    .Where(_ => Directory.Exists(MusicSelector.DirectoryPath.Value))
                    .Select(_ => new DirectoryInfo(MusicSelector.DirectoryPath.Value).GetFiles())
                    .Select(fileInfo => fileInfo.Select(file => file.FullName).ToList())
                    .Where(x => !x.SequenceEqual(MusicSelector.FilePathList.Value))
                    .Subscribe(filePathList => MusicSelector.FilePathList.Value = filePathList);

            MusicSelector.FilePathList.AsObservable()
                .Select(filePathList => filePathList.Select(path => Path.GetFileName(path)))
                .Do(_ => Enumerable.Range(0, fileItemContainerTransform.childCount)
                    .Select(i => fileItemContainerTransform.GetChild(i))
                    .ToList()
                    .ForEach(child => DestroyObject(child.gameObject)))
                .SelectMany(fileNameList => fileNameList)
                    .Select(fileName => new { fileName, obj = Instantiate(fileItem) as GameObject })
                    .Do(elm => elm.obj.transform.SetParent(fileItemContainer.transform))
                    .Subscribe(elm => elm.obj.GetComponent<FileListItem>().SetName(elm.fileName));

            loadButton.OnClickAsObservable()
                .Select(_ => MusicSelector.SelectedFileName.Value)
                    .Where(fileName => !string.IsNullOrEmpty(fileName))
                    .Subscribe(fileName => StartCoroutine(LoadMusic(fileName)));
        }

        IEnumerator LoadMusic(string fileName)
        {
            using (var www = new WWW("file:///" + MusicSelector.DirectoryPath.Value + fileName))
            {
                yield return www;

                UndoRedoManager.Clear();
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
            var fileName = Path.GetFileNameWithoutExtension(EditData.Name.Value) + ".json";
            var directoryPath = MusicSelector.DirectoryPath.Value.Substring(0, MusicSelector.DirectoryPath.Value.Length - "/Musics/".Length) + "/Notes/";
            var filePath = directoryPath + fileName;

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
