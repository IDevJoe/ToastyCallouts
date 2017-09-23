using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System.Windows.Forms;

namespace ToastyCallouts.Callouts
{
    [CalloutInfo("TC_PursuitinProgress", CalloutProbability.Medium)]

    class PursuitinProgress : Callout
    {
        private Vector3 _sP;
        private LHandle _pursuit;
        private Ped _suspectPed, _officerPed;
        private Vehicle _suspectVehicle, _officerVehicle;
        private Blip _suspectBlip;

        public override bool OnBeforeCalloutDisplayed()
        {
            _sP = Spawnpoints.GetGoodSpawnpoint();

            ShowCalloutAreaBlipBeforeAccepting(_sP, 100f);
            CalloutMessage = "Pursuit in Progress";
            CalloutPosition = _sP;

            Functions.PlayScannerAudioUsingPosition("", _sP);

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutDisplayed()
        {
            _suspectVehicle = new Vehicle("BJXL", _sP) { IsPersistent = true };
            _suspectPed = new Ped(_sP) { IsPersistent = true, BlockPermanentEvents = true };

            _officerVehicle = Util.PoliceCar(_suspectVehicle.GetOffsetPositionFront(-15f));
            _officerPed = Util.PolicePed(_suspectVehicle.GetOffsetPositionFront(-15f));

            if (_suspectVehicle && _suspectPed && _officerVehicle && _officerPed)
            {
                _suspectPed.WarpIntoVehicle(_suspectVehicle, -1);
                _officerPed.WarpIntoVehicle(_officerVehicle, -1);

                _suspectVehicle.Heading = Natives.ClosestVehicleHeading(_sP);
                _officerVehicle.Heading = _suspectVehicle.Heading;

                _suspectVehicle.BeginRollingStart();
                _officerVehicle.BeginRollingStart();
                Util.Log("Vehicles spawned = " + _suspectVehicle.Model.Name + " and " + _officerVehicle.Model.Name, 0);
            }
            else
            {
                Cleaning.CatchInvalidObjects(new Entity[] { _suspectVehicle, _suspectPed, _officerVehicle, _officerPed });
                End();
            }

            base.OnCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            _pursuit = Functions.CreatePursuit();
            Functions.AddPedToPursuit(_pursuit, _suspectPed);
            Functions.AddCopToPursuit(_pursuit, _officerPed);
            Functions.SetPursuitIsActiveForPlayer(_pursuit, true);

            _suspectBlip = new Blip(_suspectVehicle)
            {
                Alpha = 0f,
                IsRouteEnabled = true
            };
            _suspectBlip.SetStandardColor(CalloutStandardization.BlipTypes.ENEMY);

            Util.SpectateCameraToggler(_officerPed, _suspectPed);
            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            if (Game.IsKeyDown(Keys.End)) End();
            base.Process();
        }

        public override void End()
        {
            Cleaning.EndAndClean(new Entity[] { _suspectPed, _suspectVehicle, _officerPed, _officerVehicle }, new[] { _suspectBlip }, new[] { _pursuit });

            base.End();
        }
    }
}
