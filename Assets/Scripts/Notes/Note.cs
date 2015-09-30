namespace NoteEditor.Notes
{
    public class Note
    {
        public NotePosition position = NotePosition.None;
        public NoteTypes type = NoteTypes.Single;
        public NotePosition next = NotePosition.None;
        public NotePosition prev = NotePosition.None;

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

        public Note(Note note)
        {
            this.position = note.position;
            this.type = note.type;
            this.next = note.next;
            this.prev = note.prev;
        }

        public Note() { }


        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var c = (Note)obj;

            return position.Equals(c.position) &&
                type == c.type &&
                next.Equals(c.next) &&
                prev.Equals(c.prev);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
