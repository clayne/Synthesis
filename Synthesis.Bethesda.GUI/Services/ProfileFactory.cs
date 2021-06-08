﻿using System;
using System.Linq;
using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using Mutagen.Bethesda;
using Noggog.WPF;
using Serilog;
using SimpleInjector;
using Synthesis.Bethesda.Execution;
using Synthesis.Bethesda.Execution.CLI;
using Synthesis.Bethesda.Execution.DotNet;
using Synthesis.Bethesda.Execution.GitRespository;
using Synthesis.Bethesda.Execution.Patchers.Git;
using Synthesis.Bethesda.Execution.Settings;
using Synthesis.Bethesda.GUI.Settings;
using Synthesis.Bethesda.GUI.Temporary;

namespace Synthesis.Bethesda.GUI.Services
{
    public interface IProfileFactory
    {
        ProfileVM Get(SynthesisProfile settings);
        ProfileVM Get(GameRelease release, string id, string nickname);
    }

    public class ProfileFactory : IProfileFactory
    {
        public ProfileVM Get(SynthesisProfile settings)
        {
            var profile = Get(settings.TargetRelease, settings.ID, settings.Nickname);
            profile.Versioning.MutagenVersioning = settings.MutagenVersioning;
            profile.Versioning.ManualMutagenVersion = settings.MutagenManualVersion;
            profile.Versioning.SynthesisVersioning = settings.SynthesisVersioning;
            profile.Versioning.ManualSynthesisVersion = settings.SynthesisManualVersion;
            profile.DataFolderOverride.DataPathOverride = settings.DataPathOverride;
            profile.ConsiderPrereleaseNugets = settings.ConsiderPrereleaseNugets;
            profile.LockSetting.Lock = settings.LockToCurrentVersioning;
            profile.SelectedPersistenceMode = settings.Persistence;
            profile.Patchers.AddRange(settings.Patchers.Select<PatcherSettings, PatcherVM>(p =>
            {
                return p switch
                {
                    GithubPatcherSettings git => new GitPatcherVM(
                        profile.Scope.GetInstance<ProfileIdentifier>(),
                        profile.Scope.GetInstance<ProfileDirectories>(),
                        profile.Scope.GetInstance<ProfileLoadOrder>(),
                        profile.Scope.GetInstance<ProfilePatchersList>(),
                        profile.Scope.GetInstance<ProfileVersioning>(),
                        profile.Scope.GetRequiredService<ProfileDataFolder>(),
                        profile.Scope.GetInstance<IRemovePatcherFromProfile>(),
                        profile.Scope.GetRequiredService<INavigateTo>(),
                        profile.Scope.GetRequiredService<ICheckOrCloneRepo>(),
                        profile.Scope.GetRequiredService<IProvideRepositoryCheckouts>(),
                        profile.Scope.GetRequiredService<ICheckoutRunnerRepository>(),
                        profile.Scope.GetRequiredService<ICheckRunnability>(),
                        profile.Scope.GetInstance<IProfileDisplayControllerVm>(),
                        profile.Scope.GetInstance<IConfirmationPanelControllerVm>(),
                        profile.Scope.GetInstance<ILockToCurrentVersioning>(),
                        profile.Scope.GetRequiredService<IBuild>(),
                        git),
                    SolutionPatcherSettings soln => new SolutionPatcherVM(
                        profile.Scope.GetInstance<ProfileLoadOrder>(),
                        profile.Scope.GetInstance<IRemovePatcherFromProfile>(),
                        profile.Scope.GetInstance<IProvideInstalledSdk>(),
                        profile.Scope.GetInstance<IProfileDisplayControllerVm>(),
                        profile.Scope.GetInstance<IConfirmationPanelControllerVm>(),
                        soln),
                    CliPatcherSettings cli => new CliPatcherVM(
                        profile.Scope.GetInstance<IRemovePatcherFromProfile>(),
                        profile.Scope.GetInstance<IProfileDisplayControllerVm>(),
                        profile.Scope.GetInstance<IConfirmationPanelControllerVm>(),
                        profile.Scope.GetInstance<IShowHelpSetting>(),
                        cli),
                    _ => throw new NotImplementedException(),
                };
            }));
            return profile;
        }

        public ProfileVM Get(GameRelease release, string id, string nickname)
        {
            var scope = new Scope(Inject.Container);
            var ident = scope.GetInstance<ProfileIdentifier>();
            ident.ID = id;
            ident.Release = release;
            ident.Nickname = nickname;
            var profile = new ProfileVM(
                scope, 
                scope.GetInstance<ProfilePatchersList>(),
                scope.GetInstance<ProfileDataFolder>(),
                scope.GetInstance<PatcherInitializationVM>(),
                ident,
                scope.GetInstance<ProfileLoadOrder>(),
                scope.GetInstance<ProfileDirectories>(),
                scope.GetInstance<ProfileVersioning>(),
                scope.GetInstance<INavigateTo>(),
                scope.GetInstance<ILogger>());
            scope.DisposeWith(profile);
            return profile;
        }
    }
}