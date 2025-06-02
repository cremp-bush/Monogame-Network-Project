using System;
using Microsoft.Xna.Framework;
using NIMSAP_Lib;
using NIMSAP_Lib.Entity;
using NIMSAP_Lib.Map;

namespace Codename_NIMSAP.Managers;

/* TODO: Мне очень не нравится миллион switch */ 
/* Менеджер игровой карты */
public static class DataManager
{
    // Пока что без надобности
    public static int playerId;
    public static Map map;
    
    // Выбор функции для обработки запроса
    public static void UpdateData(PacketType packetType, byte[] data)
    {
        switch (packetType)
        {
            case PacketType.Map:
            {
                LoadMap(data);
                break;
            }
            case PacketType.CreateEntity:
            {
                CreateEntity(data);
                break;
            }
            case PacketType.UpdateEntity:
            {
                UpdateEntity(data);
                break;
            }
            case PacketType.DeleteEntity:
            {
                DeleteEntity(data);
                break;
            }
        }
    }

    // Получение позиции игрока
    public static Vector2 GetPlayerPosition()
    {
        return map.GetEntity(playerId).position;
    }
    
    /* Обновление карты */
    static void LoadMap(byte[] data)
    {
        map = PacketAdapter.Unpack<Map>(data);
    }

    static void CreateEntity(byte[] data)
    {
        Entity entity = PacketAdapter.Unpack<Entity>(data);
        map.AddEntity(entity);
    }
    
    static void UpdateEntity(byte[] data)
    {
        Entity entity = PacketAdapter.Unpack<Entity>(data);
        map.UpdateEntity(entity as Human);
    }
    
    static void DeleteEntity(byte[] data)
    {
        int id = BitConverter.ToInt32(data);
        map.DeleteEntity(id);
    }
}