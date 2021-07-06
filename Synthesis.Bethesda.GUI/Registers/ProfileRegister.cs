﻿using StructureMap;
using Synthesis.Bethesda.GUI.Profiles.Plugins;
using Synthesis.Bethesda.GUI.Services.Profile;

namespace Synthesis.Bethesda.GUI.Registers
{
    public class ProfileRegister : Registry
    {
        public ProfileRegister()
        {
            ForSingletonOf<ProfilePatchersList>();
            Forward<ProfilePatchersList, IRemovePatcherFromProfile>();
            Forward<ProfilePatchersList, IProfilePatchersList>();
            ForSingletonOf<IProfileLoadOrder>().Use<ProfileLoadOrder>();
            ForSingletonOf<IProfileDirectories>().Use<ProfileDirectories>();
            ForSingletonOf<IProfileDataFolder>().Use<ProfileDataFolder>();
            ForSingletonOf<IProfileVersioning>().Use<ProfileVersioning>();
            ForSingletonOf<IProfileSimpleLinkCache>().Use<ProfileSimpleLinkCache>();
            
            Scan(s =>
            {
                s.AssemblyContainingType<IPatcherFactory>();
                s.IncludeNamespaceContainingType<IPatcherFactory>();
                s.WithDefaultConventions();
            });
        }
    }
}