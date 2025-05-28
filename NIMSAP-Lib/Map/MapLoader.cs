using System.Security.Cryptography;
using System.Text.Json;
using MessagePack;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace NIMSAP_Lib.Map;

public static class MapLoader
{
    // Загрузка карты по пути
    public static Map Load(string path)
    {
        string json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<Map>(json)!;
    }

    // Сохранение карты в путь
    public static void Save(string path, Map map)
    {
        string json = JsonConvert.SerializeObject(map);
        File.WriteAllText(path, json);
    }
}