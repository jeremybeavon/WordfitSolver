namespace WordfitSolver
{
    public sealed class Letter
    {
        public char Character { get; set; }

        public Word HorizontalWord { get; internal set; }

        public Word VerticalWord { get; internal set; }

        public bool IsUnsolved
        {
            get { return Character == '\0'; }
        }
    }
}
