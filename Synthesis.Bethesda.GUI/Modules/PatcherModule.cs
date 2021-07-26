﻿using Autofac;
using Noggog.Autofac;
using Synthesis.Bethesda.Execution.Patchers.Running;
using Synthesis.Bethesda.Execution.Patchers.TopLevel;
using Synthesis.Bethesda.GUI.ViewModels.Patchers.Initialization;
using Synthesis.Bethesda.GUI.ViewModels.Patchers.TopLevel;

namespace Synthesis.Bethesda.GUI.Modules
{
    public class PatcherModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(PatcherVm).Assembly)
                .InNamespacesOf(
                    typeof(PatcherVm),
                    typeof(PatcherInitVm))
                .NotInjection()
                .InstancePerMatchingLifetimeScope(MainModule.PatcherNickname)
                .AsImplementedInterfaces()
                .AsSelf();
            builder.RegisterAssemblyTypes(typeof(IPatcherNameProvider).Assembly)
                .InNamespacesOf(
                    typeof(IPatcherNameProvider))
                .NotInjection()
                .InstancePerMatchingLifetimeScope(MainModule.PatcherNickname)
                .AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(typeof(IPatcherNameProvider).Assembly)
                .InNamespacesOf(
                    typeof(IPatcherRun))
                .AsImplementedInterfaces();
        }
    }
}