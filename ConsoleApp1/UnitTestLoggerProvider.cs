using System;
using Microsoft.Extensions.Logging;

namespace Test
{
    public class UnitTestLoggerProvider : ILoggerProvider
    {
        public IWriter Writer { get; private set; }

        public UnitTestLoggerProvider(IWriter writer)
        {
            Writer = writer;
        }

        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new UnitTestLogger(Writer);
        }

        public class UnitTestLogger : ILogger
        {
            public IWriter Writer { get; }

            public UnitTestLogger(IWriter writer)
            {
                Writer = writer;
                Name = nameof(UnitTestLogger);
            }

            public string Name { get; set; }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter)
            {
                if (!this.IsEnabled(logLevel)) //-V3022
                    return;

                if (formatter == null)
                    throw new ArgumentNullException(nameof(formatter));

                var message = formatter(state, exception);
                if (string.IsNullOrEmpty(message) && exception == null)
                    return;

                var line = $"{logLevel}: {this.Name}: {message}";

                Writer.WriteLine(line);

                if (exception != null)
                    Writer.WriteLine(exception.ToString());
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return new UnitTestScope();
            }
        }

        public class UnitTestScope : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}