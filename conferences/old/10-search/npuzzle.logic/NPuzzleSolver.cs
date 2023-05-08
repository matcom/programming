namespace npuzzle.logic;

public static class NPuzzleSolver
{
    public static NPuzzle.Movement[] Solve(NPuzzle puzzle, int maxSteps)
    {
        var steps = new List<NPuzzle.Movement>();
        var seen = new HashSet<NPuzzle>();

        if (Solve(puzzle, steps, seen, maxSteps))
        {
            return steps.ToArray();
        }

        throw new InvalidOperationException("Unsolvable puzzle");
    }

    private static bool Solve(NPuzzle puzzle, List<NPuzzle.Movement> steps, HashSet<NPuzzle> seen, int maxSteps)
    {
        if (seen.Contains(puzzle))
        {
            return false;
        }

        if (steps.Count > maxSteps)
        {
            return false;
        }

        seen.Add(puzzle);

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

                if (Solve(puzzle.Move(m), steps, seen, maxSteps))
                {
                    return true;
                }

                steps.RemoveAt(steps.Count - 1);
            }
        }

        seen.Remove(puzzle);
        return false;
    }
}
