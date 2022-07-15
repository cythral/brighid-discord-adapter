using System;
using System.Diagnostics.Tracing;

#pragma warning disable SA1600, IDE0044

namespace Brighid.Discord.Adapter
{
    public class NetEventLogListener : EventListener
    {
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            Console.WriteLine(eventSource.Name);
            if (eventSource.Name == "Private.InternalDiagnostics.System.Net.Http")
            {
                // initialize a string, string dictionary of arguments to pass to the EventSource.
                // Turn on loggers matching App* to Information, everything else (*) is the default level (which is EventLevel.Error)
                // Set the default level (verbosity) to Error, and only ask for the formatted messages in this case.
                EnableEvents(eventSource, EventLevel.LogAlways);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            Console.WriteLine("Logger {0}: {1}", eventData.Payload![2], eventData.Payload[4]);
        }
    }
}
