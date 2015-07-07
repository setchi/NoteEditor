using UnityEngine;

public struct NotePosition
{
    public int BPM, LPB, num, block;

    public NotePosition(int BPM, int LPB, int num, int block)
    {
        this.BPM = BPM;
        this.LPB = LPB;
        this.num = num;
        this.block = block;
    }


    public int ToSamples(AudioClip audioClip)
    {
        return Mathf.FloorToInt(num * (audioClip.frequency * 60f / BPM / LPB));
    }

    public override string ToString()
    {
        return BPM + "-" + LPB + "-" + num + "-" + block;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is NotePosition))
        {
            return false;
        }

        NotePosition target = (NotePosition)obj;
        return (
            BPM == target.BPM &&
            Mathf.Approximately((float)num / LPB, (float)target.num / target.LPB) &&
            block == target.block);
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }
}
