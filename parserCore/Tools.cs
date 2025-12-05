using System.Text;

namespace spinner;

public class Tools
{
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
        //backtrack marked node
        marked[node] = false;
    }

    public static string GenerateRandomString(int len, string prefix = "")
    {
        StringBuilder builder = new();
        string alphaNum = "abcdefghijklmnopkrstuvwxyz0123456789";
        int i = 0;
        for (; i < prefix.Length && i < len; i++)
        {
            builder.Append(prefix[i]);
        }

        for (; i < len; i++)
        {
            var pick = Random.Shared.Next(alphaNum.Length);
            builder.Append(alphaNum[pick]);
        }

        return builder.ToString();
    }
}
