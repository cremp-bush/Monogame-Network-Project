using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;

namespace NIMSAP_Lib.Entity;

[JsonDerivedType(typeof(Entity))]
public class Human : Creature
{
    Human() {}
    Human(Vector2 position)
    {
        health = 100;
        maxHealth = 100;
        this.position = position;
    }

    public static Human CreateEntity(Vector2 position, Guid guid)
    {
        Human human = new Human();
        
        human.health = 100;
        human.maxHealth = 100;
        human.position = position;
        human.guid = guid;
        
        return human;
    }
}