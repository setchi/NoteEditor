using NoteEditor.Common;
using NoteEditor.Utility;
using UniRx;

namespace NoteEditor.Presenter
{
    public class ChangeLocationCommandManager : SingletonMonoBehaviour<ChangeLocationCommandManager>
    {
        CommandManager commandManager = new CommandManager();

        ReactiveProperty<bool> canRedo;
        ReactiveProperty<bool> canUndo;

        static public ReactiveProperty<bool> CanRedo { get { return Instance.canRedo; } }
        static public ReactiveProperty<bool> CanUndo { get { return Instance.canUndo; } }

        void Awake()
        {
            canRedo = this.ObserveEveryValueChanged(_ => commandManager.CanRedo())
                .ToReactiveProperty();

            canUndo = this.ObserveEveryValueChanged(_ => commandManager.CanUndo())
                .ToReactiveProperty();
        }

        static public void Do(Command command) { Instance.commandManager.Do(command); }
        static public void Clear() { Instance.commandManager.Clear(); }
        static public void Undo() { Instance.commandManager.Undo(); }
        static public void Redo() { Instance.commandManager.Redo(); }
    }
}
