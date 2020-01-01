﻿using System;
using System.Collections;

using Server.Network;

namespace Server.HuePickers
{
    public class HuePicker
    {
        private static int m_NextSerial = 1;

        private int m_Serial;
        private int m_ItemID;

        public int Serial
        {
            get
            {
                return m_Serial;
            }
        }

        public int ItemID
        {
            get
            {
                return m_ItemID;
            }
        }

        public HuePicker(int itemID)
        {
            do
            {
                m_Serial = m_NextSerial++;
            } while (m_Serial == 0);

            m_ItemID = itemID;
        }

        public virtual void OnResponse(int hue)
        {
        }

        public void SendTo(NetState state)
        {
            state.Send(new DisplayHuePicker(this));
            state.AddHuePicker(this);
        }
    }

    /// <summary>
    /// Strongly typed collection of Server.HuePickers.HuePicker.
    /// </summary>
    public class HuePickerCollection : System.Collections.CollectionBase
    {

        /// <summary>
        /// Default constructor.
        /// </summary>
        public HuePickerCollection() :
            base()
        {
        }

        /// <summary>
        /// Gets or sets the value of the Server.HuePickers.HuePicker at a specific position in the HuePickerCollection.
        /// </summary>
        public Server.HuePickers.HuePicker this[int index]
        {
            get
            {
                return ((Server.HuePickers.HuePicker)(this.List[index]));
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Append a Server.HuePickers.HuePicker entry to this collection.
        /// </summary>
        /// <param name="value">Server.HuePickers.HuePicker instance.</param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(Server.HuePickers.HuePicker value)
        {
            return this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specified Server.HuePickers.HuePicker instance is in this collection.
        /// </summary>
        /// <param name="value">Server.HuePickers.HuePicker instance to search for.</param>
        /// <returns>True if the Server.HuePickers.HuePicker instance is in the collection; otherwise false.</returns>
        public bool Contains(Server.HuePickers.HuePicker value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Retrieve the index a specified Server.HuePickers.HuePicker instance is in this collection.
        /// </summary>
        /// <param name="value">Server.HuePickers.HuePicker instance to find.</param>
        /// <returns>The zero-based index of the specified Server.HuePickers.HuePicker instance. If the object is not found, the return value is -1.</returns>
        public int IndexOf(Server.HuePickers.HuePicker value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Removes a specified Server.HuePickers.HuePicker instance from this collection.
        /// </summary>
        /// <param name="value">The Server.HuePickers.HuePicker instance to remove.</param>
        public void Remove(Server.HuePickers.HuePicker value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Returns an enumerator that can iterate through the Server.HuePickers.HuePicker instance.
        /// </summary>
        /// <returns>An Server.HuePickers.HuePicker's enumerator.</returns>
        public new HuePickerCollectionEnumerator GetEnumerator()
        {
            return new HuePickerCollectionEnumerator(this);
        }

        /// <summary>
        /// Insert a Server.HuePickers.HuePicker instance into this collection at a specified index.
        /// </summary>
        /// <param name="index">Zero-based index.</param>
        /// <param name="value">The Server.HuePickers.HuePicker instance to insert.</param>
        public void Insert(int index, Server.HuePickers.HuePicker value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Strongly typed enumerator of Server.HuePickers.HuePicker.
        /// </summary>
        public class HuePickerCollectionEnumerator : System.Collections.IEnumerator
        {

            /// <summary>
            /// Current index
            /// </summary>
            private int _index;

            /// <summary>
            /// Current element pointed to.
            /// </summary>
            private Server.HuePickers.HuePicker _currentElement;

            /// <summary>
            /// Collection to enumerate.
            /// </summary>
            private HuePickerCollection _collection;

            /// <summary>
            /// Default constructor for enumerator.
            /// </summary>
            /// <param name="collection">Instance of the collection to enumerate.</param>
            internal HuePickerCollectionEnumerator(HuePickerCollection collection)
            {
                _index = -1;
                _collection = collection;
            }

            /// <summary>
            /// Gets the Server.HuePickers.HuePicker object in the enumerated HuePickerCollection currently indexed by this instance.
            /// </summary>
            public Server.HuePickers.HuePicker Current
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