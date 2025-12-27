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
        public static int winduptime = GenDate.TicksPerDay;
        public static int launchwindow = GenDate.TicksPerHour * 6;
        public static bool printDebug = false;
        public static bool sendLetters = true;
        public static bool showAlerts = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref winduptime, "winduptime", GenDate.TicksPerDay, true);
            Scribe_Values.Look(ref launchwindow, "launchwindow", GenDate.TicksPerHour * 6, true);
            Scribe_Values.Look(ref printDebug, "printDebug", defaultValue: false);
            Scribe_Values.Look(ref sendLetters, "sendLetters", defaultValue: true);
            Scribe_Values.Look(ref showAlerts, "showAlerts", defaultValue: true);
        }

        string buffer1;
        string buffer2;
        public void DoWindowContents(Rect inRect)
        {
            var list = new Listing_Standard()
            {
                ColumnWidth = inRect.width
            };
            list.Begin(inRect);
            list.Label("glwwinduptime".Translate());
            // A windwup time of more than a year is insane. Why even have a gravship at that point? So we'll leave a year as the cap.
            list.TextFieldNumeric(ref winduptime, ref buffer1, 0, GenDate.TicksPerYear);

            list.Label("glwlaunchwindow".Translate());
            list.TextFieldNumeric(ref launchwindow, ref buffer2, 0, GenDate.TicksPerYear);

            list.CheckboxLabeled("glwsendLetters".Translate(), ref sendLetters);

            list.CheckboxLabeled("glwshowAlerts".Translate(), ref sendLetters);

            list.CheckboxLabeled("glwprintDebug".Translate(), ref printDebug);

            list.End();
        }

    }
    public class GLWMod : Mod
    {
        public static GLWSettings settings = new GLWSettings();

        public GLWMod(ModContentPack content) : base(content)
        {
            Pack = content;
            settings = GetSettings<GLWSettings>();
        }

        public ModContentPack Pack { get; }

        public override string SettingsCategory() => Pack.Name;

        public override void DoSettingsWindowContents(Rect inRect) => settings.DoWindowContents(inRect);
    }
}
