using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class UndoRedoPresenter : MonoBehaviour
{
    Stack<EditorState> undoStack = new Stack<EditorState>();
    Stack<EditorState> redoStack = new Stack<EditorState>();

    NotesEditorModel model;
    bool isUndoRedoAction = false;

    void Awake()
    {
        model = NotesEditorModel.Instance;

        model.OnLoadedMusicObservable
            .Do(_ => undoStack.Clear())
            .Do(_ => redoStack.Clear())
            .DelayFrame(1)
            .Subscribe(_ => undoStack.Push(GetState()));

        this.UpdateAsObservable()
            .Where(_ => KeyInput.ShiftPlus(KeyCode.Z))
            .Subscribe(_ => Undo());

        this.UpdateAsObservable()
            .Where(_ => KeyInput.ShiftPlus(KeyCode.Y))
            .Subscribe(_ => Redo());

        var allUpdateObservable = Observable.Merge(
                model.LPB.Select(_ => true),
                model.BPM.Select(_ => true),
                model.BeatOffsetSamples.Select(_ => true),
                model.EditNoteObservable.Select(_ => true),
                model.MaxBlock.Select(_ => true),
                model.TimeSamples.Select(_ => true),
                model.CanvasOffsetX.Select(_ => true),
                model.CanvasWidth.Select(_ => true))
            .SkipUntil(model.OnLoadedMusicObservable)
            .ThrottleFrame(2)
            .Select(_ => GetState())
            .Buffer(2, 1).Where(b => 2 <= b.Count)
            .Where(b => !b[0].Equals(b[1]));

        allUpdateObservable
            .Where(_ => !isUndoRedoAction)
            .Do(_ => redoStack.Clear())
            .Do(_ => Debug.Log("PushState"))
            .Subscribe(b =>
            {
                if (undoStack.Count == 0)
                {
                    undoStack.Push(b[0]);
                }
                undoStack.Push(b[1]);
            });

        allUpdateObservable
            .Where(_ => isUndoRedoAction)
            .Subscribe(_ => isUndoRedoAction = false);
    }

    EditorState GetState()
    {
        var state = new EditorState();

        state.LPB = model.LPB.Value;
        state.BPM = model.BPM.Value;
        state.BeatOffsetSamples = model.BeatOffsetSamples.Value;
        state.MaxBlock = model.MaxBlock.Value;
        state.NotesData = model.NoteObjects
            .ToDictionary(kv => kv.Key, kv => kv.Value.ToNote());
        state.TimeSamples = model.Audio.timeSamples;
        state.CanvasOffsetX = model.CanvasOffsetX.Value;
        state.CanvasWidth = model.CanvasWidth.Value;

        return state;
    }

    void Undo()
    {
        var currentState = GetState();

        while (undoStack.Count > 0 && undoStack.Peek().Equals(currentState))
            undoStack.Pop();

        if (undoStack.Count == 0)
            return;

        Debug.Log("Undo");
        var state = undoStack.Pop();
        redoStack.Push(currentState);
        ApplyState(state);
        isUndoRedoAction = true;
    }

    void Redo()
    {
        var currentState = GetState();

        while (redoStack.Count > 0 && redoStack.Peek().Equals(currentState))
            redoStack.Pop();

        if (redoStack.Count == 0)
            return;

        Debug.Log("Redo");
        var state = redoStack.Pop();
        undoStack.Push(currentState);
        ApplyState(state);
        isUndoRedoAction = true;
    }

    void ApplyState(EditorState state)
    {
        model.LPB.Value = state.LPB;
        model.BPM.Value = state.BPM;
        model.BeatOffsetSamples.Value = state.BeatOffsetSamples;
        model.MaxBlock.Value = state.MaxBlock;
        model.Audio.timeSamples = state.TimeSamples;
        model.CanvasOffsetX.Value = state.CanvasOffsetX;
        model.CanvasWidth.Value = state.CanvasWidth;

        model.NoteObjects.Values
            .Where(noteObj => !state.NotesData.ContainsKey(noteObj.notePosition))
            .Select(noteObj => noteObj.notePosition)
            .Concat(state.NotesData.Values
                .Where(note => !model.NoteObjects.ContainsKey(note.position))
                .Select(note => note.position))
            .ToList()
            .ForEach(position => model.EditNoteObservable.OnNext(new Note(position)));

        foreach (var note in state.NotesData.Values)
        {
            var instantiatedNote = model.NoteObjects[note.position];
            instantiatedNote.noteType.Value = note.type;
            instantiatedNote.next = model.NoteObjects.ContainsKey(note.next) ? model.NoteObjects[note.next] : null;
            instantiatedNote.prev = model.NoteObjects.ContainsKey(note.prev) ? model.NoteObjects[note.prev] : null;
        }
    }

    class EditorState
    {
        public int BPM = 0;
        public int LPB = 0;
        public int BeatOffsetSamples = 0;
        public Dictionary<NotePosition, Note> NotesData = new Dictionary<NotePosition, Note>();
        public int MaxBlock = 0;
        public int TimeSamples = 0;
        public float CanvasOffsetX = 0;
        public float CanvasWidth = 0;

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var target = (EditorState)obj;

            if (target.NotesData.Values.Any(targetNote => !NotesData.ContainsKey(targetNote.position) || !NotesData[targetNote.position].Equals(targetNote)) ||
                NotesData.Values.Any(selfNote => !target.NotesData.ContainsKey(selfNote.position) || !target.NotesData[selfNote.position].Equals(selfNote)))
                return false;

            return BPM == target.BPM &&
                LPB == target.LPB &&
                BeatOffsetSamples == target.BeatOffsetSamples &&
                MaxBlock == target.MaxBlock &&
                TimeSamples == target.TimeSamples &&
                CanvasOffsetX == target.CanvasOffsetX &&
                CanvasWidth == target.CanvasWidth;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
