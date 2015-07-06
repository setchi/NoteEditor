using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class VolumePresenter : MonoBehaviour
{
    [SerializeField]
    Slider volumeController;

    NotesEditorModel model;

    void Awake()
    {
        model = NotesEditorModel.Instance;
        model.OnLoadedMusicObservable.First().Subscribe(_ => Init());
    }

    void Init()
    {
        model.Volume = volumeController.OnValueChangedAsObservable().ToReactiveProperty();
        model.Volume.DistinctUntilChanged().Subscribe(x => model.Audio.volume = x);
    }
}
