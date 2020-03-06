using System;

namespace AppTemplate.GenericHost.Core
{
    public interface ILoggerAdapter
    {
        void Info(string message);
        void Warning(string message);
        void Error(string message);
        void Error(Exception exception, string message);
    }
}