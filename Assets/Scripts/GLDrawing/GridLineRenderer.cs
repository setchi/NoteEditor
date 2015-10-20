using NoteEditor.Notes;
using NoteEditor.Model;
using NoteEditor.Utility;
using System;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace NoteEditor.GLDrawing
{
    public class GridLineRenderer : MonoBehaviour
    {
        [SerializeField]
        Color highlightColor;
        [SerializeField]
        Color mainBeatLineColor;
        [SerializeField]
        Color subBeatLineColor;
        [SerializeField]
        Color blockLineColor;

        void Awake()
        {
            var beatSamples = new int[1];
            var beatLines = new Line[1];
            var blockLines = new Line[1];
            var cachedZeroSamplePosX = -1f;
            var cachedCanvasWidth = 0f;

            this.LateUpdateAsObservable()
                .Where(_ => Audio.Source != null && Audio.Source.clip != null)
                .Subscribe(_ =>
                {
                    var unitBeatSamples = Mathf.FloorToInt(Audio.Source.clip.frequency * 60f / EditData.BPM.Value);
                    var beatNum = EditData.LPB.Value * Mathf.CeilToInt(Audio.Source.clip.samples / (float)unitBeatSamples);


                    if (beatSamples.Length != beatNum || cachedCanvasWidth != NoteCanvas.Width.Value)
                    {
                        beatSamples = Enumerable.Range(0, beatNum)
                            .Select(i => i * unitBeatSamples / EditData.LPB.Value)
                            .ToArray();

                        beatLines = beatSamples
                            .Select(x => ConvertUtils.SamplesToCanvasPositionX(x))
                            .Select((x, i) => new Line(
                                ConvertUtils.CanvasToScreenPosition(new Vector3(x, 140, 0)),
                                ConvertUtils.CanvasToScreenPosition(new Vector3(x, -140, 0)),
                                i % EditData.LPB.Value == 0 ? mainBeatLineColor : subBeatLineColor))
                            .ToArray();

                        cachedZeroSamplePosX = beatLines[0].start.x;
                        cachedCanvasWidth = NoteCanvas.Width.Value;
                    }
                    else
                    {
                        float currentX = ConvertUtils.CanvasToScreenPosition(Vector3.right * ConvertUtils.SamplesToCanvasPositionX(0)).x;
                        float diffX = currentX - cachedZeroSamplePosX;

                        for (int i = 0; i < beatNum; i++)
                        {
                            beatLines[i].end.x = (beatLines[i].start.x += diffX);
                            beatLines[i].color = i % EditData.LPB.Value == 0 ? mainBeatLineColor : subBeatLineColor;
                        }

                        cachedZeroSamplePosX = currentX;
                    }


                    if (blockLines.Length != EditData.MaxBlock.Value)
                    {
                        blockLines = Enumerable.Range(0, EditData.MaxBlock.Value)
                            .Select(i => ConvertUtils.BlockNumToCanvasPositionY(i))
                            .Select(i => i + Screen.height * 0.5f)
                            .Select((y, i) => new Line(
                                new Vector3(0, y, 0),
                                new Vector3(Screen.width, y, 0),
                                blockLineColor))
                            .ToArray();
                    }
                    else
                    {
                        for (int i = 0; i < EditData.MaxBlock.Value; i++)
                        {
                            blockLines[i].color = blockLineColor;
                        }
                    }


                    // Highlighting closest line to mouse pointer
                    if (NoteCanvas.IsMouseOverNotesRegion.Value)
                    {
                        var mouseX = Input.mousePosition.x;
                        var closestLineIndex = GetClosestLineIndex(beatLines, c => Mathf.Abs(c.start.x - mouseX));

                        var mouseY = Input.mousePosition.y;
                        var closestBlockLindex = GetClosestLineIndex(blockLines, c => Mathf.Abs(c.start.y - mouseY));

                        var distance = new Vector2(beatLines[closestLineIndex].start.x, blockLines[closestBlockLindex].start.y) - new Vector2(mouseX, mouseY);

                        var thresholdX = Mathf.Abs(ConvertUtils.SamplesToCanvasPositionX(beatSamples[0]) - ConvertUtils.SamplesToCanvasPositionX(beatSamples[1])) / 2f;
                        var thresholdY = Mathf.Abs(ConvertUtils.BlockNumToCanvasPositionY(0) - ConvertUtils.BlockNumToCanvasPositionY(1)) / 2f;

                        if (distance.x < thresholdX && distance.y < thresholdY)
                        {
                            blockLines[closestBlockLindex].color = highlightColor;
                            beatLines[closestLineIndex].color = highlightColor;
                            NoteCanvas.ClosestNotePosition.Value = new NotePosition(EditData.LPB.Value, closestLineIndex, closestBlockLindex);
                        }
                        else
                        {
                            NoteCanvas.ClosestNotePosition.Value = NotePosition.None;
                        }
                    }

                    var beatGridInteral = beatLines[EditData.LPB.Value].start.x - beatLines[0].start.x;
                    var beatGridMinInterval = 100;
                    var intervalFactor = beatGridInteral < beatGridMinInterval
                        ? Mathf.RoundToInt(beatGridMinInterval / beatGridInteral)
                        : 1;

                    BeatNumberRenderer.Begin();
                    var screenWidth = Screen.width;
                    for (int i = 0, l = beatLines.Length; i < l && beatLines[i].start.x < screenWidth; i++)
                    {
                        if (beatLines[i].start.x > 0)
                        {
                            GLLineDrawer.Draw(beatLines[i]);

                            if (i % (EditData.LPB.Value * intervalFactor) == 0)
                            {
                                BeatNumberRenderer.Render(
                                    new Vector3(beatLines[i].start.x, Screen.height / 2f + 154 / NoteCanvas.ScaleFactor.Value, 0),
                                    i / EditData.LPB.Value);
                            }
                        }
                    }
                    BeatNumberRenderer.End();

                    GLLineDrawer.Draw(blockLines);
                });
        }

        int GetClosestLineIndex(Line[] lines, Func<Line, float> calcDistance)
        {
            var minValue = lines.Min(calcDistance);
            return Array.FindIndex(lines, c => calcDistance(c) == minValue);
        }
    }
}
