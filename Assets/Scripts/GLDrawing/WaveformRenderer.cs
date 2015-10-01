using NoteEditor.UI.Model;
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
            var model = NoteEditorModel.Instance;
            var waveData = new float[500000];
            var skipSamples = 50;
            var lines = Enumerable.Range(0, waveData.Length / skipSamples)
                .Select(_ => new Line(Vector3.zero, Vector3.zero, color))
                .ToArray();


            this.LateUpdateAsObservable()
                .Where(_ => EditorState.WaveformDisplayEnabled.Value)
                .Where(_ => Audio.Source.clip != null)
                .Subscribe(_ =>
                {
                    var timeSamples = Mathf.Min(Audio.SmoothedTimeSamples.Value, Audio.Source.clip.samples - 1);
                    Audio.Source.clip.GetData(waveData, Mathf.RoundToInt(timeSamples));

                    var x = (NoteCanvas.Width.Value / Audio.Source.clip.samples) / 2f;
                    var offsetX = NoteCanvas.OffsetX.Value;
                    var offsetY = 200;

                    var min = NoteCanvas.OffsetX.Value;
                    var max = Screen.width / NoteCanvas.ScaleFactor.Value * 1.3f;

                    for (int li = 0, wi = skipSamples / 2, l = waveData.Length; wi < l; li++, wi += skipSamples)
                    {
                        lines[li].start.x = lines[li].end.x = wi * x + offsetX;
                        lines[li].end.y = waveData[wi] * 45 - offsetY;
                        lines[li].start.y = waveData[wi - skipSamples / 2] * 45 - offsetY;
                        lines[li].start = ConvertUtils.CanvasToScreenPosition(lines[li].start);
                        lines[li].end = ConvertUtils.CanvasToScreenPosition(lines[li].end);

                        var posX = lines[li].start.x;
                        if (min < posX && posX < max)
                        {
                            GLLineDrawer.Draw(lines[li]);
                        }
                    }
                });
        }
    }
}
