using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;


public enum EditTypeEnum
{
    NormalNotes,
    LongNotes
}


public struct NotePosition
{
    public int samples, blockNum;

    public NotePosition(int samples, int blockNum)
    {
        this.samples = samples;
        this.blockNum = blockNum;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is NotePosition))
        {
            return false;
        }

        NotePosition target = (NotePosition)obj;
        return (samples == target.samples && blockNum == target.blockNum);
    }

    public override int GetHashCode()
    {
        return (blockNum + "-" + samples).GetHashCode();
    }
}


public class NotesEditorModel : SingletonGameObject<NotesEditorModel>
{
    public ReactiveProperty<float> BPM = new ReactiveProperty<float>(0);
    public ReactiveProperty<int> BeatOffsetSamples = new ReactiveProperty<int>(0);
    public ReactiveProperty<float> Volume = new ReactiveProperty<float>(1);
    public ReactiveProperty<bool> IsPlaying = new ReactiveProperty<bool>(false);
    public ReactiveProperty<int> DivisionNumOfOneMeasure = new ReactiveProperty<int>();
    public ReactiveProperty<float> CanvasOffsetX = new ReactiveProperty<float>();
    public ReactiveProperty<float> CanvasScaleFactor = new ReactiveProperty<float>();
    public ReactiveProperty<float> CanvasWidth = new ReactiveProperty<float>();
    public ReactiveProperty<bool> IsMouseOverCanvas = new ReactiveProperty<bool>();
    public ReactiveProperty<int> UnitBeatSamples = new ReactiveProperty<int>();
    public ReactiveProperty<bool> IsDraggingDuringPlay = new ReactiveProperty<bool>();
    public ReactiveProperty<NotePosition> ClosestNotePosition = new ReactiveProperty<NotePosition>();
    public ReactiveProperty<bool> WaveformDisplayEnabled = new ReactiveProperty<bool>(true);
    public Subject<NotePosition> NormalNoteObservable = new Subject<NotePosition>();

    public AudioSource Audio;
    public ReactiveProperty<EditTypeEnum> EditType = new ReactiveProperty<EditTypeEnum>(EditTypeEnum.NormalNotes);
    public Dictionary<NotePosition, NoteObject> NoteObjects = new Dictionary<NotePosition, NoteObject>();

    void Awake()
    {

    }

    public float SamplesToScreenPositionX(int samples)
    {
        return (samples - Audio.timeSamples + BeatOffsetSamples.Value)
            * CanvasWidth.Value / Audio.clip.samples
            + CanvasOffsetX.Value;
    }

    public float BlockNumToScreenPositionY(int blockNum)
    {
        return blockNum * 70 - 140;
    }
}
