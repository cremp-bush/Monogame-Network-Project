using Microsoft.Xna.Framework;

namespace NIMSAP_Lib.Entity;

public abstract class Entity
{
    public int id;
    public int health;
    public int maxHealth;
    public Rectangle collider;
    public Dictionary<string, Rectangle> hitbox;
    public Vector2 position;
}