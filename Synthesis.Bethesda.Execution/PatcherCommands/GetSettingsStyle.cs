﻿using Noggog;
using Serilog;
using Synthesis.Bethesda.Commands;
using Synthesis.Bethesda.DTO;
using Synthesis.Bethesda.Execution.DotNet.Builder;
using Synthesis.Bethesda.Execution.Patchers.Git;
using Synthesis.Bethesda.Execution.Patchers.Running.Git;
using Synthesis.Bethesda.Execution.Settings.Json;
using Synthesis.Bethesda.Execution.Utility;
using Noggog.WorkEngine;

namespace Synthesis.Bethesda.Execution.PatcherCommands;

public interface IGetSettingsStyle
{
    Task<SettingsConfiguration> Get(
        string path,
        bool directExe,
        FilePath? buildMetaPath,
        CancellationToken cancel,
        bool build);
}

public class GetSettingsStyle : IGetSettingsStyle
{
    private readonly ILogger _logger;
    private readonly IWorkDropoff _workDropoff;
    private readonly IBuildMetaFileReader _metaFileReader;
    private readonly IBuild _build;
    private readonly IShortCircuitSettingsProvider _shortCircuitSettingsProvider;
    private readonly IWriteShortCircuitMeta _writeShortCircuitMeta;
    public ILinesToReflectionConfigsParser LinesToConfigsParser { get; }
    public ISynthesisSubProcessRunner ProcessRunner { get; }
    public IRunProcessStartInfoProvider GetRunProcessStartInfoProvider { get; }

    public GetSettingsStyle(
        ILogger logger,
        ISynthesisSubProcessRunner processRunner,
        IWorkDropoff workDropoff,
        IBuildMetaFileReader metaFileReader,
        IBuild build,
        IShortCircuitSettingsProvider shortCircuitSettingsProvider,
        ILinesToReflectionConfigsParser linesToConfigsParser,
        IWriteShortCircuitMeta writeShortCircuitMeta,
        IRunProcessStartInfoProvider getRunProcessStartInfoProvider)
    {
        _logger = logger;
        _workDropoff = workDropoff;
        _metaFileReader = metaFileReader;
        _build = build;
        _shortCircuitSettingsProvider = shortCircuitSettingsProvider;
        _writeShortCircuitMeta = writeShortCircuitMeta;
        LinesToConfigsParser = linesToConfigsParser;
        ProcessRunner = processRunner;
        GetRunProcessStartInfoProvider = getRunProcessStartInfoProvider;
    }
        
    public async Task<SettingsConfiguration> Get(
        string path,
        bool directExe,
        FilePath? buildMetaPath,
        CancellationToken cancel,
        bool build)
    {
        var meta = buildMetaPath != null ? _metaFileReader.Read(buildMetaPath.Value) : default;

        if (_shortCircuitSettingsProvider.Shortcircuit && meta?.SettingsConfiguration != null) return meta.SettingsConfiguration;

        var settingsConfig = await ExecuteSettingsRetrieval(path, directExe, cancel, build);

        if (meta != null
            && buildMetaPath != null)
        {
            meta = meta with
            {
                SettingsConfiguration = settingsConfig
            };
            _writeShortCircuitMeta.WriteMeta(buildMetaPath.Value, meta);
        }

        return settingsConfig;
    }

    private async Task<SettingsConfiguration> ExecuteSettingsRetrieval(string path, bool directExe, CancellationToken cancel, bool build)
    {
        return await _workDropoff.EnqueueAndWait(async () =>
        {
            if (build)
            {
                var buildResult = await _build.Compile(path, cancel);
                if (buildResult.Failed)
                {
                    _logger.Error("Could not build solution patcher in order to query for settings: {Error}", buildResult);
                    return new SettingsConfiguration(SettingsStyle.None, Array.Empty<ReflectionSettingsConfig>());
                }
            }
                
            var result = await ProcessRunner.RunAndCapture(
                GetRunProcessStartInfoProvider.GetStart(path, directExe, new SettingsQuery(), build: false),
                cancel: cancel);
                
            switch ((Codes)result.Result)
            {
                case Codes.OpensForSettings:
                    return new SettingsConfiguration(SettingsStyle.Open, Array.Empty<ReflectionSettingsConfig>());
                case Codes.AutogeneratedSettingsClass:
                    return new SettingsConfiguration(
                        SettingsStyle.SpecifiedClass,
                        LinesToConfigsParser.Parse(result.Out).Configs);
                default:
                    return new SettingsConfiguration(SettingsStyle.None, Array.Empty<ReflectionSettingsConfig>());
            }
        }, cancel).ConfigureAwait(false);
    }
}