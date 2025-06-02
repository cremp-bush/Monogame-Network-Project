using System.Net;
using Microsoft.Xna.Framework;

namespace NIMSAP_Lib;

/* УСТАРЕЛО - УДАЛИТЬ ПОЗЖЕ */

public class Player
{
    public bool connected = false;
    public bool loaded = false;
    public int entityId;
    public Vector2 motion = Vector2.Zero;
    public Vector2 lastMotion = Vector2.Zero;
    public Guid guid = Guid.Empty;
    public DateTime connectTime = DateTime.Now;
    public DateTime lastPing = DateTime.Now;
    // Проверка пакетов
    public EventWaitHandle check = new EventWaitHandle(false, EventResetMode.AutoReset);
    public int checkKey;
    
    public IPEndPoint endPoint;
}