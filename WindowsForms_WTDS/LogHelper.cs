using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace WindowsForms_WTDS
{
   
    public class Log
    {
        private const string SError = "logerror";
        private const string Slaser = "loglaser";
        private const string DefaultName = "loginfo";

        static Log()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + @"\log4net_config.xml";
            log4net.Config.XmlConfigurator.Configure(new FileInfo(path));
        }

        public static log4net.ILog GetLog(string logName)
        {
            var log = log4net.LogManager.GetLogger(logName);
            return log;
        }

        public static void laser(string message)
        {
            var log = log4net.LogManager.GetLogger(Slaser);
            if (log.IsWarnEnabled)
                Task.Run(() => log.Warn(message));           
        }

        public static void laser(string message, Exception ex)
        {
            var log = log4net.LogManager.GetLogger(Slaser);
            if (log.IsWarnEnabled)
                Task.Run(() => log.Warn(message, ex));
        }

        public static void Error(string message)
        {
            var log = log4net.LogManager.GetLogger(SError);
            if (log.IsErrorEnabled)
                Task.Run(() => log.Error(message));
        }

        public static void Error(string message, Exception ex)
        {
            var log = log4net.LogManager.GetLogger(SError);
            if (log.IsErrorEnabled)
                Task.Run(() => log.Error(message, ex));
        }

        public static void Fatal(string message)
        {
            var log = log4net.LogManager.GetLogger(DefaultName);
            if (log.IsFatalEnabled)
                Task.Run(() => log.Fatal(message));
        }

        public static void Info(object message)
        {
           log4net.ILog log = log4net.LogManager.GetLogger(DefaultName);
          //  log4net.ILog log = log4net.LogManager.GetLogger("loginfo");
            if (log.IsInfoEnabled)
              Task.Run(()=> log.Info(message));
        }

        public static void Warn(string message)
        {
            var log = log4net.LogManager.GetLogger(DefaultName);
            if (log.IsWarnEnabled)
                Task.Run(() => log.Warn(message));
        }
    }
}
