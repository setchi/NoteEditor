using LitJson;
using NoteEditor.Common;
using NoteEditor.JSONModel;
using NoteEditor.Notes;
using NoteEditor.UI.Model;
using NoteEditor.Utility;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.UI.Presenter
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
        Button LoadButton;
        [SerializeField]
        GameObject notesRegion;

        /*
        [SerializeField]
        Text selectedFileNameText;
        */
        [SerializeField]
        GameObject noteObjectPrefab;

        MusicSelectorModel model;

        void Start()
        {
            ClearNotesData();

            model = MusicSelectorModel.Instance;
            directoryPathInputField.OnValueChangeAsObservable()
                .Subscribe(path => model.DirectoryPath.Value = path);
            model.DirectoryPath.Subscribe(path => directoryPathInputField.text = path);
            model.DirectoryPath.Value = NoteEditorSettingsModel.Instance.WorkSpaceDirectoryPath.Value + "/Musics/";


            if (!Directory.Exists(model.DirectoryPath.Value))
            {
                Directory.CreateDirectory(model.DirectoryPath.Value);
            }


            Observable.Timer(TimeSpan.FromMilliseconds(300), TimeSpan.Zero)
                    .Where(_ => Directory.Exists(model.DirectoryPath.Value))
                    .Select(_ => new DirectoryInfo(model.DirectoryPath.Value).GetFiles())
                    .Select(fileInfo => fileInfo.Select(file => file.FullName).ToList())
                    .Where(x => !x.SequenceEqual(model.FilePathList.Value))
                    .Subscribe(filePathList => model.FilePathList.Value = filePathList);


            model.FilePathList.AsObservable()
                .Select(filePathList => filePathList.Select(path => Path.GetFileName(path)))
                .Do(_ => Enumerable.Range(0, fileItemContainerTransform.childCount)
                    .Select(i => fileItemContainerTransform.GetChild(i))
                    .ToList()
                    .ForEach(child => DestroyObject(child.gameObject)))
                .SelectMany(fileNameList => fileNameList)
                    .Select(fileName => new { fileName, obj = Instantiate(fileItem) as GameObject })
                    .Do(elm => elm.obj.transform.SetParent(fileItemContainer.transform))
                    .Subscribe(elm => elm.obj.GetComponent<FileListItem>().SetName(elm.fileName));


            LoadButton.OnClickAsObservable()
                .Select(_ => model.SelectedFileName.Value)
                    .Where(fileName => !string.IsNullOrEmpty(fileName))
                    .Subscribe(fileName => StartCoroutine(LoadMusic(fileName)));

            // model.SelectedFileName.SubscribeToText(selectedFileNameText);
        }

        IEnumerator LoadMusic(string fileName)
        {
            using (var www = new WWW("file:///" + model.DirectoryPath.Value + fileName))
            {
                yield return www;

                UndoRedoManager.Clear();
                ClearNotesData();
                Audio.Source.clip = www.audioClip;

                if (Audio.Source.clip == null)
                {
                }
                else
                {
                    EditData.Name.Value = fileName;
                    Audio.OnLoad.OnNext(Unit.Default);
                    LoadNotesData();
                }
            }
        }

        void LoadNotesData()
        {
            var fileName = Path.GetFileNameWithoutExtension(EditData.Name.Value) + ".json";
            var directoryPath = Application.persistentDataPath + "/Notes/";
            var filePath = directoryPath + fileName;

            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                var notesData = JsonMapper.ToObject<SaveDataModel.NotesData>(json);
                InstantiateNotesData(notesData);
            }
        }

        void InstantiateNotesData(SaveDataModel.NotesData notesData)
        {
            var notePresenter = EditNotesPresenter.Instance;

            EditData.BPM.Value = notesData.BPM;
            EditData.MaxBlock.Value = notesData.maxBlock;
            EditData.OffsetSamples.Value = notesData.offset;

            foreach (var note in notesData.notes)
            {
                if (note.type == 1)
                {
                    notePresenter.AddNote(ConvertUtils.ToNote(note));
                    continue;
                }

                var longNoteObjects = new[] { note }.Concat(note.notes)
                    .Select(note_ =>
                    {
                        notePresenter.AddNote(ConvertUtils.ToNote(note_));
                        return EditData.Notes[ConvertUtils.ToNote(note_).position];
                    })
                    .ToList();

                for (int i = 1; i < longNoteObjects.Count; i++)
                {
                    longNoteObjects[i].note.prev = longNoteObjects[i - 1].note.position;
                    longNoteObjects[i - 1].note.next = longNoteObjects[i].note.position;
                }

                EditState.LongNoteTailPosition.Value = NotePosition.None;
            }
        }

        public void ClearNotesData()
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
            EditData.MaxBlock.Value = NoteEditorSettingsModel.Instance.MaxBlock;
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
