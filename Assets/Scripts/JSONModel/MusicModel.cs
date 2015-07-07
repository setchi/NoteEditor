using System.Collections.Generic;

public class MusicModel
{

    public class NoteInfo
    {
        public int sample;
        public int blockNum;
        public int state;
        public int longNoteStartSample = -1;
        public int longNoteStartBlockNum = -1;

        public NoteInfo(int sample, int blockNum, int state)
        {
            this.sample = sample;
            this.blockNum = blockNum;
            this.state = state;
        }
    }

    public class NotesData
    {
        public string name;
        public int BPM;
        public int offset;
        public List<Note> notes;
    }

    public class Note
    {
        public int LPB;
        public int num;
        public int block;
        public int type;
        public List<Note> notes;
    }

}
