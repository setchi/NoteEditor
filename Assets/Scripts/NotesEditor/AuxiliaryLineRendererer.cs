using System;
using System.Linq;
using UnityEngine;

public class AuxiliaryLineRendererer : MonoBehaviour
{
    NotesEditorModel model;

    void Awake()
    {
        model = NotesEditorModel.Instance;
    }

    void LateUpdate()
    {
        var beatNum = model.DivisionNumOfOneMeasure.Value * Mathf.CeilToInt(model.Audio.clip.samples / (float)model.UnitBeatSamples.Value);
        var beatSamples = Enumerable.Range(0, beatNum)
            .Select(i => i * model.UnitBeatSamples.Value / model.DivisionNumOfOneMeasure.Value)
            .ToArray();


        var beatLines = beatSamples
            .Select(x => model.SamplesToScreenPositionX(x))
            .Select((x, i) => new Line(
                new Vector3(x, 200, 0),
                new Vector3(x, -200, 0),
                i % model.DivisionNumOfOneMeasure.Value == 0 ? Color.white : Color.white / 2))
            .ToArray();


        var blockLines = Enumerable.Range(0, 5)
            .Select(i => model.BlockNumToScreenPositionY(i))
            .Select(i => i + Screen.height * 0.5f)
            .Select((y, i) => new Line(
                model.ScreenToCanvasPosition(new Vector3(0, y, 0)),
                model.ScreenToCanvasPosition(new Vector3(Screen.width, y, 0)),
                Color.white / 2f))
            .ToArray();


        // Highlighting closest line to mouse pointer
        if (model.IsMouseOverCanvas.Value)
        {
            var highlightColor = Color.yellow * 0.8f;
            var mouseX = model.ScreenToCanvasPosition(Input.mousePosition).x;
            var closestLineIndex = GetClosestLineIndex(beatLines, c => Mathf.Abs(c.start.x - mouseX));
            var closestBeatLine = beatLines[closestLineIndex];

            var mouseY = model.ScreenToCanvasPosition(Input.mousePosition).y;
            var closestBlockLindex = GetClosestLineIndex(blockLines, c => Mathf.Abs(c.start.y - mouseY));
            var closestBlockLine = blockLines[closestBlockLindex];

            var distance = Vector2.Distance(
                new Vector2(closestBeatLine.start.x, closestBlockLine.start.y),
                new Vector2(mouseX, mouseY));

            var threshold = Mathf.Min(
                Mathf.Abs(model.BlockNumToScreenPositionY(0) - model.BlockNumToScreenPositionY(1)),
                Mathf.Abs(model.SamplesToScreenPositionX(beatSamples[0]) - model.SamplesToScreenPositionX(beatSamples[1]))) / 3f;

            if (distance < threshold)
            {
                closestBlockLine.color = highlightColor;
                closestBeatLine.color = highlightColor;
                model.ClosestNotePosition.Value = new NotePosition(beatSamples[closestLineIndex], closestBlockLindex);
            }
            else
            {
                model.ClosestNotePosition.Value = new NotePosition(-1, -1);
            }
        }

        GLLineRenderer.RenderLines("beats", beatLines);
        GLLineRenderer.RenderLines("blocks", blockLines);
    }

    int GetClosestLineIndex(Line[] lines, Func<Line, float> calcDistance)
    {
        var minValue = lines.Min(calcDistance);
        return Array.FindIndex(lines, c => calcDistance(c) == minValue);
    }
}
