﻿using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using NIMSAP_Lib.Entity;

namespace NIMSAP_Lib.Map;

[Serializable]
public partial class Map
{
    [JsonProperty]
    static int entityId;
    
    public short width;
    public short height;
    public Tile[,] tileMap;
    
    [JsonProperty]
    private Dictionary<int, Entity.Entity> entityList;

    // Создание пустой карты
    [JsonConstructor]
    public Map(short width, short height)
    {
        this.width = width;
        this.height = height;
        tileMap = new Tile[width, height];
        entityList = new Dictionary<int, Entity.Entity>();
        entityId = 0;
    }
    // Конструктор полной карты
    public Map(short width, short height, Tile[,] tileMap, Dictionary<int, Entity.Entity> entityList)
    {
        this.width = width;
        this.height = height;
        this.tileMap = tileMap;
        this.entityList = entityList;
    }

    // Копирование карты
    public Map(Map map)
    {
        width = map.width;
        height = map.height;
        tileMap = map.GetTiles();
        entityList = map.GetAllEntities();
    }
    
    // Добавление сущности
    public int AddEntity(Entity.Entity entity)
    {
        entity.id = entityId;
        entityList.Add(entityId, entity);
        return entityId++;
    }
    
    // Добавление сущностей
    public int AddEntities(Entity.Entity[] entities)
    {
        foreach (Entity.Entity entity in entities)
        {
            entity.id = entityId;
            entityList.Add(entityId, entity);
        }
        return entityId++;
    }
    
    // Поиск сущности по ID
    public Entity.Entity GetEntity(int entityId)
    {
        entityList.TryGetValue(entityId, out Entity.Entity entity);
        return entity;
    }    
    
    // Поиск сущности по Guid
    public Creature? GetEntity(Guid guid)
    {
        Creature player = null;
        foreach (Entity.Entity entity in entityList.Values)
        {
            if (entity is Creature creature)
            {
                if (creature.guid == guid)
                {
                    player = creature;
                    break;
                }
            }
        }
        return player;
    }
    
    // Получить все сущности
    public Dictionary<int, Entity.Entity> GetAllEntities()
    {
        return new Dictionary<int, Entity.Entity>(entityList);
    }

    // Модификация сущностей по ID
    public void UpdateEntity(Entity.Entity entity)
    {
        entityList[entity.id] = entity;
    }

    // Удаление сущности
    public void DeleteEntity(int entityId)
    {
        entityList.Remove(entityId);
    }
    
    // Удаление всех сущностей
    public void FlushEntities()
    {
        entityList.Clear();
    }
    
    // Получить все клетки
    public Tile[,] GetTiles()
    {
        return tileMap.Clone() as Tile[,];
    }
}