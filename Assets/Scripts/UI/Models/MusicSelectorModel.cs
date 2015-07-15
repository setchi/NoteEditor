using System.Collections.Generic;
using UniRx;

public class MusicSelectorModel : SingletonGameObject<MusicSelectorModel>
{
    public readonly ReactiveProperty<string> DirectoryPath = new ReactiveProperty<string>();
    public readonly ReactiveProperty<List<string>> FilePathList = new ReactiveProperty<List<string>>(new List<string>());
    public readonly ReactiveProperty<string> SelectedFileName = new ReactiveProperty<string>();
}
