using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static HarmonyLib.AccessTools;

namespace GravshipLaunchWindup
{
    public class GLWSettings : ModSettings
    {
        /* Defaults */
        /* By default, literally everything is defined in relation to the standard gravship's default startup time (DEFAULT_STARTUPTIME).
         * We just have all of these extra fields in case the user wants to deviate from that scheme. */
        public const int DEFAULT_STARTUPTIME_JUMPER = DEFAULT_STARTUPTIME / 2; //  6
        public const int DEFAULT_STARTUPTIME = 12;                             // 12
        public const int DEFAULT_STARTUPTIME_HULK = DEFAULT_STARTUPTIME * 2;   // 24

        public const int DEFAULT_LAUNCHWINDOW_JUMPER = DEFAULT_LAUNCHWINDOW / 2; //  3
        public const int DEFAULT_LAUNCHWINDOW = DEFAULT_STARTUPTIME / 2;         //  6
        public const int DEFAULT_LAUNCHWINDOW_HULK = DEFAULT_LAUNCHWINDOW * 2;   // 12

        public const int DEFAULT_COOLDOWN_JUMPER = DEFAULT_COOLDOWN / 2; // 12
        public const int DEFAULT_COOLDOWN = DEFAULT_STARTUPTIME * 2;     // 24
        public const int DEFAULT_COOLDOWN_HULK = DEFAULT_COOLDOWN * 2;   // 48

        /* This field exists purely for ease of code editing. It's honestly pretty useless */
        private const int default_emergencystartup_factor = 14;
        public const int DEFAULT_EMERGENCYSTARTUP_COOLDOWN_JUMPER = DEFAULT_COOLDOWN_JUMPER * default_emergencystartup_factor;
        public const int DEFAULT_EMERGENCYSTARTUP_COOLDOWN = DEFAULT_COOLDOWN * default_emergencystartup_factor;
        public const int DEFAULT_EMERGENCYSTARTUP_COOLDOWN_HULK = DEFAULT_COOLDOWN_HULK * default_emergencystartup_factor;

        public const int DEFAULT_ES_EXTRAEXPIRY_COOLDOWN_JUMPER = DEFAULT_ES_EXTRAEXPIRY_COOLDOWN / 2; // 12
        public const int DEFAULT_ES_EXTRAEXPIRY_COOLDOWN = DEFAULT_COOLDOWN;                           // 24
        public const int DEFAULT_ES_EXTRAEXPIRY_COOLDOWN_HULK = DEFAULT_ES_EXTRAEXPIRY_COOLDOWN * 2;   // 48

        public const float DEFAULT_EMERGENCYSTARTUP_RITUALFACTOR = 0.75f;
        /* End defaults */

        public static bool el_enableEmergencyLaunches = true;
        public static bool el_extendPostLaunchCooldown = true;
        public static float el_emergencyLaunchRitualFactor = DEFAULT_EMERGENCYSTARTUP_RITUALFACTOR;

        public static int winduptime_jumper = DEFAULT_STARTUPTIME_JUMPER;
        public static int winduptime = DEFAULT_STARTUPTIME;
        public static int winduptime_hulk = DEFAULT_STARTUPTIME_HULK;

        public static int launchwindow_jumper = DEFAULT_LAUNCHWINDOW_JUMPER;
        public static int launchwindow = DEFAULT_LAUNCHWINDOW;
        public static int launchwindow_hulk = DEFAULT_LAUNCHWINDOW_HULK;

        public static int windupcooldown_jumper = DEFAULT_COOLDOWN_JUMPER;
        public static int windupcooldown = DEFAULT_COOLDOWN;
        public static int windupcooldown_hulk = DEFAULT_COOLDOWN_HULK;

        public static int el_emergencyCooldown_jumper = DEFAULT_EMERGENCYSTARTUP_COOLDOWN_JUMPER;
        public static int el_emergencyCooldown = DEFAULT_EMERGENCYSTARTUP_COOLDOWN;
        public static int el_emergencyCooldown_hulk = DEFAULT_EMERGENCYSTARTUP_COOLDOWN_HULK;

