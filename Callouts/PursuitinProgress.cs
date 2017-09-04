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

        private enum PlayerPursuitStatus
        {
            NONE,
            PLAYER_HAS_NO_VISUAL,
            PLAYER_HAS_VISUAL
        }

        PlayerPursuitStatus _status = PlayerPursuitStatus.NONE;

        private event EventHandler<bool> AIVisualChanged;
        private bool AIHasVisual;

        public override bool OnBeforeCalloutDisplayed()
        {
            _sP = Spawnpoints.GetGoodSpawnpoint();
            AIVisualChanged += OnAIVisualChanged;

            _suspectVehicle = new Vehicle("BJXL", _sP) { IsPersistent = true };
            _suspectPed = new Ped(_sP) { IsPersistent = true, BlockPermanentEvents = true };

            _officerVehicle = Extensions.PoliceCar(_suspectVehicle.GetOffsetPositionFront(-15f));
            _officerPed = Extensions.PolicePed(_suspectVehicle.GetOffsetPositionFront(-15f));

            if (_suspectVehicle && _suspectPed && _officerVehicle && _officerPed)
            {
                _suspectPed.WarpIntoVehicle(_suspectVehicle, -1);
                _officerPed.WarpIntoVehicle(_officerVehicle, -1);

                Extensions.SetOnGround(_suspectVehicle);
                Extensions.SetOnGround(_officerVehicle);
            }
            else
            {
                Game.LogTrivial("[TOASTYCALLOUTS PursuitinProgress - OnBeforeCalloutDisplayer()]: Vehicle or ped did not exist, ending callout and informing player.");
                Game.DisplayNotification("Toasty Callouts has encountered an error, ending the callout to prevent any crashes.");
                End();
            }

            ShowCalloutAreaBlipBeforeAccepting(_sP, 50f);
            CalloutMessage = "Pursuit in Progress";
            CalloutPosition = _sP;

            Functions.PlayScannerAudioUsingPosition("", _sP);

            return base.OnBeforeCalloutDisplayed();
        }

        private void OnAIVisualChanged(object sender, bool val)
        {
            if (!val)
            {
                Game.LogTrivial("[TOASTYCALLOUTS PursuitinProgress - DoesAIHaveVisualOnSuspect()]: AI lost visual of suspect, ending callout.");
                Functions.PlayScannerAudio("HELI_NO_VISUAL_DISPATCH_02 10-4 CODE4");
                End();
            }
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

            _suspectBlip = new Blip(_suspectVehicle)
            {
                Alpha = 0f,
                IsRouteEnabled = true
            };

            _suspectBlip.SetStandardColor(CalloutStandardization.BlipTypes.ENEMY);

            if (Extensions.IsEntityVisible(_suspectVehicle))
            {
                Game.LogTrivial("[TOASTYCALLOUTS PursuitinProgress - OnCalloutAccepted()]: _suspectVehicle is visible, changing _status to PLAYER_HAS_VISUAL.");
                _status = PlayerPursuitStatus.PLAYER_HAS_VISUAL;
            }

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            if (Game.IsKeyDown(Keys.End)) End();

            switch (_status)
            {
                case PlayerPursuitStatus.NONE:
                    PursuitUpdates();
                    DoesAIHaveVisualOnSuspect();
                    _status = PlayerPursuitStatus.PLAYER_HAS_NO_VISUAL;

                    break;
                case PlayerPursuitStatus.PLAYER_HAS_NO_VISUAL:
                    if (_suspectPed && Extensions.IsEntityVisible(_suspectPed)) _status = PlayerPursuitStatus.PLAYER_HAS_VISUAL;

                    break;
                case PlayerPursuitStatus.PLAYER_HAS_VISUAL:
                    if (_suspectBlip) _suspectBlip.Delete();

                    break;
            }

            bool v = Extensions.IsEntityVisible(_suspectPed, _officerPed);
            if (AIHasVisual != v)
            {
                AIHasVisual = v;
                AIVisualChanged?.Invoke(this, AIHasVisual);
            }

            base.Process();
        }

        private void PursuitUpdates()
        {
            GameFiber.StartNew(delegate
            {
                Game.LogTrivial("[TOASTYCALLOUTS PursuitinProgress - PursuitUpdates()]: Creating stopwatch.");

                Stopwatch timer = new Stopwatch();
                timer.Start();

                while (true)
                {
                    GameFiber.Yield();

                    if (_suspectVehicle && timer.ElapsedMilliseconds >= 10000)
                    {
                        timer.Restart();
                        Functions.PlayScannerAudioUsingPosition("SUSPECT_IS IN_OR_ON_POSITION", _suspectVehicle.Position);
                    }

                    if (_status == PlayerPursuitStatus.PLAYER_HAS_VISUAL || !Functions.IsPursuitStillRunning(_pursuit))
                    {
                        timer.Reset();
                        break;
                    }
                }
            });
        }

        private void DoesAIHaveVisualOnSuspect()
        {
            GameFiber.StartNew(delegate
            {
                while (true)
                {
                    GameFiber.Yield();

                    if (_officerPed && _officerVehicle && _status != PlayerPursuitStatus.PLAYER_HAS_VISUAL && !Extensions.IsEntityVisible(_officerVehicle, _suspectVehicle))
                    {
                        Game.LogTrivial("[TOASTYCALLOUTS PursuitinProgress - DoesAIHaveVisualOnSuspect()]: AI lost visual of suspect, ending callout.");
                        Functions.PlayScannerAudio("HELI_NO_VISUAL_DISPATCH_02 10-4 CODE4");
                        End();
                    }
                }
            });
        }

        public override void End()
        {
            Extensions.EndAndClean(new Entity[] { _suspectPed, _suspectVehicle, _officerPed, _officerVehicle }, new[] { _suspectBlip }, new[] { _pursuit });

            base.End();
        }
    }
}
