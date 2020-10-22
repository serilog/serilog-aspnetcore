using System.Collections.Generic;
using Serilog.Events;

namespace Serilog.AspNetCore.Tests.Support
{
    public static class Extensions
    {
        public static object LiteralValue(this LogEventPropertyValue @this)
        {
            return ((ScalarValue)@this).Value;
        }
        public static IReadOnlyDictionary<ScalarValue, LogEventPropertyValue> DictionaryValue(this LogEventPropertyValue @this)
        {
            return ((DictionaryValue)@this).Elements;
        }
    }
}
