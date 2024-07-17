namespace AlmostOrm.Savers;
public interface IMapSaver
{
    bool CheckIfExists(string path);
    void Save(string path, string contents);
}
