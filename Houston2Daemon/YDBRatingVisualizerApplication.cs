using Vostok.Applications.Scheduled;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Requirements;
using Vostok.Logging.Abstractions;

namespace Houston2Daemon;

[RequiresSecretConfiguration(typeof(Secrets))]
public class YDBRatingVisualizerApplication : VostokScheduledApplication
{
    public override void Setup(IScheduledActionsBuilder builder, IVostokHostingEnvironment environment)
    {
        builder.Schedule(
            "Update",
            Scheduler.Periodical(() => 1.Minutes()),
            () => PerformIteration(environment));
    }

    private Task PerformIteration(IVostokHostingEnvironment environment)
    {
        environment.Log.Info("Performed iteration.");
        return Task.CompletedTask;
    }
}
