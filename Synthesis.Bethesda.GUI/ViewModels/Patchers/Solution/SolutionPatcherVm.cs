using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Autofac;
using DynamicData;
using DynamicData.Binding;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.WPF.Plugins;
using Noggog;
using Noggog.WPF;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Synthesis.Bethesda.DTO;
using Synthesis.Bethesda.Execution.DotNet;
using Synthesis.Bethesda.Execution.Patchers.Common;
using Synthesis.Bethesda.Execution.Patchers.Git;
using Synthesis.Bethesda.Execution.Patchers.Solution;
using Synthesis.Bethesda.Execution.Settings;
using Synthesis.Bethesda.GUI.Services.Main;
using Synthesis.Bethesda.GUI.Services.Patchers.Solution;
using Synthesis.Bethesda.GUI.ViewModels.Patchers.TopLevel;
using Synthesis.Bethesda.GUI.ViewModels.Profiles;
using Synthesis.Bethesda.GUI.ViewModels.Profiles.Plugins;
using Synthesis.Bethesda.GUI.ViewModels.Top;

namespace Synthesis.Bethesda.GUI.ViewModels.Patchers.Solution
{
    public class SolutionPatcherVm : PatcherVm, 
        IProvidePatcherMetaPath, 
        IPathToProjProvider,
        ISolutionPatcherSettingsVm
    {
        public ISolutionPathInputVm SolutionPathInput { get; }
        public ISelectedProjectInputVm SelectedProjectInput { get; }
        private readonly IProfileLoadOrder _LoadOrder;

        public IObservableCollection<string> AvailableProjects { get; }

        private readonly ObservableAsPropertyHelper<ConfigurationState> _State;
        public override ConfigurationState State => _State?.Value ?? ConfigurationState.Success;

        public ICommand OpenSolutionCommand { get; }

        [Reactive]
        public string ShortDescription { get; set; } = string.Empty;

        [Reactive]
        public string LongDescription { get; set; } = string.Empty;

        [Reactive]
        public VisibilityOptions Visibility { get; set; } = DTO.VisibilityOptions.Visible;

        [Reactive]
        public PreferredAutoVersioning Versioning { get; set; }

        public ObservableCollectionExtended<PreferredAutoVersioning> VersioningOptions { get; } = new(EnumExt.GetValues<PreferredAutoVersioning>());

        public ObservableCollectionExtended<VisibilityOptions> VisibilityOptions { get; } = new(EnumExt.GetValues<VisibilityOptions>());

        public ObservableCollection<ModKeyItemViewModel> RequiredMods { get; } = new();
         
        public IObservable<IChangeSet<ModKey>> DetectedMods => _LoadOrder.LoadOrder.Connect().Transform(l => l.ModKey);

        public PatcherSettingsVm PatcherSettings { get; }

        public ReactiveCommand<Unit, Unit> ReloadAutogeneratedSettingsCommand { get; }
        
        public IObservable<string> MetaPath { get; }
        IObservable<string> IProvidePatcherMetaPath.Path => MetaPath;

        IObservable<IChangeSet<ModKey>> ISolutionPatcherSettingsVm.RequiredMods => RequiredMods
            .AsObservableChangeSet()
            .Transform(x => x.ModKey);

        public SolutionPatcherVm(
            ILifetimeScope scope,
            IPatcherNameVm nameVm,
            IProfileLoadOrder loadOrder,
            IRemovePatcherFromProfile remove,
            IInstalledSdkFollower dotNetSdkFollowerInstalled,
            IProfileDisplayControllerVm profileDisplay,
            IConfirmationPanelControllerVm confirmation, 
            ISolutionPathInputVm solutionPathInput,
            ISelectedProjectInputVm selectedProjectInput,
            PatcherSettingsVm.Factory settingsVmFactory,
            IAvailableProjectsFollower availableProjectsFollower,
            ISolutionMetaFileSync metaFileSync,
            INavigateTo navigateTo,
            IPatcherIdProvider idProvider,
            SolutionPatcherSettings? settings = null)
            : base(scope, nameVm, remove, profileDisplay, confirmation, idProvider, settings)
        {
            SolutionPathInput = solutionPathInput;
            SelectedProjectInput = selectedProjectInput;
            _LoadOrder = loadOrder;
            CopyInSettings(settings);

            AvailableProjects = availableProjectsFollower.Process(
                this.WhenAnyValue(x => x.SolutionPathInput.Picker.TargetPath).Select(x => new FilePath(x)))
                .ObserveOnGui()
                .ToObservableCollection(this);

            _State = Observable.CombineLatest(
                    this.WhenAnyValue(x => x.SolutionPathInput.Picker.ErrorState),
                    SelectedProjectInput.WhenAnyValue(x => x.Picker.ErrorState),
                    dotNetSdkFollowerInstalled.DotNetSdkInstalled,
                    (sln, proj, dotnet) =>
                    {
                        if (sln.Failed) return new ConfigurationState(sln);
                        if (!dotnet.Acceptable) return new ConfigurationState(ErrorResponse.Fail("No dotnet SDK installed"));
                        return new ConfigurationState(proj);
                    })
                .ToGuiProperty<ConfigurationState>(this, nameof(State), new ConfigurationState(ErrorResponse.Fail("Evaluating"))
                {
                    IsHaltingError = false
                });

            OpenSolutionCommand = ReactiveCommand.Create(
                canExecute: this.WhenAnyValue(x => x.SolutionPathInput.Picker.InError)
                    .Select(x => !x),
                execute: () =>
                {
                    navigateTo.Navigate(SolutionPathInput.Picker.TargetPath);
                });

            MetaPath = SelectedProjectInput.WhenAnyValue(x => x.Picker.TargetPath)
                .Select(projPath =>
                {
                    try
                    {
                        return Path.Combine(Path.GetDirectoryName(projPath)!, Constants.MetaFileName);
                    }
                    catch (Exception)
                    {
                        return string.Empty;
                    }
                })
                .Replay(1)
                .RefCount();

            metaFileSync.Sync()
                .DisposeWith(this);

            ReloadAutogeneratedSettingsCommand = ReactiveCommand.Create(() => { });
            PatcherSettings = settingsVmFactory(true, 
                SelectedProjectInput.Picker.WhenAnyValue(x => x.TargetPath)
                    .Merge(ReloadAutogeneratedSettingsCommand.EndingExecution()
                        .WithLatestFrom(SelectedProjectInput.Picker.WhenAnyValue(x => x.TargetPath), (_, p) => p))
                    .Select(p => (GetResponse<FilePath>.Succeed(p), default(string?))))
                .DisposeWith(this);
        }

        public override PatcherSettings Save()
        {
            var ret = new SolutionPatcherSettings();
            CopyOverSave(ret);
            ret.SolutionPath = this.SolutionPathInput.Picker.TargetPath;
            ret.ProjectSubpath = this.SelectedProjectInput.ProjectSubpath;
            PatcherSettings.Persist();
            return ret;
        }

        private void CopyInSettings(SolutionPatcherSettings? settings)
        {
            if (settings == null) return;
            this.SolutionPathInput.Picker.TargetPath = settings.SolutionPath;
            this.SelectedProjectInput.ProjectSubpath = settings.ProjectSubpath;
        }

        public override void PrepForRun()
        {
            base.PrepForRun();
            PatcherSettings.Persist();
        }

        public void SetRequiredMods(IEnumerable<ModKey> modKeys)
        {
            RequiredMods.SetTo(modKeys
                .Select(m => new ModKeyItemViewModel(m)));
        }

        FilePath IPathToProjProvider.Path => SelectedProjectInput.Picker.TargetPath;
    }
}
