using System.Text;

namespace spinner;

public class Tools
{
    private static string _alphaNum = "abcdefghijklmnopkrstuvwxyz0123456789";

    /// <summary>
    /// Takes an array of dependencies and return an array node sorted topologically
    /// </summary>
    /// <exception cref="CircularReferenceException"/>
    /// <param name="dependencies"></param>
    /// <returns></returns>
    public static int[] TopoSort((int, int)[] dependencies, int nodeCount)
    {
        int[] order = new int[nodeCount];
        List<int>[] adjMatrix = new List<int>[nodeCount];
        int idx = nodeCount - 1;

        bool[] visited = new bool[nodeCount];
        bool[] marked = new bool[nodeCount];

        for (int i = 0; i < adjMatrix.Length; i++)
        {
            adjMatrix[i] = [];
        }

        // build adj matrix
        for (int i = 0; i < dependencies.Length; i++)
        {
            var el = dependencies[i];
            adjMatrix[el.Item2].Add(el.Item1);
        }

        for (int i = 0; i < nodeCount; i++)
        {
            if (!visited[i])
            {
                Walk(i, adjMatrix, visited, marked, order, ref idx);
            }
        }

        return order.ToArray();
    }

    private static void Walk(
        int node,
        List<int>[] adjMatrix,
        bool[] visited,
        bool[] marked,
        int[] order,
        ref int idx
    )
    {
        visited[node] = true;
        marked[node] = true;

        List<int> nb = adjMatrix[node];

        for (int i = 0; i < nb.Count; i++)
        {
            int nbNode = nb[i];
            if (marked[nbNode])
            {
                throw new CircularReferenceException<int>([.. nb]);
            }
            if (visited[nbNode])
            {
                continue;
            }
            Walk(nbNode, adjMatrix, visited, marked, order, ref idx);
        }

        order[idx] = node;
        idx--;

        marked[node] = false;
    }

    public static string GenerateRandomString(int len, string prefix = "", int? seed = null)
    {
        StringBuilder builder = new();
        Random r = new Random();

        if (seed is not null && seed.Value >= 0 && seed.Value <= int.MaxValue)
        {
            r = new(seed.Value);
        }
        else
        {
            r = Random.Shared;
        }

        int i = 0;
        for (; i < prefix.Length && i < len; i++)
        {
            builder.Append(prefix[i]);
        }

        for (; i < len; i++)
        {
            var pick = r.Next(_alphaNum.Length);
            builder.Append(_alphaNum[pick]);
        }

        return builder.ToString();
    }

    public static string GenerateRandomString(GenerationInfo info)
    {
        StringBuilder builder = new();
        Random r = new Random();

        if (info.Seed >= 0 && info.Seed <= int.MaxValue)
        {
            r = new(info.Seed);
        }
        else
        {
            r = Random.Shared;
        }

        int i = 0;
        for (; i < info.Prefix.Length && i < info.Len; i++)
        {
            builder.Append(info.Prefix[i]);
        }

        for (; i < info.Len; i++)
        {
            var pick = r.Next(_alphaNum.Length);
            builder.Append(_alphaNum[pick]);
        }

        return builder.ToString();
    }

    public static string StingDiff(string expected, string actual)
    {
        StringBuilder builder = new();
        var expectedList = expected.Split('\n');
        var actualList = actual.Split('\n');

        var len = Math.Max(expectedList.Length, actualList.Length);

        for (int i = 0; i < len; i++)
        {
            if (i >= expectedList.Length)
            {
                builder.Append((AnsiColors.Colorize($"- {actualList[i]}\n", AnsiColors.Red)));
                continue;
            }
            if (i >= actualList.Length)
            {
                builder.Append(AnsiColors.Colorize($"+ {expectedList[i]}\n", AnsiColors.Green));
                continue;
            }

            if (actualList[i] != expectedList[i])
            {
                builder.Append(AnsiColors.Colorize($"- {actualList[i]}\n", AnsiColors.Red));
                builder.Append(AnsiColors.Colorize($"+ {expectedList[i]}\n", AnsiColors.Green));
                continue;
            }

            builder.Append(AnsiColors.Colorize($"{actualList[i]}\n", AnsiColors.Gray));
        }

        return builder.ToString();
    }
}

