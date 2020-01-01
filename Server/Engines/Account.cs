using System;
using System.Collections;
using System.Xml;

namespace Server.Accounting
{
    public interface IAccount
    {
        int Length { get; }
        int Limit { get; }
        int Count { get; }
        Mobile this[int index] { get; set; }
    }
}