        public static int el_extraExpiryCooldown_jumper = DEFAULT_ES_EXTRAEXPIRY_COOLDOWN_JUMPER;
        public static int el_extraExpiryCooldown = DEFAULT_ES_EXTRAEXPIRY_COOLDOWN;
        public static int el_extraExpiryCooldown_hulk = DEFAULT_ES_EXTRAEXPIRY_COOLDOWN_HULK;

        public static bool printDebug = false;
        public static bool sendLetters = true;
        public static bool showAlerts = true;

        private static bool isVGEActive = false;
        public static bool VGEActive => isVGEActive;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref winduptime_jumper, "winduptime_jumper", DEFAULT_STARTUPTIME_JUMPER);
            Scribe_Values.Look(ref winduptime, "winduptime", DEFAULT_STARTUPTIME);
            Scribe_Values.Look(ref winduptime_hulk, "winduptime_hulk", DEFAULT_STARTUPTIME_HULK);

            Scribe_Values.Look(ref launchwindow_jumper, "launchwindow_jumper", DEFAULT_LAUNCHWINDOW_JUMPER);
            Scribe_Values.Look(ref launchwindow, "launchwindow", DEFAULT_LAUNCHWINDOW);
            Scribe_Values.Look(ref launchwindow_hulk, "launchwindow_hulk", DEFAULT_LAUNCHWINDOW_HULK);

            Scribe_Values.Look(ref windupcooldown_jumper, "windupcooldown_jumper", DEFAULT_COOLDOWN_JUMPER);
            Scribe_Values.Look(ref windupcooldown, "windupcooldown", DEFAULT_COOLDOWN);
            Scribe_Values.Look(ref windupcooldown_hulk, "windupcooldown_hulk", DEFAULT_COOLDOWN_HULK);

            Scribe_Values.Look(ref el_enableEmergencyLaunches, "el_enableEmergencyLaunches", defaultValue: true);
            Scribe_Values.Look(ref el_extendPostLaunchCooldown, "elextendpostlaunchcooldown", defaultValue: true);
            Scribe_Values.Look(ref el_emergencyLaunchRitualFactor, "emergencylaunchritualfactor", DEFAULT_EMERGENCYSTARTUP_RITUALFACTOR);

            Scribe_Values.Look(ref el_emergencyCooldown_jumper, "el_emergencyCooldown_jumper", DEFAULT_EMERGENCYSTARTUP_COOLDOWN_JUMPER);
            Scribe_Values.Look(ref el_emergencyCooldown, "el_emergencyCooldown", DEFAULT_EMERGENCYSTARTUP_COOLDOWN);
            Scribe_Values.Look(ref el_emergencyCooldown_hulk, "el_emergencyCooldown_hulk", DEFAULT_EMERGENCYSTARTUP_COOLDOWN_HULK);

            Scribe_Values.Look(ref el_extraExpiryCooldown_jumper, "el_extraExpiryCooldown_jumper", DEFAULT_ES_EXTRAEXPIRY_COOLDOWN_JUMPER);
            Scribe_Values.Look(ref el_extraExpiryCooldown, "el_extraExpiryCooldown", DEFAULT_ES_EXTRAEXPIRY_COOLDOWN);
            Scribe_Values.Look(ref el_extraExpiryCooldown_hulk, "el_extraExpiryCooldown_hulk", DEFAULT_ES_EXTRAEXPIRY_COOLDOWN_HULK);

