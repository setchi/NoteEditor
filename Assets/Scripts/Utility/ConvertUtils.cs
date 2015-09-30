using NoteEditor.Notes;
using NoteEditor.UI.Model;
using UnityEngine;

public class ConvertUtils : SingletonMonoBehaviour<ConvertUtils>
{
    NotesEditorModel model;

    void Awake()
    {
        model = NotesEditorModel.Instance;
    }

    public static int CanvasPositionXToSamples(float x)
    {
        var per = (x - SamplesToCanvasPositionX(0)) / Instance.model.CanvasWidth.Value;
        return Mathf.RoundToInt(Instance.model.Audio.clip.samples * per);
    }

    public static float SamplesToCanvasPositionX(int samples)
    {
        if (Instance.model.Audio.clip == null)
            return 0;

        return (samples - Instance.model.SmoothedTimeSamples.Value + Instance.model.BeatOffsetSamples.Value)
            * Instance.model.CanvasWidth.Value / Instance.model.Audio.clip.samples
            + Instance.model.CanvasOffsetX.Value;
    }

    public static float BlockNumToCanvasPositionY(int blockNum)
    {
        var height = 240f;
        var maxIndex = Instance.model.MaxBlock.Value - 1;
        return ((maxIndex - blockNum) * height / maxIndex - height / 2) / Instance.model.CanvasScaleFactor.Value;
    }

    public static Vector3 NoteToCanvasPosition(NotePosition notePosition)
    {
        return new Vector3(
            SamplesToCanvasPositionX(notePosition.ToSamples(Instance.model.Audio.clip.frequency, Instance.model.BPM.Value)),
            BlockNumToCanvasPositionY(notePosition.block) * Instance.model.CanvasScaleFactor.Value,
            0);
    }

    public static Vector3 ScreenToCanvasPosition(Vector3 screenPosition)
    {
        return (screenPosition - new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0)) * Instance.model.CanvasScaleFactor.Value;
    }

    public static Vector3 CanvasToScreenPosition(Vector3 canvasPosition)
    {
        return (canvasPosition / Instance.model.CanvasScaleFactor.Value + new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
    }
}
