using NoteEditor.UI.Model;
using NoteEditor.Utility;
using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace NoteEditor.Common
{
    public class UndoRedoManager : SingletonMonoBehaviour<UndoRedoManager>
    {
        Stack<Command> undoStack = new Stack<Command>();
        Stack<Command> redoStack = new Stack<Command>();

        void Awake()
        {
            Audio.OnLoad
                .DelayFrame(1)
                .Subscribe(_ =>
                {
                    undoStack.Clear();
                    redoStack.Clear();
                });

            this.UpdateAsObservable()
                .Where(_ => KeyInput.CtrlPlus(KeyCode.Z))
                .Subscribe(_ => Undo());

            this.UpdateAsObservable()
                .Where(_ => KeyInput.CtrlPlus(KeyCode.Y))
                .Subscribe(_ => Redo());
        }

        static public void Do(Command command)
        {
            command.Do();
            Instance.undoStack.Push(command);
            Instance.redoStack.Clear();
        }

        static public void Clear()
        {
            Instance.undoStack.Clear();
            Instance.redoStack.Clear();
        }

        void Undo()
        {
            if (undoStack.Count == 0)
                return;

            var command = undoStack.Pop();
            command.Undo();
            redoStack.Push(command);
        }

        void Redo()
        {
            if (redoStack.Count == 0)
                return;

            var command = redoStack.Pop();
            command.Redo();
            undoStack.Push(command);
        }
    }

    public class Command
    {
        Action doAction;
        Action redoAction;
        Action undoAction;

        public Command(Action doAction, Action undoAction, Action redoAction)
        {
            this.doAction = doAction;
            this.undoAction = undoAction;
            this.redoAction = redoAction;
        }

        public Command(Action doAction, Action undoAction)
        {
            this.doAction = doAction;
            this.undoAction = undoAction;
            this.redoAction = doAction;
        }

        public void Do() { doAction(); }
        public void Undo() { undoAction(); }
        public void Redo() { redoAction(); }
    }
}
