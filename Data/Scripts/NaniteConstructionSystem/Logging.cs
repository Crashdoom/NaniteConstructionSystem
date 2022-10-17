using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI;
using System.IO;
using VRage;
using VRage.Utils;

namespace NaniteConstructionSystem
{
    public class WaitingToLog
    {
        public string Text;
        public int Logging;

        public WaitingToLog(string text, int logging = 0)
        {
            Text = text;
            Logging = logging;
        }
    }

    public class DebugSession
    {
        private static DebugSession m_instance;

        private bool loggingEnabled = false;
        private ConcurrentBag<string> m_debugCache = new ConcurrentBag<string>();

        static public DebugSession Instance
        {
            get
            {
                if (m_instance == null)
                    m_instance = new DebugSession();

                return m_instance;
            }
        }

        public DebugSession() {}

        public void StartLogging()
        {
            loggingEnabled = true;
            var date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
            MyLog.Default.WriteLineAndConsole($"[Nanite] Started debug logging at {date}!");
            m_debugCache.Add($"Started debug logging at {date}!");
        }

        public void StopLogging()
        {
            MyLog.Default.WriteLineAndConsole($"[Nanite] Stopped debug logging...");
            try
            {
                loggingEnabled = false;
                var date = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                TextWriter m_writer = MyAPIGateway.Utilities.WriteFileInWorldStorage($"Nanite_Debug_{date}.log", typeof(DebugSession));

                while (!m_debugCache.IsEmpty)
                {
                    string line = null;
                    m_debugCache.TryTake(out line);

                    if (line != null)
                    {
                        m_writer.Write(line);
                        m_writer.Flush();
                    }
                }

                m_writer.Flush();
                m_writer.Close();
            } catch (Exception e) {
                MyLog.Default.WriteLineAndConsole("[Nanite] Failed to save debug log file!");
                MyLog.Default.WriteLineAndConsole(e.ToString());
            }

            m_debugCache = new ConcurrentBag<string>();
        }

        public void WriteLine(string text)
        {
            if (!loggingEnabled) return;
            m_debugCache.Add(DateTime.Now.ToString("[HH:mm:ss.fff] ") + text);
        }
    }

    public class Logging
    {
        private static Logging m_instance;

        private TextWriter m_writer;
        private ConcurrentBag<string> m_writeCache;
        private ConcurrentBag<WaitingToLog> m_waitingList;
        private bool m_busy;
        private string m_logFile;

        static public Logging Instance
        {
            get
            {
                if (m_instance == null)
                    m_instance = new Logging("NaniteConstructionSystem.log");

                return m_instance;
            }
        }

        public Logging(string logFile)
        {
            try
            {
                m_instance = this;
                m_writeCache = new ConcurrentBag<string>();
                m_logFile = logFile;
                m_waitingList = new ConcurrentBag<WaitingToLog>();
                m_busy = false;
            }
            catch { }
        }

        public void WriteLine(string text, int logging = 0)
        {
            if (NaniteConstructionManager.Settings == null)
            { // Settings haven't been loaded yet, so put it in a waiting list
                m_waitingList.Add(new WaitingToLog(text, logging));
                return;
            }

            if (NaniteConstructionManager.Settings.DebugLogging != null && NaniteConstructionManager.Settings.DebugLogging < logging)
                return;

            MyAPIGateway.Parallel.Start(() =>
            {
                try
                    { m_writeCache.Add(DateTime.Now.ToString("[HH:mm:ss] ") + text + "\r\n"); }
                catch (Exception e)
                    { MyLog.Default.WriteLineAndConsole($"Nanite.Logging.WriteLine Error: {e.ToString()}"); }
            });
        }

        public void WriteToFile()
        { // Called once every second from the main logic in Core.cs
            MyAPIGateway.Parallel.StartBackground(() =>
            {
                try
                {
                    if (m_busy)
                        return;

                    m_busy = true;

                    if (!m_waitingList.IsEmpty)
                    {
                        ConcurrentBag<WaitingToLog> iterateList = new ConcurrentBag<WaitingToLog>();

                        int counter = m_waitingList.Count;
                        int i = 0;
                        while (i < counter)
                        {
                            WaitingToLog moveItem = null;
                            m_waitingList.TryTake(out moveItem);

                            if (moveItem != null)
                                iterateList.Add(moveItem);

                            i++;
                        }

                        while (!iterateList.IsEmpty)
                        {
                            WaitingToLog waitingItem = null;
                            iterateList.TryTake(out waitingItem);

                            if (waitingItem != null)
                                Instance.WriteLine(waitingItem.Text, waitingItem.Logging);
                        }
                    }

                    if (m_writer == null)
                    {
                        if (MyAPIGateway.Utilities == null)
                            return;

                        m_writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(m_logFile, typeof(Logging));
                    }

                    while (!m_writeCache.IsEmpty)
                    {
                        string line = null;
                        m_writeCache.TryTake(out line);

                        if (line != null)
                        {
                            m_writer.Write(line);
                            m_writer.Flush();
                        }
                    }
                }
                catch (Exception e)
                    { MyLog.Default.WriteLineAndConsole($"Nanite.Logging.WriteToFile Error: {e.ToString()}"); }
                finally
                    { m_busy = false; }

            });
        }

        internal void Close()
        {
            try
            {
                if (m_writer != null)
                {
                    m_writer.Flush();
                    m_writer.Close();
                    m_writer = null;
                }

                m_instance = null;
            }
            catch (Exception e)
            {
                { MyLog.Default.WriteLineAndConsole($"Nanite.Logging.Close Error: {e.ToString()}"); }
            }
        }
    }
}
