using System;

namespace NoteEditor.Common
{
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
