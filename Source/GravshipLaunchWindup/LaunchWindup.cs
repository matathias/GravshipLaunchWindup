using RimWorld;
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
            if (phase == StartupPhase.Dormant && cooldownCompleteTick > Find.TickManager.TicksGame)
            {
                yield return new Command_Action
                {
                    defaultLabel = "CommandGLWWindup".Translate(this),
                    defaultDesc = "CommandGLWWindupDescOnCooldown".Translate(),
                    icon = WindupCommandTex
                };
            }
            else if (phase == StartupPhase.Dormant)
            {
                yield return new Command_Action
                {
                    defaultLabel = "CommandGLWWindup".Translate(this),
                    defaultDesc = "CommandGLWWindupDesc".Translate(),
                    icon = WindupCommandTex,
                    action = delegate
                    {
                        BeginStartup();
                    }
                };
            }
            else if (phase == StartupPhase.Starting)
            {
                yield return new Command_Action
                {
                    defaultLabel = "CommandGLWWindup".Translate(this),
                    defaultDesc = "CommandGLWWindupDescStartingUp".Translate((WindupCompletionTick - Find.TickManager.TicksGame).ToStringTicksToPeriod(allowSeconds: false, shortForm: false, canUseDecimals: false)),
                    icon = WindupCommandTex
                };
            }
            else if (phase == StartupPhase.Started)
            {
                yield return new Command_Action
                {
                    defaultLabel = "CommandGLWWindup".Translate(this),
                    defaultDesc = "CommandGLWWindupDescStartedUp".Translate((LaunchTimeoutTick - Find.TickManager.TicksGame).ToStringTicksToPeriod(allowSeconds: false, shortForm: false, canUseDecimals: false)),
                    icon = WindupCommandTex
                };
            }
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
            WindupCompletionTick = Find.TickManager.TicksGame + GLWSettings.winduptime;

            Messages.Message("glwBeginStartupMessage".Translate((GLWSettings.winduptime).ToStringTicksToPeriod(allowSeconds: false, shortForm: false, canUseDecimals: false)), MessageTypeDefOf.NeutralEvent);
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
            LaunchTimeoutTick = Find.TickManager.TicksGame + GLWSettings.launchwindow;

            if (GLWSettings.sendLetters)
            {
                Find.LetterStack.ReceiveLetter("glwCompleteStartupLetterLabel".Translate(), "glwCompleteStartupLetterDesc".Translate((GLWSettings.launchwindow).ToStringTicksToPeriod(allowSeconds: false, shortForm: false, canUseDecimals: false)), LetterDefOf.PositiveEvent);
            }
            else
            {
                Messages.Message("glwCompleteStartupMessage".Translate((GLWSettings.launchwindow).ToStringTicksToPeriod(allowSeconds: false, shortForm: false, canUseDecimals: false)), MessageTypeDefOf.NeutralEvent);
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
    }
}
