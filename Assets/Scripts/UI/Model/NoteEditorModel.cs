using LitJson;
using NoteEditor.JSONModel;
using NoteEditor.Notes;
using NoteEditor.UI.Presenter;
using NoteEditor.Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public enum NoteTypes { Single, Long }

namespace NoteEditor.UI.Model
{
    public class NoteEditorModel : SingletonMonoBehaviour<NoteEditorModel>
    {
        [SerializeField]
        CanvasScaler canvasScaler;

        void Awake()
        {
            this.ObserveEveryValueChanged(_ => Screen.width)
                .DistinctUntilChanged()
                .Subscribe(w => NoteCanvas.ScaleFactor.Value = 1280f / w);
            // .Subscribe(w => NoteCanvas.ScaleFactor.Value = canvasScaler.referenceResolution.x / w);

            ClearNotesData();
        }

        public void ClearNotesData()
        {
            Audio.Source.clip = null;
            Audio.IsPlaying.Value = false;
            Audio.TimeSamples.Value = 0;
            Audio.SmoothedTimeSamples.Value = 0;
            EditState.NoteType.Value = NoteTypes.Single;
            EditState.LongNoteTailPosition.Value = NotePosition.None;
            EditData.BPM.Value = 120;
            EditData.OffsetSamples.Value = 0;
            EditData.Name.Value = "Note Editor";
            EditData.MaxBlock.Value = NoteEditorSettingsModel.Instance.MaxBlock;
            EditData.LPB.Value = 4;

            foreach (var note in EditData.Notes.Values)
            {
                note.Dispose();
            }

            EditData.Notes.Clear();
            Resources.UnloadUnusedAssets();
        }

        public string SerializeNotesData()
        {
            var data = new SaveDataModel.NotesData();
            data.BPM = EditData.BPM.Value;
            data.maxBlock = EditData.MaxBlock.Value;
            data.offset = EditData.OffsetSamples.Value;
            data.name = Path.GetFileNameWithoutExtension(EditData.Name.Value);

            var sortedNoteObjects = EditData.Notes.Values
                .Where(note => !(note.note.type == NoteTypes.Long && EditData.Notes.ContainsKey(note.note.prev)))
                .OrderBy(note => note.note.position.ToSamples(Audio.Source.clip.frequency, EditData.BPM.Value));

            data.notes = new List<SaveDataModel.Note>();

            foreach (var noteObject in sortedNoteObjects)
            {
                if (noteObject.note.type == NoteTypes.Single)
                {
                    data.notes.Add(ConvertToNote(noteObject));
                }
                else if (noteObject.note.type == NoteTypes.Long)
                {
                    var current = noteObject;
                    var note = ConvertToNote(noteObject);

                    while (EditData.Notes.ContainsKey(current.note.next))
                    {
                        var nextObj = EditData.Notes[current.note.next];
                        note.notes.Add(ConvertToNote(nextObj));
                        current = nextObj;
                    }

                    data.notes.Add(note);
                }
            }

            var jsonWriter = new JsonWriter();
            jsonWriter.PrettyPrint = true;
            jsonWriter.IndentValue = 4;
            JsonMapper.ToJson(data, jsonWriter);
            return jsonWriter.ToString();
        }

        public SaveDataModel.Note ConvertToNote(NoteObject noteObject)
        {
            var note = new SaveDataModel.Note();
            note.num = noteObject.note.position.num;
            note.block = noteObject.note.position.block;
            note.LPB = noteObject.note.position.LPB;
            note.type = noteObject.note.type == NoteTypes.Long ? 2 : 1;
            note.notes = new List<SaveDataModel.Note>();
            return note;
        }
    }
}
