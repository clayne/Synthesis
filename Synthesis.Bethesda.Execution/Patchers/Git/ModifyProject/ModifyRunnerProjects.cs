using System.IO.Abstractions;
using System.Xml.Linq;
using Noggog;
using NuGet.Versioning;
using Serilog;
using Synthesis.Bethesda.Execution.Patchers.Solution;
using Synthesis.Bethesda.Execution.Versioning;

namespace Synthesis.Bethesda.Execution.Patchers.Git.ModifyProject;

public interface IModifyRunnerProjects
{
    void Modify(
        FilePath solutionPath,
        string drivingProjSubPath,
        NugetVersionPair versions,
        out NugetVersionPair listedVersions);
}

public class ModifyRunnerProjects : IModifyRunnerProjects
{
    public static readonly System.Version NewtonSoftRemoveMutaVersion = new(0, 28);
    public static readonly System.Version NewtonSoftRemoveSynthVersion = new(0, 17, 5);
    public static readonly System.Version NamespaceMutaVersion = new(0, 30, 0);
    private readonly ILogger _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IAvailableProjectsRetriever _availableProjectsRetriever;
    private readonly ISwapToProperNetVersion _swapToProperNetVersion;
    private readonly IRemoveGitInfo _removeGitInfo;
    private readonly IAddNewtonsoftToOldSetups _addNewtonsoftToOldSetups;
    private readonly ISwapInDesiredVersionsForProjectString _swapDesiredVersions;
    private readonly ITurnOffWindowsSpecificationInTargetFramework _turnOffWindowsSpec;
    private readonly ISwapVersioning _swapVersioning;
    private readonly ITurnOffNullability _turnOffNullability;
    private readonly IProcessProjUsings _processProjUsings;
    private readonly IRemoveProject _removeProject;
    private readonly AddAllReleasesToOldVersions _addAllReleasesToOldVersions;

    public ModifyRunnerProjects(
        ILogger logger,
        IFileSystem fileSystem,
        IAvailableProjectsRetriever availableProjectsRetriever,
        ISwapToProperNetVersion swapToProperNetVersion,
        IRemoveGitInfo removeGitInfo,
        IAddNewtonsoftToOldSetups addNewtonsoftToOldSetups,
        ISwapInDesiredVersionsForProjectString swapDesiredVersions,
        ITurnOffWindowsSpecificationInTargetFramework turnOffWindowsSpec,
        ISwapVersioning swapVersioning,
        ITurnOffNullability turnOffNullability,
        IProcessProjUsings processProjUsings,
        IRemoveProject removeProject,
        AddAllReleasesToOldVersions addAllReleasesToOldVersions)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _availableProjectsRetriever = availableProjectsRetriever;
        _swapToProperNetVersion = swapToProperNetVersion;
        _removeGitInfo = removeGitInfo;
        _addNewtonsoftToOldSetups = addNewtonsoftToOldSetups;
        _swapDesiredVersions = swapDesiredVersions;
        _turnOffWindowsSpec = turnOffWindowsSpec;
        _swapVersioning = swapVersioning;
        _turnOffNullability = turnOffNullability;
        _processProjUsings = processProjUsings;
        _removeProject = removeProject;
        _addAllReleasesToOldVersions = addAllReleasesToOldVersions;
    }
        
    public void Modify(
        FilePath solutionPath,
        string drivingProjSubPath,
        NugetVersionPair versions,
        out NugetVersionPair listedVersions)
    {
        listedVersions = new NugetVersionPair(null, null);

        string? TrimVersion(string? version)
        {
            if (version == null) return null;
            var index = version.IndexOf('-');
            if (index == -1) return version;
            return version.Substring(0, index);
        }
        
        var trimmedMutagenVersion = TrimVersion(versions.Mutagen);
        var trimmedSynthesisVersion = TrimVersion(versions.Synthesis);
        foreach (var subProj in _availableProjectsRetriever.Get(solutionPath))
        {
            var proj = Path.Combine(Path.GetDirectoryName(solutionPath)!, subProj);
            _logger.Information("Modifying {ProjPath}", proj);
            var txt = _fileSystem.File.ReadAllText(proj);
            var projXml = XElement.Parse(txt);
            _swapDesiredVersions.Swap(
                projXml,
                versions,
                out var curListedVersions);
            _turnOffNullability.TurnOff(projXml);
            _removeGitInfo.Remove(projXml);
            _turnOffWindowsSpec.TurnOff(projXml);
            System.Version.TryParse(TrimVersion(curListedVersions.Mutagen), out var mutaVersion);
            System.Version.TryParse(TrimVersion(curListedVersions.Synthesis), out var synthVersion);
            _addNewtonsoftToOldSetups.Add(projXml, mutaVersion, synthVersion);
            System.Version.TryParse(trimmedMutagenVersion, out var targetMutaVersion);
            System.Version.TryParse(trimmedSynthesisVersion, out var targetSynthesisVersion);
            if ((targetMutaVersion != null
                 && targetMutaVersion >= NewtonSoftRemoveMutaVersion)
                || (targetSynthesisVersion != null
                    && targetSynthesisVersion >= NewtonSoftRemoveSynthVersion))
            {
                _removeProject.Remove(projXml, "Newtonsoft.Json");
            }

            if (targetMutaVersion != null)
            {
                _swapToProperNetVersion.Swap(projXml, targetMutaVersion);
            }

            if (targetMutaVersion != null && targetSynthesisVersion != null)
            {
                _addAllReleasesToOldVersions.Add(projXml, synthVersion, targetMutaVersion, targetSynthesisVersion);
            }

            if (targetMutaVersion >= NamespaceMutaVersion
                && mutaVersion < NamespaceMutaVersion)
            {
                _processProjUsings.Process(proj);
                _swapVersioning.Swap(projXml, "Mutagen.Bethesda.FormKeys.SkyrimSE", "2.1", new SemanticVersion(2, 0, 0));
            }

            var outputStr = projXml.ToString();
            if (!txt.Equals(outputStr))
            {
                _fileSystem.File.WriteAllText(proj, outputStr);
            }

            if (drivingProjSubPath.Equals(subProj))
            {
                listedVersions = curListedVersions;
            }
        }
        foreach (var item in Directory.EnumerateFiles(Path.GetDirectoryName(solutionPath)!, "Directory.Build.*"))
        {
            var txt = _fileSystem.File.ReadAllText(item);
            var projXml = XElement.Parse(txt);
            _turnOffNullability.TurnOff(projXml);
            var outputStr = projXml.ToString();
            if (!txt.Equals(outputStr))
            {
                _fileSystem.File.WriteAllText(item, outputStr);
            }
        }
    }
}