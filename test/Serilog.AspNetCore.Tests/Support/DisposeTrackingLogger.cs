using System;
using System.Collections.Generic;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.AspNetCore.Tests.Support
{
    public class DisposeTrackingLogger : ILogger, IDisposable
    {
        public bool IsDisposed { get; set; }

        public ILogger ForContext(ILogEventEnricher enricher)
        {
            return new LoggerConfiguration().CreateLogger();
        }

        public ILogger ForContext(IEnumerable<ILogEventEnricher> enrichers)
        {
            return new LoggerConfiguration().CreateLogger();
        }

        public ILogger ForContext(string propertyName, object value, bool destructureObjects = false)
        {
            return new LoggerConfiguration().CreateLogger();
        }

        public ILogger ForContext<TSource>()
        {
            return new LoggerConfiguration().CreateLogger();
        }

        public ILogger ForContext(Type source)
        {
            return new LoggerConfiguration().CreateLogger();
        }

        public void Write(LogEvent logEvent)
        {
        }

        public void Write(LogEventLevel level, string messageTemplate)
        {
        }

        public void Write<T>(LogEventLevel level, string messageTemplate, T propertyValue)
        {
        }

        public void Write<T0, T1>(LogEventLevel level, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
        }

        public void Write<T0, T1, T2>(LogEventLevel level, string messageTemplate, T0 propertyValue0, T1 propertyValue1,
            T2 propertyValue2)
        {
        }

        public void Write(LogEventLevel level, string messageTemplate, params object[] propertyValues)
        {
        }

        public void Write(LogEventLevel level, Exception exception, string messageTemplate)
        {
        }

        public void Write<T>(LogEventLevel level, Exception exception, string messageTemplate, T propertyValue)
        {
        }

        public void Write<T0, T1>(LogEventLevel level, Exception exception, string messageTemplate, T0 propertyValue0,
            T1 propertyValue1)
        {
        }

        public void Write<T0, T1, T2>(LogEventLevel level, Exception exception, string messageTemplate, T0 propertyValue0,
            T1 propertyValue1, T2 propertyValue2)
        {
        }

        public void Write(LogEventLevel level, Exception exception, string messageTemplate, params object[] propertyValues)
        {
        }

        public bool IsEnabled(LogEventLevel level)
        {
            return false;
        }

        public void Verbose(string messageTemplate)
        {
        }

        public void Verbose<T>(string messageTemplate, T propertyValue)
        {
        }

        public void Verbose<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
        }

        public void Verbose<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
        }

        public void Verbose(string messageTemplate, params object[] propertyValues)
        {
        }

        public void Verbose(Exception exception, string messageTemplate)
        {
        }

        public void Verbose<T>(Exception exception, string messageTemplate, T propertyValue)
        {
        }

        public void Verbose<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
        }

        public void Verbose<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1,
            T2 propertyValue2)
        {
        }

        public void Verbose(Exception exception, string messageTemplate, params object[] propertyValues)
        {
        }

        public void Debug(string messageTemplate)
        {
        }

        public void Debug<T>(string messageTemplate, T propertyValue)
        {
        }

        public void Debug<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
        }

        public void Debug<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
        }

        public void Debug(string messageTemplate, params object[] propertyValues)
        {
        }

        public void Debug(Exception exception, string messageTemplate)
        {
        }

        public void Debug<T>(Exception exception, string messageTemplate, T propertyValue)
        {
        }

        public void Debug<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
        }

        public void Debug<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1,
            T2 propertyValue2)
        {
        }

        public void Debug(Exception exception, string messageTemplate, params object[] propertyValues)
        {
        }

        public void Information(string messageTemplate)
        {
        }

        public void Information<T>(string messageTemplate, T propertyValue)
        {
        }

        public void Information<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
        }

        public void Information<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
        }

        public void Information(string messageTemplate, params object[] propertyValues)
        {
        }

        public void Information(Exception exception, string messageTemplate)
        {
        }

        public void Information<T>(Exception exception, string messageTemplate, T propertyValue)
        {
        }

        public void Information<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
        }

        public void Information<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1,
            T2 propertyValue2)
        {
        }

        public void Information(Exception exception, string messageTemplate, params object[] propertyValues)
        {
        }

        public void Warning(string messageTemplate)
        {
        }

        public void Warning<T>(string messageTemplate, T propertyValue)
        {
        }

        public void Warning<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
        }

        public void Warning<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
        }

        public void Warning(string messageTemplate, params object[] propertyValues)
        {
        }

        public void Warning(Exception exception, string messageTemplate)
        {
        }

        public void Warning<T>(Exception exception, string messageTemplate, T propertyValue)
        {
        }

        public void Warning<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
        }

        public void Warning<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1,
            T2 propertyValue2)
        {
        }

        public void Warning(Exception exception, string messageTemplate, params object[] propertyValues)
        {
        }

        public void Error(string messageTemplate)
        {
        }

        public void Error<T>(string messageTemplate, T propertyValue)
        {
        }

        public void Error<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
        }

        public void Error<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
        }

        public void Error(string messageTemplate, params object[] propertyValues)
        {
        }

        public void Error(Exception exception, string messageTemplate)
        {
        }

        public void Error<T>(Exception exception, string messageTemplate, T propertyValue)
        {
        }

        public void Error<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
        }

        public void Error<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1,
            T2 propertyValue2)
        {
        }

        public void Error(Exception exception, string messageTemplate, params object[] propertyValues)
        {
        }

        public void Fatal(string messageTemplate)
        {
        }

        public void Fatal<T>(string messageTemplate, T propertyValue)
        {
        }

        public void Fatal<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
        }

        public void Fatal<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
        }

        public void Fatal(string messageTemplate, params object[] propertyValues)
        {
        }

        public void Fatal(Exception exception, string messageTemplate)
        {
        }

        public void Fatal<T>(Exception exception, string messageTemplate, T propertyValue)
        {
        }

        public void Fatal<T0, T1>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
        }

        public void Fatal<T0, T1, T2>(Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1,
            T2 propertyValue2)
        {
        }

        public void Fatal(Exception exception, string messageTemplate, params object[] propertyValues)
        {
        }

        public bool BindMessageTemplate(string messageTemplate, object[] propertyValues, out MessageTemplate parsedTemplate,
            out IEnumerable<LogEventProperty> boundProperties)
        {
            parsedTemplate = null;
            boundProperties = null;
            return false;
        }

        public bool BindProperty(string propertyName, object value, bool destructureObjects, out LogEventProperty property)
        {
            property = null;
            return false;
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
