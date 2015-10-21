using NoteEditor.GLDrawing;
using NoteEditor.Notes;
using NoteEditor.Model;
using NoteEditor.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace NoteEditor.Presenter
{
    public class RangeSelectionPresenter : MonoBehaviour
    {
        [SerializeField]
        CanvasEvents canvasEvents;
        [SerializeField]
        Color selectionRectColor;

        Dictionary<NotePosition, NoteObject> selectedNoteObjects = new Dictionary<NotePosition, NoteObject>();
        List<Note> copiedNotes = new List<Note>();
        EditNotesPresenter editPresenter;

        void Awake()
        {
            editPresenter = EditNotesPresenter.Instance;


            // Select by dragging
            this.UpdateAsObservable()
                .Where(_ => KeyInput.CtrlKey())
                .Where(_ => Input.GetMouseButtonDown(0))
                .Select(_ => Input.mousePosition)
                .SelectMany(startPos => this.UpdateAsObservable()
                    .TakeWhile(_ => !Input.GetMouseButtonUp(0))
                    .Where(_ => NoteCanvas.IsMouseOverNotesRegion.Value)
                    .Select(_ => Input.mousePosition)
                    .Select(currentPos => new Rect(startPos, currentPos - startPos)))
                .Do(rect => GLLineDrawer.Draw(ToLines(rect, selectionRectColor)))
                .Do(_ => { if (!Audio.IsPlaying.Value) Deselect(); })
                .SelectMany(rect => GetNotesWithin(rect))
                .Do(kv => selectedNoteObjects[kv.Key] = kv.Value)
                .Subscribe(kv => kv.Value.isSelected.Value = true);


            // All select by Ctrl-A
            this.UpdateAsObservable()
                .Where(_ => KeyInput.CtrlPlus(KeyCode.A))
                .SelectMany(_ => EditData.Notes.Values.ToList())
                .Do(noteObj => noteObj.isSelected.Value = true)
                .Subscribe(noteObj => selectedNoteObjects[noteObj.note.position] = noteObj);


            // Copy notes by Ctrl-C
            this.UpdateAsObservable()
                .Where(_ => KeyInput.CtrlPlus(KeyCode.C))
                .Subscribe(notes => CopyNotes(selectedNoteObjects.Values));


            // Cutting notes by Ctrl-X
            this.UpdateAsObservable()
                .Where(_ => KeyInput.CtrlPlus(KeyCode.X))
                .Select(_ => selectedNoteObjects.Values
                    .Where(noteObj => EditData.Notes.ContainsKey(noteObj.note.position)))
                .Do(notes => CopyNotes(notes))
                .Subscribe(notes => DeleteNotes(notes));


            // Deselect by mousedown
            this.UpdateAsObservable()
                .Where(_ => !NoteCanvas.IsMouseOverWaveformRegion.Value)
                .Where(_ => Input.GetMouseButtonDown(0))
                .Subscribe(_ => Deselect());


            // Delete selected notes by delete key
            this.UpdateAsObservable()
                .Where(_ => Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
                .Select(_ => selectedNoteObjects.Values
                    .Where(noteObj => EditData.Notes.ContainsKey(noteObj.note.position)).ToList())
                .Do(_ => selectedNoteObjects.Clear())
                .Subscribe(notes => DeleteNotes(notes));


            // Paste to next beat by Ctrl-V
            this.UpdateAsObservable()
                .Where(_ => KeyInput.CtrlPlus(KeyCode.V))
                .Where(_ => copiedNotes.Count > 0)
                .Select(_ => copiedNotes.OrderBy(note => note.position.ToSamples(Audio.Source.clip.frequency, EditData.BPM.Value)))
                .Subscribe(sortedCopiedNotes =>
                {
                    var firstPos = sortedCopiedNotes.First().position;
                    var lastPos = sortedCopiedNotes.Last().position;
                    var beatDiff = 1 + lastPos.num / lastPos.LPB - firstPos.num / firstPos.LPB;

                    var validNotes = copiedNotes.Where(note => note.position.Add(0, note.position.LPB * beatDiff, 0).ToSamples(Audio.Source.clip.frequency, EditData.BPM.Value) < Audio.Source.clip.samples)
                        .ToList();

                    copiedNotes.Clear();

                    validNotes.ToObservable()
                        .Select(note =>
                            note.type == NoteTypes.Single
                                ? new Note(note.position.Add(0, note.position.LPB * beatDiff, 0))
                                : new Note(
                                    note.position.Add(0, note.position.LPB * beatDiff, 0),
                                    note.type,
                                    note.next.Add(0, note.next.LPB * beatDiff, 0),
                                    note.prev.Add(0, note.prev.LPB * beatDiff, 0)
                                ))
                        .Do(note => copiedNotes.Add(note))
                        .Subscribe(note =>
                            (EditData.Notes.ContainsKey(note.position)
                                ? editPresenter.RequestForChangeNoteStatus
                                : editPresenter.RequestForAddNote)
                            .OnNext(note));

                    Deselect();

                    validNotes.Select(obj => obj.position.Add(0, obj.position.LPB * beatDiff, 0))
                        .ToObservable()
                        .DelayFrame(1)
                        .Select(pastedPosition => EditData.Notes[pastedPosition])
                        .Do(pastedObj => selectedNoteObjects[pastedObj.note.position] = pastedObj)
                        .Subscribe(pastedObj => pastedObj.isSelected.Value = true);
                });
        }

        public NotePosition GetSelectedNextLongNote(NotePosition current, Func<NoteObject, NotePosition> accessor)
        {
            while (EditData.Notes.ContainsKey(current))
            {
                if (selectedNoteObjects.ContainsKey(current))
                    return current;

                current = accessor(EditData.Notes[current]);
            }

            return NotePosition.None;
        }

        Dictionary<NotePosition, NoteObject> GetNotesWithin(Rect rect)
        {
            return EditData.Notes
                .Where(kv => rect.Contains(ConvertUtils.CanvasToScreenPosition(ConvertUtils.NoteToCanvasPosition(kv.Value.note.position)), true))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        void CopyNotes(IEnumerable<NoteObject> notes)
        {
            copiedNotes = notes.Select(noteObj =>
            {
                var note = noteObj.note;
                if (noteObj.note.type == NoteTypes.Long)
                {
                    note.next = GetSelectedNextLongNote(noteObj.note.next, c => c.note.next);
                    note.prev = GetSelectedNextLongNote(noteObj.note.prev, c => c.note.prev);
                }
                return note;
            })
            .ToList();
        }

        void DeleteNotes(IEnumerable<NoteObject> notes)
        {
            notes.ToList().ForEach(note => editPresenter.RequestForRemoveNote.OnNext(note.note));
        }

        void Deselect()
        {
            selectedNoteObjects.Values
                .Where(noteObj => EditData.Notes.ContainsKey(noteObj.note.position))
                .ToList()
                .ForEach(note => note.isSelected.Value = false);

            selectedNoteObjects.Clear();
        }

        Line[] ToLines(Rect rect, Color color)
        {
            return new[] {
                new Line(rect.min, rect.min + Vector2.right * rect.size.x, color),
                new Line(rect.min, rect.min + Vector2.up    * rect.size.y, color),
                new Line(rect.max, rect.max + Vector2.left  * rect.size.x, color),
                new Line(rect.max, rect.max + Vector2.down  * rect.size.y, color)
            };
        }
    }
}
