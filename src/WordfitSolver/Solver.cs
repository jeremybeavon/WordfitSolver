using System;
using System.Collections.Generic;
using System.Linq;

namespace WordfitSolver
{
    public sealed class Solver
    {
        private readonly int boardHeight;
        private readonly int boardWidth;
        private readonly Letter[,] board;
        private readonly IReadOnlyDictionary<int, IList<Word>> unsolvedWords;
        private readonly IReadOnlyDictionary<int, IList<string>> wordsToFit;

        public Solver(
            int boardHeight,
            int boardWidth,
            int[][] locationsOfBlackSquares,
            string[] wordsToFit,
            string initialWord,
            int initialWordX,
            int initialWordY,
            Direction initialWordDirection)
        {
            this.boardHeight = boardHeight;
            this.boardWidth = boardWidth;
            board = CreateBoard(boardHeight, boardWidth, locationsOfBlackSquares);
            unsolvedWords = FindWords();
            this.wordsToFit = CreateDictionary(wordsToFit, word => word.Length);
            PlaceInitialWord(initialWord, initialWordX, initialWordY, initialWordDirection);
        }

        public static Board Solve(
            int boardHeight,
            int boardWidth,
            int[][] locationsOfBlackSquares,
            string[] wordsToFit,
            string initialWord,
            int initialWordX,
            int initialWordY,
            Direction initialWordDirection)
        {
            return new Solver(boardHeight, boardWidth, locationsOfBlackSquares, wordsToFit, initialWord, initialWordX, initialWordY, initialWordDirection).Solve();
        }

        private static Letter[,] CreateBoard(int boardHeight, int boardWidth, int[][] locationsOfBlackSquares)
        {
            Letter[,] board = new Letter[boardWidth, boardHeight];
            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    if (!locationsOfBlackSquares[y].Contains(x))
                    {
                        board[x, y] = new Letter();
                    }
                }
            }

            return board;
        }

        private static IReadOnlyDictionary<int, IList<string>> CreateWordsToFit(string[] wordsToFit)
        {
            return wordsToFit.GroupBy(word => word.Length).ToDictionary(group => group.Key, group => (IList<string>)group.ToList());
        }

        private static IReadOnlyDictionary<int, IList<T>> CreateDictionary<T>(IEnumerable<T> enumerable, Func<T, int> toLengthFunc)
        {
            return enumerable.GroupBy(toLengthFunc).ToDictionary(group => group.Key, group => (IList<T>)group.ToList());
        }

        private IReadOnlyDictionary<int, IList<Word>> FindWords()
        {
            IList<Word> words = new List<Word>();
            FindWords(words, Direction.Horizontal);
            FindWords(words, Direction.Vertical);
            return words.GroupBy(word => word.Length).ToDictionary(group => group.Key, group => (IList<Word>)group.ToList());
        }

        private void FindWords(IList<Word> words, Direction direction)
        {
            bool isHorizontal = direction.IsHorizontal();
            for (int outer = 0; outer < (isHorizontal ? boardHeight : boardWidth); outer++)
            {
                List<Letter> word = new List<Letter>();
                for (int inner = 0; inner < (isHorizontal ? boardWidth : boardHeight); inner++)
                {
                    Letter letter = isHorizontal ? board[inner, outer] : board[outer, inner];
                    if (letter != null)
                    {
                        word.Add(letter);
                    }
                    else if (word.Count != 0)
                    {
                        words.Add(new Word(word, direction));
                        word = new List<Letter>();
                    }
                }
            }
        }

        private void PlaceInitialWord(string word, int x, int y, Direction direction)
        {
            Letter initialLetter = board[x, y];
            Word letters = direction.IsHorizontal() ? initialLetter.HorizontalWord : initialLetter.VerticalWord;
            if (letters == null)
            {
                throw new InvalidOperationException("Initial word not found at coordinates at the correct direction.");
            }

            if (ReferenceEquals(initialLetter, letters[0]))
            {
                throw new InvalidOperationException("Initial word coordinates do not correspond to the start of a word.");
            }

            if (word.Length != letters.Length)
            {
                throw new InvalidOperationException("Initial word is not the same length as the coordinates.");
            }

            SolveWord(word, letters);
        }

        private Board Solve()
        {
            while (unsolvedWords.Count != 0)
            {
                Word[] wordsToSolve = unsolvedWords
                    .Where(item => item.Key != 0)
                    .OrderByDescending(group => group.Key)
                    .SelectMany(item => item.Value)
                    .ToArray();
                if (!FindSimpleMatch(wordsToSolve))
                {
                    throw new InvalidOperationException("No solution could be found.");
                }
            }

            return null;
        }

        private bool FindSimpleMatch(IEnumerable<Word> wordsToSolve)
        {
            foreach (Word wordToSolve in wordsToSolve)
            {
                string[] matchedWords = wordsToFit[wordToSolve.Length].Where(word => wordToSolve.IsMatch(word)).ToArray();
                if (matchedWords.Length == 0)
                {
                    throw new InvalidOperationException("No matches found for word: " + wordToSolve);
                }

                if (matchedWords.Length == 1)
                {
                    SolveWord(matchedWords[0], wordToSolve);
                    return true;
                }
            }

            return false;
        }

        private void SolveWord(string word, Word letters)
        {
            if (word.Length != letters.Length)
            {
                throw new InvalidOperationException("Mismatch between word and letters.");
            }

            RemoveSolvedWord(letters);
            for (int index = 0; index < word.Length; index++)
            {
                Letter letter = letters[index];
                if (!letter.IsUnsolved)
                {
                    letters[index].Character = word[index];
                    if (letters.IsHorizontal)
                    {
                        RemoveSolvedWord(letter.VerticalWord);
                    }
                    else
                    {
                        RemoveSolvedWord(letter.HorizontalWord);
                    }
                }
            }
        }
        
        private void RemoveSolvedWord(Word word)
        {
            if (word.IsSolved)
            {
                wordsToFit[word.Length].Remove(word.ToString());
                unsolvedWords[word.Length].Remove(word);
            }
        }
    }
}
