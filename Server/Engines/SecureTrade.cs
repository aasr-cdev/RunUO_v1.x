using System;
using System.Collections;

using Server.Items;
using Server.Network;

namespace Server
{
    public class SecureTrade
    {
        private SecureTradeInfo m_From, m_To;
        private bool m_Valid;

        public SecureTradeInfo From
        {
            get
            {
                return m_From;
            }
        }

        public SecureTradeInfo To
        {
            get
            {
                return m_To;
            }
        }

        public bool Valid
        {
            get
            {
                return m_Valid;
            }
        }

        public void Cancel()
        {
            if (!m_Valid)
                return;

            ArrayList list = m_From.Container.Items;

            for (int i = list.Count - 1; i >= 0; --i)
            {
                if (i < list.Count)
                {
                    Item item = (Item)list[i];

                    item.OnSecureTrade(m_From.Mobile, m_To.Mobile, m_From.Mobile, false);

                    if (!item.Deleted)
                        m_From.Mobile.AddToBackpack(item);
                }
            }

            list = m_To.Container.Items;

            for (int i = list.Count - 1; i >= 0; --i)
            {
                if (i < list.Count)
                {
                    Item item = (Item)list[i];

                    item.OnSecureTrade(m_To.Mobile, m_From.Mobile, m_To.Mobile, false);

                    if (!item.Deleted)
                        m_To.Mobile.AddToBackpack(item);
                }
            }

            Close();
        }

        public void Close()
        {
            if (!m_Valid)
                return;

            m_From.Mobile.Send(new CloseSecureTrade(m_From.Container));
            m_To.Mobile.Send(new CloseSecureTrade(m_To.Container));

            m_Valid = false;

            NetState ns = m_From.Mobile.NetState;

            if (ns != null)
                ns.RemoveTrade(this);

            ns = m_To.Mobile.NetState;

            if (ns != null)
                ns.RemoveTrade(this);

            m_From.Container.Delete();
            m_To.Container.Delete();
        }

        public void Update()
        {
            if (!m_Valid)
                return;

            if (m_From.Accepted && m_To.Accepted)
            {
                ArrayList list = m_From.Container.Items;

                bool allowed = true;

                for (int i = list.Count - 1; allowed && i >= 0; --i)
                {
                    if (i < list.Count)
                    {
                        Item item = (Item)list[i];

                        if (!item.AllowSecureTrade(m_From.Mobile, m_To.Mobile, m_To.Mobile, true))
                            allowed = false;
                    }
                }

                list = m_To.Container.Items;

                for (int i = list.Count - 1; allowed && i >= 0; --i)
                {
                    if (i < list.Count)
                    {
                        Item item = (Item)list[i];

                        if (!item.AllowSecureTrade(m_To.Mobile, m_From.Mobile, m_From.Mobile, true))
                            allowed = false;
                    }
                }

                if (!allowed)
                {
                    m_From.Accepted = false;
                    m_To.Accepted = false;

                    m_From.Mobile.Send(new UpdateSecureTrade(m_From.Container, m_From.Accepted, m_To.Accepted));
                    m_To.Mobile.Send(new UpdateSecureTrade(m_To.Container, m_To.Accepted, m_From.Accepted));

                    return;
                }

                list = m_From.Container.Items;

                for (int i = list.Count - 1; i >= 0; --i)
                {
                    if (i < list.Count)
                    {
                        Item item = (Item)list[i];

                        item.OnSecureTrade(m_From.Mobile, m_To.Mobile, m_To.Mobile, true);

                        if (!item.Deleted)
                            m_To.Mobile.AddToBackpack(item);
                    }
                }

                list = m_To.Container.Items;

                for (int i = list.Count - 1; i >= 0; --i)
                {
                    if (i < list.Count)
                    {
                        Item item = (Item)list[i];

                        item.OnSecureTrade(m_To.Mobile, m_From.Mobile, m_From.Mobile, true);

                        if (!item.Deleted)
                            m_From.Mobile.AddToBackpack(item);
                    }
                }

                Close();
            }
            else
            {
                m_From.Mobile.Send(new UpdateSecureTrade(m_From.Container, m_From.Accepted, m_To.Accepted));
                m_To.Mobile.Send(new UpdateSecureTrade(m_To.Container, m_To.Accepted, m_From.Accepted));
            }
        }

