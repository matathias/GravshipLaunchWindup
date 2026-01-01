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
            Started,
            Cooldown
        }
        private StartupPhase phase = StartupPhase.Dormant;
        private bool usedEmergencyStartup = false;
        public int WindupCompletionTick = 0;
        public int LaunchTimeoutTick = 0;
        public int WindupCooldownTick = 0;
        public int EmergencyLaunchExtraEngineCooldownTick = 0;
        public int EmergencyLaunchCooldownTick = 0;
        private static readonly Texture2D WindupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/GravStartup");

        public StartupPhase Phase => phase;
        public bool EmergencyConfiguration => usedEmergencyStartup;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref phase, "startupphase", StartupPhase.Dormant);
            Scribe_Values.Look(ref WindupCompletionTick, "windupcompletiontick", 0);
            Scribe_Values.Look(ref LaunchTimeoutTick, "launchtimeouttick", 0);
            Scribe_Values.Look(ref WindupCooldownTick, "windupcooldowntick", 0);
            Scribe_Values.Look(ref EmergencyLaunchCooldownTick, "emergencylaunchcooldowntick", 0);
            Scribe_Values.Look(ref usedEmergencyStartup, "usedemergencystartup", defaultValue: false);
            Scribe_Values.Look(ref EmergencyLaunchExtraEngineCooldownTick, "emergencylaunchextraenginecooldowntick", 0);
        }

        public AcceptanceReport CanUseNow()
        {
            if (phase == StartupPhase.Dormant && cooldownCompleteTick > Find.TickManager.TicksGame)
            {
                return new AcceptanceReport("CommandGLWWindupDescOnCooldown".Translate());
            }
            else if (phase == StartupPhase.Dormant && EmergencyLaunchExtraEngineCooldownTick > Find.TickManager.TicksGame)
            {
                return new AcceptanceReport("CommandGLWWindupDescOnEmergencyCooldown".Translate((EmergencyLaunchExtraEngineCooldownTick - Find.TickManager.TicksGame).ToStringTicksToPeriod(allowSeconds: false, shortForm: false)));
            }
            else if (phase == StartupPhase.Starting)
            {
                return new AcceptanceReport("CommandGLWWindupDescStartingUp".Translate((WindupCompletionTick - Find.TickManager.TicksGame).ToStringTicksToPeriod(allowSeconds: false, shortForm: false)));
            }
            else if (phase == StartupPhase.Started)
            {
                return new AcceptanceReport("CommandGLWWindupDescStartedUp".Translate((LaunchTimeoutTick - Find.TickManager.TicksGame).ToStringTicksToPeriod(allowSeconds: false, shortForm: false)));
            }
            else if (phase == StartupPhase.Cooldown)
            {
                return new AcceptanceReport("CommandGLWWindupDescCooldown".Translate((WindupCooldownTick - Find.TickManager.TicksGame).ToStringTicksToPeriod(allowSeconds: false, shortForm: false)));
            }
            return AcceptanceReport.WasAccepted;
        }

        public AcceptanceReport CanUseNow_EmergencyLaunch()
        {
            if (cooldownCompleteTick > Find.TickManager.TicksGame)
            {
                return new AcceptanceReport("CommandGLWWindupDescOnCooldown".Translate());
            }
            else if (EmergencyLaunchCooldownTick > Find.TickManager.TicksGame)
            {
                return new AcceptanceReport("CommandGLWEmergencyLaunchDescOnCooldown".Translate((EmergencyLaunchCooldownTick - Find.TickManager.TicksGame).ToStringTicksToPeriod(allowSeconds: false, shortForm: false)));
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

            if (phase != StartupPhase.Started && GLWSettings.el_enableEmergencyLaunches)
            {
                /* Emergency Launch button should only show up when regular launch isn't an option */
                Command_Action emergency_launch = new Command_Action
                {
                    defaultLabel = "CommandGLWWindupEmergency".Translate(this),
                    defaultDesc = "CommandGLWWindupEmergencyDesc".Translate(GetEmergencyLaunchCooldown().ToStringTicksToPeriod(allowSeconds: false, shortForm: false)),
                    icon = WindupCommandTex,
                    action = delegate
                    {
                        EmergencyLaunchStartup();
                    }
                };
                AcceptanceReport acceptanceReportEmergency = CanUseNow_EmergencyLaunch();
                if (!acceptanceReportEmergency.Accepted)
                {
                    emergency_launch.Disable(acceptanceReportEmergency.Reason);
                }
                yield return emergency_launch;
            }

            if (phase == StartupPhase.Starting)
            {
                Command_Action abort_warmup_action = new Command_Action
                {
                    defaultLabel = "CommandGLWAbort".Translate(this),
                    defaultDesc = "CommandGLWAbortDesc".Translate(GetPostLaunchExpiryCooldown(false).ToStringTicksToPeriod(allowSeconds: false, shortForm: false)),
                    icon = WindupCommandTex,
                    action = delegate
                    {
                        LaunchExpiryCooldown(true);
                    }
                };
                yield return abort_warmup_action;
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
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Close Launch Window",
                    action = delegate
                    {
                        LaunchExpiryCooldown(false, true);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEV: End Cooldown",
                    action = delegate
                    {
                        LaunchTimersReset(true);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEV: End Emergency Launch Cooldowns",
                    action = delegate
                    {
                        usedEmergencyStartup = false;
                        EmergencyLaunchCooldownTick = -1;
                        EmergencyLaunchExtraEngineCooldownTick = -1;
                    }
                };
            }
        }

        private void BeginStartup(bool force = false)
        {
            if (!force && phase != StartupPhase.Dormant)
            {
                DebugUtility.DebugLog($"Called BeginStartup in wrong phase: {phase}", LogMessageType.Warning);
                return;
            }
            phase = StartupPhase.Starting;
            int winduptime = GetWindupTime();
            WindupCompletionTick = Find.TickManager.TicksGame + winduptime;

            Messages.Message("glwBeginStartupMessage".Translate((winduptime).ToStringTicksToPeriod(allowSeconds: false, shortForm: false)), MessageTypeDefOf.NeutralEvent);
        }

        private void CompleteStartup(bool force = false)
        {
            if (!force && phase != StartupPhase.Starting)
            {
                DebugUtility.DebugLog($"Called CompleteStartup in wrong phase: {phase}", LogMessageType.Warning);
                return;
            }
            phase = StartupPhase.Started;
            int launchwindow = GetLaunchWindow();
            LaunchTimeoutTick = Find.TickManager.TicksGame + launchwindow;

            if (GLWSettings.sendLetters)
            {
                Find.LetterStack.ReceiveLetter("glwCompleteStartupLetterLabel".Translate(), "glwCompleteStartupLetterDesc".Translate((launchwindow).ToStringTicksToPeriod(allowSeconds: false, shortForm: false)), LetterDefOf.PositiveEvent);
            }
            else
            {
                Messages.Message("glwCompleteStartupMessage".Translate((launchwindow).ToStringTicksToPeriod(allowSeconds: false, shortForm: false)), MessageTypeDefOf.NeutralEvent);
            }
        }

        private void LaunchExpiryCooldown(bool earlyEject = false, bool force = false)
        {
            if (!force && !earlyEject && phase != StartupPhase.Started)
            {
                DebugUtility.DebugLog($"Called LaunchExpiryCooldown in wrong phase: {phase}", LogMessageType.Warning);
            }
            phase = StartupPhase.Cooldown;
            int cooldownTick = GetPostLaunchExpiryCooldown(true);

            /* If the cooldown phase has 0 length (which would happen if the player set it so in the settings), then
             * just jump straight to reseting the timers. */
            if (cooldownTick == 0)
            {
                LaunchTimersReset(true, true);
            }

            WindupCooldownTick = Find.TickManager.TicksGame + cooldownTick;

            if (earlyEject)
            {
                if (GLWSettings.sendLetters)
                {
                    Find.LetterStack.ReceiveLetter("glwEjectedLaunchLetterLabel".Translate(), "glwEjectedLaunchLetterDesc".Translate((cooldownTick).ToStringTicksToPeriod(allowSeconds: false, shortForm: false)), LetterDefOf.NeutralEvent);
                }
                else
                {
                    Messages.Message("glwEjectedLaunchMessage".Translate((cooldownTick).ToStringTicksToPeriod(allowSeconds: false, shortForm: false)), MessageTypeDefOf.NeutralEvent);
                }
            }
            else
            {
                if (GLWSettings.sendLetters)
                {
                    Find.LetterStack.ReceiveLetter("glwMissedLaunchLetterLabel".Translate(), "glwMissedLaunchLetterDesc".Translate((cooldownTick).ToStringTicksToPeriod(allowSeconds: false, shortForm: false)), LetterDefOf.ThreatSmall);
                }
                else
                {
                    Messages.Message("glwMissedLaunchMessage".Translate((cooldownTick).ToStringTicksToPeriod(allowSeconds: false, shortForm: false)), MessageTypeDefOf.ThreatSmall);
                }
            }
        }

        public void LaunchTimersReset(bool timedOut, bool noCooldownPhase = false)
        {
            phase = StartupPhase.Dormant;
            WindupCompletionTick = -1;
            LaunchTimeoutTick = -1;
            WindupCooldownTick = -1;

            DebugUtility.DebugLog($"Reseting startup and launch timeout timers. Timed out: {timedOut}");
            if (timedOut)
            {
                if (noCooldownPhase)
                {
                    if (GLWSettings.sendLetters)
                    {
                        Find.LetterStack.ReceiveLetter("glwMissedLaunchLetterLabel".Translate(), "glwMissedLaunchLetterDesc2".Translate(), LetterDefOf.ThreatSmall);
                    }
                    else
                    {
                        Messages.Message("glwLaunchTimersResetMessage".Translate(), MessageTypeDefOf.NegativeEvent);
                    }
                }
                else
                {
                    if (GLWSettings.sendLetters)
                    {
                        Find.LetterStack.ReceiveLetter("glwLaunchTimersResetLabel".Translate(), "glwLaunchTimersResetDesc".Translate(), LetterDefOf.NeutralEvent);
                    }
                    else
                    {
                        Messages.Message("glwLaunchTimersResetMessage".Translate(), MessageTypeDefOf.NeutralEvent);
                    }
                }
            }
        }

        private void EmergencyLaunchStartup()
        {
            /* Force the engine into the startup phase, set the emergency launch flag, and send the player a letter/notification */
            CompleteStartup(true);
            usedEmergencyStartup = true;

            int emergencyCooldownTick = GetEmergencyLaunchCooldown();
            EmergencyLaunchCooldownTick = Find.TickManager.TicksGame + emergencyCooldownTick;

            if (GLWSettings.sendLetters)
            {
                Find.LetterStack.ReceiveLetter("glwEmergencyStartupLabel".Translate(), "glwEmergencyStartupDesc".Translate((emergencyCooldownTick).ToStringTicksToPeriod(allowSeconds: false, shortForm: false)), LetterDefOf.ThreatSmall);
            }
            else
            {
                Messages.Message("glwEmergencyStartupMessage".Translate((emergencyCooldownTick).ToStringTicksToPeriod(allowSeconds: false, shortForm: false)), MessageTypeDefOf.ThreatSmall);
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
                LaunchExpiryCooldown();
            }
            if (phase == StartupPhase.Cooldown && Find.TickManager.TicksGame >= WindupCooldownTick)
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

        private int GetPostLaunchExpiryCooldown(bool resetEmergency)
        {
            int cooldownticks = 0;
            if (GLWSettings.VGEActive)
            {
                if (def.defName == "VGE_GravjumperEngine")
                {
                    cooldownticks = GLWSettings.windupcooldown_jumper * GenDate.TicksPerHour;
                }
                else if (def.defName == "GravEngine")
                {
                    cooldownticks = GLWSettings.windupcooldown * GenDate.TicksPerHour;
                }
                else if (def.defName == "VGE_GravhulkEngine")
                {
                    cooldownticks = GLWSettings.windupcooldown_hulk * GenDate.TicksPerHour;
                }
                else
                {
                    DebugUtility.DebugLog("VGE detected as active, but could not resolve grav engine defName. Using default windupcooldown", LogMessageType.Warning);
                    cooldownticks = GLWSettings.windupcooldown * GenDate.TicksPerHour;
                }
            }
            else
            {
                cooldownticks = GLWSettings.windupcooldown * GenDate.TicksPerHour;
            }

            if (EmergencyConfiguration)
            {
                cooldownticks += GetEmergencyLaunchExtraExpiryCooldown(resetEmergency);
            }
            return cooldownticks;
        }
        private int GetEmergencyLaunchExtraExpiryCooldown(bool resetEmergency)
        {
            if (resetEmergency)
            {
                DebugUtility.DebugLog("Reseting usedEmergencyStartup");
                usedEmergencyStartup = false;
            }
            int cooldownticks = 0;
            if (GLWSettings.VGEActive)
            {
                if (def.defName == "VGE_GravjumperEngine")
                {
                    cooldownticks = GLWSettings.el_extraExpiryCooldown_jumper * GenDate.TicksPerHour;
                }
                else if (def.defName == "GravEngine")
                {
                    cooldownticks = GLWSettings.el_extraExpiryCooldown * GenDate.TicksPerHour;
                }
                else if (def.defName == "VGE_GravhulkEngine")
                {
                    cooldownticks = GLWSettings.el_extraExpiryCooldown_hulk * GenDate.TicksPerHour;
                }
                else
                {
                    DebugUtility.DebugLog("VGE detected as active, but could not resolve grav engine defName. Using default windupcooldown", LogMessageType.Warning);
                    cooldownticks = GLWSettings.el_extraExpiryCooldown * GenDate.TicksPerHour;
                }
            }
            else
            {
                cooldownticks = GLWSettings.el_extraExpiryCooldown * GenDate.TicksPerHour;
            }
            return cooldownticks;
        }
        private int GetEmergencyLaunchCooldown()
        {
            int cooldownticks = 0;
            if (GLWSettings.VGEActive)
            {
                if (def.defName == "VGE_GravjumperEngine")
                {
                    cooldownticks = GLWSettings.el_emergencyCooldown_jumper * GenDate.TicksPerHour;
                }
                else if (def.defName == "GravEngine")
                {
                    cooldownticks = GLWSettings.el_emergencyCooldown * GenDate.TicksPerHour;
                }
                else if (def.defName == "VGE_GravhulkEngine")
                {
                    cooldownticks = GLWSettings.el_emergencyCooldown_hulk * GenDate.TicksPerHour;
                }
                else
                {
                    DebugUtility.DebugLog("VGE detected as active, but could not resolve grav engine defName. Using default windupcooldown", LogMessageType.Warning);
                    cooldownticks = GLWSettings.el_emergencyCooldown * GenDate.TicksPerHour;
                }
            }
            else
            {
                cooldownticks = GLWSettings.el_emergencyCooldown * GenDate.TicksPerHour;
            }
            return cooldownticks;
        }

        public void SetPostLaunchEmergencyCooldown()
        {
            if (GLWSettings.el_extendPostLaunchCooldown && EmergencyConfiguration)
            {
                DebugUtility.DebugLog("Post-Emergency-Startup Landing, start. Extending cooldown");
                int extraCooldownTicks = GetEmergencyLaunchExtraExpiryCooldown(true);
                EmergencyLaunchExtraEngineCooldownTick = cooldownCompleteTick + extraCooldownTicks;
                if (EmergencyLaunchExtraEngineCooldownTick > EmergencyLaunchCooldownTick)
                {
                    /* It'd be silly if the extra grav engine cooldown time was longer than the cooldown for emergency startup itself. So in this case, extend the cooldown for
                     * Emergency Startup to longer than the grav engine cooldown. */
                    EmergencyLaunchCooldownTick = cooldownCompleteTick + GetEmergencyLaunchCooldown();
                }
                DebugUtility.DebugLog($"Post-Emergency-Startup Landing complete. Cooldown tick: {cooldownCompleteTick} extra ticks: {extraCooldownTicks} extra cooldown tick: {EmergencyLaunchExtraEngineCooldownTick}");
            }
        }
    }
}
