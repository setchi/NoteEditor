using NoteEditor.Model;
using NoteEditor.DTO;
using NoteEditor.Notes;
using UnityEngine;

namespace NoteEditor.Utility
{
    public class ConvertUtils : SingletonMonoBehaviour<ConvertUtils>
    {
        public static int CanvasPositionXToSamples(float x)
        {
            var per = (x - SamplesToCanvasPositionX(0)) / NoteCanvas.Width.Value;
            return Mathf.RoundToInt(Audio.Source.clip.samples * per);
        }

        public static float SamplesToCanvasPositionX(int samples)
        {
            if (Audio.Source.clip == null)
                return 0;

            return (samples - Audio.SmoothedTimeSamples.Value + EditData.OffsetSamples.Value)
                * NoteCanvas.Width.Value / Audio.Source.clip.samples
                + NoteCanvas.OffsetX.Value;
        }

        public static float BlockNumToCanvasPositionY(int blockNum)
        {
            var height = 240f;
            var maxIndex = EditData.MaxBlock.Value - 1;
            return ((maxIndex - blockNum) * height / maxIndex - height / 2) / NoteCanvas.ScaleFactor.Value;
        }

        public static Vector3 NoteToCanvasPosition(NotePosition notePosition)
        {
            return new Vector3(
                SamplesToCanvasPositionX(notePosition.ToSamples(Audio.Source.clip.frequency, EditData.BPM.Value)),
                BlockNumToCanvasPositionY(notePosition.block) * NoteCanvas.ScaleFactor.Value,
                0);
        }

        public static Vector3 ScreenToCanvasPosition(Vector3 screenPosition)
        {
            return (screenPosition - new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0)) * NoteCanvas.ScaleFactor.Value;
        }

        public static Vector3 CanvasToScreenPosition(Vector3 canvasPosition)
        {
            return (canvasPosition / NoteCanvas.ScaleFactor.Value + new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
        }
    }
}
