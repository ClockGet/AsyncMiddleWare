using System;

namespace AsyncMiddleWare.Logger
{
    public sealed class NLogger : ILogger
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Type AttachParent
        {
            get;
            internal set;
        }

        public void Debug(string msg, params object[] args)
        {
            logger.Debug(msg, args);
        }

        public void Debug(string msg, Exception err, params object[] args)
        {
            logger.Debug(err, msg, args);
        }

        public void Info(string msg, params object[] args)
        {
            logger.Info(msg, args);
        }

        public void Info(string msg, Exception err, params object[] args)
        {
            logger.Info(err, msg, args);
        }

        public void Trace(string msg, params object[] args)
        {
            logger.Trace(msg, args);
        }

        public void Trace(string msg, Exception err, params object[] args)
        {
            logger.Trace(err, msg, args);
        }

        public void Error(string msg, params object[] args)
        {
            logger.Error(msg, args);
        }

        public void Error(string msg, Exception err, params object[] args)
        {
            logger.Error(err, msg, args);
        }

        public void Fatal(string msg, params object[] args)
        {
            logger.Fatal(msg, args);
        }

        public void Fatal(string msg, Exception err, params object[] args)
        {
            logger.Fatal(err, msg, args);
        }
    }
}
