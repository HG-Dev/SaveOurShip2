using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace SaveOurShip2
{
    public class Building_ShipHullExtendable : Building
    {
        //private CompFlickable _flickable;
        private CompPowerTrader _powerTrader;
        private CompRaySpawner _spawner;
        //public override Graphic Graphic => this.flickableComp.CurrentGraphic;

        private Texture2D ExtendIcon => ContentFinder<Texture2D>.Get("UI/DockingOn");
        private Texture2D RetractIcon => ContentFinder<Texture2D>.Get("UI/DockingOff");

        private Texture2D CurrentToggleIcon;
        private Command_ActionWithCooldown ToggleCommand;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this._spawner = GetComp<CompRaySpawner>();
            this._powerTrader = GetComp<CompPowerTrader>();

            if (_spawner == null)
                throw new System.MissingFieldException(nameof(_spawner));
            if (_powerTrader == null)
                throw new System.MissingFieldException(nameof(_powerTrader));

            CurrentToggleIcon = ExtendIcon;
            ToggleCommand = new Command_ActionWithCooldown
            {
                action = ToggleIntentToSpawnOrDeSpawn,
                activateSound = SoundDefOf.FlickSwitch,
                defaultLabel = TranslatorFormattedStringExtensions.Translate("SoS.ToggleDock"),
                defaultDesc = TranslatorFormattedStringExtensions.Translate("SoS.ToggleDockDesc"),
                cooldownPercentGetter = _spawner.PercentActiveStateComplete
            };
            ToggleCommand.disabled = true;
        }

        protected override void ReceiveCompSignal(string signal)
        {
            switch (signal)
            {
                // Off and on are reversed because there's no easy way to start FlickedOn as 'false'
                case CompFlickable.FlickedOffSignal:
                    _spawner.StartSpawning();
                    break;
                case CompFlickable.FlickedOnSignal:
                    _spawner.StartDeSpawning();
                    break;
            }
        }

        /*public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (!FlickUtility.WantsToBeOn(this))
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.AppendLine();
                }
                stringBuilder.Append("VentClosed".Translate());
            }
            return stringBuilder.ToString();
        }*/

        private void ToggleIntentToSpawnOrDeSpawn()
        {
            //SoundDefOf.FlickSwitch.PlayOneShot(new TargetInfo(Position, Map));
            switch (_spawner.State)
            {
                case CompRaySpawner.SpawnState.NothingSpawned:
                case CompRaySpawner.SpawnState.DeSpawning:
                case CompRaySpawner.SpawnState.SpawningPaused when !_spawner.IsSpawnBlocked:
                    _spawner.StartSpawning();
                    break;

                default:
                    _spawner.StartDeSpawning();
                    break;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
                yield return gizmo;

            if (_spawner.IsActiveState())
                CurrentToggleIcon = ExtendIcon;
            else
                CurrentToggleIcon = RetractIcon;

            ToggleCommand.icon = CurrentToggleIcon;
            ToggleCommand.disabled = !_powerTrader.PowerOn || Faction != Faction.OfPlayer;

            yield return ToggleCommand;
        }
    }
}
