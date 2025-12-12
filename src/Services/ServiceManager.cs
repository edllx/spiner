using System.Text;

namespace spinner;

public class ServiceManager
{
    public List<ServiceTemplate> Templates { get; set; } = [];

    public void SetTemplates(List<ServiceTemplate>? templates)
    {
        if (templates is null)
        {
            return;
        }
        Templates = templates;
    }

    public ServiceTemplate? GetTemplate(string name)
    {
        return Templates.Find(v => v.Name == name);
    }

    public string ToString(int depth = 0)
    {
        StringBuilder builder = new();

        builder.Append($"{"".PadRight(4 * depth)}<Services>\n");
        builder.Append(string.Join("\n", Templates.Select(v => v.ToString(depth + 1))));
        builder.Append($"\n{"".PadRight(4 * depth)}</Services>");

        return builder.ToString();
    }
}
