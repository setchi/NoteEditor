using System.Collections.Generic;
using UniRx;
using UnityEngine;

public enum NoteTypes { Normal, Long }

public class NotesEditorModel : SingletonGameObject<NotesEditorModel>
{
    public ReactiveProperty<NoteTypes> EditType = new ReactiveProperty<NoteTypes>(NoteTypes.Normal);
    public ReactiveProperty<float> BPM = new ReactiveProperty<float>(0);
    public ReactiveProperty<int> BeatOffsetSamples = new ReactiveProperty<int>(0);
    public ReactiveProperty<float> Volume = new ReactiveProperty<float>(1);
    public ReactiveProperty<bool> IsPlaying = new ReactiveProperty<bool>(false);
    public ReactiveProperty<int> LPB = new ReactiveProperty<int>(4);
    public ReactiveProperty<float> CanvasOffsetX = new ReactiveProperty<float>();
    public ReactiveProperty<float> CanvasScaleFactor = new ReactiveProperty<float>();
    public ReactiveProperty<float> CanvasWidth = new ReactiveProperty<float>();
    public ReactiveProperty<bool> IsMouseOverCanvas = new ReactiveProperty<bool>();
    public ReactiveProperty<int> UnitBeatSamples = new ReactiveProperty<int>();
    public ReactiveProperty<bool> IsDraggingDuringPlay = new ReactiveProperty<bool>();
    public ReactiveProperty<NotePosition> ClosestNotePosition = new ReactiveProperty<NotePosition>();
    public ReactiveProperty<bool> WaveformDisplayEnabled = new ReactiveProperty<bool>(true);
    public Dictionary<NotePosition, NoteObject> NoteObjects = new Dictionary<NotePosition, NoteObject>();
    public ReactiveProperty<NotePosition> LongNoteTailPosition = new ReactiveProperty<NotePosition>();
    public Subject<NotePosition> NormalNoteObservable = new Subject<NotePosition>();
    public Subject<NotePosition> LongNoteObservable = new Subject<NotePosition>();
    public Subject<SelectedMusicDataStore> OnLoadedMusicObservable = new Subject<SelectedMusicDataStore>();
    public Subject<NoteObject> AddedLongNoteObjectObservable = new Subject<NoteObject>();
    public AudioSource Audio;

    public float SamplesToScreenPositionX(int samples)
    {
        return (samples - Audio.timeSamples + BeatOffsetSamples.Value)
            * CanvasWidth.Value / Audio.clip.samples
            + CanvasOffsetX.Value;
    }

    public float BlockNumToScreenPositionY(int blockNum)
    {
        return (blockNum * 60 - 120) / CanvasScaleFactor.Value;
    }

    public Vector3 ScreenToCanvasPosition(Vector3 screenPosition)
    {
        return (screenPosition - new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0)) * CanvasScaleFactor.Value;
    }
}
