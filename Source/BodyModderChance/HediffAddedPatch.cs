using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using Verse;


namespace BodyModderChance
{
    [HarmonyPatch(
        typeof(Pawn_HealthTracker),
        nameof(Pawn_HealthTracker.AddHediff),
        new Type[]
        {
            typeof(Hediff),
            typeof(BodyPartRecord),
            typeof(DamageInfo?),
            typeof(DamageWorker.DamageResult)
        }
    )]
    public static class HediffAddedPatch
    {
        public static void Postfix(Pawn_HealthTracker __instance, Hediff hediff)
        {
            if (!(hediff is Hediff_AddedPart))
            {
                return;
            }

            if (hediff.pawn == null)
            {
                return;
            }

            Pawn pawn = hediff.pawn;

            if (pawn.health == null || pawn.health.hediffSet == null)
            {
                return;
            }

            if (pawn.RaceProps == null || !pawn.RaceProps.Humanlike || !pawn.IsColonist)
            {
                return;
            }

            if (PawnGenerator.IsBeingGenerated(pawn))
            {
                return;
            }

            int existingSuperiorPartCount = pawn.health.hediffSet.hediffs
                .Where(h => h is Hediff_AddedPart)
                .Where(h => h != hediff)
                .Count(h =>
                    h.def.addedPartProps != null &&
                    h.def.addedPartProps.partEfficiency > 1f &&
                    !h.def.organicAddedBodypart
                );

            float partEfficiency = -1f;

            if (hediff.def.addedPartProps != null)
            {
                partEfficiency = hediff.def.addedPartProps.partEfficiency;
            }

            bool isSuperior = partEfficiency > 1f &&
                              !hediff.def.organicAddedBodypart;

            if (!isSuperior)
            {
                return;
            }

            if (pawn.story == null || pawn.story.traits == null)
            {
                return;
            }

            TraitDef bodyModderTrait = DefDatabase<TraitDef>.GetNamedSilentFail("Transhumanist");
            TraitDef bodyPuristTrait = DefDatabase<TraitDef>.GetNamedSilentFail("BodyPurist");

            if (bodyModderTrait == null)
            {
                return;
            }

            if (pawn.story.traits.HasTrait(bodyModderTrait))
            {
                return;
            }

            if (bodyPuristTrait != null)
            {
                Trait bodyPurist = pawn.story.traits.GetTrait(bodyPuristTrait);

                if (bodyPurist != null && bodyPurist.sourceGene != null)
                {
                    return;
                }
            }

            float efficiencyBonus;

            if (partEfficiency <= 1.35f)
            {
                efficiencyBonus = (partEfficiency - 1f) * 4f;
            }
            else
            {
                efficiencyBonus = 1.4f +
                    116f * (float)Math.Pow(partEfficiency - 1.35f, 2);
            }

            float baseChance = BodyModderChanceMod.Settings.baseChancePercent * 0.01f;

            float chance = baseChance +
                (existingSuperiorPartCount * 0.01f) +
                (efficiencyBonus * 0.01f);

            chance = Math.Min(chance, 0.15f);

            bool rollSucceeded = Rand.Chance(chance);


            if (rollSucceeded)
            {
                if (bodyPuristTrait != null &&
                    pawn.story.traits.HasTrait(bodyPuristTrait))
                {
                    Trait bodyPurist = pawn.story.traits.GetTrait(bodyPuristTrait);

                    if (bodyPurist != null)
                    {
                        pawn.story.traits.RemoveTrait(bodyPurist);

                        Find.LetterStack.ReceiveLetter(
                            "BMC_BodyPuristLostTitle".Translate(),
                            "BMC_BodyPuristLostDescription".Translate(
                                new NamedArgument(pawn, "PAWN")
                            ),
                            LetterDefOf.PositiveEvent,
                            pawn
                        );
                    }
                }
                else
                {
                    pawn.story.traits.GainTrait(new Trait(bodyModderTrait));

                    Find.LetterStack.ReceiveLetter(
                        "BMC_BodyModderGainedTitle".Translate(),
                        "BMC_BodyModderGainedDescription".Translate(
                            new NamedArgument(pawn, "PAWN")
                        ),
                        LetterDefOf.PositiveEvent,
                        pawn
                    );
                }
            }

        }
    }
}