using LitJson;
using NoteEditor.DTO;
using NoteEditor.Notes;
using NoteEditor.Presenter;
using NoteEditor.Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NoteEditor.Model
{
    public class EditDataSerializer
    {
        public static string Serialize()
        {
            var data = new MusicDTO.EditData();
            data.BPM = EditData.BPM.Value;
            data.maxBlock = EditData.MaxBlock.Value;
            data.offset = EditData.OffsetSamples.Value;
            data.name = Path.GetFileNameWithoutExtension(EditData.Name.Value);

            var sortedNoteObjects = EditData.Notes.Values
                .Where(note => !(note.note.type == NoteTypes.Long && EditData.Notes.ContainsKey(note.note.prev)))
                .OrderBy(note => note.note.position.ToSamples(Audio.Source.clip.frequency, EditData.BPM.Value));

            data.notes = new List<MusicDTO.Note>();

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

        public static void Deserialize(string json)
        {
            var editData = JsonMapper.ToObject<MusicDTO.EditData>(json);
            var notePresenter = EditNotesPresenter.Instance;

            EditData.BPM.Value = editData.BPM;
            EditData.MaxBlock.Value = editData.maxBlock;
            EditData.OffsetSamples.Value = editData.offset;

            foreach (var note in editData.notes)
            {
                if (note.type == 1)
                {
                    notePresenter.AddNote(ConvertUtils.ToNote(note));
                    continue;
                }

                var longNoteObjects = new[] { note }.Concat(note.notes)
                    .Select(note_ =>
                    {
                        notePresenter.AddNote(ConvertUtils.ToNote(note_));
                        return EditData.Notes[ConvertUtils.ToNote(note_).position];
                    })
                    .ToList();

                for (int i = 1; i < longNoteObjects.Count; i++)
                {
                    longNoteObjects[i].note.prev = longNoteObjects[i - 1].note.position;
                    longNoteObjects[i - 1].note.next = longNoteObjects[i].note.position;
                }

                EditState.LongNoteTailPosition.Value = NotePosition.None;
            }
        }

        static MusicDTO.Note ToSaveData(NoteObject noteObject)
        {
            var note = new MusicDTO.Note();
            note.num = noteObject.note.position.num;
            note.block = noteObject.note.position.block;
            note.LPB = noteObject.note.position.LPB;
            note.type = noteObject.note.type == NoteTypes.Long ? 2 : 1;
            note.notes = new List<MusicDTO.Note>();
            return note;
        }
    }
}
