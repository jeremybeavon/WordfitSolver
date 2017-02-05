using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WordfitSolver
{
    public sealed class Word
    {
        public Word(IReadOnlyList<Letter> letters, Direction direction)
        {
            Letters = letters;
            Direction = direction;
            foreach (Letter letter in letters)
            {
                if (direction == Direction.Horizontal)
                {
                    letter.HorizontalWord = this;
                }
                else
                {
                    letter.VerticalWord = this;
                }
            }
        }

        public IReadOnlyList<Letter> Letters { get; private set; }

        public Direction Direction { get; private set; }

        public int Length
        {
            get { return Letters.Count; }
        }

        public Letter this[int index]
        {
            get { return Letters[index]; }
        }

        public bool IsHorizontal
        {
            get { return Direction.IsHorizontal(); }
        }

        public bool IsSolved
        {
            get { return UnsolvedCharacterCount == 0; }
        }

        public int UnsolvedCharacterCount
        {
            get { return Letters.Count(letter => letter.IsUnsolved); }
        }

        public bool IsMatch(string word)
        {
            return Regex.IsMatch(word, string.Format("^{0}$", ToString()), RegexOptions.IgnoreCase);
        }

        public override string ToString()
        {
            return new string(Letters.Select(letter => letter.IsUnsolved ? '.' : letter.Character).ToArray());
        }
    }
}
