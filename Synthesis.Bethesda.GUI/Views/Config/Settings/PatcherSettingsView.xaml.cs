using Noggog.WPF;
using ReactiveUI;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using Synthesis.Bethesda.GUI.ViewModels.Patchers;

namespace Synthesis.Bethesda.GUI.Views
{
    public class PatcherSettingsViewBase : NoggogUserControl<PatcherSettingsVm> { }

    /// <summary>
    /// Interaction logic for PatcherSettingsView.xaml
    /// </summary>
    public partial class PatcherSettingsView : PatcherSettingsViewBase
    {
        public PatcherSettingsView()
        {
            InitializeComponent();
            this.WhenActivated((disposable) =>
            {
                this.WhenAnyValue(x => x.ViewModel!.SettingsConfiguration)
                    .Select(x => x.Style == SettingsStyle.Open || x.Style == SettingsStyle.Host ? Visibility.Visible : Visibility.Collapsed)
                    .BindToStrict(this, x => x.OpenSettingsButton.Visibility)
                    .DisposeWith(disposable);
                Observable.CombineLatest(
                        this.WhenAnyValue(x => x.ViewModel!.SettingsConfiguration),
                        this.WhenAnyValue(x => x.ViewModel!.ReflectionSettings!.SettingsLoading),
                        (target, loading) => target.Style == SettingsStyle.None && !loading ? Visibility.Visible : Visibility.Collapsed)
                    .BindToStrict(this, x => x.NoSettingsText.Visibility)
                    .DisposeWith(disposable);
                this.WhenAnyValue(x => x.ViewModel!.OpenSettingsCommand)
                    .BindToStrict(this, x => x.OpenSettingsButton.Command)
                    .DisposeWith(disposable);
                this.WhenAnyFallback(x => x.ViewModel!.ReflectionSettings)
                    .BindToStrict(this, x => x.AutogeneratedSettingsView.DataContext)
                    .DisposeWith(disposable);
            });
        }
    }
}