            Scribe_Values.Look(ref printDebug, "printDebug", defaultValue: false);
            Scribe_Values.Look(ref sendLetters, "sendLetters", defaultValue: true);
            Scribe_Values.Look(ref showAlerts, "showAlerts", defaultValue: true);
        }

        private Vector2 scrollVector = Vector2.zero;
        private float viewRectHeight = -1f;
        public void DoWindowContents(Rect inRect)
        {
            /* Fancy schmancy math for variable-height settings box.
             * I don't know why I even bothered with this. */
            float heightPerShip = (30f * 3f) + 30f + 30f + (el_enableEmergencyLaunches ? (el_extendPostLaunchCooldown ? (30f * 2f) : 30f) : 0f);
            float heightShipSection = VGEActive ? 3 * (heightPerShip + 45f) + 15f : heightPerShip;
            viewRectHeight = heightShipSection + (el_enableEmergencyLaunches ? (30f * 7f) : (30f * 5f));
            Rect viewRect = new Rect(inRect.x, inRect.y, inRect.width - 18f, viewRectHeight);
            TextAnchor anchor;

            Widgets.BeginScrollView(inRect, ref scrollVector, viewRect);
            var list = new Listing_Standard()
            {
                ColumnWidth = viewRect.width
            };
            list.Begin(viewRect);

            anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleRight;

            list.CheckboxLabeled("glwenableEmergencyLaunches".Translate(), ref el_enableEmergencyLaunches);
            if (el_enableEmergencyLaunches)
            {
                list.CheckboxLabeled("glwExtendLaunchWindowExpiryCooldown".Translate(), ref el_extendPostLaunchCooldown);
                DrawFloatFieldWithReset(list, "glwEmergencyStartupRitualFactor", ref el_emergencyLaunchRitualFactor, DEFAULT_EMERGENCYSTARTUP_RITUALFACTOR, 0f, 1f);
            }
            Text.Anchor = anchor;

            if (VGEActive)
            {
                list.GapLine();
                Text.Font = GameFont.Medium;
                list.Label("glwgravjumpersettings".Translate());

                Text.Font = GameFont.Small;
                anchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleRight;
                list.Label("glwAllTimesInHours".Translate());
                Text.Anchor = anchor;

                DrawIntFieldWithReset(list, "glwwinduptime", ref winduptime_jumper, DEFAULT_STARTUPTIME_JUMPER, 0, GenDate.TicksPerYear);

                DrawIntFieldWithReset(list, "glwlaunchwindow", ref launchwindow_jumper, DEFAULT_LAUNCHWINDOW_JUMPER, 0, GenDate.TicksPerYear);

                DrawIntFieldWithReset(list, "glwwindowcooldown", ref windupcooldown_jumper, DEFAULT_COOLDOWN_JUMPER, 0, GenDate.TicksPerYear);

                if (el_enableEmergencyLaunches)
                {
                    DrawIntFieldWithReset(list, "glwelEmergencyCooldown", ref el_emergencyCooldown_jumper, DEFAULT_EMERGENCYSTARTUP_COOLDOWN_JUMPER, 0, GenDate.TicksPerYear);
                    if (el_extendPostLaunchCooldown)
                    {
                        DrawIntFieldWithReset(list, "glwelExtraExpiryCooldown", ref el_extraExpiryCooldown_jumper, DEFAULT_ES_EXTRAEXPIRY_COOLDOWN_JUMPER, 0, GenDate.TicksPerYear);
                    }
                }

                list.GapLine();
                Text.Font = GameFont.Medium;
                list.Label("glwgravshipsettings".Translate());

                Text.Font = GameFont.Small;
            }
            anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleRight;
            list.Label("glwAllTimesInHours".Translate());
            Text.Anchor = anchor;

            DrawIntFieldWithReset(list, "glwwinduptime", ref winduptime, DEFAULT_STARTUPTIME, 0, GenDate.TicksPerYear);

            DrawIntFieldWithReset(list, "glwlaunchwindow", ref launchwindow, DEFAULT_LAUNCHWINDOW, 0, GenDate.TicksPerYear);

            DrawIntFieldWithReset(list, "glwwindowcooldown", ref windupcooldown, DEFAULT_COOLDOWN, 0, GenDate.TicksPerYear);

            if (el_enableEmergencyLaunches)
            {
                DrawIntFieldWithReset(list, "glwelEmergencyCooldown", ref el_emergencyCooldown, DEFAULT_EMERGENCYSTARTUP_COOLDOWN, 0, GenDate.TicksPerYear);
                if (el_extendPostLaunchCooldown)
                {
                    DrawIntFieldWithReset(list, "glwelExtraExpiryCooldown", ref el_extraExpiryCooldown, DEFAULT_ES_EXTRAEXPIRY_COOLDOWN, 0, GenDate.TicksPerYear);
                }
            }

            if (VGEActive)
            {
                list.GapLine();
                Text.Font = GameFont.Medium;
                list.Label("glwgravhulksettings".Translate());

                Text.Font = GameFont.Small;
                anchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleRight;
                list.Label("glwAllTimesInHours".Translate());
                Text.Anchor = anchor;

                DrawIntFieldWithReset(list, "glwwinduptime", ref winduptime_hulk, DEFAULT_STARTUPTIME_HULK, 0, GenDate.TicksPerYear);

                DrawIntFieldWithReset(list, "glwlaunchwindow", ref launchwindow_hulk, DEFAULT_LAUNCHWINDOW_HULK, 0, GenDate.TicksPerYear);

                DrawIntFieldWithReset(list, "glwwindowcooldown", ref windupcooldown_hulk, DEFAULT_COOLDOWN_HULK, 0, GenDate.TicksPerYear);

                if (el_enableEmergencyLaunches)
                {
                    DrawIntFieldWithReset(list, "glwelEmergencyCooldown", ref el_emergencyCooldown_hulk, DEFAULT_EMERGENCYSTARTUP_COOLDOWN_HULK, 0, GenDate.TicksPerYear);
                    if (el_extendPostLaunchCooldown)
                    {
                        DrawIntFieldWithReset(list, "glwelExtraExpiryCooldown", ref el_extraExpiryCooldown_hulk, DEFAULT_ES_EXTRAEXPIRY_COOLDOWN_HULK, 0, GenDate.TicksPerYear);
                    }
                }
            }
            list.GapLine();

            list.CheckboxLabeled("glwsendLetters".Translate(), ref sendLetters);

            list.CheckboxLabeled("glwshowAlerts".Translate(), ref showAlerts);

            list.CheckboxLabeled("glwprintDebug".Translate(), ref printDebug);

            list.GapLine();

            Rect resetBtn = list.GetRect(30f);
            if (Widgets.ButtonText(resetBtn, "glwResetAll".Translate()))
            {
                el_enableEmergencyLaunches = true;
                el_extendPostLaunchCooldown = true;
                el_emergencyLaunchRitualFactor = DEFAULT_EMERGENCYSTARTUP_RITUALFACTOR;

                winduptime_jumper = DEFAULT_STARTUPTIME_JUMPER;
                winduptime = DEFAULT_STARTUPTIME;
                winduptime_hulk = DEFAULT_STARTUPTIME_HULK;

                launchwindow_jumper = DEFAULT_LAUNCHWINDOW_JUMPER;
                launchwindow = DEFAULT_LAUNCHWINDOW;
                launchwindow_hulk = DEFAULT_LAUNCHWINDOW_HULK;

                windupcooldown_jumper = DEFAULT_COOLDOWN_JUMPER;
                windupcooldown = DEFAULT_COOLDOWN;
                windupcooldown_hulk = DEFAULT_COOLDOWN_HULK;

                el_emergencyCooldown_jumper = DEFAULT_EMERGENCYSTARTUP_COOLDOWN_JUMPER;
                el_emergencyCooldown = DEFAULT_EMERGENCYSTARTUP_COOLDOWN;
                el_emergencyCooldown_hulk = DEFAULT_EMERGENCYSTARTUP_COOLDOWN_HULK;

                el_extraExpiryCooldown_jumper = DEFAULT_ES_EXTRAEXPIRY_COOLDOWN_JUMPER;
                el_extraExpiryCooldown = DEFAULT_ES_EXTRAEXPIRY_COOLDOWN;
                el_extraExpiryCooldown_hulk = DEFAULT_ES_EXTRAEXPIRY_COOLDOWN_HULK;

                printDebug = false;
                sendLetters = true;
                showAlerts = true;
            }

            list.End();

            Widgets.EndScrollView();
        }

        private void DrawIntFieldWithReset(Listing_Standard listing_Standard,
                                            string labelKey,               // translation key for the label
                                            ref int value,                 // the setting field we edit
                                            int defaultValue,              // the hard‑coded default we want to reset to
                                            int minValue = int.MinValue,
                                            int maxValue = int.MaxValue)
        {
            // One row: label + numeric field + reset button
            Rect row = listing_Standard.GetRect(30f);

            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            Rect fieldrect = new Rect(0f, row.yMin + 3f, row.xMax * 0.9f, 24f);
            string valstr = value.ToString();
            TextFieldNumericLabeled2(fieldrect, labelKey.Translate(defaultValue), ref value, ref valstr, minValue, maxValue);

            // Reset button
            Rect btnRect = new Rect(fieldrect.xMax + 5f, fieldrect.y, row.xMax - fieldrect.xMax - 5f, 24f);
            if (Widgets.ButtonText(btnRect, "Reset".Translate()))
            {
                value = defaultValue;
            }
        }

        private void DrawFloatFieldWithReset(Listing_Standard listing_Standard,
                                             string labelKey,               // translation key for the label
                                             ref float value,                 // the setting field we edit
                                             float defaultValue,              // the hard‑coded default we want to reset to
                                             float minValue = float.MinValue,
                                             float maxValue = float.MaxValue)
        {
            // One row: label + numeric field + reset button
            Rect row = listing_Standard.GetRect(30f);

            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            Rect fieldrect = new Rect(0f, row.yMin + 3f, row.xMax * 0.9f, 24f);
            string valstr = value.ToString();
            TextFieldNumericLabeled2(fieldrect, labelKey.Translate(defaultValue), ref value, ref valstr, minValue, maxValue);

            // Reset button
            Rect btnRect = new Rect(fieldrect.xMax + 5f, fieldrect.y, row.xMax - fieldrect.xMax - 5f, row.height);
            if (Widgets.ButtonText(btnRect, "Reset".Translate()))
            {
                value = defaultValue;
            }
        }

        /* We make our own version of TextFieldNumericLabeled that gives more space to the text field */
        private static void TextFieldNumericLabeled2<T>(Rect rect, string label, ref T val, ref string buffer, float min = 0f, float max = 1E+09f) where T : struct
        {
            Rect rect_text = rect.LeftPart(0.89f).Rounded();
            Rect rect_numeric = rect.RightPart(0.1f).Rounded();
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(rect_text, label);
            Text.Anchor = anchor;
            Widgets.TextFieldNumeric(rect_numeric, ref val, ref buffer, min, max);
        }

        public void CheckForVGE()
        {
            if (LoadedModManager.RunningMods.Any(mod => mod.PackageId.ToLower() == "vanillaexpanded.gravship"))
            {
                isVGEActive = true;
            }
            DebugUtility.DebugLog($"VGEActive: {VGEActive}");
        }

    }
    public class GLWMod : Mod
    {
        public static GLWSettings settings = new GLWSettings();

        public GLWMod(ModContentPack content) : base(content)
        {
            Pack = content;
            settings = GetSettings<GLWSettings>();
            settings.CheckForVGE();
        }

        public ModContentPack Pack { get; }

        public override string SettingsCategory() => Pack.Name;

        public override void DoSettingsWindowContents(Rect inRect) => settings.DoWindowContents(inRect);
    }
}
