using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NIMSAP_Server;

namespace Codename_NIMSAP.Managers;

public static class TextureManager
{
    public static Dictionary<string, Texture> textures;
    
    public static void Load(ContentManager content)
    {
        textures = new Dictionary<string, Texture>();

        try
        {
            foreach (string file in Directory.EnumerateFiles(content.RootDirectory + @"\Textures", "*.*", SearchOption.AllDirectories))
            {
                FileInfo fileInfo = new FileInfo(file);
                string fileName = Path.GetFileNameWithoutExtension(new FileInfo(file).Name);
                
                textures.Add(fileName, content.Load<Texture2D>(file.Replace(fileInfo.Extension, string.Empty).Replace(content.RootDirectory + @"\", string.Empty)));
            }
            foreach (string file in Directory.EnumerateFiles(content.RootDirectory + @"\AnimatedTextures", "*.*", SearchOption.AllDirectories))
            {
                FileInfo fileInfo = new FileInfo(file);
                string[] fileName = Regex.Split(Path.GetFileNameWithoutExtension(fileInfo.Name), "_");
                int frames = Convert.ToInt32(fileName.First());
                
                textures.Add(fileName.Last(), new AnimatedTexture().Load(content, file.Replace(fileInfo.Extension, string.Empty).Replace(content.RootDirectory + @"\", string.Empty), frames, 2));
            }
        }
        catch (Exception e)
        {
            Logger.Log("Ошибка загрузки текстур", e);
        }
    }

    public static void Update(GameTime gameTime)
    {
        foreach (Texture texture in textures.Values)
        {
            if (texture is AnimatedTexture animatedTexture) animatedTexture.UpdateFrame((float)gameTime.ElapsedGameTime.TotalSeconds);
        }
    }

    public static Texture2D GetTexture(string name, out Rectangle rectangle, out Vector2 origin)
    {
        Texture2D texture2D = null;
        rectangle = Rectangle.Empty;
        origin = Vector2.Zero;
        Texture texture = textures[name];
        
        if (texture is Texture2D common)
        {
            texture2D = common;
            rectangle = common.Bounds;
            origin = new Vector2(rectangle.Width, rectangle.Height) / 2;
        }
        else if (texture is AnimatedTexture animated)
        {
            texture2D = animated.GetTexture();
            rectangle = animated.GetRectangle();
            origin = new Vector2(rectangle.Right - rectangle.Left, rectangle.Bottom-rectangle.Top) / 2;
        }
        
        return texture2D;
    }
}