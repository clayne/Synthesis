using DynamicData;
using Noggog;
using Noggog.WPF;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Synthesis.Bethesda;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Order;

namespace Mutagen.Bethesda.Synthesis.WPF
{
    public class AutogeneratedSettingsVm : ViewModel
    {
        private readonly ObservableAsPropertyHelper<bool> _SettingsLoading;
        public bool SettingsLoading => _SettingsLoading.Value;

        private readonly ObservableAsPropertyHelper<ErrorResponse> _Status;
        public ErrorResponse Error => _Status.Value;

        [Reactive]
        public ReflectionSettingsVM? SelectedSettings { get; set; }

        private readonly ObservableAsPropertyHelper<ReflectionSettingsBundleVm?> _Bundle;
        public ReflectionSettingsBundleVm? Bundle => _Bundle.Value;

        public AutogeneratedSettingsVm(
            SettingsConfiguration config,
            string projPath,
            IObservable<IChangeSet<IModListingGetter>> loadOrder,
            IObservable<ILinkCache?> linkCache,
            IProvideReflectionSettingsBundle provideBundle)
        {
            var targetSettingsVm = Observable.Return(Unit.Default)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Select(_ =>
                {
                    return Observable.Create<(bool Processing, GetResponse<ReflectionSettingsBundleVm> SettingsVM)>(async (observer, cancel) =>
                    {
                        try
                        {
                            observer.OnNext((true, GetResponse<ReflectionSettingsBundleVm>.Fail("Loading")));
                            var reflectionBundle = await provideBundle.ExtractBundle(
                                projPath,
                                targets: config.Targets,
                                detectedLoadOrder: loadOrder,
                                linkCache: linkCache,
                                cancel: cancel);
                            if (reflectionBundle.Failed)
                            {
                                observer.OnNext((false, reflectionBundle));
                                return;
                            }
                            observer.OnNext((false, reflectionBundle.Value));
                        }
                        catch (Exception ex)
                        {
                            observer.OnNext((false, GetResponse<ReflectionSettingsBundleVm>.Fail(ex)));
                        }
                        observer.OnCompleted();
                    });
                })
                .Switch()
                .DisposePrevious()
                .Replay(1)
                .RefCount();

            _SettingsLoading = targetSettingsVm
                .Select(t => t.Processing)
                .ToGuiProperty(this, nameof(SettingsLoading), deferSubscription: true);

            _Bundle = targetSettingsVm
                .Select(x =>
                {
                    if (x.Processing || x.SettingsVM.Failed)
                    {
                        return new ReflectionSettingsBundleVm();
                    }
                    return x.SettingsVM.Value;
                })
                .ObserveOnGui()
                .Select(x =>
                {
                    SelectedSettings = x.Settings?.FirstOrDefault();
                    return x;
                })
                .DisposePrevious()
                .ToGuiProperty<ReflectionSettingsBundleVm?>(this, nameof(Bundle), initialValue: null, deferSubscription: true);

            _Status = targetSettingsVm
                .Select(x => (ErrorResponse)x.SettingsVM)
                .ToGuiProperty(this, nameof(Error), ErrorResponse.Success, deferSubscription: true);
        }
    }
}
