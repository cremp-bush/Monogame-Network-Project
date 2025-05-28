using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NIMSAP_Server;

namespace Codename_NIMSAP.Managers;

public static class TextureManager
{
    public static Dictionary<string, Texture2D> textures;
    
    public static void Load(ContentManager content)
    {
        textures = new Dictionary<string, Texture2D>();

        try
        {
            foreach (string file in Directory.EnumerateFiles(content.RootDirectory, "*.*", SearchOption.AllDirectories))
            {
                FileInfo fileInfo = new FileInfo(file);
                textures.Add(fileInfo.Name.Replace(fileInfo.Extension, string.Empty), content.Load<Texture2D>(file.Replace(fileInfo.Extension, string.Empty).Replace(content.RootDirectory + @"\", string.Empty)));
            }
        }
        catch (Exception e)
        {
            Logger.Log("Ошибка загрузки текстур", e);
        }
    }
}