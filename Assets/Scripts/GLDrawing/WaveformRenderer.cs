using NoteEditor.Model;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.GLDrawing
{
    public class WaveformRenderer : MonoBehaviour
    {
        [SerializeField]
        RawImage image;

        Texture2D texture;

        int imageWidth = 1280;
        float[] samples = new float[500000];

        float cachedCanvasWidth = 0;
        float cachedTimeSamples = 0;

        void Start()
        {
            texture = new Texture2D(imageWidth, 1);
            image.texture = texture;
            ResetTexture();

            EditorState.WaveformDisplayEnabled
                .Where(enabled => !enabled)
                .Subscribe(_ => ResetTexture());
        }

        void LateUpdate()
        {
            if (Audio.Source.clip == null || !EditorState.WaveformDisplayEnabled.Value)
                return;

            var timeSamples = Mathf.Min(Audio.SmoothedTimeSamples.Value, Audio.Source.clip.samples - 1);

            if (!HasUpdate(timeSamples))
                return;

            UpdateCache(timeSamples);

            Audio.Source.clip.GetData(samples, Mathf.RoundToInt(timeSamples));

            int textureX = 0;
            float maxSample = 0;
            int skipSamples = Mathf.RoundToInt(1 / (NoteCanvas.Width.Value * 0.5f / Audio.Source.clip.samples));

            for (int i = 0, l = samples.Length; textureX < imageWidth && i < l; i++)
            {
                maxSample = Mathf.Max(maxSample, samples[i]);

                if (i % skipSamples == 0)
                {
                    texture.SetPixel(textureX, 0, new Color(maxSample, 0, 0));
                    maxSample = 0;
                    textureX++;
                }
            }

            texture.Apply();
        }

        void ResetTexture()
        {
            texture.SetPixels(Enumerable.Range(0, imageWidth).Select(_ => Color.clear).ToArray());
            texture.Apply();
        }

        bool HasUpdate(float timeSamples)
        {
            return cachedCanvasWidth != NoteCanvas.Width.Value || cachedTimeSamples != timeSamples;
        }

        void UpdateCache(float timeSamples)
        {
            cachedCanvasWidth = NoteCanvas.Width.Value;
            cachedTimeSamples = timeSamples;
        }
    }
}
