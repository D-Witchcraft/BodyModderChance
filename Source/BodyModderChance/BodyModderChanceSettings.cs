using Verse;

namespace BodyModderChance
{
    public class BodyModderChanceSettings : ModSettings
    {
        public int baseChancePercent = 1;

        public override void ExposeData()
        {
            Scribe_Values.Look(
                ref baseChancePercent,
                "baseChancePercent",
                1
            );

            if (Scribe.mode == LoadSaveMode.PostLoadInit &&
    (baseChancePercent < 1 || baseChancePercent > 3))
            {
                baseChancePercent = 1;
            }
        }
    }
}