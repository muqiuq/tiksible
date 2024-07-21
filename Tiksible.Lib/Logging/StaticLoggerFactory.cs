using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Lib.Logging
{
    public static class StaticLoggerFactory
    {
        private static ILoggerFactory _loggerFactory;

        private static ConcurrentDictionary<Type, ILogger> loggerByType = new();

        public static LogLevel GlobalLogLevel = LogLevel.Information;

        public static void Initialize(ILoggerFactory loggerFactory)
        {
            if (_loggerFactory is not null)
                throw new InvalidOperationException("StaticLogger already initialized!");

            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            loggerByType.Clear();
        }

        public static ILogger<T> GetStaticLogger<T>()
        {
            var currentFactory = _loggerFactory;
            if (currentFactory is null)
            {
                currentFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                    builder.AddFilter((provider, category, logLevel) =>
                    {
                        return logLevel >= GlobalLogLevel;
                    });
                });

            }
            return (ILogger<T>) loggerByType.GetOrAdd(typeof(T), currentFactory.CreateLogger<T>());
        }
    }
}
