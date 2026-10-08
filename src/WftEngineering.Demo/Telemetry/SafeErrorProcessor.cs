using System.Diagnostics;
using OpenTelemetry;

namespace WftEngineering.Demo;

internal sealed class SafeErrorProcessor : BaseProcessor<Activity>
{
    public override void OnEnd(Activity activity)
    {
        // Framework instrumentation can set raw exception messages even with sensitive capture disabled.
        if (activity.Status == ActivityStatusCode.Error)
        {
            activity.SetStatus(ActivityStatusCode.Error);
        }
    }
}
