using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace SaveOurShip2
{
    /// <summary>
    /// Place worker for a 1x1 building with a ray spawner component that can link with others of its kind.
    /// Similar to Placeworker_SolarShip, but only requires one passible tile in front of it to be placed.
    /// </summary>
    public class PlaceWorker_LinkedRaySpawner : PlaceWorker
    {
        /*
        private static IntVec3 ClosestTo(IReadOnlyList<IntVec3> collection, IntVec3 target)
        {
            var closest = collection[0];
            var closestDist = collection[0].DistanceToSquared(target);
            for (int i = 1; i < collection.Count; i++)
            {
                var dist = collection[i].DistanceToSquared(target);
                if (dist > closestDist)
                    continue;

                closest = collection[i];
                closestDist = dist;                    
            }
            return closest;
        }*/

        private static (int mapIndex, long nextUpdateTick) LastCacheUpdate = (0, 0);
        private static List<Thing> OtherThingsOnMapCache = new List<Thing>();

        /// <summary>
        /// Draw a zone that represents the extended state of this ExtendableWall.
        /// If an extendable wall of the same ThingDef lies in its extension zone,
        /// use the closest to create a preview of where their extending walls would link.
        /// </summary>
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing _)
        {
            // NOTE: Thing is more than likely to be null.
            var props = def.GetCompProperties<CompProps_RaySpawner>();
            if (props == null)
                throw new System.NullReferenceException($"{nameof(PlaceWorker_LinkedRaySpawner)} should not be used with anything" +
                $"that does not also have {nameof(CompProps_RaySpawner)}.");

            var currentMap = Find.CurrentMap;
            var spawnZone = new RaySpawnZone(center, rot, props.range, currentMap);

            // Get linking candidates in case a "midpoint" tile will be created
            var ticks = Find.TickManager.TicksGame;
            if (LastCacheUpdate.mapIndex != currentMap.Index 
                || ticks > LastCacheUpdate.nextUpdateTick)
            {
                LastCacheUpdate = (currentMap.Index, ticks + 60);
                // TODO: Consider including other blueprints in visualization
                /*from t in Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint)
                    where t.def.entityDefToBuild == ThingDefOf.WatermillGenerator
                    select t);*/

                OtherThingsOnMapCache = currentMap.listerThings.ThingsOfDef(def);
            }

            List<IntVec3> otherZones = new List<IntVec3>();
            List<IntVec3> otherZoneHits = new List<IntVec3>();
            foreach (var tuple in spawnZone.FindCollidingSources(OtherThingsOnMapCache))
            {
                otherZones.AddRange(tuple.zone);
                otherZoneHits.Add(tuple.hit.Cell);
            }

            GenDraw.DrawFieldEdges(otherZones, Designator_Place.LowLightBgColor.ToOpaque());
            GenDraw.DrawFieldEdges(otherZoneHits, Designator_Place.CanPlaceColor.ToOpaque());
            GenDraw.DrawFieldEdges(spawnZone.ToList());
        }

        public override void PostPlace(Map map, BuildableDef def, IntVec3 loc, Rot4 rot)
        {
            // Get rid of building lookup cache
            LastCacheUpdate = (0, 0);
            OtherThingsOnMapCache.Clear();
            base.PostPlace(map, def, loc, rot);
        }

        public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            var minimumExpansionCell = center + (IntVec3.South.RotatedBy(rot));
            if (minimumExpansionCell.Impassable(map))
                return (AcceptanceReport)TranslatorFormattedStringExtensions.Translate("MustPlaceSolarShipWithFreeSpaces");

            return (AcceptanceReport)true;
        }
    }
}


// RimWorld.PlaceWorker_WatermillGenerator
// Token: 0x0600B744 RID: 46916 RVA: 0x0041B190 File Offset: 0x00419390
/*public override void DrawGhost(ThingDef def, IntVec3 loc, Rot4 rot, Color ghostCol, Thing thing = null)
{
    GenDraw.DrawFieldEdges(CompPowerPlantWater.GroundCells(loc, rot).ToList<IntVec3>(), Color.white, null);
    Color color = this.WaterCellsPresent(loc, rot, Find.CurrentMap) ? Designator_Place.CanPlaceColor.ToOpaque() : Designator_Place.CannotPlaceColor.ToOpaque();
    GenDraw.DrawFieldEdges(CompPowerPlantWater.WaterCells(loc, rot).ToList<IntVec3>(), color, null);
    bool flag = false;
    CellRect cellRect = CompPowerPlantWater.WaterUseRect(loc, rot);
    PlaceWorker_WatermillGenerator.waterMills.AddRange(Find.CurrentMap.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.WatermillGenerator).Cast<Thing>());
    PlaceWorker_WatermillGenerator.waterMills.AddRange(from t in Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint)
                                                       where t.def.entityDefToBuild == ThingDefOf.WatermillGenerator
                                                       select t);
    PlaceWorker_WatermillGenerator.waterMills.AddRange(from t in Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame)
                                                       where t.def.entityDefToBuild == ThingDefOf.WatermillGenerator
                                                       select t);
    foreach (Thing thing2 in PlaceWorker_WatermillGenerator.waterMills)
    {
        GenDraw.DrawFieldEdges(CompPowerPlantWater.WaterUseCells(thing2.Position, thing2.Rotation).ToList<IntVec3>(), new Color(0.2f, 0.2f, 1f), null);
        if (cellRect.Overlaps(CompPowerPlantWater.WaterUseRect(thing2.Position, thing2.Rotation)))
        {
            flag = true;
        }
    }
    PlaceWorker_WatermillGenerator.waterMills.Clear();
    Color color2 = flag ? new Color(1f, 0.6f, 0f) : Designator_Place.CanPlaceColor.ToOpaque();
    if (!flag || Time.realtimeSinceStartup % 0.4f < 0.2f)
    {
        GenDraw.DrawFieldEdges(CompPowerPlantWater.WaterUseCells(loc, rot).ToList<IntVec3>(), color2, null);
    }
}*/

//private static readonly CachedTexture EjectTex = new CachedTexture("UI/Gizmos/EjectAll");