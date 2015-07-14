using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class RangeSelectionPresenter : MonoBehaviour
{
    [SerializeField]
    CanvasEvents canvasEvents;
    [SerializeField]
    Color selectionRectColor;

    NotesEditorModel model;
    Dictionary<NotePosition, NoteObject> selectedNoteObjects = new Dictionary<NotePosition, NoteObject>();
    List<Note> copiedNotes = new List<Note>();

    void Awake()
    {
        model = NotesEditorModel.Instance;


        // Select by dragging
        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButtonDown(0))
            .Select(_ => model.ScreenToCanvasPosition(Input.mousePosition))
            .SelectMany(startPos => this.UpdateAsObservable()
                .TakeWhile(_ => !Input.GetMouseButtonUp(0))
                .Where(_ => model.IsMouseOverNotesRegion.Value)
                .Select(_ => model.ScreenToCanvasPosition(Input.mousePosition))
                .Select(currentPos => new Rect(startPos, currentPos - startPos)))
            .Do(rect => GLLineRenderer.RenderLines("selectionRect", ToLines(rect, selectionRectColor)))
            .Do(_ => { if (!model.IsPlaying.Value) Deselect(); })
            .SelectMany(rect => GetNotesWithin(rect))
            .Do(kv => selectedNoteObjects.Set(kv))
            .Subscribe(kv => kv.Value.isSelected.Value = true);


        // All select by Ctrl-A
        this.UpdateAsObservable()
            .Where(_ => KeyInput.CtrlPlus(KeyCode.A))
            .SelectMany(_ => model.NoteObjects.Values.ToList())
            .Do(noteObj => noteObj.isSelected.Value = true)
            .Subscribe(noteObj => selectedNoteObjects.Set(noteObj.notePosition, noteObj));


        // Copy notes by Ctrl-C
        this.UpdateAsObservable()
            .Where(_ => KeyInput.CtrlPlus(KeyCode.C))
            .Subscribe(notes => CopyNotes(selectedNoteObjects.Values));


        // Cutting notes by Ctrl-X
        this.UpdateAsObservable()
            .Where(_ => KeyInput.CtrlPlus(KeyCode.X))
            .Select(_ => selectedNoteObjects.Values)
            .Do(notes => CopyNotes(notes))
            .Subscribe(notes => DeleteNotes(notes));


        // Deselect by mousedown
        this.UpdateAsObservable()
            .Where(_ => !model.IsMouseOverWaveformRegion.Value)
            .Where(_ => Input.GetMouseButtonDown(0))
            .Subscribe(_ => Deselect());


        // Delete selected notes by delete key
        this.UpdateAsObservable()
            .Where(_ => Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            .Select(_ => selectedNoteObjects.Values
                .Where(noteObj => model.NoteObjects.ContainsKey(noteObj.notePosition)).ToList())
            .Do(_ => selectedNoteObjects.Clear())
            .Subscribe(notes => DeleteNotes(notes));


        // Paste to next beat by mousedown
        this.UpdateAsObservable()
            .Where(_ => KeyInput.CtrlPlus(KeyCode.V))
            .Where(_ => copiedNotes.Count > 0)
            .Select(_ => copiedNotes.OrderBy(note => note.position.ToSamples(model.Audio.clip.frequency, model.BPM.Value)))
            .Subscribe(sortedCopiedNotes =>
            {
                var firstPos = sortedCopiedNotes.First().position;
                var lastPos = sortedCopiedNotes.Last().position;
                var beatDiff = 1 + lastPos.num / lastPos.LPB - firstPos.num / firstPos.LPB;

                var validNotes = copiedNotes.Where(note => note.position.Add(0, note.position.LPB * beatDiff, 0).ToSamples(model.Audio.clip.frequency, model.BPM.Value) < model.Audio.clip.samples)
                    .ToList();

                validNotes.Where(note => note.type == NoteTypes.Long)
                    .ToObservable()
                    .Select(note => new Note(
                        note.position.Add(0, note.position.LPB * beatDiff, 0),
                        note.type,
                        note.next.Add(0, note.next.LPB * beatDiff, 0),
                        note.prev.Add(0, note.prev.LPB * beatDiff, 0)
                    ))
                    .Do(obj => {
                        if (!model.NoteObjects.ContainsKey(obj.position))
                            model.EditNoteObservable.OnNext(new Note(obj.position, NoteTypes.Normal));
                    })
                    .DelayFrame(1)
                    .Subscribe(obj =>
                    {
                        var current = model.NoteObjects[obj.position];
                        current.noteType.Value = NoteTypes.Long;

                        current.next = model.NoteObjects.ContainsKey(obj.next) ? model.NoteObjects[obj.next] : null;
                        current.prev = model.NoteObjects.ContainsKey(obj.prev) ? model.NoteObjects[obj.prev] : null;

                        foreach (var noteObj in model.NoteObjects.Values
                            .Where(noteObj => noteObj.next == current && noteObj != current.prev))
                        {
                            noteObj.next = null;
                        }

                        foreach (var noteObj in model.NoteObjects.Values
                            .Where(noteObj => noteObj.prev == current && noteObj != current.next))
                        {
                            noteObj.prev = null;
                        }
                    });


                validNotes.Where(note => note.type == NoteTypes.Normal)
                    .Select(noteObj => noteObj.position.Add(0, noteObj.position.LPB * beatDiff, 0))
                    .ToObservable()
                    .Do(pastedPosition =>
                    {
                        if (!model.NoteObjects.ContainsKey(pastedPosition))
                            model.EditNoteObservable.OnNext(new Note(pastedPosition, NoteTypes.Normal));
                    })
                    .Select(pastedPosition => model.NoteObjects[pastedPosition])
                    .Do(pastedObj => pastedObj.next = pastedObj.prev = null)
                    .Subscribe(pastedObj => pastedObj.noteType.Value = NoteTypes.Normal);


                Deselect();
                copiedNotes.Clear();

                validNotes.Select(obj => obj.position.Add(0, obj.position.LPB * beatDiff, 0))
                    .Select(pastedPosition => model.NoteObjects[pastedPosition])
                    .ToObservable()
                    .Do(pastedObj => selectedNoteObjects.Set(pastedObj.notePosition, pastedObj))
                    .Do(pastedObj => pastedObj.isSelected.Value = true)
                    .DelayFrame(1)
                    .Subscribe(pastedObj => copiedNotes.Add(pastedObj.ToNote()));
            });
    }

    public NotePosition GetSelectedNextLongNote(NoteObject current, Func<NoteObject, NoteObject> accessor)
    {
        while (current != null)
        {
            if (selectedNoteObjects.ContainsKey(current.notePosition))
            {
                return current.notePosition;
            }

            current = accessor(current);
        }

        return NotePosition.None;
    }

    Dictionary<NotePosition, NoteObject> GetNotesWithin(Rect rect)
    {
        return model.NoteObjects
            .Where(kv => rect.Contains(kv.Value.rectTransform.localPosition, true))
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    void CopyNotes(IEnumerable<NoteObject> notes)
    {
        copiedNotes = notes.Select(noteObj =>
        {
            var note = noteObj.ToNote();
            if (noteObj.noteType.Value == NoteTypes.Long)
            {
                note.next = GetSelectedNextLongNote(noteObj.next, c => c.next);
                note.prev = GetSelectedNextLongNote(noteObj.prev, c => c.prev);
            }
            return note;
        })
        .ToList();
    }

    void DeleteNotes(IEnumerable<NoteObject> notes)
    {
        notes.ToList().ForEach(note => model.EditNoteObservable.OnNext(note.ToNote()));
    }

    void Deselect()
    {
        selectedNoteObjects.Values
            .Where(noteObj => model.NoteObjects.ContainsKey(noteObj.notePosition))
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
