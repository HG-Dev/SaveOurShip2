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

```
Tried to use an uninitialized DefOf of type SoundDefOf. DefOfs are initialized right after all defs all loaded. Uninitialized DefOfs will return only nulls. (hint: don't use DefOfs as default field values in Defs, try to resolve them in ResolveReferences() instead) Debug info: DirectXmlToObject is currently instantiating an object of type SaveOurShip2.CompProps_RaySpawner
```

## Ideas
### Optimizations
- Space maps do not use things like WindManager, DeepResourceGrid, ExitMapGrid(?), SnowGrid, WaterInfo, TreeDestructionTracker(?), WeatherDecider (unless psychic stuff is weather), WildAnimalSpawner, WildPlantSpawner
    - These are generally sealed classes, so perhaps it would be better to edit MapPostTick, which does each ticking component with a try catch

### QOL
- Deorbit gizmo similar to Haul Urgently for removing items from map