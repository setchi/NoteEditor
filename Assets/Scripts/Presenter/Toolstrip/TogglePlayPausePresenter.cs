using NoteEditor.Model;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.Presenter
{
    public class TogglePlayPausePresenter : MonoBehaviour
    {
        [SerializeField]
        Button togglePlayPauseButton;
        [SerializeField]
        Sprite iconPlay;
        [SerializeField]
        Sprite iconPause;

        void Awake()
        {
            Audio.OnLoad.First().Subscribe(_ => Init());
        }

        void Init()
        {
            this.UpdateAsObservable()
                .Where(_ => Input.GetKeyDown(KeyCode.Space))
                .Merge(togglePlayPauseButton.OnClickAsObservable())
                .Subscribe(_ => Audio.IsPlaying.Value = !Audio.IsPlaying.Value);

            Audio.IsPlaying.Subscribe(playing =>
            {
                var playButtonImage = togglePlayPauseButton.GetComponent<Image>();

                if (playing)
                {
                    Audio.Source.Play();
                    playButtonImage.sprite = iconPause;

                }
                else
                {
                    Audio.Source.Pause();
                    playButtonImage.sprite = iconPlay;
                }
            });
        }
    }
}
