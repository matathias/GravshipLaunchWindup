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
    public class GLWSettings : ModSettings
    {
        public static int winduptime = 12;
        public static int launchwindow = 6;
        public static bool printDebug = false;
        public static bool sendLetters = true;
        public static bool showAlerts = true;

        public static bool VGEActive = false;
        public static int winduptime_jumper = 6;
        public static int launchwindow_jumper = 3;
        public static int winduptime_hulk = 24;
        public static int launchwindow_hulk = 12;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref winduptime, "winduptime", 12, true);
            Scribe_Values.Look(ref launchwindow, "launchwindow", 6, true);
            Scribe_Values.Look(ref printDebug, "printDebug", defaultValue: false);
            Scribe_Values.Look(ref sendLetters, "sendLetters", defaultValue: true);
            Scribe_Values.Look(ref showAlerts, "showAlerts", defaultValue: true);

            Scribe_Values.Look(ref winduptime_jumper, "winduptime_jumper", 6);
            Scribe_Values.Look(ref launchwindow_jumper, "launchwindow_jumper", 3);
            Scribe_Values.Look(ref winduptime_hulk, "winduptime_hulk", 24);
            Scribe_Values.Look(ref launchwindow_hulk, "launchwindow_hulk", 12);
        }

        string buffer1;
        string buffer2;
        string buffer3;
        string buffer4;
        string buffer5;
        string buffer6;
        public void DoWindowContents(Rect inRect)
        {
            var list = new Listing_Standard()
            {
                ColumnWidth = inRect.width
            };
            list.Begin(inRect);

            if (VGEActive)
            {
                list.Label("glwwinduptimejumper".Translate());
                list.TextFieldNumeric(ref winduptime_jumper, ref buffer1, 0, GenDate.TicksPerYear);

                list.Label("glwlaunchwindowjumper".Translate());
                list.TextFieldNumeric(ref launchwindow_jumper, ref buffer2, 0, GenDate.TicksPerYear);
            }
            
            list.Label("glwwinduptime".Translate());
            list.TextFieldNumeric(ref winduptime, ref buffer3, 0, GenDate.TicksPerYear);

            list.Label("glwlaunchwindow".Translate());
            list.TextFieldNumeric(ref launchwindow, ref buffer4, 0, GenDate.TicksPerYear);

            if (VGEActive)
            {
                list.Label("glwwinduptimehulk".Translate());
                list.TextFieldNumeric(ref winduptime_hulk, ref buffer5, 0, GenDate.TicksPerYear);

                list.Label("glwlaunchwindowhulk".Translate());
                list.TextFieldNumeric(ref launchwindow_hulk, ref buffer6, 0, GenDate.TicksPerYear);
            }

            list.CheckboxLabeled("glwsendLetters".Translate(), ref sendLetters);

            list.CheckboxLabeled("glwshowAlerts".Translate(), ref showAlerts);

            list.CheckboxLabeled("glwprintDebug".Translate(), ref printDebug);

            list.End();
        }

        public void CheckForVGE()
        {
            if (LoadedModManager.RunningMods.Any(mod => mod.PackageId.ToLower() == "vanillaexpanded.gravship"))
            {
                VGEActive = true;
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
