using System.Timers;

namespace NIMSAP_Lib.Entity.Items.Weapons.Grenades;

public abstract class Grenade : Weapon
{
    public bool activated = false;
    public double timer = 5;

    public void Activate()
    {
        System.Timers.Timer t = new System.Timers.Timer(timer);
        t.Elapsed += Explode;
    }
    public abstract void Explode(Object source, ElapsedEventArgs e);
}