        public SecureTrade(Mobile from, Mobile to)
        {
            m_Valid = true;

            m_From = new SecureTradeInfo(this, from, new SecureTradeContainer(this));
            m_To = new SecureTradeInfo(this, to, new SecureTradeContainer(this));

            from.Send(new MobileStatus(from, to));
            from.Send(new UpdateSecureTrade(m_From.Container, false, false));
            from.Send(new SecureTradeEquip(m_To.Container, to));
            from.Send(new UpdateSecureTrade(m_From.Container, false, false));
            from.Send(new SecureTradeEquip(m_From.Container, from));
            from.Send(new DisplaySecureTrade(to, m_From.Container, m_To.Container, to.Name));
            from.Send(new UpdateSecureTrade(m_From.Container, false, false));

            to.Send(new MobileStatus(to, from));
            to.Send(new UpdateSecureTrade(m_To.Container, false, false));
            to.Send(new SecureTradeEquip(m_From.Container, from));
            to.Send(new UpdateSecureTrade(m_To.Container, false, false));
            to.Send(new SecureTradeEquip(m_To.Container, to));
            to.Send(new DisplaySecureTrade(from, m_To.Container, m_From.Container, from.Name));
            to.Send(new UpdateSecureTrade(m_To.Container, false, false));
        }
    }

    public class SecureTradeInfo
    {
        private SecureTrade m_Owner;
        private Mobile m_Mobile;
        private SecureTradeContainer m_Container;
        private bool m_Accepted;

        public SecureTradeInfo(SecureTrade owner, Mobile m, SecureTradeContainer c)
        {
            m_Owner = owner;
            m_Mobile = m;
            m_Container = c;

            m_Mobile.AddItem(m_Container);
        }

        public SecureTrade Owner
        {
            get
            {
                return m_Owner;
            }
        }

        public Mobile Mobile
        {
            get
            {
                return m_Mobile;
            }
        }

        public SecureTradeContainer Container
        {
            get
            {
                return m_Container;
            }
        }

        public bool Accepted
        {
            get
            {
                return m_Accepted;
            }
            set
            {
                m_Accepted = value;
            }
        }
    }
}

namespace Server.Items
{
    public class SecureTradeContainer : Container
    {
        private SecureTrade m_Trade;

        public SecureTrade Trade
        {
            get
            {
                return m_Trade;
            }
        }

        public override int DefaultGumpID { get { return 0x52; } }
        public override int DefaultDropSound { get { return 0x42; } }

        public override Rectangle2D Bounds
        {
            get { return new Rectangle2D(0, 0, 110, 62); }
        }

        public SecureTradeContainer(SecureTrade trade)
            : base(0x1E5E)
        {
            m_Trade = trade;

            Movable = false;
        }

        public SecureTradeContainer(Serial serial)
            : base(serial)
        {
        }

        public override bool CheckHold(Mobile m, Item item, bool message, bool checkItems, int plusItems, int plusWeight)
        {
            Mobile to;

            if (this.Trade.From.Container != this)
                to = this.Trade.From.Mobile;
            else
                to = this.Trade.To.Mobile;

            return m.CheckTrade(to, item, this, message, checkItems, plusItems, plusWeight);
        }

        public override bool CheckLift(Mobile from, Item item)
        {
            return false;
        }

        public override bool IsAccessibleTo(Mobile check)
        {
            if (!IsChildOf(check))
                return false;

            return base.IsAccessibleTo(check);
        }

        public override void OnItemAdded(Item item)
        {
            ClearChecks();
        }

        public override void OnItemRemoved(Item item)
        {
            ClearChecks();
        }

        public override void OnSubItemAdded(Item item)
        {
            ClearChecks();
        }

        public override void OnSubItemRemoved(Item item)
        {
            ClearChecks();
        }

        public void ClearChecks()
        {
            if (m_Trade != null)
            {
                m_Trade.From.Accepted = false;
                m_Trade.To.Accepted = false;
                m_Trade.Update();
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}