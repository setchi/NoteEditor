using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class NoteRenderer : MonoBehaviour
{
    NotesEditorModel model;

    void Start()
    {
        model = NotesEditorModel.Instance;
    }

    void LateUpdate()
    {
        foreach (var noteObj in model.NoteObjects.Values)
        {
            var canvasPosOfNote = ConvertUtils.NoteToCanvasPosition(noteObj.note.position);
            var min = ConvertUtils.ScreenToCanvasPosition(Vector3.zero).x;
            var max = ConvertUtils.ScreenToCanvasPosition(Vector3.right * Screen.width).x * 1.1f;

            if (min <= canvasPosOfNote.x && canvasPosOfNote.x <= max)
            {
                noteObj.LateUpdateObservable.OnNext(Unit.Default);
                var screenPos = ConvertUtils.CanvasToScreenPosition(canvasPosOfNote);
                var drawSize = 9 / model.CanvasScaleFactor.Value;
                GLQuadRenderer.Render(new Geometry(
                    new[] {
                        new Vector3(screenPos.x, screenPos.y - drawSize, 0),
                        new Vector3(screenPos.x + drawSize, screenPos.y, 0),
                        new Vector3(screenPos.x, screenPos.y + drawSize, 0),
                        new Vector3(screenPos.x - drawSize, screenPos.y, 0)
                    },
                    noteObj.NoteColor));
            }
        }
    }
}
