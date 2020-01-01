using System;
using System.Collections;
using System.Text;

using Server.Network;

namespace Server.Gumps
{
    public class Gump
    {
        private ArrayList m_Entries;
        private ArrayList m_Strings;

        internal int m_TextEntries, m_Switches;

        private static int m_NextSerial = 1;

        private int m_Serial;
        private int m_TypeID;
        private int m_X, m_Y;

        private bool m_Dragable = true;
        private bool m_Closable = true;
        private bool m_Resizable = true;
        private bool m_Disposable = true;

        public static int GetTypeID(Type type)
        {
            return type.GetHashCode() ^ type.FullName.GetHashCode() ^ type.TypeHandle.Value.ToInt32();
        }

        public Gump(int x, int y)
        {
            do
            {
                m_Serial = m_NextSerial++;
            } while (m_Serial == 0); // standard client apparently doesn't send a gump response packet if serial == 0

            m_X = x;
            m_Y = y;

            m_TypeID = GetType().FullName.GetHashCode();

            m_Entries = new ArrayList();
            m_Strings = new ArrayList();
        }

        public void Invalidate()
        {
            if (m_Packet != null)
            {
                m_Packet = null;

                if (m_Strings.Count > 0)
                    m_Strings.Clear();
            }
        }

        public int TypeID
        {
            get
            {
                return m_TypeID;
            }
        }

        public ArrayList Entries
        {
            get { return m_Entries; }
        }

        public int Serial
        {
            get
            {
                return m_Serial;
            }
            set
            {
                if (m_Serial != value)
                {
                    m_Serial = value;
                    Invalidate();
                }
            }
        }

        public int X
        {
            get
            {
                return m_X;
            }
            set
            {
                if (m_X != value)
                {
                    m_X = value;
                    Invalidate();
                }
            }
        }

        public int Y
        {
            get
            {
                return m_Y;
            }
            set
            {
                if (m_Y != value)
                {
                    m_Y = value;
                    Invalidate();
                }
            }
        }

        public bool Disposable
        {
            get
            {
                return m_Disposable;
            }
            set
            {
                if (m_Disposable != value)
                {
                    m_Disposable = value;
                    Invalidate();
                }
            }
        }

        public bool Resizable
        {
            get
            {
                return m_Resizable;
            }
            set
            {
                if (m_Resizable != value)
                {
                    m_Resizable = value;
                    Invalidate();
                }
            }
        }

        public bool Dragable
        {
            get
            {
                return m_Dragable;
            }
            set
            {
                if (m_Dragable != value)
                {
                    m_Dragable = value;
                    Invalidate();
                }
            }
        }

        public bool Closable
        {
            get
            {
                return m_Closable;
            }
            set
            {
                if (m_Closable != value)
                {
                    m_Closable = value;
                    Invalidate();
                }
            }
        }

        public void AddPage(int page)
        {
            Add(new GumpPage(page));
        }

        public void AddAlphaRegion(int x, int y, int width, int height)
        {
            Add(new GumpAlphaRegion(x, y, width, height));
        }

        public void AddBackground(int x, int y, int width, int height, int gumpID)
        {
            Add(new GumpBackground(x, y, width, height, gumpID));
        }

        public void AddButton(int x, int y, int normalID, int pressedID, int buttonID, GumpButtonType type, int param)
        {
            Add(new GumpButton(x, y, normalID, pressedID, buttonID, type, param));
        }

        public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, int switchID)
        {
            Add(new GumpCheck(x, y, inactiveID, activeID, initialState, switchID));
        }

        public void AddGroup(int group)
        {
            Add(new GumpGroup(group));
        }

        public void AddHtml(int x, int y, int width, int height, string text, bool background, bool scrollbar)
        {
            Add(new GumpHtml(x, y, width, height, text, background, scrollbar));
        }

        public void AddHtmlLocalized(int x, int y, int width, int height, int number, bool background, bool scrollbar)
        {
            Add(new GumpHtmlLocalized(x, y, width, height, number, background, scrollbar));
        }

        public void AddHtmlLocalized(int x, int y, int width, int height, int number, int color, bool background, bool scrollbar)
        {
            Add(new GumpHtmlLocalized(x, y, width, height, number, color, background, scrollbar));
        }

        public void AddImage(int x, int y, int gumpID)
        {
            Add(new GumpImage(x, y, gumpID));
        }

        public void AddImage(int x, int y, int gumpID, int hue)
        {
            Add(new GumpImage(x, y, gumpID, hue));
        }

        public void AddImageTiled(int x, int y, int width, int height, int gumpID)
        {
            Add(new GumpImageTiled(x, y, width, height, gumpID));
        }

        public void AddItem(int x, int y, int itemID)
        {
            Add(new GumpItem(x, y, itemID));
        }

        public void AddItem(int x, int y, int itemID, int hue)
        {
            Add(new GumpItem(x, y, itemID, hue));
        }

        public void AddLabel(int x, int y, int hue, string text)
        {
            Add(new GumpLabel(x, y, hue, text));
        }

        public void AddLabelCropped(int x, int y, int width, int height, int hue, string text)
        {
            Add(new GumpLabelCropped(x, y, width, height, hue, text));
        }

