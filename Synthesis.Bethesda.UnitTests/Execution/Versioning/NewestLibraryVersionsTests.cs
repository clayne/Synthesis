﻿namespace Synthesis.Bethesda.UnitTests.Execution.Versioning;

public class NewestLibraryVersionsTests
{
    // [Theory, SynthAutoData]
    // public void DotNetNotAcceptable(NewestLibraryVersions sut)
    // {
    //     sut.InstalledSdkFollower.DotNetSdkInstalled.Returns(_ => Observable.Return(new DotNetVersion(string.Empty, Acceptable: false)));
    //     sut.NewestSynthesisVersion.Subscribe();
    //     sut.NewestSynthesisVersion.Subscribe(x =>
    //     {
    //         x.Should().BeNull();
    //     });
    //     sut.NewestMutagenVersion.Subscribe();
    //     sut.NewestMutagenVersion.Subscribe(x =>
    //     {
    //         x.Should().BeNull();
    //     });
    // }
    //
    // [Theory, SynthAutoData]
    // public void ConsiderPrereleases(NewestLibraryVersions sut)
    // {
    //     sut.InstalledSdkFollower.DotNetSdkInstalled.Returns(_ => Observable.Return(new DotNetVersion(string.Empty, Acceptable: true)));
    //     sut.ConsiderPrerelease.ConsiderPrereleases.Returns(_ => Observable.Return(true));
    //     sut.QueryNewest.GetLatestVersions(includePrerelease: true, Arg.Any<FilePath>())
    //         .Returns(_ => ("PrereleaseMutagenVersion", "PrereleaseSynthesisVersion"));
    //     sut.QueryNewest.GetLatestVersions(includePrerelease: false, Arg.Any<FilePath>())
    //         .Returns(_ => ("MutagenVersion", "SynthesisVersion"));
    //     
    //     sut.NewestSynthesisVersion.Subscribe();
    //     sut.NewestSynthesisVersion.Subscribe(x =>
    //     {
    //         x.Should().Be("PrereleaseSynthesisVersion");
    //     });
    //     sut.NewestMutagenVersion.Subscribe();
    //     sut.NewestMutagenVersion.Subscribe(x =>
    //     {
    //         x.Should().Be("PrereleaseMutagenVersion");
    //     });
    // }
    //
    // [Theory, SynthAutoData]
    // public void DoNotConsiderPrereleases(NewestLibraryVersions sut)
    // {
    //     sut.InstalledSdkFollower.DotNetSdkInstalled.Returns(_ => Observable.Return(new DotNetVersion(string.Empty, Acceptable: true)));
    //     sut.ConsiderPrerelease.ConsiderPrereleases.Returns(_ => Observable.Return(false));
    //     sut.QueryNewest.GetLatestVersions(includePrerelease: true, Arg.Any<FilePath>())
    //         .Returns(_ => ("PrereleaseMutagenVersion", "PrereleaseSynthesisVersion"));
    //     sut.QueryNewest.GetLatestVersions(includePrerelease: false, Arg.Any<FilePath>())
    //         .Returns(_ => ("MutagenVersion", "SynthesisVersion"));
    //     
    //     sut.NewestSynthesisVersion.Subscribe();
    //     sut.NewestSynthesisVersion.Subscribe(x =>
    //     {
    //         x.Should().Be("SynthesisVersion");
    //     });
    //     sut.NewestMutagenVersion.Subscribe();
    //     sut.NewestMutagenVersion.Subscribe(x =>
    //     {
    //         x.Should().Be("MutagenVersion");
    //     });
    // }
}