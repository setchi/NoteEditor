using LitJson;
using NoteEditor.Model.JSON;
using NoteEditor.Notes;
using NoteEditor.Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniRx;

namespace NoteEditor.Model
{
    public class EditData : SingletonMonoBehaviour<EditData>
    {
        ReactiveProperty<string> name_ = new ReactiveProperty<string>();
        ReactiveProperty<int> maxBlock_ = new ReactiveProperty<int>(5);
        ReactiveProperty<int> LPB_ = new ReactiveProperty<int>(4);
        ReactiveProperty<int> BPM_ = new ReactiveProperty<int>(120);
        ReactiveProperty<int> offsetSamples_ = new ReactiveProperty<int>(0);
        Dictionary<NotePosition, NoteObject> notes_ = new Dictionary<NotePosition, NoteObject>();

        public static ReactiveProperty<string> Name { get { return Instance.name_; } }
        public static ReactiveProperty<int> MaxBlock { get { return Instance.maxBlock_; } }
        public static ReactiveProperty<int> LPB { get { return Instance.LPB_; } }
        public static ReactiveProperty<int> BPM { get { return Instance.BPM_; } }
        public static ReactiveProperty<int> OffsetSamples { get { return Instance.offsetSamples_; } }
        public static Dictionary<NotePosition, NoteObject> Notes { get { return Instance.notes_; } }

        public static string SerializeEditData()
        {
            var data = new SaveDataModel.EditData();
            data.BPM = BPM.Value;
            data.maxBlock = MaxBlock.Value;
            data.offset = OffsetSamples.Value;
            data.name = Path.GetFileNameWithoutExtension(Name.Value);

            var sortedNoteObjects = Notes.Values
                .Where(note => !(note.note.type == NoteTypes.Long && Notes.ContainsKey(note.note.prev)))
                .OrderBy(note => note.note.position.ToSamples(Audio.Source.clip.frequency, BPM.Value));

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

                    while (Notes.ContainsKey(current.note.next))
                    {
                        var nextObj = Notes[current.note.next];
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
