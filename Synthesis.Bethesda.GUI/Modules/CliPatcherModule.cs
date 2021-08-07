﻿using Autofac;
using Noggog.Autofac;
using Synthesis.Bethesda.Execution.Patchers.TopLevel;
using Synthesis.Bethesda.GUI.ViewModels.Patchers.Cli;

namespace Synthesis.Bethesda.GUI.Modules
{
    public class CliPatcherModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<CliModule>();
            
            builder.RegisterAssemblyTypes(typeof(CliPatcherVm).Assembly)
                .InNamespacesOf(typeof(CliPatcherVm))
                .SingleInstance()
                .NotInjection()
                .AsImplementedInterfaces()
                .AsSelf();
            
            builder.RegisterType<PatcherLogDecorator>()
                .AsImplementedInterfaces()
                .SingleInstance();
            
            base.Load(builder);
        }
    }
}