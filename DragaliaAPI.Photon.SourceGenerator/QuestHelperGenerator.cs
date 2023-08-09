using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;

namespace DragaliaAPI.Photon.Shared.SourceGenerator
{
    [Generator]
    public class QuestHelperGenerator : IIncrementalGenerator
    {
        private const int RaidDungeonType = 2;

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
                Debugger.Launch();

#endif

            IncrementalValuesProvider<AdditionalText> jsonFiles = context.AdditionalTextsProvider;

            IncrementalValuesProvider<int> questIds = jsonFiles
                .SelectMany(
                    (text, _) =>
                        JsonSerializer.Deserialize<IEnumerable<QuestData>>(
                            text.GetText().ToString()
                        )
                )
                .Select((x, _) => x._Id);

            IncrementalValueProvider<ImmutableArray<int>> collected = questIds.Collect();

            context.RegisterSourceOutput(
                collected,
                (sourceProductionContext, raidQuestIds) =>
                    sourceProductionContext.AddSource(
                        "QuestHelper.g.cs",
                        @"
namespace DragaliaAPI.Photon.Plugin.Helpers
{

    using System.Collections.Immutable;

    public partial class QuestHelper 
    {
        static partial void InitializeRaidQuestIds() 
        {
            RaidQuestIds = new int[] {"
                            + string.Join(", ", raidQuestIds)
                            + @"}.ToImmutableHashSet();
        }
    }
}"
                    )
            );
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE1006:Naming Styles",
        Justification = "JSON property name"
    )]
    public class QuestData
    {
        public int _Id { get; set; }
        public int _DungeonType { get; set; }
    }
}
