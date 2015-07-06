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
        model.MusicName.SubscribeToText(titleText);

        // Binds canvas scale factor
        model.OnLoadedMusicObservable.Select(_ =>  model.CanvasScaleFactor.Value = canvasScaler.referenceResolution.x / Screen.width);

        this.UpdateAsObservable()
            .Select(_ => Screen.width)
            .DistinctUntilChanged()
            .Subscribe(w => model.CanvasScaleFactor.Value = canvasScaler.referenceResolution.x / w);
    }
}
