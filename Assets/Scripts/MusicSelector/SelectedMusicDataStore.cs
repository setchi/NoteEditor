using UnityEngine;

public class SelectedMusicDataStore : SingletonGameObject<SelectedMusicDataStore>
{

    public AudioClip audioClip;
    public string fileName;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }
}
