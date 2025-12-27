using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace GravshipLaunchWindup
{
    [StaticConstructorOnStartup]
    public class Building_GravEngineWithWindup : Building_GravEngine
    {
        public enum StartupPhase
        {
            Dormant,
            Starting,
            Started
        }
        public StartupPhase phase = StartupPhase.Dormant;
        public int WindupCompletionTick = 0;
        public int LaunchTimeoutTick = 0;
        //TODO: get startup texture
        private static readonly Texture2D WindupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/GravStartup");

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref phase, "startupphase", StartupPhase.Dormant);
            Scribe_Values.Look(ref WindupCompletionTick, "windupcompletiontick", -1);
            Scribe_Values.Look(ref LaunchTimeoutTick, "launchtimeouttick", -1);
        }

        public AcceptanceReport CanUseNow()
        {
            if (phase == StartupPhase.Dormant && cooldownCompleteTick > Find.TickManager.TicksGame)
            {
                return new AcceptanceReport("CommandGLWWindupDescOnCooldown".Translate());
            }
            else if (phase == StartupPhase.Starting)
            {
                return new AcceptanceReport("CommandGLWWindupDescStartingUp".Translate((WindupCompletionTick - Find.TickManager.TicksGame).ToStringTicksToPeriod(allowSeconds: false, shortForm: false, canUseDecimals: false)));
            }
            else if (phase == StartupPhase.Started)
            {
                return new AcceptanceReport("CommandGLWWindupDescStartedUp".Translate((LaunchTimeoutTick - Find.TickManager.TicksGame).ToStringTicksToPeriod(allowSeconds: false, shortForm: false, canUseDecimals: false)));
            }
            return AcceptanceReport.WasAccepted;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (!base.Spawned)
            {
                yield break;
            }

            Command_Action command_action = new Command_Action
            {
                defaultLabel = "CommandGLWWindup".Translate(this),
                defaultDesc = "CommandGLWWindupDesc".Translate(),
                icon = WindupCommandTex,
                action = delegate
                {
                    BeginStartup();
                }
            };
            AcceptanceReport acceptanceReport = CanUseNow();
            if (!acceptanceReport.Accepted)
            {
                command_action.Disable(acceptanceReport.Reason);
            }
            yield return command_action;

            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Startup Now",
                    action = delegate
                    {
                        BeginStartup(true);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Complete Startup Now",
                    action = delegate
                    {
                        CompleteStartup(true);
                    }
                };
            }
        }

        private void BeginStartup(bool force = false)
        {
            if (!force && phase == StartupPhase.Starting)
            {
                DebugUtility.DebugLog("Called BeginStartup when windup has already been started");
                return;
            }
            else if (!force && phase == StartupPhase.Started)
            {
                DebugUtility.DebugLog("Called BeginStartup when windup has already been completed");
                return;
            }
            phase = StartupPhase.Starting;
            int winduptime = GetWindupTime();
            WindupCompletionTick = Find.TickManager.TicksGame + winduptime;

            Messages.Message("glwBeginStartupMessage".Translate((winduptime).ToStringTicksToPeriod(allowSeconds: false, shortForm: false, canUseDecimals: false)), MessageTypeDefOf.NeutralEvent);
        }

        private void CompleteStartup(bool force = false)
        {
            if (!force && phase == StartupPhase.Dormant)
            {
                DebugUtility.DebugLog("Called CompleteStartup when windup hasn't been started");
                return;
            }
            else if (!force && phase == StartupPhase.Started)
            {
                DebugUtility.DebugLog("Called CompleteStartup when windup has already been completed");
                return;
            }
            phase = StartupPhase.Started;
            int launchwindow = GetLaunchWindow();
            LaunchTimeoutTick = Find.TickManager.TicksGame + launchwindow;

            if (GLWSettings.sendLetters)
            {
                Find.LetterStack.ReceiveLetter("glwCompleteStartupLetterLabel".Translate(), "glwCompleteStartupLetterDesc".Translate((launchwindow).ToStringTicksToPeriod(allowSeconds: false, shortForm: false, canUseDecimals: false)), LetterDefOf.PositiveEvent);
            }
            else
            {
                Messages.Message("glwCompleteStartupMessage".Translate((launchwindow).ToStringTicksToPeriod(allowSeconds: false, shortForm: false, canUseDecimals: false)), MessageTypeDefOf.NeutralEvent);
            }
        }

        public void LaunchTimersReset(bool timedOut)
        {
            phase = StartupPhase.Dormant;
            WindupCompletionTick = -1;
            LaunchTimeoutTick = -1;

            DebugUtility.DebugLog($"Reseting startup and launch timeout timers. Timed out: {timedOut}");
            if (timedOut)
            {
                if (GLWSettings.sendLetters)
                {
                    Find.LetterStack.ReceiveLetter("glwLaunchTimersResetLabel".Translate(), "glwLaunchTimersResetDesc".Translate(), LetterDefOf.ThreatSmall);
                }
                else
                {
                    Messages.Message("glwLaunchTimersResetMessage".Translate(), MessageTypeDefOf.NegativeEvent);
                }
            }
        }

        protected override void Tick()
        {
            if (phase == StartupPhase.Starting && Find.TickManager.TicksGame >= WindupCompletionTick)
            {
                CompleteStartup();
            }
            if (phase == StartupPhase.Started && Find.TickManager.TicksGame >= LaunchTimeoutTick)
            {
                LaunchTimersReset(true);
            }
        }

        private int GetWindupTime()
        {
            int winduptime = 0;
            if (GLWSettings.VGEActive)
            {
                if (def.defName == "VGE_GravjumperEngine")
                {
                    winduptime = GLWSettings.winduptime_jumper * GenDate.TicksPerHour;
                }
                else if (def.defName == "GravEngine")
                {
                    winduptime = GLWSettings.winduptime * GenDate.TicksPerHour;
                }
                else if (def.defName == "VGE_GravhulkEngine")
                {
                    winduptime = GLWSettings.winduptime_hulk * GenDate.TicksPerHour;
                }
                else
                {
                    DebugUtility.DebugLog("VGE detected as active, but could not resolve grav engine defName. Using default winduptime", LogMessageType.Warning);
                    winduptime = GLWSettings.winduptime * GenDate.TicksPerHour;
                }
            }
            else
            {
                winduptime = GLWSettings.winduptime * GenDate.TicksPerHour;
            }
            return winduptime;
        }
        private int GetLaunchWindow()
        {
            int launchwindow = 0;
            if (GLWSettings.VGEActive)
            {
                if (def.defName == "VGE_GravjumperEngine")
                {
                    launchwindow = GLWSettings.launchwindow_jumper * GenDate.TicksPerHour;
                }
                else if (def.defName == "GravEngine")
                {
                    launchwindow = GLWSettings.launchwindow * GenDate.TicksPerHour;
                }
                else if (def.defName == "VGE_GravhulkEngine")
                {
                    launchwindow = GLWSettings.launchwindow_hulk * GenDate.TicksPerHour;
                }
                else
                {
                    DebugUtility.DebugLog("VGE detected as active, but could not resolve grav engine defName. Using default launchwindow", LogMessageType.Warning);
                    launchwindow = GLWSettings.launchwindow * GenDate.TicksPerHour;
                }
            }
            else
            {
                launchwindow = GLWSettings.launchwindow * GenDate.TicksPerHour;
            }
            return launchwindow;
        }
    }
}
