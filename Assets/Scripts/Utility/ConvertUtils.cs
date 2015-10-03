using LitJson;
using NoteEditor.Model.JSON;
using NoteEditor.Notes;
using NoteEditor.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace NoteEditor.Utility
{
    public class ConvertUtils : SingletonMonoBehaviour<ConvertUtils>
    {
        public static int CanvasPositionXToSamples(float x)
        {
            var per = (x - SamplesToCanvasPositionX(0)) / NoteCanvas.Width.Value;
            return Mathf.RoundToInt(Audio.Source.clip.samples * per);
        }

        public static float SamplesToCanvasPositionX(int samples)
        {
            if (Audio.Source.clip == null)
                return 0;

            return (samples - Audio.SmoothedTimeSamples.Value + EditData.OffsetSamples.Value)
                * NoteCanvas.Width.Value / Audio.Source.clip.samples
                + NoteCanvas.OffsetX.Value;
        }

        public static float BlockNumToCanvasPositionY(int blockNum)
        {
            var height = 240f;
            var maxIndex = EditData.MaxBlock.Value - 1;
            return ((maxIndex - blockNum) * height / maxIndex - height / 2) / NoteCanvas.ScaleFactor.Value;
        }

        public static Vector3 NoteToCanvasPosition(NotePosition notePosition)
        {
            return new Vector3(
                SamplesToCanvasPositionX(notePosition.ToSamples(Audio.Source.clip.frequency, EditData.BPM.Value)),
                BlockNumToCanvasPositionY(notePosition.block) * NoteCanvas.ScaleFactor.Value,
                0);
        }

        public static Vector3 ScreenToCanvasPosition(Vector3 screenPosition)
        {
            return (screenPosition - new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0)) * NoteCanvas.ScaleFactor.Value;
        }

        public static Vector3 CanvasToScreenPosition(Vector3 canvasPosition)
        {
            return (canvasPosition / NoteCanvas.ScaleFactor.Value + new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
        }

        public static Note ToNote(SaveDataModel.Note musicNote)
        {
            return new Note(
                new NotePosition(musicNote.LPB, musicNote.num, musicNote.block),
                musicNote.type == 1 ? NoteTypes.Single : NoteTypes.Long);
        }

        public static string SerializeEditData()
        {
            var data = new SaveDataModel.EditData();
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
                    data.notes.Add(ToSaveData(noteObject));
                }
                else if (noteObject.note.type == NoteTypes.Long)
                {
                    var current = noteObject;
                    var note = ToSaveData(noteObject);

                    while (EditData.Notes.ContainsKey(current.note.next))
                    {
                        var nextObj = EditData.Notes[current.note.next];
                        note.notes.Add(ToSaveData(nextObj));
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

        static SaveDataModel.Note ToSaveData(NoteObject noteObject)
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
