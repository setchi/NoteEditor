public class Note
{
    public NotePosition position = NotePosition.None;
    public NoteTypes type = NoteTypes.Normal;
    public NotePosition next;
    public NotePosition prev;

    public Note(NotePosition position, NoteTypes type, NotePosition next, NotePosition prev)
    {
        this.position = position;
        this.type = type;
        this.next = next;
        this.prev = prev;
    }

    public Note(NotePosition position, NoteTypes type)
    {
        this.position = position;
        this.type = type;
    }

    public Note(NotePosition position)
    {
        this.position = position;
    }

    public Note() { }
}