        public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, int switchID)
        {
            Add(new GumpRadio(x, y, inactiveID, activeID, initialState, switchID));
        }

        public void AddTextEntry(int x, int y, int width, int height, int hue, int entryID, string initialText)
        {
            Add(new GumpTextEntry(x, y, width, height, hue, entryID, initialText));
        }

        public void Add(GumpEntry g)
        {
            if (g.Parent != this)
            {
                g.Parent = this;
            }
            else if (!m_Entries.Contains(g))
            {
                Invalidate();
                m_Entries.Add(g);
            }
        }

        public void Remove(GumpEntry g)
        {
            Invalidate();
            m_Entries.Remove(g);
            g.Parent = null;
        }

        public int Intern(string value)
        {
            int indexOf = m_Strings.IndexOf(value);

            if (indexOf >= 0)
            {
                return indexOf;
            }
            else
            {
                Invalidate();
                return m_Strings.Add(value);
            }
        }

        public void SendTo(NetState state)
        {
            state.AddGump(this);
            state.Send(Compile());
        }

        private DisplayGumpFast m_Packet;

        public static byte[] StringToBuffer(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        private static byte[] m_BeginLayout = StringToBuffer("{ ");
        private static byte[] m_EndLayout = StringToBuffer(" }");

        private static byte[] m_NoMove = StringToBuffer("{ nomove }");
        private static byte[] m_NoClose = StringToBuffer("{ noclose }");
        private static byte[] m_NoDispose = StringToBuffer("{ nodispose }");
        private static byte[] m_NoResize = StringToBuffer("{ noresize }");

        private Packet Compile()
        {
            if (m_Packet == null)
            {
                DisplayGumpFast disp = new DisplayGumpFast(this);

                if (!m_Dragable)
                    disp.AppendLayout(m_NoMove);

                if (!m_Closable)
                    disp.AppendLayout(m_NoClose);

                if (!m_Disposable)
                    disp.AppendLayout(m_NoDispose);

                if (!m_Resizable)
                    disp.AppendLayout(m_NoResize);

                int count = m_Entries.Count;
                GumpEntry e;

                for (int i = 0; i < count; ++i)
                {
                    e = (GumpEntry)m_Entries[i];

                    disp.AppendLayout(m_BeginLayout);
                    e.AppendTo(disp);
                    disp.AppendLayout(m_EndLayout);
                }

                disp.WriteText(m_Strings);

                m_TextEntries = disp.TextEntries;
                m_Switches = disp.Switches;

                m_Packet = disp;
            }

            return m_Packet;
        }

        public virtual void OnResponse(NetState sender, RelayInfo info)
        {
        }
    }

    public abstract class GumpEntry
    {
        private Gump m_Parent;

        public GumpEntry()
        {
        }

        protected void Delta(ref int var, int val)
        {
            if (var != val)
            {
                var = val;

                if (m_Parent != null)
                {
                    m_Parent.Invalidate();
                }
            }
        }

        protected void Delta(ref bool var, bool val)
        {
            if (var != val)
            {
                var = val;

                if (m_Parent != null)
                {
                    m_Parent.Invalidate();
                }
            }
        }

        protected void Delta(ref string var, string val)
        {
            if (var != val)
            {
                var = val;

                if (m_Parent != null)
                {
                    m_Parent.Invalidate();
                }
            }
        }

        public Gump Parent
        {
            get
            {
                return m_Parent;
            }
            set
            {
                if (m_Parent != value)
                {
                    if (m_Parent != null)
                    {
                        m_Parent.Remove(this);
                    }

                    m_Parent = value;

                    m_Parent.Add(this);
                }
            }
        }

        public abstract string Compile();
        public abstract void AppendTo(DisplayGumpFast disp);
    }

    public class GumpAlphaRegion : GumpEntry
    {
        private int m_X, m_Y;
        private int m_Width, m_Height;

        public int X
        {
            get
            {
                return m_X;
            }
            set
            {
                Delta(ref m_X, value);
            }
        }

        public int Y
        {
            get
            {
                return m_Y;
            }
            set
            {
                Delta(ref m_Y, value);
            }
        }

        public int Width
        {
            get
            {
                return m_Width;
            }
            set
            {
                Delta(ref m_Width, value);
            }
        }

        public int Height
        {
            get
            {
                return m_Height;
            }
            set
            {
                Delta(ref m_Height, value);
            }
        }

        public GumpAlphaRegion(int x, int y, int width, int height)
        {
            m_X = x;
            m_Y = y;
            m_Width = width;
            m_Height = height;
        }

        public override string Compile()
        {
            return String.Format("{{ checkertrans {0} {1} {2} {3} }}", m_X, m_Y, m_Width, m_Height);
        }

        private static byte[] m_LayoutName = Gump.StringToBuffer("checkertrans");

        public override void AppendTo(DisplayGumpFast disp)
        {
            disp.AppendLayout(m_LayoutName);
            disp.AppendLayout(m_X);
            disp.AppendLayout(m_Y);
            disp.AppendLayout(m_Width);
            disp.AppendLayout(m_Height);
        }
    }

    public class GumpBackground : GumpEntry
    {
        private int m_X, m_Y;
        private int m_Width, m_Height;
        private int m_GumpID;

        public int X
        {
            get
            {
                return m_X;
            }
            set
            {
                Delta(ref m_X, value);
            }
        }

        public int Y
        {
            get
            {
                return m_Y;
            }
            set
            {
                Delta(ref m_Y, value);
            }
        }

        public int Width
        {
            get
            {
                return m_Width;
            }
            set
            {
                Delta(ref m_Width, value);
            }
        }

        public int Height
        {
            get
            {
                return m_Height;
            }
            set
            {
                Delta(ref m_Height, value);
            }
        }

        public int GumpID
        {
            get
            {
                return m_GumpID;
            }
            set
            {
                Delta(ref m_GumpID, value);
            }
        }

        public GumpBackground(int x, int y, int width, int height, int gumpID)
        {
            m_X = x;
            m_Y = y;
            m_Width = width;
            m_Height = height;
            m_GumpID = gumpID;
        }

        public override string Compile()
        {
            return String.Format("{{ resizepic {0} {1} {2} {3} {4} }}", m_X, m_Y, m_GumpID, m_Width, m_Height);
        }

        private static byte[] m_LayoutName = Gump.StringToBuffer("resizepic");

        public override void AppendTo(DisplayGumpFast disp)
        {
            disp.AppendLayout(m_LayoutName);
            disp.AppendLayout(m_X);
            disp.AppendLayout(m_Y);
            disp.AppendLayout(m_GumpID);
            disp.AppendLayout(m_Width);
            disp.AppendLayout(m_Height);
        }
    }

    public enum GumpButtonType
    {
        Page = 0,
        Reply = 1
    }

    public class GumpButton : GumpEntry
    {
        private int m_X, m_Y;
        private int m_ID1, m_ID2;
        private int m_ButtonID;
        private GumpButtonType m_Type;
        private int m_Param;

        public GumpButton(int x, int y, int normalID, int pressedID, int buttonID, GumpButtonType type, int param)
        {
            m_X = x;
            m_Y = y;
            m_ID1 = normalID;
            m_ID2 = pressedID;
            m_ButtonID = buttonID;
            m_Type = type;
            m_Param = param;
        }

        public int X
        {
            get
            {
                return m_X;
            }
            set
            {
                Delta(ref m_X, value);
            }
        }

        public int Y
        {
            get
            {
                return m_Y;
            }
            set
            {
                Delta(ref m_Y, value);
            }
        }

        public int NormalID
        {
            get
            {
                return m_ID1;
            }
            set
            {
                Delta(ref m_ID1, value);
            }
        }

        public int PressedID
        {
            get
            {
                return m_ID2;
            }
            set
            {
                Delta(ref m_ID2, value);
            }
        }

        public int ButtonID
        {
            get
            {
                return m_ButtonID;
            }
            set
            {
                Delta(ref m_ButtonID, value);
            }
        }

        public GumpButtonType Type
        {
            get
            {
                return m_Type;
            }
            set
            {
                if (m_Type != value)
                {
                    m_Type = value;

                    Gump parent = Parent;

                    if (parent != null)
                    {
                        parent.Invalidate();
                    }
                }
            }
        }

        public int Param
        {
            get
            {
                return m_Param;
            }
            set
            {
                Delta(ref m_Param, value);
            }
        }

        public override string Compile()
        {
            return String.Format("{{ button {0} {1} {2} {3} {4} {5} {6} }}", m_X, m_Y, m_ID1, m_ID2, (int)m_Type, m_Param, m_ButtonID);
        }

        private static byte[] m_LayoutName = Gump.StringToBuffer("button");

        public override void AppendTo(DisplayGumpFast disp)
        {
            disp.AppendLayout(m_LayoutName);
            disp.AppendLayout(m_X);
            disp.AppendLayout(m_Y);
            disp.AppendLayout(m_ID1);
            disp.AppendLayout(m_ID2);
            disp.AppendLayout((int)m_Type);
            disp.AppendLayout(m_Param);
            disp.AppendLayout(m_ButtonID);
        }
    }

    public class GumpCheck : GumpEntry
    {
        private int m_X, m_Y;
        private int m_ID1, m_ID2;
        private bool m_InitialState;
        private int m_SwitchID;

        public int X
        {
            get
            {
                return m_X;
            }
            set
            {
                Delta(ref m_X, value);
            }
        }

        public int Y
        {
            get
            {
                return m_Y;
            }
            set
            {
                Delta(ref m_Y, value);
            }
        }

        public int InactiveID
        {
            get
            {
                return m_ID1;
            }
            set
            {
                Delta(ref m_ID1, value);
            }
        }

        public int ActiveID
        {
            get
            {
                return m_ID2;
            }
            set
            {
                Delta(ref m_ID2, value);
            }
        }

        public bool InitialState
        {
            get
            {
                return m_InitialState;
            }
            set
            {
                Delta(ref m_InitialState, value);
            }
        }

        public int SwitchID
        {
            get
            {
                return m_SwitchID;
            }
            set
            {
                Delta(ref m_SwitchID, value);
            }
        }

        public GumpCheck(int x, int y, int inactiveID, int activeID, bool initialState, int switchID)
        {
            m_X = x;
            m_Y = y;
            m_ID1 = inactiveID;
            m_ID2 = activeID;
            m_InitialState = initialState;
            m_SwitchID = switchID;
        }

        public override string Compile()
        {
            return String.Format("{{ checkbox {0} {1} {2} {3} {4} {5} }}", m_X, m_Y, m_ID1, m_ID2, m_InitialState ? 1 : 0, m_SwitchID);
        }

        private static byte[] m_LayoutName = Gump.StringToBuffer("checkbox");

        public override void AppendTo(DisplayGumpFast disp)
        {
            disp.AppendLayout(m_LayoutName);
            disp.AppendLayout(m_X);
            disp.AppendLayout(m_Y);
            disp.AppendLayout(m_ID1);
            disp.AppendLayout(m_ID2);
            disp.AppendLayout(m_InitialState);
            disp.AppendLayout(m_SwitchID);

            disp.Switches++;
        }
    }

    public class GumpGroup : GumpEntry
    {
        private int m_Group;

        public GumpGroup(int group)
        {
            m_Group = group;
        }

        public int Group
        {
            get
            {
                return m_Group;
            }
            set
            {
                Delta(ref m_Group, value);
            }
        }

        public override string Compile()
        {
            return String.Format("{{ group {0} }}", m_Group);
        }

        private static byte[] m_LayoutName = Gump.StringToBuffer("group");

        public override void AppendTo(DisplayGumpFast disp)
        {
            disp.AppendLayout(m_LayoutName);
            disp.AppendLayout(m_Group);
        }
    }

    public class GumpHtml : GumpEntry
    {
        private int m_X, m_Y;
        private int m_Width, m_Height;
        private string m_Text;
        private bool m_Background, m_Scrollbar;

        public int X
        {
            get
            {
                return m_X;
            }
            set
            {
                Delta(ref m_X, value);
            }
        }

        public int Y
        {
            get
            {
                return m_Y;
            }
            set
            {
                Delta(ref m_Y, value);
            }
        }

        public int Width
        {
            get
            {
                return m_Width;
            }
            set
            {
                Delta(ref m_Width, value);
            }
        }

        public int Height
        {
            get
            {
                return m_Height;
            }
            set
            {
                Delta(ref m_Height, value);
            }
        }

        public string Text
        {
            get
            {
                return m_Text;
            }
            set
            {
                Delta(ref m_Text, value);
            }
        }

        public bool Background
        {
            get
            {
                return m_Background;
            }
            set
            {
                Delta(ref m_Background, value);
            }
        }

        public bool Scrollbar
        {
            get
            {
                return m_Scrollbar;
            }
            set
            {
                Delta(ref m_Scrollbar, value);
            }
        }

        public GumpHtml(int x, int y, int width, int height, string text, bool background, bool scrollbar)
        {
            m_X = x;
            m_Y = y;
            m_Width = width;
            m_Height = height;
            m_Text = text;
            m_Background = background;
            m_Scrollbar = scrollbar;
        }

        public override string Compile()
        {
            return String.Format("{{ htmlgump {0} {1} {2} {3} {4} {5} {6} }}", m_X, m_Y, m_Width, m_Height, Parent.Intern(m_Text), m_Background ? 1 : 0, m_Scrollbar ? 1 : 0);
        }

        private static byte[] m_LayoutName = Gump.StringToBuffer("htmlgump");

        public override void AppendTo(DisplayGumpFast disp)
        {
            disp.AppendLayout(m_LayoutName);
            disp.AppendLayout(m_X);
            disp.AppendLayout(m_Y);
            disp.AppendLayout(m_Width);
            disp.AppendLayout(m_Height);
            disp.AppendLayout(Parent.Intern(m_Text));
            disp.AppendLayout(m_Background);
            disp.AppendLayout(m_Scrollbar);
        }
    }

    public class GumpHtmlLocalized : GumpEntry
    {
        private int m_X, m_Y;
        private int m_Width, m_Height;
        private int m_Number;
        private bool m_Background, m_Scrollbar;
        private int m_Color;
        private bool m_UseColor;

        public int X
        {
            get
            {
                return m_X;
            }
            set
            {
                Delta(ref m_X, value);
            }
        }

        public int Y
        {
            get
            {
                return m_Y;
            }
            set
            {
                Delta(ref m_Y, value);
            }
        }

        public int Width
        {
            get
            {
                return m_Width;
            }
            set
            {
                Delta(ref m_Width, value);
            }
        }

        public int Height
        {
            get
            {
                return m_Height;
            }
            set
            {
                Delta(ref m_Height, value);
            }
        }

        public int Number
        {
            get
            {
                return m_Number;
            }
            set
            {
                Delta(ref m_Number, value);
            }
        }

        public int Color
        {
            get
            {
                return m_Color;
            }
            set
            {
                Delta(ref m_Color, value);
            }
        }

        public bool UseColor
        {
            get
            {
                return m_UseColor;
            }
            set
            {
                Delta(ref m_UseColor, value);
            }
        }

        public bool Background
        {
            get
            {
                return m_Background;
            }
            set
            {
                Delta(ref m_Background, value);
            }
        }

        public bool Scrollbar
        {
            get
            {
                return m_Scrollbar;
            }
            set
            {
                Delta(ref m_Scrollbar, value);
            }
        }

        public GumpHtmlLocalized(int x, int y, int width, int height, int number, bool background, bool scrollbar)
        {
            m_X = x;
            m_Y = y;
            m_Width = width;
            m_Height = height;
            m_Number = number;
            m_Background = background;
            m_Scrollbar = scrollbar;
        }

        public GumpHtmlLocalized(int x, int y, int width, int height, int number, int color, bool background, bool scrollbar)
        {
            m_X = x;
            m_Y = y;
            m_Width = width;
            m_Height = height;
            m_Number = number;
            m_Background = background;
            m_Scrollbar = scrollbar;

            m_Color = color;
            m_UseColor = true;
        }

        public override string Compile()
        {
            if (m_UseColor)
                return String.Format("{{ xmfhtmlgumpcolor {0} {1} {2} {3} {4} {5} {6} {7} }}", m_X, m_Y, m_Width, m_Height, m_Number, m_Background ? 1 : 0, m_Scrollbar ? 1 : 0, m_Color);
            else
                return String.Format("{{ xmfhtmlgump {0} {1} {2} {3} {4} {5} {6} }}", m_X, m_Y, m_Width, m_Height, m_Number, m_Background ? 1 : 0, m_Scrollbar ? 1 : 0);
        }

        private static byte[] m_LayoutNameColor = Gump.StringToBuffer("xmfhtmlgumpcolor");
        private static byte[] m_LayoutName = Gump.StringToBuffer("xmfhtmlgump");

        public override void AppendTo(DisplayGumpFast disp)
        {
            disp.AppendLayout(m_UseColor ? m_LayoutNameColor : m_LayoutName);
            disp.AppendLayout(m_X);
            disp.AppendLayout(m_Y);
            disp.AppendLayout(m_Width);
            disp.AppendLayout(m_Height);
            disp.AppendLayout(m_Number);
            disp.AppendLayout(m_Background);
            disp.AppendLayout(m_Scrollbar);

            if (m_UseColor)
                disp.AppendLayout(m_Color);
        }
    }

    public class GumpImage : GumpEntry
    {
        private int m_X, m_Y;
        private int m_GumpID;
        private int m_Hue;

        public GumpImage(int x, int y, int gumpID)
            : this(x, y, gumpID, 0)
        {
        }

        public GumpImage(int x, int y, int gumpID, int hue)
        {
            m_X = x;
            m_Y = y;
            m_GumpID = gumpID;
            m_Hue = hue;
        }

        public int X
        {
            get
            {
                return m_X;
            }
            set
            {
                Delta(ref m_X, value);
            }
        }

        public int Y
        {
            get
            {
                return m_Y;
            }
            set
            {
                Delta(ref m_Y, value);
            }
        }

        public int GumpID
        {
            get
            {
                return m_GumpID;
            }
            set
            {
                Delta(ref m_GumpID, value);
            }
        }

        public int Hue
        {
            get
            {
                return m_Hue;
            }
            set
            {
                Delta(ref m_Hue, value);
            }
        }

        public override string Compile()
        {
            if (m_Hue == 0)
                return String.Format("{{ gumppic {0} {1} {2} }}", m_X, m_Y, m_GumpID);
            else
                return String.Format("{{ gumppic {0} {1} {2} hue={3} }}", m_X, m_Y, m_GumpID, m_Hue);
        }

        private static byte[] m_LayoutName = Gump.StringToBuffer("gumppic");
        private static byte[] m_HueEquals = Gump.StringToBuffer(" hue=");

        public override void AppendTo(DisplayGumpFast disp)
        {
            disp.AppendLayout(m_LayoutName);
            disp.AppendLayout(m_X);
            disp.AppendLayout(m_Y);
            disp.AppendLayout(m_GumpID);

            if (m_Hue != 0)
            {
                disp.AppendLayout(m_HueEquals);
                disp.AppendLayoutNS(m_Hue);
            }
        }
    }

    public class GumpImageTiled : GumpEntry
    {
        private int m_X, m_Y;
        private int m_Width, m_Height;
        private int m_GumpID;

        public GumpImageTiled(int x, int y, int width, int height, int gumpID)
        {
            m_X = x;
            m_Y = y;
            m_Width = width;
            m_Height = height;
            m_GumpID = gumpID;
        }

        public int X
        {
            get
            {
                return m_X;
            }
            set
            {
                Delta(ref m_X, value);
            }
        }

        public int Y
        {
            get
            {
                return m_Y;
            }
            set
            {
                Delta(ref m_Y, value);
            }
        }

        public int Width
        {
            get
            {
                return m_Width;
            }
            set
            {
                Delta(ref m_Width, value);
            }
        }

        public int Height
        {
            get
            {
                return m_Height;
            }
            set
            {
                Delta(ref m_Height, value);
            }
        }

        public int GumpID
        {
            get
            {
                return m_GumpID;
            }
            set
            {
                Delta(ref m_GumpID, value);
            }
        }

        public override string Compile()
        {
            return String.Format("{{ gumppictiled {0} {1} {2} {3} {4} }}", m_X, m_Y, m_Width, m_Height, m_GumpID);
        }

        private static byte[] m_LayoutName = Gump.StringToBuffer("gumppictiled");

        public override void AppendTo(DisplayGumpFast disp)
        {
            disp.AppendLayout(m_LayoutName);
            disp.AppendLayout(m_X);
            disp.AppendLayout(m_Y);
            disp.AppendLayout(m_Width);
            disp.AppendLayout(m_Height);
            disp.AppendLayout(m_GumpID);
        }
    }

    public class GumpItem : GumpEntry
    {
        private int m_X, m_Y;
        private int m_ItemID;
        private int m_Hue;

        public GumpItem(int x, int y, int itemID)
            : this(x, y, itemID, 0)
        {
        }

        public GumpItem(int x, int y, int itemID, int hue)
        {
            m_X = x;
            m_Y = y;
            m_ItemID = itemID;
            m_Hue = hue;
        }

        public int X
        {
            get
            {
                return m_X;
            }
            set
            {
                Delta(ref m_X, value);
            }
        }

        public int Y
        {
            get
            {
                return m_Y;
            }
            set
            {
                Delta(ref m_Y, value);
            }
        }

        public int ItemID
        {
            get
            {
                return m_ItemID;
            }
            set
            {
                Delta(ref m_ItemID, value);
            }
        }

        public int Hue
        {
            get
            {
                return m_Hue;
            }
            set
            {
                Delta(ref m_Hue, value);
            }
        }

        public override string Compile()
        {
            if (m_Hue == 0)
                return String.Format("{{ tilepic {0} {1} {2} }}", m_X, m_Y, m_ItemID);
            else
                return String.Format("{{ tilepichue {0} {1} {2} {3} }}", m_X, m_Y, m_ItemID, m_Hue);
        }

        private static byte[] m_LayoutName = Gump.StringToBuffer("tilepic");
        private static byte[] m_LayoutNameHue = Gump.StringToBuffer("tilepichue");

        public override void AppendTo(DisplayGumpFast disp)
        {
            disp.AppendLayout(m_Hue == 0 ? m_LayoutName : m_LayoutNameHue);
            disp.AppendLayout(m_X);
            disp.AppendLayout(m_Y);
            disp.AppendLayout(m_ItemID);

            if (m_Hue != 0)
                disp.AppendLayout(m_Hue);
        }
    }

    public class GumpLabel : GumpEntry
    {
        private int m_X, m_Y;
        private int m_Hue;
        private string m_Text;

        public GumpLabel(int x, int y, int hue, string text)
        {
            m_X = x;
            m_Y = y;
            m_Hue = hue;
            m_Text = text;
        }

        public int X
        {
            get
            {
                return m_X;
            }
            set
            {
                Delta(ref m_X, value);
            }
        }

        public int Y
        {
            get
            {
                return m_Y;
            }
            set
            {
                Delta(ref m_Y, value);
            }
        }

        public int Hue
        {
            get
            {
                return m_Hue;
            }
            set
            {
                Delta(ref m_Hue, value);
            }
        }

        public string Text
        {
            get
            {
                return m_Text;
            }
            set
            {
                Delta(ref m_Text, value);
            }
        }

        public override string Compile()
        {
            return String.Format("{{ text {0} {1} {2} {3} }}", m_X, m_Y, m_Hue, Parent.Intern(m_Text));
        }

        private static byte[] m_LayoutName = Gump.StringToBuffer("text");

        public override void AppendTo(DisplayGumpFast disp)
        {
            disp.AppendLayout(m_LayoutName);
            disp.AppendLayout(m_X);
            disp.AppendLayout(m_Y);
            disp.AppendLayout(m_Hue);
            disp.AppendLayout(Parent.Intern(m_Text));
        }
    }

    public class GumpLabelCropped : GumpEntry
    {
        private int m_X, m_Y;
        private int m_Width, m_Height;
        private int m_Hue;
        private string m_Text;

        public int X
        {
            get
            {
                return m_X;
            }
            set
            {
                Delta(ref m_X, value);
            }
        }

        public int Y
        {
            get
            {
                return m_Y;
            }
            set
            {
                Delta(ref m_Y, value);
            }
        }

        public int Width
        {
            get
            {
                return m_Width;
            }
            set
            {
                Delta(ref m_Width, value);
            }
        }

        public int Height
        {
            get
            {
                return m_Height;
            }
            set
            {
                Delta(ref m_Height, value);
            }
        }

        public int Hue
        {
            get
            {
                return m_Hue;
            }
            set
            {
                Delta(ref m_Hue, value);
            }
        }

        public string Text
        {
            get
            {
                return m_Text;
            }
            set
            {
                Delta(ref m_Text, value);
            }
        }

        public GumpLabelCropped(int x, int y, int width, int height, int hue, string text)
        {
            m_X = x;
            m_Y = y;
            m_Width = width;
            m_Height = height;
            m_Hue = hue;
            m_Text = text;
        }

        public override string Compile()
        {
            return String.Format("{{ croppedtext {0} {1} {2} {3} {4} {5} }}", m_X, m_Y, m_Width, m_Height, m_Hue, Parent.Intern(m_Text));
        }

        private static byte[] m_LayoutName = Gump.StringToBuffer("croppedtext");

        public override void AppendTo(DisplayGumpFast disp)
        {
            disp.AppendLayout(m_LayoutName);
            disp.AppendLayout(m_X);
            disp.AppendLayout(m_Y);
            disp.AppendLayout(m_Width);
            disp.AppendLayout(m_Height);
            disp.AppendLayout(m_Hue);
            disp.AppendLayout(Parent.Intern(m_Text));
        }
    }

    public class GumpPage : GumpEntry
    {
        private int m_Page;

        public GumpPage(int page)
        {
            m_Page = page;
        }

        public int Page
        {
            get
            {
                return m_Page;
            }
            set
            {
                Delta(ref m_Page, value);
            }
        }

        public override string Compile()
        {
            return String.Format("{{ page {0} }}", m_Page);
        }

        private static byte[] m_LayoutName = Gump.StringToBuffer("page");

        public override void AppendTo(DisplayGumpFast disp)
        {
            disp.AppendLayout(m_LayoutName);
            disp.AppendLayout(m_Page);
        }
    }

    public class GumpRadio : GumpEntry
    {
        private int m_X, m_Y;
        private int m_ID1, m_ID2;
        private bool m_InitialState;
        private int m_SwitchID;

        public int X
        {
            get
            {
                return m_X;
            }
            set
            {
                Delta(ref m_X, value);
            }
        }

        public int Y
        {
            get
            {
                return m_Y;
            }
            set
            {
                Delta(ref m_Y, value);
            }
        }

        public int InactiveID
        {
            get
            {
                return m_ID1;
            }
            set
            {
                Delta(ref m_ID1, value);
            }
        }

        public int ActiveID
        {
            get
            {
                return m_ID2;
            }
            set
            {
                Delta(ref m_ID2, value);
            }
        }

        public bool InitialState
        {
            get
            {
                return m_InitialState;
            }
            set
            {
                Delta(ref m_InitialState, value);
            }
        }

        public int SwitchID
        {
            get
            {
                return m_SwitchID;
            }
            set
            {
                Delta(ref m_SwitchID, value);
            }
        }

        public GumpRadio(int x, int y, int inactiveID, int activeID, bool initialState, int switchID)
        {
            m_X = x;
            m_Y = y;
            m_ID1 = inactiveID;
            m_ID2 = activeID;
            m_InitialState = initialState;
            m_SwitchID = switchID;
        }

        public override string Compile()
        {
            return String.Format("{{ radio {0} {1} {2} {3} {4} {5} }}", m_X, m_Y, m_ID1, m_ID2, m_InitialState ? 1 : 0, m_SwitchID);
        }

        private static byte[] m_LayoutName = Gump.StringToBuffer("radio");

        public override void AppendTo(DisplayGumpFast disp)
        {
            disp.AppendLayout(m_LayoutName);
            disp.AppendLayout(m_X);
            disp.AppendLayout(m_Y);
            disp.AppendLayout(m_ID1);
            disp.AppendLayout(m_ID2);
            disp.AppendLayout(m_InitialState);
            disp.AppendLayout(m_SwitchID);

            disp.Switches++;
        }
    }

    public class GumpTextEntry : GumpEntry
    {
        private int m_X, m_Y;
        private int m_Width, m_Height;
        private int m_Hue;
        private int m_EntryID;
        private string m_InitialText;

        public int X
        {
            get
            {
                return m_X;
            }
            set
            {
                Delta(ref m_X, value);
            }
        }

        public int Y
        {
            get
            {
                return m_Y;
            }
            set
            {
                Delta(ref m_Y, value);
            }
        }

        public int Width
        {
            get
            {
                return m_Width;
            }
            set
            {
                Delta(ref m_Width, value);
            }
        }

        public int Height
        {
            get
            {
                return m_Height;
            }
            set
            {
                Delta(ref m_Height, value);
            }
        }

        public int Hue
        {
            get
            {
                return m_Hue;
            }
            set
            {
                Delta(ref m_Hue, value);
            }
        }

        public int EntryID
        {
            get
            {
                return m_EntryID;
            }
            set
            {
                Delta(ref m_EntryID, value);
            }
        }

        public string InitialText
        {
            get
            {
                return m_InitialText;
            }
            set
            {
                Delta(ref m_InitialText, value);
            }
        }

        public GumpTextEntry(int x, int y, int width, int height, int hue, int entryID, string initialText)
        {
            m_X = x;
            m_Y = y;
            m_Width = width;
            m_Height = height;
            m_Hue = hue;
            m_EntryID = entryID;
            m_InitialText = initialText;
        }

        public override string Compile()
        {
            return String.Format("{{ textentry {0} {1} {2} {3} {4} {5} {6} }}", m_X, m_Y, m_Width, m_Height, m_Hue, m_EntryID, Parent.Intern(m_InitialText));
        }

        private static byte[] m_LayoutName = Gump.StringToBuffer("textentry");

        public override void AppendTo(DisplayGumpFast disp)
        {
            disp.AppendLayout(m_LayoutName);
            disp.AppendLayout(m_X);
            disp.AppendLayout(m_Y);
            disp.AppendLayout(m_Width);
            disp.AppendLayout(m_Height);
            disp.AppendLayout(m_Hue);
            disp.AppendLayout(m_EntryID);
            disp.AppendLayout(Parent.Intern(m_InitialText));

            disp.TextEntries++;
        }
    }

    public class TextRelay
    {
        private int m_EntryID;
        private string m_Text;

        public TextRelay(int entryID, string text)
        {
            m_EntryID = entryID;
            m_Text = text;
        }

        public int EntryID
        {
            get
            {
                return m_EntryID;
            }
        }

        public string Text
        {
            get
            {
                return m_Text;
            }
        }
    }

    public class RelayInfo
    {
        private int m_ButtonID;
        private int[] m_Switches;
        private TextRelay[] m_TextEntries;

        public RelayInfo(int buttonID, int[] switches, TextRelay[] textEntries)
        {
            m_ButtonID = buttonID;
            m_Switches = switches;
            m_TextEntries = textEntries;
        }

        public int ButtonID
        {
            get
            {
                return m_ButtonID;
            }
        }

        public int[] Switches
        {
            get
            {
                return m_Switches;
            }
        }

        public TextRelay[] TextEntries
        {
            get
            {
                return m_TextEntries;
            }
        }

        public bool IsSwitched(int switchID)
        {
            for (int i = 0; i < m_Switches.Length; ++i)
            {
                if (m_Switches[i] == switchID)
                {
                    return true;
                }
            }

            return false;
        }

        public TextRelay GetTextEntry(int entryID)
        {
            for (int i = 0; i < m_TextEntries.Length; ++i)
            {
                if (m_TextEntries[i].EntryID == entryID)
                {
                    return m_TextEntries[i];
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Strongly typed collection of Server.Gumps.Gump.
    /// </summary>
    public class GumpCollection : System.Collections.CollectionBase
    {

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GumpCollection() :
            base()
        {
        }

        /// <summary>
        /// Gets or sets the value of the Server.Gumps.Gump at a specific position in the GumpCollection.
        /// </summary>
        public Server.Gumps.Gump this[int index]
        {
            get
            {
                return ((Server.Gumps.Gump)(this.List[index]));
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Append a Server.Gumps.Gump entry to this collection.
        /// </summary>
        /// <param name="value">Server.Gumps.Gump instance.</param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(Server.Gumps.Gump value)
        {
            return this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specified Server.Gumps.Gump instance is in this collection.
        /// </summary>
        /// <param name="value">Server.Gumps.Gump instance to search for.</param>
        /// <returns>True if the Server.Gumps.Gump instance is in the collection; otherwise false.</returns>
        public bool Contains(Server.Gumps.Gump value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Retrieve the index a specified Server.Gumps.Gump instance is in this collection.
        /// </summary>
        /// <param name="value">Server.Gumps.Gump instance to find.</param>
        /// <returns>The zero-based index of the specified Server.Gumps.Gump instance. If the object is not found, the return value is -1.</returns>
        public int IndexOf(Server.Gumps.Gump value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Removes a specified Server.Gumps.Gump instance from this collection.
        /// </summary>
        /// <param name="value">The Server.Gumps.Gump instance to remove.</param>
        public void Remove(Server.Gumps.Gump value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Returns an enumerator that can iterate through the Server.Gumps.Gump instance.
        /// </summary>
        /// <returns>An Server.Gumps.Gump's enumerator.</returns>
        public new GumpCollectionEnumerator GetEnumerator()
        {
            return new GumpCollectionEnumerator(this);
        }

        /// <summary>
        /// Insert a Server.Gumps.Gump instance into this collection at a specified index.
        /// </summary>
        /// <param name="index">Zero-based index.</param>
        /// <param name="value">The Server.Gumps.Gump instance to insert.</param>
        public void Insert(int index, Server.Gumps.Gump value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Strongly typed enumerator of Server.Gumps.Gump.
        /// </summary>
        public class GumpCollectionEnumerator : System.Collections.IEnumerator
        {

            /// <summary>
            /// Current index
            /// </summary>
            private int _index;

            /// <summary>
            /// Current element pointed to.
            /// </summary>
            private Server.Gumps.Gump _currentElement;

            /// <summary>
            /// Collection to enumerate.
            /// </summary>
            private GumpCollection _collection;

            /// <summary>
            /// Default constructor for enumerator.
            /// </summary>
            /// <param name="collection">Instance of the collection to enumerate.</param>
            internal GumpCollectionEnumerator(GumpCollection collection)
            {
                _index = -1;
                _collection = collection;
            }

            /// <summary>
            /// Gets the Server.Gumps.Gump object in the enumerated GumpCollection currently indexed by this instance.
            /// </summary>
            public Server.Gumps.Gump Current
            {
                get
                {
                    if (((_index == -1)
                                || (_index >= _collection.Count)))
                    {
                        throw new System.IndexOutOfRangeException("Enumerator not started.");
                    }
                    else
                    {
                        return _currentElement;
                    }
                }
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            object IEnumerator.Current
            {
                get
                {
                    if (((_index == -1)
                                || (_index >= _collection.Count)))
                    {
                        throw new System.IndexOutOfRangeException("Enumerator not started.");
                    }
                    else
                    {
                        return _currentElement;
                    }
                }
            }

            /// <summary>
            /// Reset the cursor, so it points to the beginning of the enumerator.
            /// </summary>
            public void Reset()
            {
                _index = -1;
                _currentElement = null;
            }

            /// <summary>
            /// Advances the enumerator to the next queue of the enumeration, if one is currently available.
            /// </summary>
            /// <returns>true, if the enumerator was succesfully advanced to the next queue; false, if the enumerator has reached the end of the enumeration.</returns>
            public bool MoveNext()
            {
                if ((_index
                            < (_collection.Count - 1)))
                {
                    _index = (_index + 1);
                    _currentElement = this._collection[_index];
                    return true;
                }
                _index = _collection.Count;
                return false;
            }
        }
    }
}