using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class LPBPresenter : MonoBehaviour
{
    [SerializeField]
    Text LPBDisplayText;
    [SerializeField]
    Button LPBUpButton;
    [SerializeField]
    Button LPBDownButton;

    void Awake()
    {
        var model = NotesEditorModel.Instance;
        Observable.Merge(
                LPBUpButton.OnClickAsObservable().Select(_ => model.LPB.Value + 1),
                LPBDownButton.OnClickAsObservable().Select(_ => model.LPB.Value - 1))
            .Select(LPB => Mathf.Clamp(LPB, 2, 20))
            .Subscribe(LPB => model.LPB.Value = LPB);

        model.LPB.SubscribeToText(LPBDisplayText);
    }
}
