using System.Text.Json.Serialization;

namespace AlmostOrm
{
    public class Settings
    {
        private string? tableTemplate;
        private string? procedureTemplate;
        [JsonIgnore] public string TableTemplate => tableTemplate ??= GetTemplate(TableTemplatePath!);
        public string? TableTemplatePath { get; set; }
        [JsonIgnore] public string ProcedureTemplate => procedureTemplate ??= GetTemplate(ProcedureTemplatePath!);
        public string? ProcedureTemplatePath { get; set; }
        public Dictionary<string, string>? TypeMaps { get; set; }
        public string? IndexTemplate { get; set; }
        public string? IdTemplate { get; set; }

        private string GetTemplate(string path)
        {
            using (var reader = new StreamReader(path))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
