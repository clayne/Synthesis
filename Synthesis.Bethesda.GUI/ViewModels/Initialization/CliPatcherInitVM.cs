using Noggog;
using Noggog.WPF;
using ReactiveUI;
using System.Collections.Generic;
using Synthesis.Bethesda.GUI.Settings;
using Synthesis.Bethesda.GUI.Temporary;

namespace Synthesis.Bethesda.GUI
{
    public class CliPatcherInitVM : PatcherInitVM
    {
        private readonly ObservableAsPropertyHelper<ErrorResponse> _CanCompleteConfiguration;
        public override ErrorResponse CanCompleteConfiguration => _CanCompleteConfiguration.Value;

        public CliPatcherVM Patcher { get; }

        public CliPatcherInitVM(
            PatcherInitializationVM init, 
            IProfileDisplayControllerVm profileDisplay,
            IConfirmationPanelControllerVm confirmation,
            IRemovePatcherFromProfile remove,
            IShowHelpSetting showHelp)
            : base(init)
        {
            Patcher = new CliPatcherVM(
                remove,
                profileDisplay,
                confirmation,
                showHelp);
            _CanCompleteConfiguration = Patcher.WhenAnyValue(x => x.PathToExecutable.ErrorState)
                .Cast<ErrorResponse, ErrorResponse>()
                .ToGuiProperty(this, nameof(CanCompleteConfiguration), ErrorResponse.Success);
        }

        public override async IAsyncEnumerable<PatcherVM> Construct()
        {
            yield return Patcher;
        }
    }
}
