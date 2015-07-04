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