public static class AnsiColors
{
    public const string Reset = "\u001b[0m";
    public const string Black = "\u001b[30m";
    public const string Red = "\u001b[31m";
    public const string Green = "\u001b[32m";
    public const string Yellow = "\u001b[33m";
    public const string Blue = "\u001b[34m";
    public const string Magenta = "\u001b[35m";
    public const string Cyan = "\u001b[36m";
    public const string White = "\u001b[37m";
    public const string BrightRed = "\u001b[91m";
    public const string BrightGreen = "\u001b[92m";
    public const string Gray = "\x1b[38;5;244m";

    public static string Colorize(string text, string colorCode) => colorCode + text + Reset;
}

public struct Diag2
{
    public Diag2(int start, int end, int len)
    {
        X1 = start;
        Y1 = end;
        Len = len;
    }

    public int X1 { get; init; }
    public int Y1 { get; init; }
    public int Len { get; init; }
    public Point TL => new(X1, Y1);
    public Point BR => new(X1 + Len, Y1 + Len);

    public override string ToString()
    {
        return $"({X1},{Y1}): {Len}";
    }
}

public struct Point
{
    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; init; }
    public int Y { get; init; }

    public override string ToString()
    {
        return $"({X},{Y})";
    }
}

public struct Range2
{
    public Range2(int start, int end)
    {
        Start = start;
        End = end;
    }

    public int Start { get; init; }
    public int End { get; init; }

    public override string ToString()
    {
        return $"[{Start},{End}]";
    }

    public static Range2[] ComputeRanges(Diag2 diag)
    {
        return [new(diag.X1, diag.X1 + diag.Len), new(diag.Y1, diag.Y1 + diag.Len)];
    }

    public static Range2[] ComputeRanges(Point p1, Point p2)
    {
        return
        [
            new(Math.Min(p1.X, p2.X), Math.Max(p1.X, p2.X)),
            new(Math.Min(p1.Y, p2.Y), Math.Max(p1.Y, p2.Y)),
        ];
    }
}

public enum DiffChangeType
{
    Add,
    Remove,
    Keep,
}

public struct DiffChange
{
    public int HashCode;
    public DiffChangeType Type = DiffChangeType.Add;

    public DiffChange(DiffChangeType type, int hashCode)
    {
        Type = type;
        HashCode = hashCode;
    }

    public override string ToString()
    {
        switch (Type)
        {
            case DiffChangeType.Add:
                return AnsiColors.Colorize($"+ {HashCode}", AnsiColors.Green);

            case DiffChangeType.Remove:
                return AnsiColors.Colorize($"- {HashCode}", AnsiColors.Red);

            case DiffChangeType.Keep:
                return AnsiColors.Colorize($"  {HashCode}", AnsiColors.Gray);
        }
        return "";
    }
}

public class DiffResult
{
    private Dictionary<int, string> Hashes = [];
    private DiffChange[] Changes = [];

    public DiffResult(Dictionary<int, string> hashes, DiffChange[] changes)
    {
        Hashes = hashes;
        Changes = changes;
    }

    public override string ToString()
    {
        StringBuilder builder = new();
        int added = 0;
        int removed = 0;

        for (int i = 0; i < Changes.Length; i++)
        {
            var line = Hashes[Changes[i].HashCode];
            if (line is null)
            {
                continue;
            }

            if (i > 0)
            {
                builder.Append("\n");
            }

            switch (Changes[i].Type)
            {
                case DiffChangeType.Add:
                    builder.Append(AnsiColors.Colorize($"+ {line}", AnsiColors.Green));
                    added++;
                    break;

                case DiffChangeType.Remove:
                    builder.Append(AnsiColors.Colorize($"- {line}", AnsiColors.Red));
                    removed++;
                    break;

                case DiffChangeType.Keep:
                    builder.Append(AnsiColors.Colorize($"  {line}", AnsiColors.Gray));
                    break;
            }
        }

        builder.Append(
            $"\n\n{AnsiColors.Colorize($"+ {added}", AnsiColors.Green)}\n{AnsiColors.Colorize($"- {removed}", AnsiColors.Red)}"
        );

        return builder.ToString();
    }
}

public static class Diff
{
    public static Diag2[] GetDiags(int[] expected, int[] actual)
    {
        Dictionary<(int, int), bool> visited = [];
        List<Diag2> res = [];

        for (int i = 0; i < expected.Length; i++)
        {
            for (int j = 0; j < actual.Length; j++)
            {
                int digLen = 0;
                int x = j;
                int y = i;

                while (
                    y < expected.Length
                    && x < actual.Length
                    && expected[y] == actual[x]
                    && !visited.ContainsKey((x, y))
                )
                {
                    visited.Add((x, y), true);
                    digLen++;
                    x++;
                    y++;
                }

                if (digLen > 0)
                {
                    res.Add(new(j, i, digLen));
                }
            }
        }

        return res.ToArray();
    }

