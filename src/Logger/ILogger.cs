using System;

namespace AsyncMiddleWare.Logger
{
    public interface ILogger
    {
        Type AttachParent { get; }
        void Debug(string msg, params object[] args);
        void Debug(string msg, Exception err, params object[] args);
        void Info(string msg, params object[] args);
        void Info(string msg, Exception err, params object[] args);
        void Trace(string msg, params object[] args);
        void Trace(string msg, Exception err, params object[] args);
        void Error(string msg, params object[] args);
        void Error(string msg, Exception err, params object[] args);
        void Fatal(string msg, params object[] args);
        void Fatal(string msg, Exception err, params object[] args);
    }
}
