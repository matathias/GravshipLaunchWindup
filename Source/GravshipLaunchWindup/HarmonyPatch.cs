using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GravshipLaunchWindup
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatcher
    {
        static HarmonyPatcher()
        {
            new Harmony("matathias.gravshiplaunchwindup").PatchAll(Assembly.GetExecutingAssembly());
        }
    }
    [HarmonyPatch(typeof(Building_GravEngine), "CanLaunch")]
    public static class GravshipLaunchWindup_Building_GravEngine_CanLaunch_Patch
    {
        private static void Postfix(CompPilotConsole console, ref AcceptanceReport __result, Building_GravEngine __instance)
        {
            if (__result == AcceptanceReport.WasAccepted)
            {
                if (__instance is Building_GravEngineWithWindup engineW)
                {
                    if (engineW.phase == Building_GravEngineWithWindup.StartupPhase.Dormant)
                    {
                        __result = new AcceptanceReport("glwWindupNotStarted".Translate());
                    }
                    else if (engineW.phase == Building_GravEngineWithWindup.StartupPhase.Starting)
                    {
                        __result = new AcceptanceReport("glwWindupNotComplete".Translate((engineW.WindupCompletionTick - Find.TickManager.TicksGame).ToStringTicksToPeriod(allowSeconds: false, shortForm: false, canUseDecimals: false)));
                    }
                }
                else
                {
                    DebugUtility.DebugLog("CanLaunch Postfix called on non-windup gravengine");
                }
            }
        }
    }
    [HarmonyPatch(typeof(Building_GravEngine), "ConsumeFuel")]
    public static class GravshipLaunchWindup_Building_GravEngine_ConsumeFuel_Patch
    {
        private static void Postfix(Building_GravEngine __instance)
        {
            if(__instance is Building_GravEngineWithWindup engineW)
            {
                engineW.LaunchTimersReset(false);
            }
            else
            {
                DebugUtility.DebugLog("ConsumeFuel Postfix called on non-windup gravengine");
            }
        }
    }
}
