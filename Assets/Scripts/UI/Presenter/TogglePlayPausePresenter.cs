using NoteEditor.UI.Model;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.UI.Presenter
{
    public class TogglePlayPausePresenter : MonoBehaviour
    {
        [SerializeField]
        Button togglePlayPauseButton;
        [SerializeField]
        Sprite iconPlay;
        [SerializeField]
        Sprite iconPause;

        NoteEditorModel model;

        void Awake()
        {
            model = NoteEditorModel.Instance;
            model.OnLoadMusicObservable.First().Subscribe(_ => Init());
        }

        void Init()
        {
            this.UpdateAsObservable()
                .Where(_ => Input.GetKeyDown(KeyCode.Space))
                .Merge(togglePlayPauseButton.OnClickAsObservable())
                .Subscribe(_ => model.IsPlaying.Value = !model.IsPlaying.Value);

            model.IsPlaying.Subscribe(playing =>
            {
                var playButtonImage = togglePlayPauseButton.GetComponent<Image>();

                if (playing)
                {
                    model.Audio.Play();
                    playButtonImage.sprite = iconPause;

                }
                else
                {
                    model.Audio.Pause();
                    playButtonImage.sprite = iconPlay;
                }
            });
        }
    }
}
