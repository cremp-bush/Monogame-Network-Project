using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Codename_NIMSAP.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NIMSAP_Lib;
using NIMSAP_Lib.Map;
using NIMSAP_Server;

namespace Codename_NIMSAP;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    Client network = new Client();

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        // Content.RootDirectory = "Resources/Textures";
        Content.RootDirectory = @"Content\Textures";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        Logger.Initialise("logs");
        
        network.Start(1488);
        network.DataReceived += DataManager.UpdateData;
            
        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;
        _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();

        // ViewManager.SetResolution(_graphics.GraphicsDevice.Viewport.Width, _graphics.GraphicsDevice.Viewport.Height);
        ViewManager.SetResolution(1920, 1080);
        
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        TextureManager.Load(Content);

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        // Управление игрока
        // Пока что управление включается после загрузки карты
        if (DataManager.map != null)
        {
            byte[]? data = InputManager.Update();
            // TODO: Это явно не самый лучший способ передавать действия клиента
            if (data is byte[] bytes)
            {
                if (bytes.Length == 1 && bytes[0] == 0) Exit();
                network.input = bytes;
            }
        }
        
        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
        
        // Пока что текстуры прогружаются после загрузки карты
        if (DataManager.map != null) ViewManager.Update(_spriteBatch, _graphics);
        
        _spriteBatch.End();

        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }
    
    protected override void OnExiting(object sender, ExitingEventArgs args)
    {
        network.Disconnect();

        base.OnExiting(sender, args);
    }
}