using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace SaveOurShip2
{
    public readonly struct RaySpawnZone : IEnumerable<IntVec3>, IReadOnlyList<IntVec3>
    {
        /// <summary>
        /// The origin cell of the spawn zone. Not inside the spawn zone.
        /// </summary>
        public readonly IntVec3 Origin;
        /// <summary>
        /// Direction of spawn action, emanating from origin.
        /// </summary>
        public readonly Rot4 Direction;
        /// <summary>
        /// CellRect where objects may be spawned.
        /// </summary>
        public readonly IntVec3 End;
        /// <summary>
        /// The intended maximum range of the spawn zone.
        /// </summary>
        public readonly int MaxRange;
        /// <summary>
        /// Normalized vector in direction of spawn action.
        /// </summary>
        public readonly IntVec3 Forward;
        /// <summary>
        /// The map index this zone was create for.
        /// </summary>
        public readonly int MapIndex;
        /// <summary>
        /// The actual range of the spawn zone, clamped by map size.
        /// </summary>
        public int Range => (int)End.DistanceTo(Origin);
        public bool Valid => !End.Equals(Origin);

        public override int GetHashCode()
        {
            // Expectation: only one source can exist at any Origin on a given Map.
            return Tuple.Create(Origin, MapIndex).GetHashCode();
        }

        public RaySpawnZone(Thing source, int range, Map map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            MapIndex = map.Index;
            Origin = source.Position;
            MaxRange = range;
            Direction = source.Rotation;
            Forward = IntVec3.South.RotatedBy(Direction);
            var start = source.Position + Forward;
            if (!start.ClampInsideMap(map).Equals(start))
            {
                End = Origin;
                return;
            }

            End = (source.Position + Forward * range).ClampInsideMap(map);
        }

        public RaySpawnZone(IntVec3 center, Rot4 rot, int range, Map map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(Map));

            MapIndex = map.Index;
            Origin = center;
            MaxRange = range;
            Direction = rot;
            Forward = IntVec3.South.RotatedBy(Direction);
            var start = center + Forward;
            // If the first cell of the zone is outside the map,
            // this is invalid
            if (!start.ClampInsideMap(map).Equals(start))
            {
                End = Origin;
                return;
            }

            End = (center + Forward * range).ClampInsideMap(map);
        }

        public CellRect ToCellRect => CellRect.FromCellList(new IntVec3[] { Origin + Forward, End });

        public int Count => Range;

        public IntVec3 this[int index]
        {
            get
            {
                index = Math.Max(index, -1);
                return Origin + Forward * Math.Min(index + 1, Range);
            }
        }

        public override string ToString()
        {
            return $"RaySpawnZone from M[{MapIndex}]:{Origin} with unblocked terminus of {End}";
        }

        public List<IntVec3> ToList() => new List<IntVec3>(this);

        /*public int Count => Range;

        public IntVec3 this[int index]
        {
            get
            {
                using (var enumerator = new Enumerator(this))
                {
                    for (; index > 0 && enumerator.MoveNext(); index--) { }
                    return enumerator.Current;
                }
            }
        }*/

        private bool IsSameMap(Map map) => map.Index == MapIndex;
        /// <summary>
        /// Filter a list of things that could extend to reach or intrude on a given extension zone.
        /// </summary>
        public IEnumerable<(Thing src, RaySpawnZone zone, RayOverlap hit)> FindCollidingSources(IEnumerable<Thing> othersOnMap)
        {
            if (!othersOnMap.Any())
                yield break;

            foreach (var other in othersOnMap)
            {
                // Cannot link to anything extending in the same direction-- including self
                if (other.Rotation.Equals(Direction))
                    continue;

                UnityEngine.Debug.Assert(IsSameMap(other.Map));
                var otherZone = new RaySpawnZone(other, MaxRange, other.Map);

                // Find the intersection closest to the host center
                if (!TryPredictLinkTerminus(otherZone, out var terminus))
                    continue; // No intersection found

                yield return (other, otherZone, terminus);
            }
        }

        /// <summary>
        /// Using this RaySpawnZone and one 'other', confirm they intersect
        /// and return the point at which this ray spawner will stop if the other
        /// zone is used at the same time.
        /// </summary>
        public bool TryPredictLinkTerminus(RaySpawnZone other, out RayOverlap terminus)
        {
            terminus = RayOverlap.Invalid;
            if (Direction == other.Direction)
                return false;

            var selfSet = new List<IntVec3>(this);
            var otherSet = new List<IntVec3>(other);

            // Find first intersection
            for (int i = 0; i < selfSet.Count; i++)
                for (int j = 0; j < otherSet.Count; j++)
                    if (otherSet[j].Equals(selfSet[i]))
                    {
                        terminus = new RayOverlap(selfSet[i], i, j);
                        return true;
                    }

            return false;
        }

        public readonly struct RayOverlap
        {
            public readonly IntVec3 Cell;
            public readonly int AlphaDist;
            public readonly int BetaDist;
            public bool IsValid => Cell != IntVec3.Invalid;

            public bool AlphaStops => AlphaDist >= BetaDist;
            public bool BetaStops => BetaDist >= AlphaDist;
            public bool IsLinkTerminus => AlphaDist == BetaDist;

            public static RayOverlap Invalid
                => new RayOverlap(IntVec3.Invalid, 0, 0);

            public RayOverlap(IntVec3 cell, int alphaDist, int betaDist) =>
                (Cell, AlphaDist, BetaDist) = (cell, alphaDist, betaDist);
        }

        public struct Enumerator : IEnumerator<IntVec3>, IEnumerator, IDisposable
        {
            private IntVec3 pos;
            private int i;
            private readonly int range;
            private readonly IntVec3 forward;

            public IntVec3 Current => pos;

            object IEnumerator.Current => pos;

            public Enumerator(RaySpawnZone zone)
            {
                pos = zone.Origin;
                i = 0;
                range = zone.Range;
                forward = zone.Forward;
            }

            public bool MoveNext()
            {
                pos += forward;
                i++;
                return i <= range;
            }

            public void Reset()
            {
                pos -= forward * i;
                i = 0;
            }

            void IDisposable.Dispose()
            {
            }
        }

        public IEnumerator<IntVec3> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
}
