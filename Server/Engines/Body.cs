﻿using System;
using System.IO;

namespace Server
{
    public enum BodyType : byte
    {
        Empty,
        Monster,
        Sea,
        Animal,
        Human,
        Equipment
    }

    public struct Body
    {
        private int m_BodyID;

        private static BodyType[] m_Types;

        static Body()
        {
            if (File.Exists("Data/Binary/BodyTypes.bin"))
            {
                using (BinaryReader bin = new BinaryReader(new FileStream("Data/Binary/BodyTypes.bin", FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    m_Types = new BodyType[(int)bin.BaseStream.Length];

                    for (int i = 0; i < m_Types.Length; ++i)
                        m_Types[i] = (BodyType)bin.ReadByte();
                }
            }
            else
            {
                Console.WriteLine("Warning: Data/Binary/BodyTypes.bin does not exist");

                m_Types = new BodyType[0];
            }
        }

        public Body(int bodyID)
        {
            m_BodyID = bodyID;
        }

        public BodyType Type
        {
            get
            {
                if (m_BodyID >= 0 && m_BodyID < m_Types.Length)
                    return m_Types[m_BodyID];
                else
                    return BodyType.Empty;
            }
        }

        public bool IsHuman
        {
            get
            {
                return m_BodyID >= 0
                    && m_BodyID < m_Types.Length
                    && m_Types[m_BodyID] == BodyType.Human
                    && m_BodyID != 402
                    && m_BodyID != 404
                    && m_BodyID != 970;
            }
        }

        public bool IsMale
        {
            get
            {
                return m_BodyID == 183
                    || m_BodyID == 185
                    || m_BodyID == 400
                    || m_BodyID == 402
                    || m_BodyID == 750;
            }
        }

        public bool IsFemale
        {
            get
            {
                return m_BodyID == 184
                    || m_BodyID == 186
                    || m_BodyID == 401
                    || m_BodyID == 403
                    || m_BodyID == 751;
            }
        }

        public bool IsGhost
        {
            get
            {
                return m_BodyID == 402
                    || m_BodyID == 403
                    || m_BodyID == 970;
            }
        }

        public bool IsMonster
        {
            get
            {
                return m_BodyID >= 0
                    && m_BodyID < m_Types.Length
                    && m_Types[m_BodyID] == BodyType.Monster;
            }
        }

        public bool IsAnimal
        {
            get
            {
                return m_BodyID >= 0
                    && m_BodyID < m_Types.Length
                    && m_Types[m_BodyID] == BodyType.Animal;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return m_BodyID >= 0
                    && m_BodyID < m_Types.Length
                    && m_Types[m_BodyID] == BodyType.Empty;
            }
        }

        public bool IsSea
        {
            get
            {
                return m_BodyID >= 0
                    && m_BodyID < m_Types.Length
                    && m_Types[m_BodyID] == BodyType.Sea;
            }
        }

        public bool IsEquipment
        {
            get
            {
                return m_BodyID >= 0
                    && m_BodyID < m_Types.Length
                    && m_Types[m_BodyID] == BodyType.Equipment;
            }
        }

        public int BodyID
        {
            get
            {
                return m_BodyID;
            }
        }

        public static implicit operator int(Body a)
        {
            return a.m_BodyID;
        }

        public static implicit operator Body(int a)
        {
            return new Body(a);
        }

        public override string ToString()
        {
            return string.Format("0x{0:X}", m_BodyID);
        }

        public override int GetHashCode()
        {
            return m_BodyID;
        }

        public override bool Equals(object o)
        {
            if (o == null || !(o is Body)) return false;

            return ((Body)o).m_BodyID == m_BodyID;
        }

        public static bool operator ==(Body l, Body r)
        {
            return l.m_BodyID == r.m_BodyID;
        }

        public static bool operator !=(Body l, Body r)
        {
            return l.m_BodyID != r.m_BodyID;
        }

        public static bool operator >(Body l, Body r)
        {
            return l.m_BodyID > r.m_BodyID;
        }

        public static bool operator >=(Body l, Body r)
        {
            return l.m_BodyID >= r.m_BodyID;
        }

        public static bool operator <(Body l, Body r)
        {
            return l.m_BodyID < r.m_BodyID;
        }

        public static bool operator <=(Body l, Body r)
        {
            return l.m_BodyID <= r.m_BodyID;
        }
    }
}