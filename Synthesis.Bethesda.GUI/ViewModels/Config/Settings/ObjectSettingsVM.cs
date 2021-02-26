using Newtonsoft.Json.Linq;
using Noggog;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;

namespace Synthesis.Bethesda.GUI
{
    public class ObjectSettingsVM : SettingsNodeVM
    {
        private readonly Dictionary<string, SettingsNodeVM> _nodes;
        public ObservableCollection<SettingsNodeVM> Nodes { get; }

        public ObjectSettingsVM(FieldMeta fieldMeta, Dictionary<string, SettingsNodeVM> nodes)
            : base(fieldMeta)
        {
            _nodes = nodes;
            Nodes = new ObservableCollection<SettingsNodeVM>(_nodes.Values);
        }

        public ObjectSettingsVM(SettingsParameters param, FieldMeta fieldMeta)
            : base(fieldMeta)
        {
            var nodes = Factory(param);
            _nodes = nodes
                .ToDictionary(x => x.Meta.DiskName);
            _nodes.ForEach(n => n.Value.WrapUp());
            Nodes = new ObservableCollection<SettingsNodeVM>(_nodes.Values);
        }

        public override void Import(JsonElement property, ILogger logger)
        {
            ImportStatic(_nodes, property, logger);
        }

        public override void Persist(JObject obj, ILogger logger)
        {
            PersistStatic(_nodes, Meta.DiskName, obj, logger);
        }

        public static void ImportStatic(
            Dictionary<string, SettingsNodeVM> nodes,
            JsonElement root,
            ILogger logger)
        {
            foreach (var elem in root.EnumerateObject())
            {
                if (!nodes.TryGetValue(elem.Name, out var node))
                {
                    logger.Error($"Could not locate proper node for setting with name: {elem.Name}");
                    continue;
                }
                try
                {
                    node.Import(elem.Value, logger);
                }
                catch (InvalidOperationException ex)
                {
                    logger.Error(ex, $"Error parsing {elem.Name}");
                }
            }
        }

        public static void PersistStatic(
            Dictionary<string, SettingsNodeVM> nodes,
            string? name,
            JObject obj,
            ILogger logger)
        {
            if (!name.IsNullOrWhitespace())
            {
                var subObj = new JObject();
                obj[name] = subObj;
                obj = subObj;
            }
            foreach (var node in nodes.Values)
            {
                node.Persist(obj, logger);
            }
        }

        public override SettingsNodeVM Duplicate()
        {
            return new ObjectSettingsVM(
                Meta,
                this._nodes.Values
                    .Select(f =>
                    {
                        var ret = f.Duplicate();
                        ret.WrapUp();
                        return ret;
                    })
                    .ToDictionary(f => f.Meta.DiskName));
        }
    }
}
