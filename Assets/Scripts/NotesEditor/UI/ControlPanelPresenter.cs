using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class ControlPanelPresenter : MonoBehaviour
{
    [SerializeField]
    CanvasScaler canvasScaler;
    [SerializeField]
    Text titleText;

    void Awake()
    {
        var model = NotesEditorModel.Instance;
        model.Audio = gameObject.AddComponent<AudioSource>();


        // Binds canvas scale factor
        model.CanvasScaleFactor.Value = canvasScaler.referenceResolution.x / Screen.width;
        this.UpdateAsObservable()
            .Select(_ => Screen.width)
            .DistinctUntilChanged()
            .Subscribe(w => model.CanvasScaleFactor.Value = canvasScaler.referenceResolution.x / w);


        ObservableWWW.GetWWW("file:///" + Application.persistentDataPath + "/Musics/test.wav").Subscribe(www =>
        {
            var selectedMusicData = SelectedMusicDataStore.Instance;
            selectedMusicData.audioClip = www.audioClip;

            // Apply music data
            model.Audio.clip = selectedMusicData.audioClip;
            titleText.text = selectedMusicData.fileName ?? "Test";

            model.OnLoadedMusicObservable.OnNext(selectedMusicData);
        });
    }
}
