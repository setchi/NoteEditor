using System.Collections.Generic;
using UniRx;

public class MusicSelectorModel : SingletonGameObject<MusicSelectorModel>
{
    public ReactiveProperty<List<string>> FilePathList = new ReactiveProperty<List<string>>();
    public ReactiveProperty<string> SelectedFileName = new ReactiveProperty<string>();

    void Awake()
    {
        FilePathList.Value = new List<string>();
    }
}
