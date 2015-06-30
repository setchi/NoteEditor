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
    Text directoryNameText;
    [SerializeField]
    GameObject fileItem;
    [SerializeField]
    GameObject fileListGroup;
    [SerializeField]
    Button OKButton;
    [SerializeField]
    Text selectedFileNameText;

    void Start()
    {
        var fileItemList = new List<GameObject>();
        var model = MusicSelectorModel.Instance;
        var resourcePath = Application.persistentDataPath + "/Musics/";
        directoryNameText.text = resourcePath;

        if (!File.Exists(resourcePath))
        {
            Directory.CreateDirectory(resourcePath);
        }


        Observable.Timer(TimeSpan.FromMilliseconds(300), TimeSpan.Zero)
                .Select(_ => new DirectoryInfo(resourcePath).GetFiles())
                .Select(fileInfo => fileInfo.Select(file => file.FullName).ToList())
                .DistinctUntilChanged(x => x.Count)
                .Subscribe(filePathList => model.FilePathList.Value = filePathList);


        model.FilePathList.AsObservable()
            .Select(filePathList => filePathList.Select(path => Path.GetFileName(path)))
            .Do(_ => fileItemList.ForEach(DestroyObject))
            .Do(_ => fileItemList.Clear())
            .SelectMany(fileNameList => fileNameList)
                .Select(fileName => new { fileName, obj = Instantiate(fileItem) as GameObject })
                .Do(elm => fileItemList.Add(elm.obj))
                .Do(elm => elm.obj.transform.SetParent(fileListGroup.transform))
                .Subscribe(elm => elm.obj.GetComponent<FileItem>().SetName(elm.fileName));


        OKButton.OnClickAsObservable()
            .Select(_ => model.SelectedFileName.Value)
                .Where(fileName => !string.IsNullOrEmpty(fileName))
                .Subscribe(fileName =>
                {
                    ObservableWWW.GetWWW("file:///" + Application.persistentDataPath + "/Musics/" + fileName).Subscribe(www =>
                    {

                        if (www.audioClip == null)
                        {
                            selectedFileNameText.text = fileName + " は音楽ファイルじゃない件!!!!!!!!!!!!!";
                            return;
                        }

                        SelectedMusicDataStore.Instance.audioClip = www.audioClip;
                        SelectedMusicDataStore.Instance.fileName = fileName;
                        Application.LoadLevel("NotesEditor");
                    });
                });

        model.SelectedFileName.SubscribeToText(selectedFileNameText);
    }
}
