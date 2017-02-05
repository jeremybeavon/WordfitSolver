using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordfitSolver
{
    public static class DirectionExtensions
    {
        public static bool IsHorizontal(this Direction direction)
        {
            return direction == Direction.Horizontal;
        }
    }
}
