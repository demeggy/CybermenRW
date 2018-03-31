using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;

namespace Cybermen
{
    public class Building_CyberConverter : Building_Casket
    {

        private CompPowerTrader powerComp;

        [DefOf]
        class CyberConverter_JobDef
        {
            public static JobDef EnterCyberConverter;
            public static JobDef CarryToCyberConverter;
        }

        [DefOf]
        public class Cyberman_DefOf
        {
            public static PawnKindDef Cyberman;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = base.GetComp<CompPowerTrader>();
        }

        public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (base.TryAcceptThing(thing, allowSpecialEffects))
            {
                if (allowSpecialEffects)
                {
                    SoundDef.Named("CryptosleepCasketAccept").PlayOneShot(new TargetInfo(base.Position, base.Map, false));
                }
                return true;
            }
            return false;
        }

        [DebuggerHidden]
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {

            if (!this.powerComp.PowerOn)
            {
                {
                    FloatMenuOption failer = new FloatMenuOption("CannotUseNoPower".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                    yield return failer;
                }
            }
            else
            {
                foreach (FloatMenuOption o in base.GetFloatMenuOptions(myPawn))
                {
                    yield return o;
                }
                if (this.innerContainer.Count == 0)
                {
                    if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly, false, TraverseMode.ByPawn))
                    {
                        FloatMenuOption failer = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                        yield return failer;
                    }
                    else
                    {
                        JobDef jobDef = CyberConverter_JobDef.EnterCyberConverter;
                        string jobStr = "EnterConversionUnit".Translate();
                        Action jobAction = delegate
                        {
                            Job job = new Job(jobDef, this);
                            myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                            //bUpgrade = true;
                        };
                        yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn, this, "ReservedBy");
                    }
                }
            }
        }

        [DebuggerHidden]
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo c in base.GetGizmos())
            {
                yield return c;
            }
            if (base.Faction == Faction.OfPlayer && this.innerContainer.Count > 0 && this.def.building.isPlayerEjectable)
            {
                Command_Action eject = new Command_Action();
                eject.action = new Action(this.EjectContents);
                eject.defaultLabel = "CommandPodEject".Translate();
                eject.defaultDesc = "CommandPodEjectDesc".Translate();
                if (this.innerContainer.Count == 0)
                {
                    eject.Disable("CommandPodEjectFailEmpty".Translate());
                }
                eject.hotKey = KeyBindingDefOf.Misc1;
                eject.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject", true);
                yield return eject;
            }
        }

        bool bUpgrade;
        int iCounter = 0;

        public override void Tick()
        {

            if (this.innerContainer.Count > 0 && this.powerComp.PowerOn)
            {
                ++iCounter;
                Log.Warning("Upgrade: " + iCounter);
                if (iCounter > 2500)
                {
                    bUpgrade = true;
                    EjectContents();
                }
            }
            base.Tick();
        }

        public override void EjectContents()
        {
            ThingDef filthSlime = ThingDefOf.FilthSlime;
            foreach (Thing current in ((IEnumerable<Thing>)this.innerContainer))
            {
                Pawn pawn = current as Pawn;

                BodyPartRecord LeftLeg = pawn.health.hediffSet.GetNotMissingParts().First(bpr => bpr.def == BodyPartDefOf.LeftLeg);
                BodyPartRecord RightLeg = pawn.health.hediffSet.GetNotMissingParts().First(bpr => bpr.def == BodyPartDefOf.RightLeg);
                BodyPartRecord LeftArm = pawn.health.hediffSet.GetNotMissingParts().First(bpr => bpr.def == BodyPartDefOf.LeftArm);
                BodyPartRecord RightArm = pawn.health.hediffSet.GetNotMissingParts().First(bpr => bpr.def == BodyPartDefOf.RightArm);

                if (pawn != null)
                {
                    if(bUpgrade)
                    {
                        PawnKindDef cyberman = Cyberman_DefOf.Cyberman;
                        Pawn newPawn = PawnGenerator.GeneratePawn(cyberman, Faction.OfPlayer);
                        GenSpawn.Spawn(newPawn, this.Position, this.MapHeld);
                        //BodyPartRecord Heart = pawn.health.hediffSet.GetNotMissingParts().First(bpr => bpr.def == BodyPartDefOf.Heart);
                        //pawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, 100, 0, null, Heart, null));
                        pawn.Destroy();
                        bUpgrade = false;
                    }
                    else
                    {
                        if (iCounter < 500)
                        {
                            pawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, 100, 0, null, LeftArm, null));
                        }
                        if (iCounter >500 && iCounter < 1000)
                        {
                            pawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, 100, 0, null, LeftArm, null));
                            pawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, 100, 0, null, RightArm, null));
                        }
                        if (iCounter > 1000 && iCounter < 1500)
                        {
                            pawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, 100, 0, null, LeftArm, null));
                            pawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, 100, 0, null, RightArm, null));
                            pawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, 100, 0, null, LeftLeg, null));
                        }
                        if (iCounter > 1500 && iCounter < 2499)
                        {
                            pawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, 100, 0, null, LeftArm, null));
                            pawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, 100, 0, null, RightArm, null));
                            pawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, 100, 0, null, LeftLeg, null));
                            pawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, 100, 0, null, RightLeg, null));
                        }
                    }
                    //PawnComponentsUtility.AddComponentsForSpawn(pawn);
                    //pawn.filth.GainFilth(filthSlime);
                    this.innerContainer.TryDropAll(this.InteractionCell, base.Map, ThingPlaceMode.Near);
                    this.contentsKnown = true;
                    iCounter = 0;
                }
            }
            if (!base.Destroyed)
            {
                SoundDef.Named("CryptosleepCasketEject").PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map, false), MaintenanceType.None));
            }
            //base.EjectContents();
            
        }

        public static Building_CyberConverter FindCryptosleepCasketFor(Pawn p, Pawn traveler, bool ignoreOtherReservations = false)
        {
            IEnumerable<ThingDef> enumerable = from def in DefDatabase<ThingDef>.AllDefs
                                               where typeof(Building_CyberConverter).IsAssignableFrom(def.thingClass)
                                               select def;
            foreach (ThingDef current in enumerable)
            {
                Building_CyberConverter building_CyberConverter = (Building_CyberConverter)GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForDef(current), PathEndMode.InteractionCell, TraverseParms.For(traveler, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, delegate (Thing x)
                {
                    bool arg_33_0;
                    if (!((Building_CyberConverter)x).HasAnyContents)
                    {
                        Pawn traveler2 = traveler;
                        LocalTargetInfo target = x;
                        bool ignoreOtherReservations2 = ignoreOtherReservations;
                        arg_33_0 = traveler2.CanReserve(target, 1, -1, null, ignoreOtherReservations2);
                    }
                    else
                    {
                        arg_33_0 = false;
                    }
                    return arg_33_0;
                }, null, 0, -1, false, RegionType.Set_Passable, false);
                if (building_CyberConverter != null)
                {
                    return building_CyberConverter;
                }
            }
            return null;
        }

    }
}
