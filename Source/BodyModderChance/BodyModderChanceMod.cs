using Verse;
using HarmonyLib;
using UnityEngine;

namespace BodyModderChance
{
    public class BodyModderChanceMod : Mod
    {
        public static BodyModderChanceSettings Settings;

        public BodyModderChanceMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<BodyModderChanceSettings>();

            var harmony = new Harmony("DWitchcraft.bodymodderchance");
            harmony.PatchAll();

        }

        public override string SettingsCategory()
        {
            return "BMC_SettingsCategory".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();

            listing.Begin(inRect);

            listing.Label("BMC_BaseChance".Translate());

            if (listing.RadioButton("BMC_OnePercentDefault".Translate(), Settings.baseChancePercent == 1))
            {
                Settings.baseChancePercent = 1;
            }

            if (listing.RadioButton("2%", Settings.baseChancePercent == 2))
            {
                Settings.baseChancePercent = 2;
            }

            if (listing.RadioButton("3%", Settings.baseChancePercent == 3))
            {
                Settings.baseChancePercent = 3;
            }

            listing.End();
        }
    }
}