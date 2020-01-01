﻿using System;
using System.Collections;
using System.Text;

namespace Server
{
    public enum ClientType
    {
        Regular,
        UOTD,
        God
    }

    public class ClientVersion : IComparable, IComparer
    {
        private static ClientVersion m_Required;
        private static bool m_AllowRegular = true, m_AllowUOTD = true, m_AllowGod = true;
        private static TimeSpan m_KickDelay = TimeSpan.FromSeconds(10.0);

        public static ClientVersion Required
        {
            get
            {
                return m_Required;
            }
            set
            {
                m_Required = value;
            }
        }

        public static bool AllowRegular
        {
            get
            {
                return m_AllowRegular;
            }
            set
            {
                m_AllowRegular = value;
            }
        }

        public static bool AllowUOTD
        {
            get
            {
                return m_AllowUOTD;
            }
            set
            {
                m_AllowUOTD = value;
            }
        }

        public static bool AllowGod
        {
            get
            {
                return m_AllowGod;
            }
            set
            {
                m_AllowGod = value;
            }
        }

        public static TimeSpan KickDelay
        {
            get
            {
                return m_KickDelay;
            }
            set
            {
                m_KickDelay = value;
            }
        }

        private int m_Major, m_Minor, m_Revision, m_Patch;
        private ClientType m_Type;
        private string m_SourceString;

        public int Major
        {
            get
            {
                return m_Major;
            }
        }

        public int Minor
        {
            get
            {
                return m_Minor;
            }
        }

        public int Revision
        {
            get
            {
                return m_Revision;
            }
        }

        public int Patch
        {
            get
            {
                return m_Patch;
            }
        }

        public ClientType Type
        {
            get
            {
                return m_Type;
            }
        }

        public string SourceString
        {
            get
            {
                return m_SourceString;
            }
        }

        public ClientVersion(int maj, int min, int rev, int pat, ClientType type)
        {
            m_Major = maj;
            m_Minor = min;
            m_Revision = rev;
            m_Patch = pat;
            m_Type = type;

            m_SourceString = ToString();
        }

        public static bool operator ==(ClientVersion l, ClientVersion r)
        {
            return (Compare(l, r) == 0);
        }

        public static bool operator !=(ClientVersion l, ClientVersion r)
        {
            return (Compare(l, r) != 0);
        }

        public static bool operator >=(ClientVersion l, ClientVersion r)
        {
            return (Compare(l, r) >= 0);
        }

        public static bool operator >(ClientVersion l, ClientVersion r)
        {
            return (Compare(l, r) > 0);
        }

        public static bool operator <=(ClientVersion l, ClientVersion r)
        {
            return (Compare(l, r) <= 0);
        }

        public static bool operator <(ClientVersion l, ClientVersion r)
        {
            return (Compare(l, r) < 0);
        }

        public override int GetHashCode()
        {
            return m_Major ^ m_Minor ^ m_Revision ^ m_Patch ^ (int)m_Type;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            ClientVersion v = obj as ClientVersion;

            if (v == null)
                return false;

            return m_Major == v.m_Major
                && m_Minor == v.m_Minor
                && m_Revision == v.m_Revision
                && m_Patch == v.m_Patch
                && m_Type == v.m_Type;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(16);

            builder.Append(m_Major);
            builder.Append('.');
            builder.Append(m_Minor);
            builder.Append('.');
            builder.Append(m_Revision);

            if (m_Patch > 0)
                builder.Append((char)('a' + (m_Patch - 1)));

            if (m_Type != ClientType.Regular)
            {
                builder.Append(' ');
                builder.Append(m_Type.ToString());
            }

            return builder.ToString();
        }

        public ClientVersion(string fmt)
        {
            m_SourceString = fmt;

            try
            {
                fmt = fmt.ToLower();

                m_Major = int.Parse(fmt[0].ToString());
                m_Minor = int.Parse(fmt[2].ToString());
                m_Revision = int.Parse(fmt[4].ToString());

                if (fmt.Length >= 6 && Char.IsWhiteSpace(fmt[5]))
                    m_Patch = 0;
                else
                    m_Patch = (fmt[5] - 'a') + 1;

                if (fmt.IndexOf("god") >= 0 || fmt.IndexOf("gq") >= 0)
                    m_Type = ClientType.God;
                else if (fmt.IndexOf("third dawn") >= 0 || fmt.IndexOf("uo:td") >= 0 || fmt.IndexOf("uotd") >= 0 || fmt.IndexOf("uo3d") >= 0 || fmt.IndexOf("uo:3d") >= 0)
                    m_Type = ClientType.UOTD;
                else
                    m_Type = ClientType.Regular;
            }
            catch
            {
                m_Major = 0;
                m_Minor = 0;
                m_Revision = 0;
                m_Patch = 0;
                m_Type = ClientType.Regular;
            }
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            ClientVersion o = obj as ClientVersion;

            if (o == null)
                throw new ArgumentException();

            if (m_Major > o.m_Major)
                return 1;
            else if (m_Major < o.m_Major)
                return -1;
            else if (m_Minor > o.m_Minor)
                return 1;
            else if (m_Minor < o.m_Minor)
                return -1;
            else if (m_Revision > o.m_Revision)
                return 1;
            else if (m_Revision < o.m_Revision)
                return -1;
            else if (m_Patch > o.m_Patch)
                return 1;
            else if (m_Patch < o.m_Patch)
                return -1;
            else
                return 0;
        }

        public static bool IsNull(object x)
        {
            return Object.ReferenceEquals(x, null);
        }

        public int Compare(object x, object y)
        {
            if (IsNull(x) && IsNull(y))
                return 0;
            else if (IsNull(x))
                return -1;
            else if (IsNull(y))
                return 1;

            ClientVersion a = x as ClientVersion;
            ClientVersion b = y as ClientVersion;

            if (IsNull(a) || IsNull(b))
                throw new ArgumentException();

            return a.CompareTo(b);
        }

        public static int Compare(ClientVersion a, ClientVersion b)
        {
            if (IsNull(a) && IsNull(b))
                return 0;
            else if (IsNull(a))
                return -1;
            else if (IsNull(b))
                return 1;

            return a.CompareTo(b);
        }
    }
}