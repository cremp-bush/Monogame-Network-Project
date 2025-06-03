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
    // Текущий снимок мира
    public static Map map;
    // Прошлый снимок мира
    public static Map oldMap;
    
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
    
    // Получение старой позиции игрока
    public static Vector2 GetOldPlayerPosition()
    {
        return oldMap.GetEntity(playerId).position;
    }
    
    /* Обновление карты */
    static void LoadMap(byte[] data)
    {
        map = PacketAdapter.Unpack<Map>(data);
        oldMap = new Map(map);
    }

    static void CreateEntity(byte[] data)
    {
        Entity entity = PacketAdapter.Unpack<Entity>(data);
        map.AddEntity(entity);
    }
    
    // static void UpdateEntityPosition(byte[] data)
    // {
    //     Entity entity = PacketAdapter.Unpack<Entity>(data);
    //     oldMap = map;
    //     map.UpdateEntity(entity as Human);
    // }
    
    static void UpdateEntity(byte[] data)
    {
        Entity entity = PacketAdapter.Unpack<Entity>(data);
        // Обновляем старый снимок мира до текущего состояния
        oldMap = new Map(map);
        // Обновляем мир до нового состояния
        map.UpdateEntity(entity as Human);
        // Обновляем deltaTime
        TimeManager.deltaTime = 0f;
    }
    
    static void DeleteEntity(byte[] data)
    {
        int id = BitConverter.ToInt32(data);
        map.DeleteEntity(id);
    }
}