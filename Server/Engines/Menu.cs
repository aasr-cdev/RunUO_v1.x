using System;
using System.Collections;

using Server.Network;

namespace Server.Menus
{
    public interface IMenu
    {
        int Serial { get; }
        int EntryLength { get; }
        void SendTo(NetState state);
        void OnCancel(NetState state);
        void OnResponse(NetState state, int index);
    }

    /// <summary>
    /// Strongly typed collection of Server.Menus.IMenu.
    /// </summary>
    public class MenuCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public MenuCollection() :
            base()
        {
        }

        /// <summary>
        /// Gets or sets the value of the Server.Menus.IMenu at a specific position in the MenuCollection.
        /// </summary>
        public Server.Menus.IMenu this[int index]
        {
            get
            {
                return ((Server.Menus.IMenu)(this.List[index]));
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Append a Server.Menus.IMenu entry to this collection.
        /// </summary>
        /// <param name="value">Server.Menus.IMenu instance.</param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(Server.Menus.IMenu value)
        {
            return this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specified Server.Menus.IMenu instance is in this collection.
        /// </summary>
        /// <param name="value">Server.Menus.IMenu instance to search for.</param>
        /// <returns>True if the Server.Menus.IMenu instance is in the collection; otherwise false.</returns>
        public bool Contains(Server.Menus.IMenu value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Retrieve the index a specified Server.Menus.IMenu instance is in this collection.
        /// </summary>
        /// <param name="value">Server.Menus.IMenu instance to find.</param>
        /// <returns>The zero-based index of the specified Server.Menus.IMenu instance. If the object is not found, the return value is -1.</returns>
        public int IndexOf(Server.Menus.IMenu value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Removes a specified Server.Menus.IMenu instance from this collection.
        /// </summary>
        /// <param name="value">The Server.Menus.IMenu instance to remove.</param>
        public void Remove(Server.Menus.IMenu value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Returns an enumerator that can iterate through the Server.Menus.IMenu instance.
        /// </summary>
        /// <returns>An Server.Menus.IMenu's enumerator.</returns>
        public new MenuCollectionEnumerator GetEnumerator()
        {
            return new MenuCollectionEnumerator(this);
        }

        /// <summary>
        /// Insert a Server.Menus.IMenu instance into this collection at a specified index.
        /// </summary>
        /// <param name="index">Zero-based index.</param>
        /// <param name="value">The Server.Menus.IMenu instance to insert.</param>
        public void Insert(int index, Server.Menus.IMenu value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Strongly typed enumerator of Server.Menus.IMenu.
        /// </summary>
        public class MenuCollectionEnumerator : System.Collections.IEnumerator
        {

            /// <summary>
            /// Current index
            /// </summary>
            private int _index;

            /// <summary>
            /// Current element pointed to.
            /// </summary>
            private Server.Menus.IMenu _currentElement;

            /// <summary>
            /// Collection to enumerate.
            /// </summary>
            private MenuCollection _collection;

            /// <summary>
            /// Default constructor for enumerator.
            /// </summary>
            /// <param name="collection">Instance of the collection to enumerate.</param>
            internal MenuCollectionEnumerator(MenuCollection collection)
            {
                _index = -1;
                _collection = collection;
            }

            /// <summary>
            /// Gets the Server.Menus.IMenu object in the enumerated MenuCollection currently indexed by this instance.
            /// </summary>
            public Server.Menus.IMenu Current
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

namespace Server.Menus.ItemLists
{
    public class ItemListEntry
    {
        private string m_Name;
        private int m_ItemID;
        private int m_Hue;

        public string Name
        {
            get
            {
                return m_Name;
            }
        }

        public int ItemID
        {
            get
            {
                return m_ItemID;
            }
        }

        public int Hue
        {
            get
            {
                return m_Hue;
            }
        }

        public ItemListEntry(string name, int itemID)
            : this(name, itemID, 0)
        {
        }

        public ItemListEntry(string name, int itemID, int hue)
        {
            m_Name = name;
            m_ItemID = itemID;
            m_Hue = hue;
        }
    }

    public class ItemListMenu : IMenu
    {
        private string m_Question;
        private ItemListEntry[] m_Entries;

        private int m_Serial;
        private static int m_NextSerial;

        int IMenu.Serial
        {
            get
            {
                return m_Serial;
            }
        }

        int IMenu.EntryLength
        {
            get
            {
                return m_Entries.Length;
            }
        }

        public string Question
        {
            get
            {
                return m_Question;
            }
        }

        public ItemListEntry[] Entries
        {
            get
            {
                return m_Entries;
            }
            set
            {
                m_Entries = value;
            }
        }

        public ItemListMenu(string question, ItemListEntry[] entries)
        {
            m_Question = question;
            m_Entries = entries;

            do
            {
                m_Serial = m_NextSerial++;
                m_Serial &= 0x7FFFFFFF;
            } while (m_Serial == 0);

            m_Serial = (int)((uint)m_Serial | 0x80000000);
        }

        public virtual void OnCancel(NetState state)
        {
        }

        public virtual void OnResponse(NetState state, int index)
        {
        }

        public void SendTo(NetState state)
        {
            state.AddMenu(this);
            state.Send(new DisplayItemListMenu(this));
        }
    }
}

namespace Server.Menus.Questions
{
    public class QuestionMenu : IMenu
    {
        private string m_Question;
        private string[] m_Answers;

        private int m_Serial;
        private static int m_NextSerial;

        int IMenu.Serial
        {
            get
            {
                return m_Serial;
            }
        }

        int IMenu.EntryLength
        {
            get
            {
                return m_Answers.Length;
            }
        }

        public string Question
        {
            get
            {
                return m_Question;
            }
            set
            {
                m_Question = value;
            }
        }

        public string[] Answers
        {
            get
            {
                return m_Answers;
            }
        }

        public QuestionMenu(string question, string[] answers)
        {
            m_Question = question;
            m_Answers = answers;

            do
            {
                m_Serial = ++m_NextSerial;
                m_Serial &= 0x7FFFFFFF;
            } while (m_Serial == 0);
        }

        public virtual void OnCancel(NetState state)
        {
        }

        public virtual void OnResponse(NetState state, int index)
        {
        }

        public void SendTo(NetState state)
        {
            state.AddMenu(this);
            state.Send(new DisplayQuestionMenu(this));
        }
    }
}