using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Diagnostics;
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

            _suspectVehicle = new Vehicle("BJXL", _sP) { IsPersistent = true };
            _suspectPed = new Ped(_sP) { IsPersistent = true, BlockPermanentEvents = true };

            _officerVehicle = Extensions.PoliceCar(_suspectVehicle.GetOffsetPositionFront(-15f));
            _officerPed = Extensions.PolicePed(_suspectVehicle.GetOffsetPositionFront(-15f));

            if (_suspectVehicle && _suspectPed && _officerVehicle && _officerPed)
            {
                _suspectPed.WarpIntoVehicle(_suspectVehicle, -1);
                _officerPed.WarpIntoVehicle(_officerVehicle, -1);

                _suspectVehicle.Heading = Extensions.ClosestVehicleHeading(_sP);
                _officerVehicle.Heading = _suspectVehicle.Heading;

                Extensions.DebugLog("Vehicles spawned = " + _suspectVehicle.Model + " and " + _officerVehicle.Model);
                Extensions.SpectateCameraNormal(_officerVehicle);
            }
            else
            {
                try
                {
                    if (!_suspectVehicle) throw new NullReferenceException("Suspect Vehicle");
                    else if (!_suspectPed) throw new NullReferenceException("Suspect Ped");
                    else if (!_officerVehicle) throw new NullReferenceException("Officer Vehicle");
                    else if (!_officerPed) throw new NullReferenceException("Officer Ped");
                    else throw new InvalidOperationException();
                }
                catch (ArgumentException ex)
                {
                    Extensions.PrintRecursiveExceptions(ex);
                }
                catch (InvalidOperationException ex)
                {
                    Extensions.PrintRecursiveExceptions(ex);
                }

                Game.DisplayNotification("Toasty Callouts has encountered an error, ending the callout to prevent any crashes.");
                End();
            }

            ShowCalloutAreaBlipBeforeAccepting(_sP, 50f);
            CalloutMessage = "Pursuit in Progress";
            CalloutPosition = _sP;

            Functions.PlayScannerAudioUsingPosition("", _sP);

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutDisplayed()
        {
            base.OnCalloutDisplayed();
        }

        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();
        }

        public override bool OnCalloutAccepted()
        {
            _pursuit = Functions.CreatePursuit();
            Functions.AddPedToPursuit(_pursuit, _suspectPed);
            Functions.AddCopToPursuit(_pursuit, _officerPed);
            Functions.SetPursuitIsActiveForPlayer(_pursuit, true);
            Functions.SetPursuitCopsCanJoin(_pursuit, false);
            PursuitVisual.Start(_pursuit, true);

            _suspectBlip = new Blip(_suspectVehicle)
            {
                Alpha = 0f,
                IsRouteEnabled = true
            };

            _suspectBlip.SetStandardColor(CalloutStandardization.BlipTypes.ENEMY);

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            if (Game.IsKeyDown(Keys.End)) End();

            if (Game.IsKeyDown(Keys.F8)) _officerPed.Kill();
            if (Game.IsKeyDown(Keys.F9)) _suspectVehicle.IsPositionFrozen = true;

            base.Process();
        }

        public override void End()
        {
            Extensions.EndAndClean(new Entity[] { _suspectPed, _suspectVehicle, _officerPed, _officerVehicle }, new[] { _suspectBlip }, new[] { _pursuit });

            base.End();
        }
    }
}
