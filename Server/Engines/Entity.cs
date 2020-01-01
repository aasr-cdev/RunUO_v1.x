using System;

namespace Server
{
    public interface IEntity : IPoint3D
    {
        Serial Serial { get; }
        Point3D Location { get; }
        Map Map { get; }
    }

    public class Entity : IEntity
    {
        private Serial m_Serial;
        private Point3D m_Location;
        private Map m_Map;

        public Entity(Serial serial, Point3D loc, Map map)
        {
            m_Serial = serial;
            m_Location = loc;
            m_Map = map;
        }

        public Serial Serial
        {
            get
            {
                return m_Serial;
            }
        }

        public Point3D Location
        {
            get
            {
                return m_Location;
            }
        }

        public int X
        {
            get
            {
                return m_Location.X;
            }
        }

        public int Y
        {
            get
            {
                return m_Location.Y;
            }
        }

        public int Z
        {
            get
            {
                return m_Location.Z;
            }
        }

        public Map Map
        {
            get
            {
                return m_Map;
            }
        }
    }
}