using System.Net.NetworkInformation;
using System.Text;
using Newtonsoft.Json;
using NIMSAP_Lib.Entity;

namespace NIMSAP_Lib;

/* TODO: Создать свою сериализацию */
/* MessagePack говно, ProtoBuf тоже, пока что Newtonsoft.Json - потом своя сериализация! */

public static class PacketAdapter
{
    // Упаковка объекта в пакет
    public static byte[] Pack<T>(T obj)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
        
        string json = JsonConvert.SerializeObject(obj, settings);
        return Encoding.UTF8.GetBytes(json);
    }

    // Распаковка пакета в объект
    public static T Unpack<T>(byte[] data)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };

        string json = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<T>(json, settings)!;
    }
}