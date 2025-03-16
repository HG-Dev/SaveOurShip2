using Verse;

namespace SaveOurShip2
{
    public class CompRaySpawner : ThingComp
    {
        private CompProps_RaySpawner _props;
        protected CompProps_RaySpawner Props
        {
            get
            {
                if (_props == null)
                    _props = (props as CompProps_RaySpawner) ?? new CompProps_RaySpawner();
                return _props;
            }
        }

        public ThingDef ThingDefToSpawn => Props.thingDefToSpawn;
        public int Range => Props.range;

        public override string TransformLabel(string label)
        {
            return base.TransformLabel(label) + $" (Hello from CompLinearSpawner! using {ThingDefToSpawn.defName} with range = {Range})";
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            Verse.Log.Message("[HG] DeSpawned " + nameof(CompRaySpawner));
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
        }

        public override void CompTick()
        {
            base.CompTick();
        }
    }
}
