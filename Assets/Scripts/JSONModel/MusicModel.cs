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

    public class Note
    {
        public int sample;
        public int blockNum;
        public int state;
        public List<Note> noteList;
    }

    public class NotesData
    {
        public int offset;
        public string fileName;
        public string BPM;
        public List<Note> notes;
    }
}
