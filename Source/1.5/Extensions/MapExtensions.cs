using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SaveOurShip2.Extensions
{
    public static class MapExtensions
    {
        /*
        private static HashSet<T> GetAllBuildingsWithDef<T>(this Map map, ThingDef def) where T : Thing
        {
            var buildings = new HashSet<T>();

            buildings.AddRange(map.listerBuildings.AllBuildingsColonistOfDef(def).Cast<T>());
            buildings.AddRange(map.listerBuildings.AllBuildingsNonColonistOfDef(def).Cast<T>());

            return buildings;
        }

        private static HashSet<T> GetAllBlueprintsAndFramesWithDef<T>(this Map map, ThingDef def) where T : Thing
        {
            var things = new HashSet<T>();
            
            foreach (var blueprint in map.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint))
                if (blueprint.def.entityDefToBuild == def)
                    things.Add(blueprint as T);
            foreach (var frame in map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame))
                if (frame.def.entityDefToBuild == def)
                    things.Add(frame as T);

            return things;
        }*/
    }
}
