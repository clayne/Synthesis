using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Synthesis.Versioning;
using Mutagen.Bethesda.Synthesis.WPF;
using Noggog;
using SimpleInjector;
using Synthesis.Bethesda.Execution.GitRespository;
using Synthesis.Bethesda.Execution.Versioning;
using Synthesis.Bethesda.GUI.Services;
using Synthesis.Bethesda.GUI.Settings;
using Synthesis.Bethesda.GUI.Temporary;

namespace Synthesis.Bethesda.GUI
{
    public class Inject
    {
        private Container _coll = new();
        public readonly static Scope Scope;
        public readonly static Container Container;

        static Inject()
        {
            var inject = new Inject();
            inject.Configure();
            Container = inject._coll;
            Scope = new Scope(Container);
        }
        
        private void Configure()
        {
            #if DEBUG
            _coll.Options.EnableAutoVerification = true;
            #else
            _coll.Options.EnableAutoVerification = false;
            #endif
            
            _coll.Options.DefaultLifestyle = Lifestyle.Scoped;
            _coll.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            RegisterBaseLib();
            
            RegisterCurrentLib();

            RegisterWpfLib();
            
            RegisterExecutionLib();
        }

        private void RegisterCurrentLib()
        {
            _coll.Register<MainVM>();
            _coll.Register<ConfigurationVM>();
            _coll.Register<CliPatcherInitVM>();
            _coll.Register<PatcherInitializationVM>(Lifestyle.Singleton);
            _coll.RegisterInstance(Log.Logger);
            _coll.Collection.Register<IEnvironmentErrorVM>(
                typeof(IEnvironmentErrorVM).Assembly.AsEnumerable());
            
            _coll.Register<ILockToCurrentVersioning, LockToCurrentVersioning>();
            _coll.Register<IProfileDisplayControllerVm, ProfileDisplayControllerVm>();
            _coll.Register<IEnvironmentErrorsVM, EnvironmentErrorsVM>();
            _coll.Register<IRemovePatcherFromProfile, ProfilePatchersList>();
            _coll.Register<ProfilePatchersList>();
            _coll.Register<ProfileIdentifier>();
            _coll.Register<ProfileLoadOrder>();
            _coll.Register<ProfileDirectories>();
            _coll.Register<ProfileDataFolder>();
            _coll.Register<ProfileVersioning>();
            _coll.Register<ProfileSimpleLinkCache>();
            
            RegisterNamespaceFromType(typeof(INavigateTo), Lifestyle.Singleton);
            _coll.Register<ISettingsSingleton, SettingsSingleton>(Lifestyle.Singleton);
            _coll.Register<IShowHelpSetting, ShowHelpSetting>(Lifestyle.Singleton);
            _coll.Register<IConsiderPrereleasePreference, ConsiderPrereleasePreference>(Lifestyle.Singleton);
            _coll.Register<IRetrieveSaveSettings, RetrieveSaveSettings>(Lifestyle.Singleton);
            _coll.Register<IConfirmationPanelControllerVm, ConfirmationPanelControllerVm>(Lifestyle.Singleton);
            _coll.Register<ISelectedProfileControllerVm, SelectedProfileControllerVm>(Lifestyle.Singleton);
            _coll.Register<IActivePanelControllerVm, ActivePanelControllerVm>(Lifestyle.Singleton);
            _coll.Register<ISaveSignal, RetrieveSaveSettings>(Lifestyle.Singleton);
        }

        private void RegisterBaseLib()
        {
            RegisterNamespaceFromType(typeof(IProvideCurrentVersions));
        }

        private void RegisterWpfLib()
        {
            RegisterNamespaceFromType(typeof(IProvideAutogeneratedSettings));
        }

        private void RegisterExecutionLib()
        {
            _coll.Register<IProvideRepositoryCheckouts, ProvideRepositoryCheckouts>(Lifestyle.Singleton);

            RegisterMatchingInterfaces(
                from type in typeof(ICheckOrCloneRepo).Assembly.GetExportedTypes()
                where type != typeof(ProvideRepositoryCheckouts)
                select type);
        }

        private void RegisterNamespaceFromType(Type type, Lifestyle? lifestyle = null)
        {
            RegisterMatchingInterfaces(
                from t in type.Assembly.GetExportedTypes()
                where t.Namespace!.StartsWith(type.Namespace!)
                select t,
                lifestyle);
        }

        private void RegisterMatchingInterfaces(IEnumerable<Type> types, Lifestyle? lifestyle = null)
        {
            foreach (var type in types)
            {
                RegisterMatchingInterfaces(type, lifestyle);
            }
        }

        private void RegisterMatchingInterfaces(Type type, Lifestyle? lifestyle = null)
        {
            if (type.IsGenericType) return;
            type.GetInterfaces()
                .Where(i => IsMatchingInterface(i, type))
                .ForEach(i =>
                {
                    if (lifestyle == null)
                    {
                        _coll.Register(i, type);
                    }
                    else
                    {
                        _coll.Register(i, type, lifestyle);
                    }
                });
        }

        private bool IsMatchingInterface(Type interf, Type concrete)
        {
            return interf.Name == $"I{concrete.Name}";
        }
    }
}