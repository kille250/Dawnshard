using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DragaliaAPI.Photon.Shared.Enums;

namespace DragaliaAPI.Photon.Plugin.Helpers
{
    public static partial class QuestHelper
    {
        static QuestHelper()
        {
            InitializeRaidQuestIds();
        }

        private static ImmutableHashSet<int> RaidQuestIds;

        static partial void InitializeRaidQuestIds();

        public static bool IsRaid(int questId)
        {
            return RaidQuestIds.Contains(questId);
        }
    }
}
