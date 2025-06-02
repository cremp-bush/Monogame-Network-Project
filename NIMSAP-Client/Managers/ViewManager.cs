using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NIMSAP_Lib;
using NIMSAP_Lib.Entity;
using NIMSAP_Lib.Map;
using NIMSAP_Server;

namespace Codename_NIMSAP.Managers;

public static class ViewManager
{
    private static float tileScale;
    private static float viewWidth = 15f;
    private static float viewHeight = 8.4375f;

    // Определение размера тайла
    public static void SetResolution(int width, int height)
    {
        int GetAspect(int a, int b) => b == 0 ? a : GetAspect(b, a % b);
        
        int gcd = GetAspect(width, height);
        if (width / gcd == 16 && height / gcd == 9)
        {
            tileScale = width / viewWidth;
            // tileScale /= 1.5f;
        }
        else
        {
            tileScale = width / viewWidth;

            Logger.Log("Обнаружено неподдерживаемое разрешение!");
        }
    }
    
    // Отрисовка мира
    public static void Update(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
    {
        Vector2 playerPosition = DataManager.GetPlayerPosition();
        
        // Прогрузка тайлов в поле видимости игрока
        for (int y = 0; y <= Math.Ceiling(viewHeight); y++)
        {
            for (int x = 0; x <= Math.Floor(viewWidth); x++)
            {
                Texture2D texture;
                float borderX = 0.5f - Math.Abs(((1 + (playerPosition.X - viewWidth % 1) % 1) % 1) % 1);
                float borderY = 0.5f - Math.Abs(((1 + (playerPosition.Y - viewHeight % 1) % 1) % 1) % 1);
                float _x = borderX + x;
                float _y = borderY + y;
                int mapX = (int)Math.Floor(playerPosition.X - viewWidth / 2 + borderX) + x;
                int mapY = (int)Math.Floor(playerPosition.Y - viewHeight / 2 + borderY) + y;

                // Отрисовка за пределами карты
                if (mapX < 0 || mapY < 0 || mapX > DataManager.map.width || mapY > DataManager.map.height)
                {
                    texture = TextureManager.textures["NoneFloor"];
                    spriteBatch.Draw(
                        texture, 
                        new Vector2(_x, _y) * tileScale, 
                        null, 
                        Color.White, 
                        0f, 
                        new Vector2(texture.Width, texture.Height)/2,
                        tileScale/32, 
                        SpriteEffects.None, 
                        0f);
                }
                else
                {
                    Tile tile = DataManager.map.tileMap[mapX, mapY];
                    string floor = ((FloorType)tile.floor).ToString();
                    string wall = ((WallType)tile.wall).ToString();
                    // Отрисовка пола карты
                    if (tile.floor >= 0)
                    {
                        texture = TextureManager.textures[floor];
                        spriteBatch.Draw(
                            texture,
                            new Vector2(_x, _y) * tileScale,
                            null,
                            Color.White,
                            0f,
                            new Vector2(texture.Width, texture.Height) / 2,
                            tileScale / 32,
                            SpriteEffects.None,
                            0f);
                    }

                    // Отрисовка стен карты
                    if (tile.wall > 0)
                    {
                        texture = TextureManager.textures[wall];
                        spriteBatch.Draw(
                            texture,
                            new Vector2(_x, _y) * tileScale,
                            null,
                            Color.White,
                            0f,
                            new Vector2(texture.Width, texture.Height) / 2,
                            tileScale / 32,
                            SpriteEffects.None,
                            0f);
                    }
                }
                /*var pixel = new Texture2D(graphics.GraphicsDevice, 1, 1);
                pixel.SetData(new[] { Color.White });
                
                spriteBatch.Draw(
                    pixel,
                    new Vector2(_x, _y) * tileScale,
                    null,
                    Color.White,
                    0f,
                    new Vector2(pixel.Width, pixel.Height) / 2,
                    tileScale/32,
                    SpriteEffects.None,
                    0f); */
            }
        }
        
        // Прогрузка сущностей в полне видимости игрока
        List<Entity> entities = new List<Entity>();
        
        // Выборка сущностей с экрана
        foreach (Entity entity in DataManager.map.GetAllEntities().Values)
        {
            if ((entity.position.X >= playerPosition.X - viewWidth/2 && entity.position.X <= playerPosition.X + viewWidth/2) &&
                (entity.position.Y >= playerPosition.Y - viewHeight/2 && entity.position.Y <= playerPosition.Y + viewHeight/2))
            {
                entities.Add(entity);
            }
        }
        // Сортировка сущностей по высоте
        entities.Sort(new Comparison<Entity>((entity1, entity2) => entity1.position.Y.CompareTo(entity2.position.Y)));
        // Отрисовка сущностей
        foreach (Entity entity in entities)
        {
            if (entity is Creature creature)
            {
                Texture2D texture = TextureManager.textures[creature.GetType().Name.ToString() + creature.rotation];
                spriteBatch.Draw(
                    texture,
                    new Vector2((entity.position.X - playerPosition.X) * tileScale + spriteBatch.GraphicsDevice.Viewport.Width/2,
                        (entity.position.Y - playerPosition.Y) * tileScale + spriteBatch.GraphicsDevice.Viewport.Height/2),
                    null,
                    Color.White,
                    0f,
                    new Vector2(texture.Width, texture.Height)/2,
                    tileScale/32,
                    SpriteEffects.None,
                    0f);
            }
        }
    }
}