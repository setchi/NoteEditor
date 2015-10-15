using NoteEditor.Model;
using NoteEditor.Utility;
using UniRx;
using UnityEngine;

namespace NoteEditor.GLDrawing
{
    public class NoteRenderer : MonoBehaviour
    {
        void LateUpdate()
        {
            if (Audio.Source.clip == null)
                return;

            foreach (var noteObj in EditData.Notes.Values)
            {
                var canvasPosOfNote = ConvertUtils.NoteToCanvasPosition(noteObj.note.position);
                var min = ConvertUtils.ScreenToCanvasPosition(Vector3.zero).x;
                var max = ConvertUtils.ScreenToCanvasPosition(Vector3.right * Screen.width).x * 1.1f;

                if (min <= canvasPosOfNote.x && canvasPosOfNote.x <= max)
                {
                    noteObj.LateUpdateObservable.OnNext(Unit.Default);
                    var screenPos = ConvertUtils.CanvasToScreenPosition(canvasPosOfNote);
                    var drawSize = 9 / NoteCanvas.ScaleFactor.Value;
                    GLQuadDrawer.Draw(new Geometry(
                        new[] {
                            new Vector3(screenPos.x, screenPos.y - drawSize, 0),
                            new Vector3(screenPos.x + drawSize, screenPos.y, 0),
                            new Vector3(screenPos.x, screenPos.y + drawSize, 0),
                            new Vector3(screenPos.x - drawSize, screenPos.y, 0)
                        },
                        noteObj.NoteColor));

                    if (noteObj.note.type == Notes.NoteTypes.Long && EditData.Notes.ContainsKey(noteObj.note.prev))
                    {
                        EditData.Notes[noteObj.note.prev].LateUpdateObservable.OnNext(Unit.Default);
                    }
                }
            }
        }
    }
}
