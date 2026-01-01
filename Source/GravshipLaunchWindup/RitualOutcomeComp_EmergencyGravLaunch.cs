using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GravshipLaunchWindup
{
    public class RitualOutcomeComp_EmergencyGravLaunch : RitualOutcomeComp_QualitySingleOffset
    {
        public override float QualityOffset(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
        {
            if (!IsEmergencyLaunch(ritual.Ritual, ritual.selectedTarget))
            {
                return 0f;
            }
            return base.QualityOffset(ritual, data);
        }

        public override bool Applies(LordJob_Ritual ritual)
        {
            return IsEmergencyLaunch(ritual.Ritual, ritual.selectedTarget);
        }

        public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
        {
            if (!IsEmergencyLaunch(ritual, ritualTarget))
            {
                return null;
            }
            float factor = -1 * GLWSettings.el_emergencyLaunchRitualFactor;
            return new QualityFactor
            {
                label = LabelForDesc,
                qualityChange = ExpectedOffsetDesc(factor > 0f, factor),
                present = (factor < 0f),
                quality = factor,
                positive = (factor > 0f),
                priority = 1f,
                noMiddleColumnInfo = true
            };
        }

        private static bool IsEmergencyLaunch(Precept_Ritual ritual, TargetInfo ritualTarget)
        {
            Building_GravEngine engine = ritualTarget.Thing?.TryGetComp<CompPilotConsole>()?.engine;
            if (engine != null && engine is Building_GravEngineWithWindup windupEngine)
            {
                return windupEngine.EmergencyConfiguration;
            }
            return false;
        }
        protected override string ExpectedOffsetDesc(bool positive, float quality = -1f)
        {
            return (TaggedString)quality.ToStringWithSign("0.#%");
        }
    }
}
