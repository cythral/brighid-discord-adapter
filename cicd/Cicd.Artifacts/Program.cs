using Amazon.CDK;

using Brighid.Discord.Adapter.Artifacts;

#pragma warning disable SA1516

var app = new App();
_ = new ArtifactsStack(app, "ArtifactsStack", new StackProps
{
    Synthesizer = new BootstraplessSynthesizer(new BootstraplessSynthesizerProps()),
});

app.Synth();
