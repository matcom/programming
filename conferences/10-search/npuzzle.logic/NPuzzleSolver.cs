namespace npuzzle.logic
{
    public static class NPuzzleSolver
    {
        public static NPuzzle.Movement[] Solve(NPuzzle puzzle)
        {
            var steps = new List<NPuzzle.Movement>();

            if (Solve(puzzle, steps))
            {
                return steps.ToArray();
            }

            throw new InvalidOperationException("Unsolvable puzzle");
        }

        private static bool Solve(NPuzzle puzzle, List<NPuzzle.Movement> steps)
        {
            if (puzzle.Solved())
            {
                return true;
            }

            for (int i = 0; i < 4; i++)
            {
                NPuzzle.Movement m = (NPuzzle.Movement)i;

                if (puzzle.CanMove(m))
                {
                    steps.Add(m);

                    if (Solve(puzzle.Move(m), steps))
                    {
                        return true;
                    }

                    steps.RemoveAt(steps.Count - 1);
                }
            }

            return false;
        }
    }
}