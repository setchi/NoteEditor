using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class WaveformRenderer : MonoBehaviour
{
    void Awake()
    {
        var model = NotesEditorModel.Instance;
        var waveData = new float[500000];
        var skipSamples = 50;
        var lineColor = Color.green * 0.5f;
        var lines = Enumerable.Range(0, waveData.Length / skipSamples)
            .Select(_ => new Line(Vector3.zero, Vector3.zero, lineColor))
            .ToArray();


        this.LateUpdateAsObservable().Subscribe(_ =>
        {
            model.Audio.clip.GetData(waveData, model.Audio.timeSamples);
            var x = (model.CanvasWidth.Value / model.Audio.clip.samples) / 2f;
            var offsetX = model.CanvasOffsetX.Value;

            for (int li = 0, wi = 0, l = waveData.Length; wi < l; li++, wi += skipSamples)
            {
                lines[li].start.x = lines[li].end.x = wi * x + offsetX;
                lines[li].end.y = -(lines[li].start.y = waveData[wi] * 200);
            }

            GLLineRenderer.RenderLines("wave", lines);
        });
    }
}
