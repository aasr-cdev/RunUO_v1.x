using System;
using System.Collections;

namespace Server
{
    public interface IVendor
    {
        bool OnBuyItems(Mobile from, ArrayList list);
        bool OnSellItems(Mobile from, ArrayList list);

        DateTime LastRestock { get; set; }
        TimeSpan RestockDelay { get; }
        void Restock();
    }

    public interface IPoint2D
    {
        int X { get; }
        int Y { get; }
    }

    public interface IPoint3D : IPoint2D
    {
        int Z { get; }
    }

    public interface ICarvable
    {
        void Carve(Mobile from, Item item);
    }

    public interface IWeapon
    {
        int MaxRange { get; }
        TimeSpan OnSwing(Mobile attacker, Mobile defender);
        void GetStatusDamage(Mobile from, out int min, out int max);
    }

    public interface IHued
    {
        int HuedItemID { get; }
    }

    public interface ISpell
    {
        bool IsCasting { get; }
        void OnCasterHurt();
        void OnCasterKilled();
        void OnConnectionChanged();
        bool OnCasterMoving(Direction d);
        bool OnCasterEquiping(Item item);
        bool OnCasterUsingObject(object o);
        bool OnCastInTown(Region r);
    }

    public interface IParty
    {
        void OnStamChanged(Mobile m);
        void OnManaChanged(Mobile m);
        void OnStatsQuery(Mobile beholder, Mobile beheld);
    }
}

namespace Server.Mobiles
{
    public interface IMount
    {
        Mobile Rider { get; set; }
    }

    public interface IMountItem
    {
        IMount Mount { get; }
    }
}