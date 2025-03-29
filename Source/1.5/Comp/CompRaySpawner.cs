using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace SaveOurShip2
{
    public class CompRaySpawner : ThingComp
    {
        public enum SpawnState
        {
            NothingSpawned,   // Initial state
            DeSpawning,        // Removing spawned things
            Spawning,         // Creation in progress
            SpawningPaused,   // Creation was paused (due to obstacle)
            SpawningComplete, // Creation finished
        }

        [Unsaved]
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

        private const string StartTickAlias = "s";
        [LoadAlias(StartTickAlias)]
        private int _startTick = -1;

        private const string EventTicksAlias = "et";
        [LoadAlias(EventTicksAlias)]
        private Queue<int> _eventTicks = new Queue<int>();

        private const string SpawnedThingsAlias = "sp";
        [LoadAlias(SpawnedThingsAlias)]
        private Stack<Thing> _spawnedThings = new Stack<Thing>();

        [Unsaved]
        private RaySpawnZone _spawnZone;
        [Unsaved]
        private Map _map;


        public ThingDef ThingDefToSpawn => Props.thingDefToSpawn;
        public int Range => Props.range;
        public SpawnState State { get; private set; }
        public bool IsActiveState() => State >= SpawnState.Spawning;
        public float PercentActiveStateComplete()
        {
            switch (State)
            {
                case SpawnState.Spawning:
                case SpawnState.DeSpawning:
                    var allSteps = _spawnedThings.Count / (float)Range;
                    TryGetTicksUntilNextEvent(out _, out float percentage);
                    return allSteps + (percentage / Range);
                case SpawnState.SpawningPaused:
                    allSteps = _spawnedThings.Count / (float)Range;
                    return allSteps;
                default:
                    return 1f;
            }
        }

        public bool IsSpawnBlocked => !_spawnZone[_spawnedThings.Count].Standable(_map);

        public void StartSpawning()
        {
            State = SpawnState.Spawning;

            _startTick = Find.TickManager.TicksGame + 60;

            if (_spawnedThings is null)
                _spawnedThings = new Stack<Thing>(Range);

            var alreadyExisting = _spawnedThings.Count;
            Verse.Log.Message($"[HG] Start spawning -- range: {Range}, already exist: {alreadyExisting}");
            _eventTicks = new Queue<int>(Range);
            for (int i = 0; i < Range - alreadyExisting; i++)
                _eventTicks.Enqueue(i * Props.ticksPerSpawn + _startTick);

            Verse.Log.Message($"[HG] Initialized spawn ticks: {string.Join(", ", _eventTicks)}");
            //var request = new SpawnRequest(thingsToSpawn, 
            //    spawnPositions: _spawnZone.ToList(), 
            //    1, Props.ticksPerCell.TicksToSeconds());
                //new SpawnRequest(thingsToSpawn, 1, Props.ticksPerCell.TicksToSeconds());
            //parent.MapHeld.deferredSpawner.AddRequest(request, true);
            //_isSpawning = true;
            //_spawnTicksElapsed = 1;
        }

        public void StartDeSpawning()
        {
            State = SpawnState.DeSpawning;

            _startTick = Find.TickManager.TicksGame + 60;
            _eventTicks = new Queue<int>(Range);
            for (int i = 0; i < _spawnedThings.Count; i++)
                _eventTicks.Enqueue(-i * Props.ticksPerSpawn - _startTick);
            Verse.Log.Message("[HG] Initialized despawn ticks: " + string.Join(", ", _eventTicks));
        }

        public override string CompInspectStringExtra()
        {
            return State.ToString();
        }


        public override string TransformLabel(string label)
        {
            return base.TransformLabel(label) + $" (Hello from CompLinearSpawner! using {ThingDefToSpawn.defName} with range = {Range})";
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _map = parent.MapHeld;
            _spawnZone = new RaySpawnZone(parent, Props.range, _map);

            var thingsAreSpawned = _spawnedThings.Count > 0;
            var isSpawning = _eventTicks.Count > 0 && _eventTicks.Peek() > 0;
            var isDeSpawning = _eventTicks.Count > 0 && _eventTicks.Peek() < 0;

            if (isSpawning)
                State = SpawnState.Spawning;
            else if (isDeSpawning)
                State = SpawnState.DeSpawning;
            else if (_spawnedThings.Count == 0)
                State = SpawnState.NothingSpawned;
            else if (_spawnedThings.Count < Range)
                State = SpawnState.SpawningPaused;
            else if (_spawnedThings.Count == Range)
                State = SpawnState.SpawningComplete;
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            Verse.Log.Message("[HG] DeSpawned " + nameof(CompRaySpawner));
        }

        public override void PostExposeData()
        {
            if (_eventTicks.Count > 0)
                _eventTicks = new Queue<int>(_eventTicks.Select(tick => tick - _startTick));

            Scribe_Values.Look<int>(ref _startTick, StartTickAlias, -1, false);
            Scribe_Collections.Look<int>(ref _eventTicks, EventTicksAlias, LookMode.Value);
            Scribe_Collections.Look<Thing>(ref _spawnedThings, SpawnedThingsAlias, LookMode.Reference);

            if (_eventTicks.Count > 0)
                _eventTicks = new Queue<int>(_eventTicks.Select(tick => tick + _startTick));
        }

        private bool TryGetTicksUntilNextEvent(out int ticks, out float percentage)
        {
            ticks = 0;
            percentage = 1;
            if (_eventTicks.Count <= 0)
                return false;

            var nextTickRaw = _eventTicks.Peek();
            var ticksPerStep = (nextTickRaw > 0 ? Props.ticksPerSpawn : Props.ticksPerDeSpawn);

            if (ticksPerStep <= 0)
                return true;

            ticks = Find.TickManager.TicksGame - Math.Abs(nextTickRaw);
            percentage = 1 - (Math.Max(0, ticks) / (float)ticksPerStep);
            return true;
        }

        public override void CompTick()
        {
            if (!_spawnZone.Valid || _eventTicks.Count == 0)
                return;

            var nextTickRaw = _eventTicks.Peek();
            var currentTick = Find.TickManager.TicksGame;

            if (Math.Abs(nextTickRaw) > currentTick)
                return;

            // If is spawning
            if (nextTickRaw > 0)
            {
                State = SpawnState.Spawning;
                var spawnLocation = _spawnZone[_spawnedThings.Count];
                if (!spawnLocation.Standable(_map))
                {
                    // Spawn location is blocked!
                    _eventTicks.Clear();
                    Verse.Log.Message("[HG] SPAWN CANCELLED!");
                    State = SpawnState.SpawningPaused;
                    return;
                }
                //Verse.Log.Message($"[HG] SPAWN TICK! {currentTick - _startTick} > {nextTickRaw - _startTick} ");
                // Spawn thing!
                var spawned = GenSpawn.Spawn(ThingDefToSpawn, spawnLocation, _map, WipeMode.VanishOrMoveAside);
                spawned.Rotation = _spawnZone.Direction;
                _spawnedThings.Push(spawned);
            }
            else if (_spawnedThings.Count > 0)
            {
                // Despawn
                //Verse.Log.Message("[HG] DESPAWN TICK!");
                State = SpawnState.DeSpawning;
                var thing = _spawnedThings.Pop();
                thing.Destroy(DestroyMode.Vanish);
                // TODO: Try using WillReplace when using retract animation
            }

            _eventTicks.Dequeue();

            if (_eventTicks.Count == 0)
            {
                State = nextTickRaw > 0
                    ? SpawnState.SpawningComplete
                    : SpawnState.NothingSpawned;

                if (State is SpawnState.NothingSpawned && _spawnedThings.Count > 0)
                {
                    Verse.Log.Error($"CompRaySpawner failed to de-spawn {_spawnedThings.Count} things");
                }
            }
        }
    }
}
