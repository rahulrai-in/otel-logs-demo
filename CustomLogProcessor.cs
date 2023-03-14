using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTelemetry;
using OpenTelemetry.Logs;

public class CustomLogProcessor : BaseProcessor<LogRecord>
{
    public override void OnEnd(LogRecord data)
    {
        // Custom state information
        var logState = new List<KeyValuePair<string, object?>>
        {
            new("ProcessID", Environment.ProcessId),
            new("DotnetFramework", RuntimeInformation.FrameworkDescription),
            new("Runtime", RuntimeInformation.RuntimeIdentifier),
        };
        // Example of masking sensitive data
        if (data.StateValues != null)
        {
            var state = data.StateValues.ToList();

            // Find a key value pair with key "password" and update its value to "masked value"
            var foundPair = state.Find(kvp => kvp.Key.Equals("password", StringComparison.OrdinalIgnoreCase));
            if (!foundPair.Equals(default(KeyValuePair<string, object?>)))
            {
                // Find the index of the original pair in the list
                var index = state.IndexOf(foundPair);

                // Replace the original pair with the updated pair at the same index
                state[index] = new(foundPair.Key, "masked value");
                data.FormattedMessage = "Message masked due to sensitive data";
            }

            data.StateValues = new ReadOnlyCollectionBuilder<KeyValuePair<string, object?>>(state.Concat(logState))
                .ToReadOnlyCollection();
        }

        base.OnEnd(data);
    }
}