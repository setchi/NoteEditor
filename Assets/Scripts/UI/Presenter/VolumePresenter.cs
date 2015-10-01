using NoteEditor.UI.Model;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.UI.Presenter
{
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

        NoteEditorModel model;

        void Awake()
        {
            model = NoteEditorModel.Instance;
            Audio.OnLoad.First().Subscribe(_ => Init());
        }

        void Init()
        {
            volumeController.OnValueChangedAsObservable().Subscribe(volume => Audio.Volume.Value = volume);
            Audio.Volume.DistinctUntilChanged().Subscribe(x => Audio.Source.volume = x);
            Audio.Volume.Select(volume => Mathf.Approximately(volume, 0f) ? iconMute : volume < 0.6f ? iconSound : iconSound2)
                .DistinctUntilChanged()
                .Subscribe(sprite => image.sprite = sprite);
        }
    }
}
