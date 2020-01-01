﻿using System;
using System.Diagnostics;

using DiagELog = System.Diagnostics.EventLog;

namespace Server
{
    public class EventLog
    {
        static EventLog()
        {
            if (!DiagELog.SourceExists("RunUO"))
            {
                DiagELog.CreateEventSource("RunUO", "Application");
            }
        }

        public static void Error(int eventID, string text)
        {
            DiagELog.WriteEntry("RunUO", text, EventLogEntryType.Error, eventID);
        }

        public static void Error(int eventID, string format, params object[] args)
        {
            Error(eventID, String.Format(format, args));
        }

        public static void Warning(int eventID, string text)
        {
            DiagELog.WriteEntry("RunUO", text, EventLogEntryType.Warning, eventID);
        }

        public static void Warning(int eventID, string format, params object[] args)
        {
            Warning(eventID, String.Format(format, args));
        }

        public static void Inform(int eventID, string text)
        {
            DiagELog.WriteEntry("RunUO", text, EventLogEntryType.Information, eventID);
        }

        public static void Inform(int eventID, string format, params object[] args)
        {
            Inform(eventID, String.Format(format, args));
        }
    }
}