using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NIMSAP_Lib;
using NIMSAP_Lib.Entity;
using NIMSAP_Lib.Map;

namespace Codename_NIMSAP.Managers;

public static class ViewManager
{
    
    public static void Update(SpriteBatch spriteBatch)
    {
        Vector2 playerPosition = DataManager.GetPlayerPosition();

        // Прогрузка тайлов в поле видимости игрока
        for (float y = (float)Math.Floor(playerPosition.Y - spriteBatch.GraphicsDevice.Viewport.Height/64), screenY = 0 - playerPosition.Y % 1; y <= (float)Math.Ceiling(playerPosition.Y + spriteBatch.GraphicsDevice.Viewport.Height/64); y++, screenY++)
        {
            for (float x = (float)Math.Floor(playerPosition.X - spriteBatch.GraphicsDevice.Viewport.Width/64), screenX = 0 - playerPosition.X % 1; x <= (float)Math.Ceiling(playerPosition.X + spriteBatch.GraphicsDevice.Viewport.Width/64); x++, screenX++)
            {
                if (x < 0 || y < 0 || x > DataManager.map.width || y > DataManager.map.height)
                {
                    spriteBatch.Draw(
                        TextureManager.textures["NoneFloor"], 
                        new Vector2(screenX * 32f, screenY * 32f), 
                        null, 
                        Color.White, 
                        0f, 
                        Vector2.One, 
                        1f, 
                        SpriteEffects.None, 
                        0f);
                    
                    continue;
                }
                Tile tile = DataManager.map.tileMap[(int)x, (int)y];
                string floor = ((FloorType)tile.floor).ToString();
                string wall = ((WallType)tile.wall).ToString();
                if (tile.floor >= 0) 
                    spriteBatch.Draw(
                        TextureManager.textures[floor], 
                        new Vector2(screenX * 32f, screenY * 32f), 
                        null, 
                        Color.White, 
                        0f, 
                        Vector2.One, 
                        1f, 
                        SpriteEffects.None, 
                        0f);
                if (tile.wall > 0) 
                    spriteBatch.Draw(
                        TextureManager.textures[wall], 
                        new Vector2((float)screenX, spriteBatch.GraphicsDevice.Viewport.Height-(float)screenY) * 32f, 
                        null, 
                        Color.White, 
                        0f, 
                        Vector2.One, 
                        1f, 
                        SpriteEffects.None, 
                        0f);
            }
        }
        
        // Прогрузка сущностей в полне видимости игрока
        foreach (Entity entity in DataManager.map.GetAllEntities().Values)
        {
            if ((entity.position.X >= playerPosition.X - 30 && entity.position.X <= playerPosition.X + 30) &&
                (entity.position.Y >= playerPosition.Y - 16.875 && entity.position.Y <= playerPosition.Y + 16.875))
            {
                if (entity is Creature creature)
                {
                    spriteBatch.Draw(
                        TextureManager.textures[creature.GetType().Name.ToString() + creature.rotation],
                        new Vector2((entity.position.X - playerPosition.X) * 32 + spriteBatch.GraphicsDevice.Viewport.Width/2,
                            (entity.position.Y - playerPosition.Y) * 32 + spriteBatch.GraphicsDevice.Viewport.Height/2),
                        null,
                        Color.White,
                        0f,
                        Vector2.One,
                        1f,
                        SpriteEffects.None,
                        0f);
                }
            }
        }
    }
}