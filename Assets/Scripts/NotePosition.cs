using UnityEngine;

public struct NotePosition
{
    public int LPB, num, block;

    public NotePosition(int BPM, int LPB, int num, int block)
    {
        this.LPB = LPB;
        this.num = num;
        this.block = block;
    }


    public int ToSamples(int frequency, int BPM)
    {
        return Mathf.FloorToInt(num * (frequency * 60f / BPM / LPB));
    }

    public override string ToString()
    {
        return LPB + "-" + num + "-" + block;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is NotePosition))
        {
            return false;
        }

        NotePosition target = (NotePosition)obj;
        return (
            Mathf.Approximately((float)num / LPB, (float)target.num / target.LPB) &&
            block == target.block);
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }
}
