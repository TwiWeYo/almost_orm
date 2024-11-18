namespace ORMini.Savers;
public class MapSaver : IMapSaver
{
    public bool CheckIfExists(string path)
    {
        return File.Exists(Path.Combine(path));
    }

    public void Save(string path, string contents)
    {
        var directory = Path.GetDirectoryName(path);

        if (CheckIfExists(path))
        {
            return;
        }

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }

        using (var fs = new FileStream(path, FileMode.Create))
        using (var sw = new StreamWriter(fs))
        {
            sw.Write(contents);
        }
    }
}
