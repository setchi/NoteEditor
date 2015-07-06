using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MusicSelectorPresenter : MonoBehaviour
{
    [SerializeField]
    InputField directoryPathInputField;
    [SerializeField]
    GameObject fileItem;
    [SerializeField]
    GameObject fileItemContainer;
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


    void Awake()
    {
        var fileItemList = new List<GameObject>();
        var model = MusicSelectorModel.Instance;
        var resourcePath = Application.persistentDataPath + "/Musics/";

        directoryPathInputField.OnValueChangeAsObservable()
            .Subscribe(path => model.DirectoryPath.Value = path);

        model.DirectoryPath.DistinctUntilChanged()
            .Subscribe(path => directoryPathInputField.text = path);

        model.DirectoryPath.Value = resourcePath;


        if (!File.Exists(resourcePath))
        {
            Directory.CreateDirectory(resourcePath);
        }


        Observable.Timer(TimeSpan.FromMilliseconds(300), TimeSpan.Zero)
                .Select(_ => new DirectoryInfo(resourcePath).GetFiles())
                .Select(fileInfo => fileInfo.Select(file => file.FullName).ToList())
                .Where(x => !x.SequenceEqual(model.FilePathList.Value))
                .Subscribe(filePathList => model.FilePathList.Value = filePathList);


        model.FilePathList.AsObservable()
            .Select(filePathList => filePathList.Select(path => Path.GetFileName(path)))
            .Do(_ => fileItemList.ForEach(DestroyObject))
            .Do(_ => fileItemList.Clear())
            .SelectMany(fileNameList => fileNameList)
                .Select(fileName => new { fileName, obj = Instantiate(fileItem) as GameObject })
                .Do(elm => fileItemList.Add(elm.obj))
                .Do(elm => elm.obj.transform.SetParent(fileItemContainer.transform))
                .Subscribe(elm => elm.obj.GetComponent<FileItem>().SetName(elm.fileName));


        LoadButton.OnClickAsObservable()
            .Select(_ => model.SelectedFileName.Value)
                .Where(fileName => !string.IsNullOrEmpty(fileName))
                .Subscribe(fileName =>
                {
                    ObservableWWW.GetWWW("file:///" + Application.persistentDataPath + "/Musics/" + fileName).Subscribe(www =>
                    {

                        if (www.audioClip == null)
                        {
                            // selectedFileNameText.text = fileName + " は音楽ファイルじゃない件!!!!!!!!!!!!!";
                            return;
                        }

                        var editorModel = NotesEditorModel.Instance;
                        editorModel.ClearNotesData();

                        // Apply music data
                        editorModel.Audio.clip = www.audioClip;
                        editorModel.MusicName.Value = fileName;

                        editorModel.OnLoadedMusicObservable.OnNext(0);

                        LoadNotesData();
                    });
                });

        // model.SelectedFileName.SubscribeToText(selectedFileNameText);
    }

    void LoadNotesData()
    {
        var editorModel = NotesEditorModel.Instance;

        var notesFileName = Path.GetFileNameWithoutExtension(editorModel.MusicName.Value) + ".json";
        var notesFilePath = Application.persistentDataPath + "/Notes/";
        var notesFileFullPath = notesFilePath + notesFileName;

        if (File.Exists(notesFileFullPath))
        {
            var json = File.ReadAllText(notesFileFullPath, System.Text.Encoding.UTF8);
            var notesData = JsonMapper.ToObject<MusicModel.NotesData>(json);
            InstantiateNotesData(notesData);
        }
    }

    void InstantiateNotesData(MusicModel.NotesData notesData)
    {
        var editorModel = NotesEditorModel.Instance;

        editorModel.BPM.Value = float.Parse(notesData.BPM);
        editorModel.BeatOffsetSamples.Value = notesData.offset;

        foreach (var note in notesData.notes)
        {
            if (note.state == 1)
            {
                InstantiateNoteObject(note);
                continue;
            }

            var longNoteObjects = new MusicModel.Note[] { note }.Concat(note.noteList)
                .Select(note_ => InstantiateNoteObject(note_))
                .ToList();

            for (int i = 1; i < longNoteObjects.Count; i++)
            {
                longNoteObjects[i].prev = longNoteObjects[i - 1];
                longNoteObjects[i - 1].next = longNoteObjects[i];
            }
        }
    }

    NoteObject InstantiateNoteObject(MusicModel.Note note)
    {
        var noteObject = (Instantiate(noteObjectPrefab) as GameObject).GetComponent<NoteObject>();
        noteObject.notePosition = new NotePosition(note.sample, note.blockNum);
        noteObject.noteType.Value = note.state == 2 ? NoteTypes.Long : NoteTypes.Normal;
        noteObject.transform.SetParent(notesRegion.transform);
        NotesEditorModel.Instance.NoteObjects.Add(noteObject.notePosition, noteObject);
        return noteObject;
    }
}
