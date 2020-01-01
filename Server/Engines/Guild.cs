using System;
using System.Collections;

using Server.Items;

namespace Server.Guilds
{
    public enum GuildType
    {
        Regular,
        Chaos,
        Order
    }

    public abstract class BaseGuild
    {
        private int m_Id;

        public BaseGuild(int Id)//serialization ctor
        {
            m_Id = Id;
            m_GuildList.Add(m_Id, this);
            if (m_Id + 1 > m_NextID)
                m_NextID = m_Id + 1;
        }

        public BaseGuild()
        {
            m_Id = m_NextID++;
            m_GuildList.Add(m_Id, this);
        }

        public int Id { get { return m_Id; } }

        public abstract void Deserialize(GenericReader reader);
        public abstract void Serialize(GenericWriter writer);

        public abstract string Abbreviation { get; set; }
        public abstract string Name { get; set; }
        public abstract GuildType Type { get; set; }
        public abstract bool Disbanded { get; }
        public abstract void OnDelete(Mobile mob);

        private static Hashtable m_GuildList = new Hashtable();
        private static int m_NextID = 1;

        public static Hashtable List
        {
            get
            {
                return m_GuildList;
            }
        }

        public static BaseGuild Find(int id)
        {
            return (BaseGuild)m_GuildList[id];
        }

        public static BaseGuild FindByName(string name)
        {
            foreach (BaseGuild g in m_GuildList.Values)
            {
                if (g.Name == name)
                    return g;
            }

            return null;
        }

        public static BaseGuild FindByAbbrev(string abbr)
        {
            foreach (BaseGuild g in m_GuildList.Values)
            {
                if (g.Abbreviation == abbr)
                    return g;
            }

            return null;
        }

        public static BaseGuild[] Search(string find)
        {
            string[] words = find.ToLower().Split(' ');
            ArrayList results = new ArrayList();

            foreach (BaseGuild g in m_GuildList.Values)
            {
                bool match = true;
                string name = g.Name.ToLower();
                for (int i = 0; i < words.Length; i++)
                {
                    if (name.IndexOf(words[i]) == -1)
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    results.Add(g);
            }

            return (BaseGuild[])results.ToArray(typeof(BaseGuild));
        }
    }
}