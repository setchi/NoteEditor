using NoteEditor.Model;
using NoteEditor.Utility;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace NoteEditor.GLDrawing
{
    public class WaveformRenderer : MonoBehaviour
    {
        [SerializeField]
        Color color;

        void Awake()
        {
            var samples = new float[500000];
            var skipSamples = 50;

            this.LateUpdateAsObservable()
                .Where(_ => EditorState.WaveformDisplayEnabled.Value)
                .Where(_ => Audio.Source.clip != null)
                .Subscribe(_ =>
                {
                    var timeSamples = Mathf.Min(Audio.SmoothedTimeSamples.Value, Audio.Source.clip.samples - 1);
                    Audio.Source.clip.GetData(samples, Mathf.RoundToInt(timeSamples));

                    var x = (NoteCanvas.Width.Value / Audio.Source.clip.samples) / 2f;
                    var offsetX = NoteCanvas.OffsetX.Value;
                    var offsetY = 200;
                    var max = Screen.width / NoteCanvas.ScaleFactor.Value * 1.3f;

                    for (int li = 0, wi = skipSamples / 2, l = samples.Length; wi < l; li++, wi += skipSamples)
                    {
                        var pos = wi * x + offsetX;

                        if (pos > max)
                            break;

                        GLLineDrawer.Draw(new Line(
                            ConvertUtils.CanvasToScreenPosition(new Vector3(pos, samples[wi - skipSamples / 2] * 45 - offsetY, 0)),
                            ConvertUtils.CanvasToScreenPosition(new Vector3(pos, samples[wi] * 45 - offsetY, 0)),
                            color));
                    }
                });
        }
    }
}
