using System.Text;

namespace spinner;

public class RequestManager
{
    public List<RequestTemplate> Templates { get; set; } = [];

    public void SetTemplates(List<RequestTemplate>? templates)
    {
        if (templates is null)
        {
            return;
        }
        Templates = templates;
    }

    public RequestTemplate? GetTemplate(string name)
    {
        return Templates.Find(v => v.Name == name);
    }

    public string ToString(int depth = 0)
    {
        StringBuilder builder = new();

        builder.Append($"{"".PadRight(4 * depth)}<Requests>\n");
        builder.Append(string.Join("\n", Templates.Select(v => v.ToString(depth + 1))));
        builder.Append($"\n{"".PadRight(4 * depth)}</Requests>");

        return builder.ToString();
    }
}