    public static DiffResult TextDiff(string actual, string expected)
    {
        Dictionary<int, string> linesHashes = [];

        var actualHList = SplitTextToHash(actual, linesHashes);
        var expectedHList = SplitTextToHash(expected, linesHashes);
        var diags = GetDiags(expectedHList, actualHList);

        var bds = FindBDS(diags.ToList(), expectedHList.Length, actualHList.Length);
        var diff = BuildDiffPath(bds, expectedHList, actualHList);

        return new(linesHashes, diff);
    }

    private static int[] SplitTextToHash(string text, Dictionary<int, string> hashes)
    {
        var parts = text.Split("\n");
        List<int> res = [];

        for (int i = 0; i < parts.Length; i++)
        {
            var hash = HashCode(parts[i]);
            res.Add(hash);
            if (hashes.ContainsKey(hash))
            {
                continue;
            }
            hashes.Add(hash, parts[i]);
        }
        return res.ToArray();
    }

    private static DiffChange[] BuildDiffPath(List<Diag2> diags, int[] expected, int[] actual)
    {
        List<DiffChange> changes = [];
        int x = 0;
        int y = 0;

        for (int i = 0; i < diags.Count; i++)
        {
            for (; x < diags[i].X1; x++)
            {
                changes.Add(new(DiffChangeType.Remove, actual[x]));
            }
            for (; y < diags[i].Y1; y++)
            {
                changes.Add(new(DiffChangeType.Add, expected[y]));
            }

            for (int j = 0; j < diags[i].Len; j++, x++, y++)
            {
                changes.Add(new(DiffChangeType.Keep, actual[x]));
            }
        }

        for (int i = x; i < actual.Length; i++)
        {
            changes.Add(new(DiffChangeType.Remove, actual[i]));
        }

        for (int i = y; i < expected.Length; i++)
        {
            changes.Add(new(DiffChangeType.Add, expected[i]));
        }

        return changes.ToArray();
    }

    private static Diag2[] FindLDS(Diag2[] diags)
    {
        int bestLen = int.MinValue;
        List<Diag2> result = [];
        List<Diag2> path = [];
        Dictionary<(int, int), int> scores = [];

        FindLDSRecursive(diags, 0, ref bestLen, 0, 0, 0, path, result);
        return result.ToArray();
    }

    public static List<Diag2> FindBDS(List<Diag2> diags, int rows, int cols)
    {
        diags.Sort(
            (a, b) =>
            {
                return b.Len - a.Len;
            }
        );

        Dictionary<(Range2, Range2), (int, List<Diag2>)> cache = [];

        var res = FindBDS(diags, new Range2(0, rows), new Range2(0, cols), cache);

        return res.Item2;
    }

    private static (int, List<Diag2>) FindBDS(
        List<Diag2> diags,
        Range2 xRange,
        Range2 yRange,
        Dictionary<(Range2, Range2), (int, List<Diag2>)> cache
    )
    {
        if (cache.ContainsKey((xRange, yRange)))
        {
            return cache[(xRange, yRange)];
        }

        if (diags.Count == 0 || xRange.Start >= xRange.End || yRange.Start >= yRange.End)
        {
            return (0, []);
        }

        if (diags.Count == 1)
        {
            return (diags[0].Len, diags);
        }

        List<Diag2> res = [];
        int bestIdx = 0;
        int bestLen = 0;
        (int, List<Diag2>) bestL = (0, []);
        (int, List<Diag2>) bestR = (0, []);
        for (int i = 0; i < diags.Count && i < 10; i++)
        {
            Range2 leftXRange = new(xRange.Start, diags[i].X1);
            Range2 leftYRange = new(yRange.Start, diags[i].Y1);

            Range2 rightXRange = new(diags[i].X1 + diags[i].Len, xRange.End);
            Range2 rightYRange = new(diags[i].Y1 + diags[i].Len, yRange.End);

            var l1 = diags
                .Where(v =>
                {
                    if (!IsDiagIncluded(v, leftXRange, leftYRange))
                    {
                        return false;
                    }
                    return true;
                })
                .ToList();

            var l2 = diags
                .Where(v =>
                {
                    if (!IsDiagIncluded(v, rightXRange, rightYRange))
                    {
                        return false;
                    }
                    return true;
                })
                .ToList();

            var left = FindBDS(l1, leftXRange, leftYRange, cache);
            var right = FindBDS(l2, rightXRange, rightYRange, cache);

            if (left.Item1 + right.Item1 + diags[i].Len > bestLen)
            {
                bestIdx = i;
                bestLen = left.Item1 + right.Item1 + diags[i].Len;
                bestL = left;
                bestR = right;
            }
        }

        for (int i = 0; i < bestL.Item2.Count; i++)
        {
            res.Add(bestL.Item2[i]);
        }
        res.Add(diags[bestIdx]);

        for (int i = 0; i < bestR.Item2.Count; i++)
        {
            res.Add(bestR.Item2[i]);
        }

        cache.Add((xRange, yRange), (bestL.Item1 + bestR.Item1 + diags[bestIdx].Len, res));

        return (bestL.Item1 + bestR.Item1 + diags[bestIdx].Len, res);
    }

