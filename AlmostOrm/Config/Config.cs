using System.Text;
using System.Text.Json.Serialization;

namespace AlmostOrm;

public class Config
{
    private Dictionary<string, string>? _templates;
    public string? TemplatesPath { get; set; }
    public Dictionary<string, string>? TypeMaps { get; set; }
    public Dictionary<string, string>? TemplateTags { get; set; }
    [JsonIgnore]
    public Dictionary<string, string> Templates => _templates ??= CreateTemplates(TemplatesPath!, TemplateTags!);

    private Dictionary<string, string> CreateTemplates(string templatesPath, Dictionary<string, string> templateTags)
    {
        if (string.IsNullOrEmpty(templatesPath)) throw new ArgumentNullException(nameof(templatesPath));
        if (templateTags?.Any() != true) throw new ArgumentNullException(nameof(templateTags));

        var res = new Dictionary<string, string>();

        var line = string.Empty;
        string? currentTag = null;
        int? initialContentIndent = null;

        var tagContents = new StringBuilder();

        using var reader = new StreamReader(templatesPath);
        while (true)
        {
            line = reader.ReadLine();
            if (line == null)
            {
                if (currentTag != null)
                {
                    throw new FormatException($"Haven't found the closing tag for {currentTag}");
                }
                break;
            }

            if (currentTag == null)
            {
                currentTag = templateTags.FirstOrDefault(q => line.Contains($"<{q.Value}>")).Key;
                continue;
            }

            if (line.Contains($"</{templateTags[currentTag]}>"))
            {
                res[currentTag] = tagContents.ToString();

                tagContents.Clear();
                currentTag = null;
                continue;
            }

            // Janky way to remove tabs at the start
            if (initialContentIndent == null)
            {
                initialContentIndent = line
                    .TakeWhile(q => q == '\t')
                    .Count();
            }
            tagContents.AppendLine(string.IsNullOrWhiteSpace(line) ? line : line[initialContentIndent!.Value..]);
        }

        var missingTags = templateTags.Keys.Except(res.Keys);
        if (missingTags.Any())
            throw new FormatException($"Not all of the tags are present in the template. Missing:\n{string.Join('\n', missingTags)}");

        return res;
    }
}
