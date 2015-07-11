using LitJson;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public enum NoteTypes { Normal, Long }

public class NotesEditorModel : SingletonGameObject<NotesEditorModel>
{
    public ReactiveProperty<NoteTypes> EditType = new ReactiveProperty<NoteTypes>(NoteTypes.Normal);
    public ReactiveProperty<string> MusicName = new ReactiveProperty<string>();
    public ReactiveProperty<int> MaxBlock = new ReactiveProperty<int>(5);
    public ReactiveProperty<int> LPB = new ReactiveProperty<int>(4);
    public ReactiveProperty<int> BPM = new ReactiveProperty<int>(0);
    public ReactiveProperty<int> BeatOffsetSamples = new ReactiveProperty<int>(0);
    public ReactiveProperty<float> Volume = new ReactiveProperty<float>(1);
    public ReactiveProperty<bool> IsPlaying = new ReactiveProperty<bool>(false);
    public ReactiveProperty<int> TimeSamples = new ReactiveProperty<int>();
    public ReactiveProperty<float> CanvasOffsetX = new ReactiveProperty<float>();
    public ReactiveProperty<float> CanvasScaleFactor = new ReactiveProperty<float>();
    public ReactiveProperty<float> CanvasWidth = new ReactiveProperty<float>();
    public ReactiveProperty<bool> IsMouseOverNotesRegion = new ReactiveProperty<bool>();
    public ReactiveProperty<bool> IsOperatingPlaybackPositionDuringPlay = new ReactiveProperty<bool>(false);
    public ReactiveProperty<NotePosition> ClosestNotePosition = new ReactiveProperty<NotePosition>();
    public ReactiveProperty<bool> WaveformDisplayEnabled = new ReactiveProperty<bool>(true);
    public ReactiveProperty<bool> PlaySoundEffectEnabled = new ReactiveProperty<bool>(true);
    public Dictionary<NotePosition, NoteObject> NoteObjects = new Dictionary<NotePosition, NoteObject>();
    public ReactiveProperty<NotePosition> LongNoteTailPosition = new ReactiveProperty<NotePosition>();
    public Subject<NotePosition> NormalNoteObservable = new Subject<NotePosition>();
    public Subject<NotePosition> LongNoteObservable = new Subject<NotePosition>();
    public Subject<int> OnLoadedMusicObservable = new Subject<int>();
    public Subject<NoteObject> AddedLongNoteObjectObservable = new Subject<NoteObject>();

    [HideInInspector]
    public AudioSource Audio;

    [SerializeField]
    CanvasScaler canvasScaler;

    void Awake()
    {
        Audio = gameObject.AddComponent<AudioSource>();

        CanvasScaleFactor = this.UpdateAsObservable()
            .Select(_ => Screen.width)
            .DistinctUntilChanged()
            .Select(w => CanvasScaleFactor.Value = canvasScaler.referenceResolution.x / w)
            .ToReactiveProperty();

        ClearNotesData();
    }

    public void ClearNotesData()
    {
        BPM.Value = 120;
        BeatOffsetSamples.Value = 0;
        MusicName.Value = "Notes Editor";
        MaxBlock.Value = NotesEditorSettingsModel.Instance.MaxBlock;
        LPB.Value = 4;
        IsPlaying.Value = false;
        TimeSamples.Value = 0;

        foreach (var noteObject in NoteObjects.Values)
        {
            DestroyObject(noteObject.gameObject);
        }

        NoteObjects.Clear();
    }

    public float SamplesToScreenPositionX(int samples)
    {
        return (samples - Audio.timeSamples + BeatOffsetSamples.Value)
            * CanvasWidth.Value / Audio.clip.samples
            + CanvasOffsetX.Value;
    }

    public float BlockNumToScreenPositionY(int blockNum)
    {
        var height = 240f;
        var maxIndex = MaxBlock.Value - 1;
        return ((maxIndex - blockNum) * height / maxIndex - height / 2) / CanvasScaleFactor.Value;
    }

    public Vector3 NoteToScreenPosition(NotePosition notePosition)
    {
        return new Vector3(
            SamplesToScreenPositionX(notePosition.ToSamples(Audio.clip.frequency, BPM.Value)),
            BlockNumToScreenPositionY(notePosition.block) * CanvasScaleFactor.Value,
            0);
    }

    public Vector3 ScreenToCanvasPosition(Vector3 screenPosition)
    {
        return (screenPosition - new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0)) * CanvasScaleFactor.Value;
    }

    public string SerializeNotesData()
    {
        var data = new MusicModel.NotesData();
        data.BPM = BPM.Value;
        data.maxBlock = MaxBlock.Value;
        data.offset = BeatOffsetSamples.Value;
        data.name = Path.GetFileNameWithoutExtension(MusicName.Value);

        var sortedNoteObjects = NoteObjects.Values
            .Where(note => !(note.noteType.Value == NoteTypes.Long && note.prev != null))
            .OrderBy(note => note.notePosition.ToSamples(Audio.clip.frequency, BPM.Value));

        data.notes = new List<MusicModel.Note>();

        foreach (var noteObject in sortedNoteObjects)
        {
            if (noteObject.noteType.Value == NoteTypes.Normal)
            {
                data.notes.Add(ConvertToNote(noteObject));
            }
            else if (noteObject.noteType.Value == NoteTypes.Long)
            {
                var current = noteObject;
                var note = ConvertToNote(noteObject);

                while (current.next != null)
                {
                    note.notes.Add(ConvertToNote(current.next));
                    current = current.next;
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

    public MusicModel.Note ConvertToNote(NoteObject noteObject)
    {
        var note = new MusicModel.Note();
        note.num = noteObject.notePosition.num;
        note.block = noteObject.notePosition.block;
        note.LPB = noteObject.notePosition.LPB;
        note.type = noteObject.noteType.Value == NoteTypes.Long ? 2 : 1;
        note.notes = new List<MusicModel.Note>();
        return note;
    }
}
