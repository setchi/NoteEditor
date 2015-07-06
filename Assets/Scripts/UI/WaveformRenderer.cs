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
        var lineColor = Color.green * 0.6f;
        var lines = Enumerable.Range(0, waveData.Length / skipSamples)
            .Select(_ => new Line(Vector3.zero, Vector3.zero, lineColor))
            .ToArray();


        this.LateUpdateAsObservable()
            .Where(_ => model.WaveformDisplayEnabled.Value)
            .SkipWhile(_ => model.Audio.clip == null)
            .Subscribe(_ =>
            {
                model.Audio.clip.GetData(waveData, model.Audio.timeSamples);
                var x = (model.CanvasWidth.Value / model.Audio.clip.samples) / 2f;
                var offsetX = model.CanvasOffsetX.Value;
                var offsetY = 200;

                for (int li = 0, wi = skipSamples / 2, l = waveData.Length; wi < l; li++, wi += skipSamples)
                {
                    lines[li].start.x = lines[li].end.x = wi * x + offsetX;
                    lines[li].end.y =  waveData[wi] * 45 - offsetY;
                    lines[li].start.y = waveData[wi - skipSamples / 2] * 45 - offsetY;
                }

                GLLineRenderer.RenderLines("waveform", lines);
            });
    }
}
