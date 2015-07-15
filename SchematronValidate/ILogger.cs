using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
namespace WindowsService1
{
    public interface ILogger
    {
        void Log(string message);
    }

    public class Logger : ILogger
    {
        private EventLog eventLog;

        public Logger(EventLog log)
        {
            eventLog = log;
        }

        public void Log(string logMessage)
        {
            lock (eventLog)
            {
                DateTime now = DateTime.Now;
                var message = string.Format("{0} : {1}", now.ToLongTimeString(), logMessage);
                eventLog.WriteEntry(message);
                File.AppendAllLines(@"c:\temp\watcherservice.log", new String[] { message });
            }
        }
    }
}