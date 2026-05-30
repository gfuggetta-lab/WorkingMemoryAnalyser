using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Microsoft.Extensions.Logging;

namespace godot.Scripts
{
    public class GodotMSLogger : ILogger
    {

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public void Log<TState>(LogLevel logLevel, 
            EventId eventId, TState state, 
            Exception exception, Func<TState, Exception, string> formatter)
        {
            GD.Print(formatter(state, exception));
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }


    }
}
