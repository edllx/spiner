using System.Text;

namespace spinner;

public class Test
{
    public TestRequest? Request { get; init; }
    public TestResponse? Response { get; init; }
    public TestAssert? Asserts { get; init; }
    public Scope Scope { get; init; }

    public Test()
    {
        Request = null;
        Scope = new();
        Asserts = null;
        Response = null;
    }

    public Test(
        TestRequest? request = null,
        TestResponse? response = null,
        Scope? scope = null,
        TestAssert? asserts = null
    )
    {
        Request = request;
        Scope = scope ?? new();
        Asserts = asserts;
        Response = response;
    }

    public void Resolve(Scope? scope = null)
    {
        // Resolve Scope
        if (Scope.Parent is not null)
        {
            for (int i = 0; i < Scope.Keys.Count; i++)
            {
                var key = Scope.Keys[i];
                var val = KeyManager.Resolve(key.Value, Scope.Parent);
                key.Set(val);
            }
        }

        // Resolve request
        /*
        if (Request is not null)
        {
            Request.Resolve();
        }

        if (Asserts is not null)
        {
            Asserts.Resolve(Scope);
        }
        */
    }

    public string ToString(int depth = 0)
    {
        StringBuilder builder = new();

        builder.Append($"{"".PadRight(4 * depth)}<Test>");
        if (Scope.Keys.Count > 0)
        {
            builder.Append($"\n{Scope.ToString(depth + 1)}");
        }

        if (Request is not null)
        {
            builder.Append($"\n{Request.ToString(depth + 1)}");
        }

        if (Response is not null)
        {
            builder.Append($"\n{Response.ToString(depth + 1)}");
        }

        if (Asserts is not null)
        {
            builder.Append($"\n{Asserts.ToString(depth + 1)}");
        }

        builder.Append($"\n{"".PadRight(4 * depth)}</Test>");

        return builder.ToString();
    }
}
