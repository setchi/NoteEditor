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

    void Update()
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
                ScreenToCanvasPosition(new Vector3(0, y, 0)),
                ScreenToCanvasPosition(new Vector3(Screen.width, y, 0)),
                Color.white / 2f))
            .ToArray();


        // Highlighting closest line to mouse pointer
        if (model.IsMouseOverCanvas.Value)
        {
            var highlightColor = Color.yellow * 0.8f;
            var mouoseX = ScreenToCanvasPosition(Input.mousePosition).x;
            var closestLineIndex = GetClosestLineIndex(beatLines, c => Mathf.Abs(c.start.x - mouoseX));
            var closestBeatLine = beatLines[closestLineIndex];
            closestBeatLine.color = highlightColor;

            var mouseY = ScreenToCanvasPosition(Input.mousePosition).y;
            var closestBlockLindex = GetClosestLineIndex(blockLines, c => Mathf.Abs(c.start.y - mouseY));
            var closestBlockLine = blockLines[closestBlockLindex];
            closestBlockLine.color = highlightColor;

            model.ClosestNotePosition.Value = new NotePosition(beatSamples[closestLineIndex], closestBlockLindex);
        }
        else
        {
            model.ClosestNotePosition.Value = new NotePosition(-1, -1);
        }

        GLLineRenderer.RenderLines("beats", beatLines);
        GLLineRenderer.RenderLines("blocks", blockLines);
    }

    int GetClosestLineIndex(Line[] lines, Func<Line, float> calcDistance)
    {
        var minValue = lines.Min(calcDistance);
        return Array.FindIndex(lines, c => calcDistance(c) == minValue);
    }

    Vector3 ScreenToCanvasPosition(Vector3 screenPosition)
    {
        return (screenPosition - new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0)) * model.CanvasScaleFactor.Value;
    }
}
