using NoteEditor.Common;
using NoteEditor.Model;
using NoteEditor.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace NoteEditor.Presenter
{
    public class EditCommandManager : SingletonMonoBehaviour<EditCommandManager>
    {
        CommandManager commandManager = new CommandManager();

        void Awake()
        {
            Audio.OnLoad
                .DelayFrame(1)
                .Subscribe(_ => Clear());

            this.UpdateAsObservable()
                .Where(_ => KeyInput.CtrlPlus(KeyCode.Z))
                .Subscribe(_ => Undo());

            this.UpdateAsObservable()
                .Where(_ => KeyInput.CtrlPlus(KeyCode.Y))
                .Subscribe(_ => Redo());
        }

        static public void Do(Command command)
        {
            Instance.commandManager.Do(command);
        }

        static public void Clear()
        {
            Instance.commandManager.Clear();
        }

        void Undo()
        {
            commandManager.Undo();
        }

        void Redo()
        {
            commandManager.Redo();
        }
    }
}
