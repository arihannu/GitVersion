using Common.Utilities;

namespace Docker.Tasks;

[TaskName(nameof(DockerPublish))]
[TaskDescription("Publish the docker images containing the GitVersion Tool")]
[TaskArgument(Arguments.DockerRegistry, Constants.DockerHub, Constants.GitHub)]
[TaskArgument(Arguments.DockerDotnetVersion, Constants.Version60, Constants.Version70, Constants.Version80)]
[TaskArgument(Arguments.DockerDistro, Constants.AlpineLatest, Constants.DebianLatest, Constants.UbuntuLatest)]
[TaskArgument(Arguments.Architecture, Constants.Amd64, Constants.Arm64)]
[IsDependentOn(typeof(DockerPublishInternal))]
public class DockerPublish : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        var shouldRun = true;
        shouldRun &= context.ShouldRun(context.IsGitHubActionsBuild, $"{nameof(DockerPublish)} works only on GitHub Actions.");
        return shouldRun;
    }
}

[TaskName(nameof(DockerPublishInternal))]
[TaskDescription("Publish the docker images containing the GitVersion Tool")]
[IsDependentOn(typeof(DockerTest))]
public class DockerPublishInternal : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        var shouldRun = true;
        shouldRun &= context.ShouldRun(context.IsGitHubActionsBuild, $"{nameof(DockerPublish)} works only on GitHub Actions.");
        shouldRun &= context.ShouldRun(context.IsDockerOnLinux, $"{nameof(DockerPublish)} works only on Docker on Linux agents.");
        if (context.DockerRegistry == DockerRegistry.GitHub)
        {
            shouldRun &= context.ShouldRun(context.IsInternalPreRelease, $"{nameof(DockerPublish)} to GitHub Package Registry works only for internal releases.");
        }
        if (context.DockerRegistry == DockerRegistry.DockerHub)
        {
            shouldRun &= context.ShouldRun(context.IsStableRelease || context.IsTaggedPreRelease, $"{nameof(DockerPublish)} to DockerHub works only for tagged releases.");
        }

        return shouldRun;
    }

    public override void Run(BuildContext context)
    {
        foreach (var dockerImage in context.Images)
        {
            if (context.SkipImageForDocker(dockerImage)) continue;
            context.DockerPushImage(dockerImage);
        }
    }
}
