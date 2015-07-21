using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class TogglePlayPausePresenter : MonoBehaviour
{
    [SerializeField]
    Button togglePlayPauseButton;
    [SerializeField]
    Sprite iconPlay;
    [SerializeField]
    Sprite iconPause;

    NotesEditorModel model;

    void Awake()
    {
        model = NotesEditorModel.Instance;
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
