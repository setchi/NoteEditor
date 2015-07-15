using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class VolumePresenter : MonoBehaviour
{
    [SerializeField]
    Slider volumeController;
    [SerializeField]
    Image image;
    [SerializeField]
    Sprite iconSound2;
    [SerializeField]
    Sprite iconSound;
    [SerializeField]
    Sprite iconMute;

    NotesEditorModel model;

    void Awake()
    {
        model = NotesEditorModel.Instance;
        model.OnLoadedMusicObservable.First().Subscribe(_ => Init());
    }

    void Init()
    {
        volumeController.OnValueChangedAsObservable().Subscribe(volume => model.Volume.Value = volume);
        model.Volume.DistinctUntilChanged().Subscribe(x => model.Audio.volume = x);
        model.Volume.Select(volume => Mathf.Approximately(volume, 0f) ? iconMute : volume < 0.6f ? iconSound : iconSound2)
            .DistinctUntilChanged()
            .Subscribe(sprite => image.sprite = sprite);
    }
}
