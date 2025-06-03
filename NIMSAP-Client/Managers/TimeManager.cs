using System;
using System.Data;
using Microsoft.Xna.Framework;

namespace Codename_NIMSAP.Managers;

public static class TimeManager
{
    public static float deltaTime = 0f;
    public static int tickRate = 20;

    public static void Update(GameTime gameTime)
    {
        TimeManager.deltaTime = Math.Clamp(TimeManager.deltaTime + (float)gameTime.ElapsedGameTime.TotalSeconds * tickRate, 0f, 1f);
    }
}