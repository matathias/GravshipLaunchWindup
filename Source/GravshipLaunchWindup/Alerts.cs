using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace GravshipLaunchWindup
{
    public abstract class Alert_GravEngineLaunchStartup : Alert
    {
        public Building_GravEngineWithWindup gravEngine
        {
            get
            {
                if (!GLWSettings.showAlerts)
                {
                    return null;
                }
                List<Map> maps = Find.Maps;
                foreach (Map map in maps)
                {
                    if (GravshipUtility.PlayerHasGravEngine(map))
                    {
                        Building_GravEngine geng = GravshipUtility.GetPlayerGravEngine_NewTemp(map);
                        if (geng is Building_GravEngineWithWindup gendwing)
                        {
                            return gendwing;
                        }
                        else
                            return null;

                    }
                }
                return null;
            }
        }
    }
    public class Alert_EngineStartupPhase : Alert_GravEngineLaunchStartup
    {
        public int StartupTick => gravEngine?.WindupCompletionTick ?? 0;
        public override AlertReport GetReport()
        {
            if (!GLWSettings.showAlerts || gravEngine == null || gravEngine.phase != Building_GravEngineWithWindup.StartupPhase.Starting)
            {
                return AlertReport.Inactive;
            }
            return AlertReport.Active;
        }

        public override string GetLabel()
        {
            return "glwAlertStartupSequence".Translate() + ": " + (StartupTick - Find.TickManager.TicksGame).ToStringTicksToPeriod(allowSeconds: false, shortForm: false, canUseDecimals: false);
        }
    }

    public class Alert_EngineReadyPhase : Alert_GravEngineLaunchStartup
    {
        public int LaunchTimeoutTick => gravEngine?.LaunchTimeoutTick ?? 0;

        private bool Red => Find.TickManager.TicksGame > LaunchTimeoutTick - (GenDate.TicksPerHour * 6);
        protected override Color BGColor
        {
            get
            {
                if (!Red)
                {
                    return Color.clear;
                }
                return Alert_Critical.BgColor();
            }
        }
        public override AlertReport GetReport()
        {
            if (!GLWSettings.showAlerts || gravEngine == null || gravEngine.phase != Building_GravEngineWithWindup.StartupPhase.Started)
            {
                return AlertReport.Inactive;
            }
            return AlertReport.Active;
        }

        public override string GetLabel()
        {
            return "glwAlertLaunchWindow".Translate() + ": " + (LaunchTimeoutTick - Find.TickManager.TicksGame).ToStringTicksToPeriod(allowSeconds: false, shortForm: false, canUseDecimals: false);
        }

        public override TaggedString GetExplanation()
        {
            return "glwAlertLaunchWindowDesc".Translate();
        }
    }
}
