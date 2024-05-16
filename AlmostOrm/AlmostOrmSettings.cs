namespace AlmostOrm
{
    public class AlmostOrmSettings
    {
        public string TableTemplate { get; set; }
        public string ProcedureTemplate { get; set; }
        public Dictionary<string, string> TypeMaps { get; set; }
        public string IndexTemplate { get; set; }
        public string IdTemplate { get; set; }
    }
}
