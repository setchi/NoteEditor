using System;
using System.Linq;
using UnityEngine;

public class AuxiliaryLineRendererer : MonoBehaviour
{
    [SerializeField]
    Color highlightColor;
    [SerializeField]
    Color mainBeatLineColor;
    [SerializeField]
    Color subBeatLineColor;
    [SerializeField]
    Color blockLineColor;

    NotesEditorModel model;

    void Awake()
    {
        model = NotesEditorModel.Instance;
    }

    void LateUpdate()
    {
        if (model.Audio == null || model.Audio.clip == null)
            return;

        var unitBeatSamples = Mathf.FloorToInt(model.Audio.clip.frequency * 60f / model.BPM.Value);
        var beatNum = model.LPB.Value * Mathf.CeilToInt(model.Audio.clip.samples / (float)unitBeatSamples);
        var beatSamples = Enumerable.Range(0, beatNum)
            .Select(i => i * unitBeatSamples / model.LPB.Value)
            .ToArray();


        var beatLines = beatSamples
            .Select(x => model.SamplesToScreenPositionX(x))
            .Select((x, i) => new Line(
                new Vector3(x, 140, 0),
                new Vector3(x, -140, 0),
                i % model.LPB.Value == 0 ? mainBeatLineColor : subBeatLineColor))
            .ToArray();


        var blockLines = Enumerable.Range(0, 5)
            .Select(i => model.BlockNumToScreenPositionY(i))
            .Select(i => i + Screen.height * 0.5f)
            .Select((y, i) => new Line(
                model.ScreenToCanvasPosition(new Vector3(0, y, 0)),
                model.ScreenToCanvasPosition(new Vector3(Screen.width, y, 0)),
                blockLineColor))
            .ToArray();


        // Highlighting closest line to mouse pointer
        if (model.IsMouseOverNotesRegion.Value)
        {
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
                model.ClosestNotePosition.Value = new NotePosition(model.BPM.Value, model.LPB.Value, closestLineIndex, closestBlockLindex);
            }
            else
            {
                model.ClosestNotePosition.Value = new NotePosition(-1, -1, -1, -1);
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
