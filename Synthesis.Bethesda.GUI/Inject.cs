using System;
using System.Collections.Generic;
using System.Linq;
using Noggog;
using SimpleInjector;
using Synthesis.Bethesda.Execution;

namespace Synthesis.Bethesda.GUI
{
    public class Inject
    {
        private Container _coll = new();
        public readonly static Container Instance;

        static Inject()
        {
            var inject = new Inject();
            inject.Configure();
            Instance = inject._coll;
        }
        
        private void Configure()
        {
            _coll.Register<MainVM>(Lifestyle.Singleton);
            _coll.Register<IProvideInstalledSdk, ProvideInstalledSdk>(Lifestyle.Singleton);
            _coll.Register<IEnvironmentErrorsVM, EnvironmentErrorsVM>(Lifestyle.Singleton);
            _coll.Collection.Register<IEnvironmentErrorVM>(
                typeof(IEnvironmentErrorVM).Assembly.AsEnumerable(), 
                Lifestyle.Singleton);

            RegisterMatchingInterfaces(
                from type in typeof(DotNetCommands).Assembly.GetExportedTypes()
                where type.Namespace!.StartsWith("Synthesis.Bethesda.Execution.DotNet")
                select type,
                Lifestyle.Singleton);
        }

        private void RegisterMatchingInterfaces(IEnumerable<Type> types, Lifestyle lifestyle)
        {
            foreach (var type in types)
            {
                RegisterMatchingInterfaces(type, lifestyle);
            }
        }

        private void RegisterMatchingInterfaces(Type type, Lifestyle lifestyle)
        {
            type.GetInterfaces()
                .Where(i => IsMatchingInterface(i, type))
                .ForEach(i =>
                {
                    _coll.Register(i, type, lifestyle);
                });
        }

        private bool IsMatchingInterface(Type interf, Type concrete)
        {
            return interf.Name == $"I{concrete.Name}";
        }
    }
}