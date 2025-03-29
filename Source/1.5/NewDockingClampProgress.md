# Progress for creating a new docking clamp

## Why?
- The current docking clamp modifies the airlock in non-intuitive ways for ease of implementation
- Conceptually, a docking clamp need not involve an airlock to merely tow derelicts, etc.
- Work done for "ray spawners" could offer multiple types of docking clamps and turn the airlock itself into a separate but synchronized ray spawner for its flooring tiles

## What?
- Created RaySpawnZone to conceptualize areas where Things will be placed in a ray
- Created CompRaySpawner to add to Things
    - Currently only gives Transform label / PostDeSpawn messages
- Created CompProps_RaySpawner to set values for CompRaySpawner
    - thingDefToSpawn, range, sounds (todo: speed, animation style)
- Created PlaceWorker_LinkedRaySpawner for placing docking clamps or extendable walls with arbitrary ranges

## Notes
- Extending wall pieces should use custom linkType logic to show they are separate from other walls
    - Look at door linkage logic to ensure wall pieces link only in two dimensions unless they are an intersection
- Use StorageGroupUtility as an example for creating groups
- FleshmassMapComponent maintains two queues to track things to destroy and the ticks at which they should be destroyed. These are momentarily fed into a priority queue at runtime.
- Create a shared queue of tick ints to draw from when flicked on
	- When the tick manager gives this tick, activate first phase
- Phases:
	- Cleanup: Looks things in front of the spawn zone are pushed away
	- Spawn building: spawn the animated building that slides into place
	- Wait: wait one or more frames to ensure consistency


Saving is working, but might it be that ticks are preserved between saves, and the "start" tick is unnecessary?
```
					<thing Class="SaveOurShip2.Building_ShipHullExtendable">
						<def>ShipExtendableAirlock_HG</def>
						<id>ShipExtendableAirlock_HG9558</id>
						<map>0</map>
						<pos>(112, 0, 117)</pos>
						<health>500</health>
						<faction>Faction_10</faction>
						<questTags IsNull="True" />
						<spawnedTick>811326</spawnedTick>
						<s>833154</s>
						<et>
							<li>720</li>
							<li>960</li>
						</et>
						<sp>
							<li>Thing_ShipExtendingBeam_HG9600</li>
							<li>Thing_ShipExtendingBeam_HG9597</li>
							<li>Thing_ShipExtendingBeam_HG9596</li>
						</sp>
						<parentThing>null</parentThing>
					</thing>
```

Use sustainer for extend/retract sequence?
```
		public override void CompTickRare()
		{
			if (this.sustainer == null && !this.parent.Position.Fogged(this.parent.Map))
			{
				SoundInfo info = SoundInfo.InMap(new TargetInfo(this.parent.Position, this.parent.Map, false), MaintenanceType.PerTickRare);
				this.sustainer = SustainerAggregatorUtility.AggregateOrSpawnSustainerFor(this, SoundDefOf.FleshmassAmbience, info);
			}
			Sustainer sustainer = this.sustainer;
			if (sustainer == null)
			{
				return;
			}
			sustainer.Maintain();
		}

        		public override void PostDeSpawn(Map map)
		{
			if (this.sustainer != null)
			{
				if (this.sustainer.externalParams.sizeAggregator == null)
				{
					this.sustainer.externalParams.sizeAggregator = new SoundSizeAggregator();
				}
				this.sustainer.externalParams.sizeAggregator.RemoveReporter(this);
			}
		}
```

## Ideas
### Optimizations
- Space maps do not use things like WindManager, DeepResourceGrid, ExitMapGrid(?), SnowGrid, WaterInfo, TreeDestructionTracker(?), WeatherDecider (unless psychic stuff is weather), WildAnimalSpawner, WildPlantSpawner
    - These are generally sealed classes, so perhaps it would be better to edit MapPostTick, which does each ticking component with a try catch

### QOL
- Deorbit gizmo similar to Haul Urgently for removing items from map
- Control all docking clamps remotely if someone is at the helm