    private static void FindLDSRecursive(
        Diag2[] diags,
        int current,
        ref int bestScore,
        int currentLen,
        int furthestX,
        int furthestY,
        List<Diag2> path,
        List<Diag2> result
    )
    {
        // Path heuristic
        int score = currentLen - path.Count;
        //int score = 2* currentLen + path.Count;
        //
        if (score > bestScore)
        {
            bestScore = score;
            result.Clear();
            for (int i = 0; i < path.Count; i++)
            {
                result.Add(path[i]);
            }
        }

        for (int i = current; i < diags.Length; i++)
        {
            var cur = diags[i];
            // range overlap
            if (
                path.Count > 0
                && IsOverlappingDiag(cur, new Range2(0, furthestX), new Range2(0, furthestY))
            )
            {
                continue;
            }

            var fx = furthestX;
            var fy = furthestY;

            furthestX = (cur.X1 + cur.Len);
            furthestY = (cur.Y1 + cur.Len);

            currentLen += cur.Len;
            path.Add(cur);

            FindLDSRecursive(
                diags,
                i + 1,
                ref bestScore,
                currentLen,
                furthestX,
                furthestY,
                path,
                result
            );

            // backtrack
            currentLen -= cur.Len;
            furthestX = fx;
            furthestY = fy;
            path.RemoveAt(path.Count - 1);
        }
    }

    private static bool IsOverlappingRange(Range2 r1, Range2 r2)
    {
        if (r1.Start <= r2.Start)
        {
            return r2.Start < r1.End;
        }

        return r1.Start < r2.End;
    }

    private static bool IsOverlappingDiag(Diag2 d, Range2 xr, Range2 yr)
    {
        var rs = Range2.ComputeRanges(d);
        return IsOverlappingRange(rs[0], xr) || IsOverlappingRange(rs[1], yr);
    }

    private static bool IsOverlappingDiag(Diag2 d, Point p1, Point p2)
    {
        var r1 = Range2.ComputeRanges(d);
        var r2 = Range2.ComputeRanges(p1, p2);

        return IsOverlappingRange(r1[0], r2[0]) || IsOverlappingRange(r1[1], r2[1]);
    }

    /* Create a x range and an Y range out of
     * those 2 points and check if the diagonal is completly included in those ranges
     * */
    private static bool IsDiagIncluded(Diag2 d, Point p1, Point p2)
    {
        var diagRange = Range2.ComputeRanges(d);
        var pointRange = Range2.ComputeRanges(p1, p2);

        return IsRangeIncluded(pointRange[0], diagRange[0])
            && IsRangeIncluded(pointRange[1], diagRange[1]);
    }

    private static bool IsDiagIncluded(Diag2 d, Range2 r1, Range2 r2)
    {
        var diagRange = Range2.ComputeRanges(d);

        return IsRangeIncluded(r1, diagRange[0]) && IsRangeIncluded(r2, diagRange[1]);
    }

    private static bool IsRangeIncluded(Range2 r1, Range2 r2)
    {
        return r1.Start <= r2.Start && r1.End >= r2.End;
    }

    private static bool IsOverlappingDiag(Diag2 d1, Diag2 d2)
    {
        var rs1 = Range2.ComputeRanges(d1);
        var rs2 = Range2.ComputeRanges(d2);

        return IsOverlappingRange(rs1[0], rs2[0]) || IsOverlappingRange(rs1[1], rs2[1]);
    }

    public static int HashCode(string content)
    {
        const uint prime = 16777619;
        uint hash = 2166136261;

        for (int i = 0; i < content.Length; i++)
        {
            hash = (hash ^ content[i]) * prime;
        }

        return (int)hash;
    }
}
