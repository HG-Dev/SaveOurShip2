using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SaveOurShip2
{
    /// <summary>
    /// Comp properties for creating numerous "thingToSpawn" objects in a linear ray.
    /// Properties define what things can be created, and in what style.
    /// Tiles should be spawned one at a time.
    /// If a tile created by the extending spawner is destroyed, a breakdown occurs.
    /// </summary>
    public class CompProps_RaySpawner : CompProperties
    {
        public const int MinimumRange = 1;
        public const int MaximumRange = 100;

        public ThingDef thingDefToSpawn;
        public int range = 3;
        public SoundDef loopExtendingSound;
        public SoundDef singleExtendStep;
        public SoundDef singleExtendComplete;

        public RaySpawnZone CreateRaySpawnZone(Thing thing)
        {
            return new RaySpawnZone(thing, range, thing.Map);
        }

        public CompProps_RaySpawner()
        {
            compClass = typeof(CompRaySpawner);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            if (compClass == null)
                yield return parentDef.defName + " has CompProperties with null compClass.";
            if (range < MinimumRange || range > MaximumRange)
                yield return parentDef.defName + " has linear spawner settings with incorrect range: " + range.ToString();
        }

        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);
            if (thingDefToSpawn == null)
                thingDefToSpawn = RimWorld.ThingDefOf.Wall;
            if (loopExtendingSound == null)
                loopExtendingSound = RimWorld.SoundDefOf.MechChargerCharging;
            if (singleExtendStep == null)
                singleExtendStep = RimWorld.SoundDefOf.MechChargerStart;
            if (singleExtendComplete == null)
                singleExtendComplete = RimWorld.SoundDefOf.TutorMessageAppear;
        }
    }
}

/*
using System;
using Verse;

namespace RimWorld
{
    // Token: 0x020016EB RID: 5867
    public class CompBreakdownable : ThingComp
    {
        // Token: 0x170019A2 RID: 6562
        // (get) Token: 0x0600928D RID: 37517 RVA: 0x00320B10 File Offset: 0x0031ED10
        public bool BrokenDown
        {
            get
            {
                return this.brokenDownInt;
            }
        }

        // Token: 0x0600928E RID: 37518 RVA: 0x00320B18 File Offset: 0x0031ED18
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.brokenDownInt, "brokenDown", false, false);
        }

        // Token: 0x0600928F RID: 37519 RVA: 0x00320B34 File Offset: 0x0031ED34
        private void UpdateOverlays()
        {
            if (!this.parent.Spawned)
            {
                return;
            }
            this.parent.Map.overlayDrawer.Disable(this.parent, ref this.overlayBrokenDown);
            if (this.brokenDownInt)
            {
                this.overlayBrokenDown = new OverlayHandle?(this.parent.Map.overlayDrawer.Enable(this.parent, OverlayTypes.BrokenDown));
            }
        }

        // Token: 0x06009290 RID: 37520 RVA: 0x00320BA0 File Offset: 0x0031EDA0
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.powerComp = this.parent.GetComp<CompPowerTrader>();
            this.parent.Map.GetComponent<BreakdownManager>().Register(this);
            this.UpdateOverlays();
        }

        // Token: 0x06009291 RID: 37521 RVA: 0x00320BD6 File Offset: 0x0031EDD6
        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            map.GetComponent<BreakdownManager>().Deregister(this);
        }

        // Token: 0x06009292 RID: 37522 RVA: 0x00320BEB File Offset: 0x0031EDEB
        public void CheckForBreakdown()
        {
            if (this.CanBreakdownNow() && Rand.MTBEventOccurs(13680000f, 1f, 1041f))
            {
                this.DoBreakdown();
            }
        }

        // Token: 0x06009293 RID: 37523 RVA: 0x00320C11 File Offset: 0x0031EE11
        protected bool CanBreakdownNow()
        {
            return !this.BrokenDown && (this.powerComp == null || this.powerComp.PowerOn);
        }

        // Token: 0x06009294 RID: 37524 RVA: 0x00320C34 File Offset: 0x0031EE34
        public void Notify_Repaired()
        {
            this.brokenDownInt = false;
            this.parent.Map.GetComponent<BreakdownManager>().Notify_Repaired(this.parent);
            if (this.parent is Building_PowerSwitch)
            {
                this.parent.Map.powerNetManager.Notfiy_TransmitterTransmitsPowerNowChanged(this.parent.GetComp<CompPower>());
            }
            this.UpdateOverlays();
        }

        // Token: 0x06009295 RID: 37525 RVA: 0x00320C96 File Offset: 0x0031EE96
        public void DoBreakdown()
        {
            this.brokenDownInt = true;
            this.parent.BroadcastCompSignal("Breakdown");
            this.parent.Map.GetComponent<BreakdownManager>().Notify_BrokenDown(this.parent);
            this.UpdateOverlays();
        }

        // Token: 0x06009296 RID: 37526 RVA: 0x00320CD0 File Offset: 0x0031EED0
        public override string CompInspectStringExtra()
        {
            if (this.BrokenDown)
            {
                return "BrokenDown".Translate();
            }
            return null;
        }

        // Token: 0x0400521D RID: 21021
        private bool brokenDownInt;

        // Token: 0x0400521E RID: 21022
        private CompPowerTrader powerComp;

        // Token: 0x0400521F RID: 21023
        private const int BreakdownMTBTicks = 13680000;

        // Token: 0x04005220 RID: 21024
        public const string BreakdownSignal = "Breakdown";

        // Token: 0x04005221 RID: 21025
        private OverlayHandle? overlayBrokenDown;
    }
}
*/