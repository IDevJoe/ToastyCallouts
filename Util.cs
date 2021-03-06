﻿using LSPD_First_Response.Engine.Scripting;
using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ToastyCallouts
{
    static class Util
    {
        /// <summary>Spawn a police car based on the respective Vector3.</summary>
        public static Vehicle PoliceCar(Vector3 spawnpoint)
        { //police, police2, police3, police4, policeb, sheriff, sheriff2
            Model[] cityCarModels =
            {
                "POLICE",
                "POLICE2",
                "POLICE3",
                "POLICE4"
            };

            Model[] sheriffCarModels =
            {
                "SHERIFF",
                "SHERIFF2"
            };

            Model[] selectCarModels;
            WorldZone worldZone = Functions.GetZoneAtPosition(spawnpoint);

            switch (worldZone.County)
            {
                case EWorldZoneCounty.LosSantos:
                case EWorldZoneCounty.LosSantosCounty:
                    Game.LogTrivial("[TOASTYCALLOUTS]: Identifying county as.. " + worldZone.County + ". Spawning cityCarModels.");
                    selectCarModels = cityCarModels;

                    break;
                case EWorldZoneCounty.BlaineCounty:
                    Game.LogTrivial("[TOASTYCALLOUTS]: Identifying county as.. " + worldZone.County + ". Spawning sheriffCarModels.");
                    selectCarModels = sheriffCarModels;

                    break;
                default:
                    Game.LogTrivial("[TOASTYCALLOUTS]: Identifying county as.. " + worldZone.County + ". Spawning policeCarModels.");
                    selectCarModels = ((Model[])cityCarModels.Clone()).Union(sheriffCarModels).ToArray();

                    break;
            }

            return new Vehicle(MathHelper.Choose(selectCarModels), spawnpoint, Natives.ClosestVehicleHeading(spawnpoint));
        }

        /// <summary>Spawn a police ped based on the respective Vector3.</summary>
        public static Ped PolicePed(Vector3 spawnpoint)
        { //S_M_Y_COP_01, S_F_Y_COP_01, S_M_Y_SHERIFF_01, S_F_Y_SHERIFF_01, CSB_COP, S_M_Y_HWAYCOP_01
            Model[] cityPedModels =
            {
                "S_M_Y_COP_01",
                "S_F_Y_COP_01"
            };

            Model[] sheriffPedModels =
            {
                "S_M_Y_SHERIFF_01",
                "S_F_Y_SHERIFF_01"
            };

            Model[] selectPedModels;
            WorldZone worldZone = Functions.GetZoneAtPosition(spawnpoint);

            switch (worldZone.County)
            {
                case EWorldZoneCounty.LosSantos:
                case EWorldZoneCounty.LosSantosCounty:
                    Game.LogTrivial("[TOASTYCALLOUTS]: Identifying county as.. " + worldZone.County + ". Spawning cityPedModels.");
                    selectPedModels = cityPedModels;

                    break;
                case EWorldZoneCounty.BlaineCounty:
                    Game.LogTrivial("[TOASTYCALLOUTS]: Identifying county as.. " + worldZone.County + ". Spawning sheriffPedModels.");
                    selectPedModels = sheriffPedModels;

                    break;
                default:
                    Game.LogTrivial("[TOASTYCALLOUTS]: Identifying county as.. " + worldZone.County + ". Spawning policePedModels.");
                    selectPedModels = ((Model[])cityPedModels.Clone()).Union(sheriffPedModels).ToArray();

                    break;
            }

            return new Ped(MathHelper.Choose(selectPedModels), spawnpoint, Natives.ClosestVehicleHeading(spawnpoint));
        }

        /// <summary>Check if an entity is visible to another entity. If ent2 is null, ent1 will be in check with the local player.</summary>
        public static bool IsEntityVisible(Entity ent1, Entity ent2 = null)
        {
            HitResult result;

            if (ent2 == null && ent1)
            {
                Entity[] entitiesToIgnore = { Main.Player, ent1 };
                result = World.TraceLine(Main.Player.Position, ent1.Position, TraceFlags.IntersectWorld, entitiesToIgnore);

                return ent1 && !result.Hit;
            }

            if (ent2 != null && ent1 && ent2)
            {
                Entity[] entitiesToIgnore = { Main.Player, ent1, ent2 };
                result = World.TraceLine(ent1.Position, ent2.Position, TraceFlags.IntersectWorld, entitiesToIgnore);

                return !result.Hit;
            }

            return true;
        }

        /// <summary>Write out logs for debugging and information purposes.</summary>
        /// <param name="text"></param>
        /// <param name="val">Whether to display..
        /// <para>0 = DEBUG Log</para>
        /// <para>1 = INFO Log</para>
        /// </param>
        public static void Log(string text, int val)
        {
            string logType;
            switch (val)
            {
                case 0: //DEBUG Log
                    logType = "DEBUG";
                    break;
                case 1: //INFO Log
                    logType = "INFO";
                    break;
                default:
                    logType = "OTHER";
                    break;
            }

            Game.LogTrivialDebug(string.Format("[TOASTY CALLOUTS - {0}]: {1}", logType, text));
        }

        /// <summary>Corrects a vector3 to match the relative ground position.</summary>
        /// <param name="position">The vector3 to effect.</param>
        /// <param name="groundLevelIncrement"></param>
        /// <param name="treatWaterAsGround"></param>
        /// <param name="anyMeans"></param>
        public static Vector3 SetOnGround(Vector3 position, float groundLevelIncrement = 0.25f, bool treatWaterAsGround = false, bool anyMeans = true)
        {
            var groundZ = World.GetGroundZ(position, false, true);
            if (groundZ != null) position = new Vector3(position.X, position.Y, (float)groundZ + groundLevelIncrement);
            return position;
        }

        /// <summary>Creates a camera 13m above the given entity.</summary>
        /// <param name="entToSpectate">The entity that you want to spectate.</param>
        /// <param name="keyForDefaultCam">The key to press to revert back to the normal player camera.</param>
        public static void SpectateCameraAbove(Entity entToSpectate, Keys keyForDefaultCam)
        {
            GameFiber.StartNew(delegate
            {
                Camera cam = new Camera(false);
                if (cam)
                {
                    cam.AttachToEntity(entToSpectate, new Vector3(0, 0, 13f), true);
                    cam.PointAtEntity(entToSpectate, new Vector3(0, 0, 0), true);
                }

                cam.Active = true;

                while (true)
                {
                    GameFiber.Yield();

                    if (Game.IsKeyDown(keyForDefaultCam))
                    {
                        if (cam) cam.Delete();
                        break;
                    }
                }
            });
        }

        /// <summary>Creates a camera behind the given entity.</summary>
        /// <param name="entToSpectate">The entity that you want to spectate.</param>
        /// <param name="keyForDefaultCam">The key to press to revert back to the normal player camera.</param>
        public static void SpectateCameraNormal(Entity entToSpectate, Keys keyForDefaultCam)
        {
            GameFiber.StartNew(delegate
            {
                Camera cam = new Camera(false);
                if (cam)
                {
                    cam.AttachToEntity(entToSpectate, new Vector3(0, -5f, 2.5f), true);
                    cam.PointAtEntity(entToSpectate, new Vector3(0, 0, 0), true);
                }

                cam.Active = true;

                while (true)
                {
                    GameFiber.Yield();
                    cam.PointAtEntity(entToSpectate, new Vector3(0, 0, 0), true);

                    if (Game.IsKeyDown(keyForDefaultCam))
                    {
                        if (cam) cam.Delete();
                        break;
                    }
                }
            });
        }

        /// <summary>Creates a toggle system where you can press a key to switch between cameras.</summary>
        public static void SpectateCameraToggler(Entity entToSpectate1, Entity entToSpectate2 = null)
        {
            GameFiber.StartNew(delegate
            {
                int currentCamera = 0; //Default, unknown view.
                Keys defaultCamera = Keys.F7;

                Game.DisplayHelp(string.Format("Click {0} to switch between the spectating camera views, and press {1} to change back to the default camera view.",
                    FriendlyKeys.GetFriendlyName(Keys.F6), FriendlyKeys.GetFriendlyName(Keys.F7)));

                while (true)
                {
                    if (Game.IsKeyDown(Keys.F6))
                    {
                        currentCamera++;

                        if (entToSpectate2 == null && entToSpectate1)
                        {
                            switch (currentCamera)
                            {
                                case 1: //Spectating, normal view.
                                    SpectateCameraNormal(entToSpectate1, defaultCamera);
                                    break;
                                case 2: //Spectating, top view.
                                    SpectateCameraAbove(entToSpectate1, defaultCamera);
                                    break;
                                default: //Invalid view, setting to default player view.
                                    currentCamera = 0;
                                    break;
                            }
                        }

                        if (entToSpectate2 != null && entToSpectate1 && entToSpectate2)
                        {
                            switch (currentCamera)
                            {
                                case 1: //Spectating, normal view.
                                    Util.SpectateCameraNormal(entToSpectate1, defaultCamera);
                                    break;
                                case 2: //Spectating, top view.
                                    Util.SpectateCameraAbove(entToSpectate1, defaultCamera);
                                    break;
                                case 3: //Spectating, top view.
                                    Util.SpectateCameraNormal(entToSpectate2, defaultCamera);
                                    break;
                                case 4: //Spectating, top view.
                                    Util.SpectateCameraAbove(entToSpectate2, defaultCamera);
                                    break;
                                default: //Invalid view, setting to default player view.
                                    currentCamera = 0;
                                    break;
                            }
                        }
                    }

                    if (Game.IsKeyDown(defaultCamera))
                    {
                        currentCamera = 0;
                    }

                    GameFiber.Yield();
                }
            });
        }

        /// <summary>Makes a vehicle drive at a given speed.</summary>
        /// <param name="veh">The vehicle to effect.</param>
        /// <param name="speed">The speed to start the vehicle at.</param>
        public static void BeginRollingStart(this Vehicle veh, float speed = 45f)
        {
            if (veh)
            {
                veh.IsEngineStarting = false;
                veh.IsEngineOn = true;
                veh.SetForwardSpeed(speed);
            }
        }

        public static void PointAnimation(this Ped ped, int timeout)
        {
            if (ped)
            {
                if (ped.IsMale) ped.Tasks.PlayAnimation("gestures@m@standing@casual", "gesture_point", timeout, 4f, 4f, ped.Heading, AnimationFlags.Loop);
                else ped.Tasks.PlayAnimation("gestures@f@standing@casual", "gesture_point", timeout, 4f, 4f, ped.Heading, AnimationFlags.Loop);
            }
        }

        public static void BlipUpdates(Blip blip, CalloutStandardization.BlipTypes blipType, Entity entToAttachTo, string name, float scale = 100f, float alpha = 0.40f)
        {
            GameFiber.StartNew(delegate
            {
                if (blip != null)
                {
                    blip = new Blip(entToAttachTo)
                    {
                        Name = name,
                        Scale = scale,
                        Alpha = alpha
                    };

                    blip.SetStandardColor(blipType);
                    blip.Flash(10, 10);

                    Stopwatch timer = new Stopwatch();
                    timer.Start();

                    while (true)
                    {
                        GameFiber.Yield();

                        if (Util.IsEntityVisible(entToAttachTo))
                        {
                            blip = new Blip(entToAttachTo)
                            {
                                Name = name,
                                IsRouteEnabled = true
                            };

                            blip.SetBlipScalePed();
                            blip.SetStandardColor(blipType);
                            blip.Flash(10, 10);
                            break;
                        }

                        if (timer.ElapsedMilliseconds >= 10000)
                        {
                            if (blip) blip.Delete();

                            blip = new Blip(entToAttachTo)
                            {
                                Name = name + " SEARCH AREA",
                                Scale = scale,
                                Alpha = alpha,
                                IsRouteEnabled = true
                            };

                            blip.SetStandardColor(blipType);
                            blip.Flash(10, 10);
                            timer.Restart();
                        }
                    }
                }
            });
        }

        /// <summary>Prints as much detail for an error as possible.</summary>
        public static void PrintRecursiveExceptions(Exception ex, int i = 0)
        { //MADE BY PNWPARKSFAN
            Game.LogTrivial("--------------------");
            Game.LogTrivial("[TOASTY CALLOUTS - EXCEPTIONS]: ");
            Game.LogTrivial(ex.Message);
            Game.LogTrivialDebug(ex.StackTrace);
            if (ex.InnerException != null && i < 10)
            {
                PrintRecursiveExceptions(ex.InnerException, i + 1);
            }
        }
    }

    static class Natives
    {
        /// <summary>Sets the vehicles forward speed.</summary>
        /// <param name="veh">The vehicle to effect.</param>
        /// <param name="speed">The starting speed.</param>
        public static void SetForwardSpeed(this Vehicle veh, float speed)
        {
            NativeFunction.Natives.xAB54A438726D25D5(veh, speed); //SET_VEHICLE_FORWARD_SPEED
        }

        public static float ClosestVehicleHeading(Vector3 pos)
        {
            Vector3 tempCoords;
            float tempHeading;

            NativeFunction.Natives.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING(pos.X, pos.Y, pos.Z, out tempCoords, out tempHeading, 0, 3f, 0);
            return tempHeading;
        }

        public static Vector3 ClosestVehicleNodePosition(Vector3 pos)
        {
            Vector3 tempCoords;
            NativeFunction.Natives.GET_CLOSEST_VEHICLE_NODE(pos.X, pos.Y, pos.Z, out tempCoords, 0, 3f, 0);
            return tempCoords;
        }

        /// <summary>Makes a ped face another ped. Clear the tasks of the ped to stop them from facing pedToFace.</summary>
        /// <param name="ped">The ped that is going to face pedToFace.</param>
        /// <param name="pedToFace">The ped that is going to be the one being looked at.</param>
        public static void FaceEntity(this Ped ped, Ped pedToFace)
        {
            NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(ped, pedToFace, int.MaxValue);
        }

        public static void PlaceObjectOnGroundProperly(this Rage.Object obj)
        {
            NativeFunction.Natives.PLACE_OBJECT_ON_GROUND_PROPERLY(obj);
        }

        public static bool IsPlayerFreeAimingAtEntity(Entity entity)
        {
            return NativeFunction.Natives.IS_PLAYER_FREE_AIMING_AT_ENTITY<bool>(Game.LocalPlayer, entity);
        }
    }

    static class Cleaning
    {
        public static void EndAndClean(Entity[] entities, Blip[] blips = null, LHandle[] pursuits = null, Checkpoint[] checkpoints = null)
        {
            foreach (Entity ent in entities)
            {
                if (ent)
                {
                    if (!Util.IsEntityVisible(ent)) ent.Delete();
                    else ent.Dismiss();
                }
            }

            if (blips != null)
            {
                foreach (Blip blip in blips)
                {
                    if (blip)
                    {
                        blip.Delete();
                    }
                }
            }

            if (pursuits != null)
            {
                foreach (LHandle pursuit in pursuits)
                {
                    if (Functions.IsPursuitStillRunning(pursuit))
                    {
                        Functions.ForceEndPursuit(pursuit);
                    }
                }
            }

            if (checkpoints != null)
            {
                foreach (Checkpoint checkpoint in checkpoints)
                {
                    if (checkpoint)
                    {
                        checkpoint.Delete();
                    }
                }
            }
        }

        public static void CatchInvalidObjects(Entity[] entities = null, Blip[] blips = null)
        {
            try
            {
                if (entities != null)
                {
                    foreach (Entity ent in entities)
                    {
                        if (!ent) throw new NullReferenceException(ent.Model.Name);
                        else throw new InvalidOperationException();
                    }
                }

                if (blips != null)
                {
                    foreach (Blip blip in blips)
                    {
                        if (!blip) throw new NullReferenceException(blip.Name);
                        else throw new InvalidOperationException();
                    }
                }

                Game.DisplayNotification("Toasty Callouts has encountered an error, ending the callout to prevent any crashes.");
            }
            catch (ArgumentException ex)
            {
                Util.PrintRecursiveExceptions(ex);
            }
            catch (InvalidOperationException ex)
            {
                Util.PrintRecursiveExceptions(ex);
            }
        }
    }

    class PursuitVisual
    {
        private static event EventHandler<bool> AIVisualChanged, PlayerVisualChanged;
        public static bool _aiHasVisual, _aiHasVisualCheck, _playerHasVisual, _playerHasVisualCheck, _reached, _endCalloutWhenFinished = true;
        private static Ped[] _suspectPeds = null;
        private static Ped[] _officerPeds = null;
        private static PursuitVisual _aiVisual, _playerVisual;

        public static void WaitForPursuit()
        {
            GameFiber.StartNew(delegate
            {
                bool once = false;

                while (true)
                {
                    if (once && Functions.GetActivePursuit() == null)
                    {
                        Util.Log("Setting once to false, as there is no longer an active pursuit.", 0);
                        once = false;
                    }

                    if (!once && Functions.IsCalloutRunning() && Functions.GetActivePursuit() != null)
                    {
                        Util.Log("Pursuit active, starting pursuit visual's functions.", 0);
                        LHandle pursuit = Functions.GetActivePursuit();
                        Start(pursuit);

                        once = true;
                    }

                    GameFiber.Yield();
                }
            });
        }

        public static void Start(LHandle pursuit)
        {
            GameFiber.StartNew(delegate
            { //AI cops' visual seems to not matter when starting timer.
                if (pursuit != null && Functions.IsPursuitStillRunning(pursuit))
                {
                    //string[] policePedModels = { "S_M_Y_COP_01", "S_F_Y_COP_01", "S_M_Y_SHERIFF_01", "S_F_Y_SHERIFF_01", "CSB_COP", "S_M_Y_HWAYCOP_01" };

                    _suspectPeds = Functions.GetPursuitPeds(pursuit).Where(x => !(x.Model.Name == "S_M_Y_COP_01" || x.Model.Name == "S_F_Y_COP_01" ||
                                     x.Model.Name == "S_M_Y_SHERIFF_01" || x.Model.Name == "S_F_Y_SHERIFF_01" ||
                                     x.Model.Name == "CSB_COP" || x.Model.Name == "S_M_Y_HWAYCOP_01") && !x.IsLocalPlayer).ToArray();

                    _officerPeds = Functions.GetPursuitPeds(pursuit).Where(x => (x.Model.Name == "S_M_Y_COP_01" || x.Model.Name == "S_F_Y_COP_01" ||
                                     x.Model.Name == "S_M_Y_SHERIFF_01" || x.Model.Name == "S_F_Y_SHERIFF_01" ||
                                     x.Model.Name == "CSB_COP" || x.Model.Name == "S_M_Y_HWAYCOP_01") && !x.IsLocalPlayer).ToArray();
                }

                AIVisualChanged += (object sender, bool aiVisualStatus) =>
                {
                    _aiVisual = (PursuitVisual)sender;

                    if (aiVisualStatus)
                    {
                        Util.Log("AI has visual.", 0);
                    }

                    if (_reached && !aiVisualStatus && !_playerHasVisualCheck)
                    {
                        Util.Log("AI and player do not have visual.", 0);
                        StartTimer();
                    }
                    else if (!aiVisualStatus && _playerHasVisualCheck)
                    {
                        Util.Log("AI does not have visual, but player does have visual.", 0);
                    }
                };

                PlayerVisualChanged += (object sender, bool playerVisualStatus) =>
                {
                    _playerVisual = (PursuitVisual)sender;

                    if (playerVisualStatus)
                    {
                        Util.Log("Player has visual.", 0);
                    }

                    if (_reached && !playerVisualStatus && !_aiHasVisualCheck)
                    {
                        Util.Log("Player and AI do not have visual.", 0);
                        StartTimer();
                    }
                    else if (!playerVisualStatus && _aiHasVisualCheck)
                    {
                        Util.Log("Player does not have visual, but AI does have visual.", 0);
                        PursuitUpdates();
                    }
                };

                while (true)
                {
                    GameFiber.Yield();

                    if (pursuit != null && Functions.IsPursuitStillRunning(pursuit)) Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                    if (_suspectPeds != null && _officerPeds != null)
                    {
                        foreach (var suspectPed in _suspectPeds)
                        {
                            foreach (var officerPed in _officerPeds)
                            {
                                if (suspectPed && officerPed)
                                {
                                    _reached = true;
                                    _aiHasVisualCheck = Util.IsEntityVisible(suspectPed, officerPed);
                                }
                            }
                            GameFiber.Yield();
                        }
                    }

                    if (_aiHasVisual != _aiHasVisualCheck)
                    {
                        _aiHasVisual = _aiHasVisualCheck;
                        AIVisualChanged?.Invoke(_aiVisual, _aiHasVisual);
                    }

                    if (_suspectPeds != null)
                    {
                        foreach (var suspectPed in _suspectPeds)
                        {
                            if (suspectPed) _playerHasVisualCheck = Util.IsEntityVisible(suspectPed);
                        }
                    }

                    if (_playerHasVisual != _playerHasVisualCheck)
                    {
                        _playerHasVisual = _playerHasVisualCheck;
                        PlayerVisualChanged?.Invoke(_playerVisual, _playerHasVisual);
                    }

                    if (_endCalloutWhenFinished && pursuit != null && !Functions.IsPursuitStillRunning(pursuit))
                    {
                        Util.Log("No pursuit active, ending callout.", 0);
                        if (Functions.IsCalloutRunning()) Functions.StopCurrentCallout();
                        break;
                    }

                    if (pursuit != null && !Functions.IsPursuitStillRunning(pursuit))
                    {
                        Util.Log("No pursuit active.", 0);
                        break;
                    }
                }
            });
        }

        private static void StartTimer()
        {
            GameFiber.StartNew(delegate
            {
                Util.Log("Starting timer.", 0);
                Stopwatch timer = new Stopwatch();
                timer.Start();

                while (true)
                {
                    GameFiber.Yield();

                    if (!_playerHasVisualCheck && !_aiHasVisualCheck && timer.ElapsedMilliseconds >= 10000)
                    {
                        Util.Log("Time exceeded and no visual has been made, ending pursuit.", 0);
                        Functions.PlayScannerAudio("HELI_NO_VISUAL_DISPATCH_02 10-4 CODE4");

                        LHandle pursuit = Functions.GetActivePursuit();
                        if (pursuit != null && Functions.IsPursuitStillRunning(pursuit)) Functions.ForceEndPursuit(pursuit);
                        break;
                    }

                    if (_playerHasVisualCheck || _aiHasVisualCheck)
                    {
                        Util.Log("Resetting timer, visual was made.", 0);
                        timer.Reset();
                        break;
                    }
                }
            });
        }

        private static void PursuitUpdates()
        {
            GameFiber.StartNew(delegate
            {
                Vector3 suspectPosition = new Vector3(0, 0, 0);
                LHandle pursuit = Functions.GetActivePursuit();

                Stopwatch timer = new Stopwatch();
                timer.Start();

                while (true)
                {
                    GameFiber.Yield();

                    if (_suspectPeds != null)
                    {
                        foreach (var suspectPed in _suspectPeds)
                        {
                            if (suspectPed) suspectPosition = suspectPed.Position;
                            GameFiber.Yield();
                        }
                    }

                    if (suspectPosition != new Vector3(0, 0, 0) && timer.ElapsedMilliseconds >= 15000)
                    {
                        timer.Restart();
                        Functions.PlayScannerAudioUsingPosition("SUSPECT_IS IN_OR_ON_POSITION", suspectPosition);
                    }

                    if (_playerHasVisualCheck || (pursuit != null && !Functions.IsPursuitStillRunning(pursuit)))
                    {
                        timer.Reset();
                        break;
                    }
                }
            });
        }
    }

    struct ConversationLine
    {
        public enum PedsName
        {
            SUSPECT,
            LOCALPLAYER,
            OFFICER,
            FIREFIGHTER,
            PARAMEDICS,
            ANIMALCONTROL,
            VICTIM,
            WITNESS
        };
        public PedsName _pedsName;

        public bool _showVariants; //if true it'll display Option1:, Option2: for the player
        public string[] _lineVariants; //if ShowVariants == true - contains options, otherwise a set of lines to get a random one and display
        public int _subtitleTimeout;
        public bool _repeatFirstVariant;
    }

    class Conversation
    {
        public int CurrentLine { get; set; }
        public bool HasFinished { get; set; }
        public bool Pause { get; set; }

        private ConversationLine[] _lines;
        private GameFiber _fiber;
        private string _pedsNameAsString;

        public Conversation(ConversationLine[] lines)
        {
            this._lines = lines;
        }

        public void Start()
        {
            _fiber = GameFiber.StartNew(Process);
        }

        private void Process()
        {
            while (true)
            {
                GameFiber.Yield();

                if ((CurrentLine == _lines.Length) || (HasFinished) || (!Functions.IsCalloutRunning()))
                {
                    Util.Log("Conversation has finished.", 0);
                    HasFinished = true;
                    break;
                }

                if (Pause) continue;

                //if (!(Main.KeyPressCheck(Settings._talkKeyModifier, Settings._talkKey) || Game.IsKeyDown(Settings._talkKeyOption1) || Game.IsKeyDown(Settings._talkKeyOption2) || Game.IsKeyDown(Settings._talkKeyOption3))) continue;

                var current = _lines[CurrentLine];

                string lineToDisplay;

                switch (current._pedsName)
                {
                    case ConversationLine.PedsName.SUSPECT:
                        _pedsNameAsString = "~r~Suspect";
                        break;
                    case ConversationLine.PedsName.LOCALPLAYER:
                        _pedsNameAsString = string.Format("~b~Officer {0}", "Toasty");
                        break;
                    case ConversationLine.PedsName.OFFICER:
                        _pedsNameAsString = "~b~Other Officer";
                        break;
                    case ConversationLine.PedsName.FIREFIGHTER:
                        _pedsNameAsString = "~g~Firefighter";
                        break;
                    case ConversationLine.PedsName.PARAMEDICS:
                        _pedsNameAsString = "~g~Paramedics";
                        break;
                    case ConversationLine.PedsName.ANIMALCONTROL:
                        _pedsNameAsString = "~g~Animal Control";
                        break;
                    case ConversationLine.PedsName.VICTIM:
                        _pedsNameAsString = "~o~Victim";
                        break;
                    case ConversationLine.PedsName.WITNESS:
                        _pedsNameAsString = "~o~Witness";
                        break;
                    default:
                        _pedsNameAsString = "~y~Unkown";
                        break;
                }

                if (current._showVariants)
                {
                    if (current._lineVariants.Length == 3)
                    {
                        Util.Log("Length is equal to 3.", 0);
                        lineToDisplay = SayOptionChoice(current._lineVariants[0], current._lineVariants[1], current._lineVariants[2]);
                    }
                    else if (current._lineVariants.Length < 3)
                    {
                        Util.Log("Length is less than 3.", 0);
                        lineToDisplay = SayOptionChoice(current._lineVariants[0], current._lineVariants[1]);
                    }
                    else
                    {
                        if (Functions.IsCalloutRunning()) Functions.StopCurrentCallout();
                        Game.DisplayNotification("Toasty Callouts has encountered an error, ending the callout to prevent any crashes.");
                        break;
                    }

                    if (!(Game.IsKeyDown(Keys.NumPad7) || Game.IsKeyDown(Keys.NumPad8) || Game.IsKeyDown(Keys.NumPad9))) continue;

                    if (current._repeatFirstVariant)
                    {
                        //repeat the first variant.
                    }
                }
                else
                {
                    lineToDisplay = MathHelper.Choose<string>(current._lineVariants);

                    if (!Game.IsKeyDown(Keys.T)) continue;
                }

                if (current._subtitleTimeout == 0)
                {
                    current._subtitleTimeout = int.MaxValue;
                }

                Game.DisplaySubtitle("~h~" + _pedsNameAsString + ": " + lineToDisplay, current._subtitleTimeout);

                if (_lines.Length - 1 == CurrentLine)
                {
                    CurrentLine++;
                    break;
                }
                else CurrentLine++;
            }
        }

        private string SayOptionChoice(string option1, string option2, string option3 = null)
        {
            if (option3 == null)
            {
                Game.DisplayHelp(string.Format("~h~Choose a line to say:~n~ {0}, {1}.",
                    "Press <font color=\"#F0D732\">" + FriendlyKeys.GetFriendlyName(Keys.NumPad7) + "</font> for <font color=\"#0FC80F\">Option 1: " + option1 + "</font>~n~",
                    "Press <font color=\"#F0D732\">" + FriendlyKeys.GetFriendlyName(Keys.NumPad8) + "</font> for <font color=\"#0AE60A\">Option 2: " + option2 + " </font>"));
            }
            else
            {
                Game.DisplayHelp(string.Format("~h~Choose a line to say:~n~ {0}, {1}, {2}.",
                    "Press <font color=\"#F0D732\">" + FriendlyKeys.GetFriendlyName(Keys.NumPad7) + "</font> for <font color=\"#0FC80F\">Option 1: " + option1 + "</font>~n~",
                    "Press <font color=\"#F0D732\">" + FriendlyKeys.GetFriendlyName(Keys.NumPad8) + "</font> for <font color=\"#0AE60A\">Option 2: " + option2 + " </font>~n~",
                    "Press <font color=\"#F0D732\">" + FriendlyKeys.GetFriendlyName(Keys.NumPad9) + "</font> for <font color=\"#0AE60A\">Option 2: " + option3 + " </font>~n~"));
            }

            while (true)
            {
                if (Game.IsKeyDown(Keys.NumPad7))
                {
                    Game.HideHelp();

                    /*if (Settings._talkKeyModifier == Keys.None)*/
                    Game.DisplayHelp("Press <font color=\"#F0D732\">" + FriendlyKeys.GetFriendlyName(Keys.T) + "</font> to continue the conversation.");
                    //else Game.DisplayHelp("Press <font color=\"#F0D732\">" + Settings._talkKeyModifier + " + " + Settings._talkKey + "</font> to continue the conversation.");

                    return option1;
                }

                if (Game.IsKeyDown(Keys.NumPad8))
                {
                    Game.HideHelp();

                    /*if (Settings._talkKeyModifier == Keys.None) */
                    Game.DisplayHelp("Press <font color=\"#F0D732\">" + FriendlyKeys.GetFriendlyName(Keys.T) + "</font> to continue the conversation.");
                    //else Game.DisplayHelp("Press <font color=\"#F0D732\">" + Settings._talkKeyModifier + " + " + Settings._talkKey + "</font> to continue the conversation.");

                    return option2;
                }

                if (option3 != null && Game.IsKeyDown(Keys.NumPad9))
                {
                    Game.HideHelp();

                    /*if (Settings._talkKeyModifier == Keys.None) */
                    Game.DisplayHelp("Press <font color=\"#F0D732\">" + FriendlyKeys.GetFriendlyName(Keys.T) + "</font> to continue the conversation.");
                    //else Game.DisplayHelp("Press <font color=\"#F0D732\">" + Settings._talkKeyModifier + " + " + Settings._talkKey + "</font> to continue the conversation.");

                    return option3;
                }

                GameFiber.Yield();
            }
        }
    }

    public static class CalloutStandardization
    { //MADE BY FISKEY111, EDITED BY LTFLASH
        /// <summary>
        /// Set the color of the blip
        /// </summary>
        /// <param name="blip">The blip to set the color of</param>
        /// <param name="type">The color ((Blips default to yellow))</param>
        public static void SetStandardColor(this Blip blip, BlipTypes type)
        {
            if (blip) NativeFunction.Natives.SET_BLIP_COLOUR(blip, (int)type);
        }

        /// <summary>
        /// Description
        /// <para>Enemy = Enemies  [red]</para>
        /// <para>Officers = Cops/Detectives/Commanders  [blue] (not gross system blue)</para>
        /// <para>Support = EMS/Coroner/ETC  [green]</para>
        /// <para>Civilians = Bystanders/Witnesses/broken down/etc  [orange]</para>
        ///  <para>Other = Animals/Obstacles/Rocks/etc  [purple]</para>
        /// </summary>
        public enum BlipTypes { ENEMY = 1, OFFICERS = 3, SUPPORT = 2, CIVILIANS = 17, OTHER = 19 }

        public static void SetBlipScalePed(this Blip blip)
        {
            if (blip) blip.Scale = 0.75f;
        }
    }

    public static class Scenarios
    { //MADE BY PNWPARKSFAN
#pragma warning disable 1591
        public static string WORLD_HUMAN_AA_COFFEE { get { return "WORLD_HUMAN_AA_COFFEE"; } }
        public static string WORLD_HUMAN_AA_SMOKE { get { return "WORLD_HUMAN_AA_SMOKE"; } }
        public static string WORLD_HUMAN_BINOCULARS { get { return "WORLD_HUMAN_BINOCULARS"; } }
        public static string WORLD_HUMAN_BUM_FREEWAY { get { return "WORLD_HUMAN_BUM_FREEWAY"; } }
        public static string WORLD_HUMAN_BUM_SLUMPED { get { return "WORLD_HUMAN_BUM_SLUMPED"; } }
        public static string WORLD_HUMAN_BUM_STANDING { get { return "WORLD_HUMAN_BUM_STANDING"; } }
        public static string WORLD_HUMAN_BUM_WASH { get { return "WORLD_HUMAN_BUM_WASH"; } }
        public static string WORLD_HUMAN_CAR_PARK_ATTENDANT { get { return "WORLD_HUMAN_CAR_PARK_ATTENDANT"; } }
        public static string WORLD_HUMAN_CHEERING { get { return "WORLD_HUMAN_CHEERING"; } }
        public static string WORLD_HUMAN_CLIPBOARD { get { return "WORLD_HUMAN_CLIPBOARD"; } }
        public static string WORLD_HUMAN_CONST_DRILL { get { return "WORLD_HUMAN_CONST_DRILL"; } }
        public static string WORLD_HUMAN_COP_IDLES { get { return "WORLD_HUMAN_COP_IDLES"; } }
        public static string WORLD_HUMAN_DRINKING { get { return "WORLD_HUMAN_DRINKING"; } }
        public static string WORLD_HUMAN_DRUG_DEALER { get { return "WORLD_HUMAN_DRUG_DEALER"; } }
        public static string WORLD_HUMAN_DRUG_DEALER_HARD { get { return "WORLD_HUMAN_DRUG_DEALER_HARD"; } }
        public static string WORLD_HUMAN_MOBILE_FILM_SHOCKING { get { return "WORLD_HUMAN_MOBILE_FILM_SHOCKING"; } }
        public static string WORLD_HUMAN_GARDENER_LEAF_BLOWER { get { return "WORLD_HUMAN_GARDENER_LEAF_BLOWER"; } }
        public static string WORLD_HUMAN_GARDENER_PLANT { get { return "WORLD_HUMAN_GARDENER_PLANT"; } }
        public static string WORLD_HUMAN_GOLF_PLAYER { get { return "WORLD_HUMAN_GOLF_PLAYER"; } }
        public static string WORLD_HUMAN_GUARD_PATROL { get { return "WORLD_HUMAN_GUARD_PATROL"; } }
        public static string WORLD_HUMAN_GUARD_STAND { get { return "WORLD_HUMAN_GUARD_STAND"; } }
        public static string WORLD_HUMAN_GUARD_STAND_ARMY { get { return "WORLD_HUMAN_GUARD_STAND_ARMY"; } }
        public static string WORLD_HUMAN_HAMMERING { get { return "WORLD_HUMAN_HAMMERING"; } }
        public static string WORLD_HUMAN_HANG_OUT_STREET { get { return "WORLD_HUMAN_HANG_OUT_STREET"; } }
        public static string WORLD_HUMAN_HIKER_STANDING { get { return "WORLD_HUMAN_HIKER_STANDING"; } }
        public static string WORLD_HUMAN_HUMAN_STATUE { get { return "WORLD_HUMAN_HUMAN_STATUE"; } }
        public static string WORLD_HUMAN_JANITOR { get { return "WORLD_HUMAN_JANITOR"; } }
        public static string WORLD_HUMAN_JOG_STANDING { get { return "WORLD_HUMAN_JOG_STANDING"; } }
        public static string WORLD_HUMAN_LEANING { get { return "WORLD_HUMAN_LEANING"; } }
        public static string WORLD_HUMAN_MAID_CLEAN { get { return "WORLD_HUMAN_MAID_CLEAN"; } }
        public static string WORLD_HUMAN_MUSCLE_FLEX { get { return "WORLD_HUMAN_MUSCLE_FLEX"; } }
        public static string WORLD_HUMAN_MUSCLE_FREE_WEIGHTS { get { return "WORLD_HUMAN_MUSCLE_FREE_WEIGHTS"; } }
        public static string WORLD_HUMAN_MUSICIAN { get { return "WORLD_HUMAN_MUSICIAN"; } }
        public static string WORLD_HUMAN_PAPARAZZI { get { return "WORLD_HUMAN_PAPARAZZI"; } }
        public static string WORLD_HUMAN_PARTYING { get { return "WORLD_HUMAN_PARTYING"; } }
        public static string WORLD_HUMAN_PICNIC { get { return "WORLD_HUMAN_PICNIC"; } }
        public static string WORLD_HUMAN_PROSTITUTE_HIGH_CLASS { get { return "WORLD_HUMAN_PROSTITUTE_HIGH_CLASS"; } }
        public static string WORLD_HUMAN_PROSTITUTE_LOW_CLASS { get { return "WORLD_HUMAN_PROSTITUTE_LOW_CLASS"; } }
        public static string WORLD_HUMAN_PUSH_UPS { get { return "WORLD_HUMAN_PUSH_UPS"; } }
        public static string WORLD_HUMAN_SEAT_LEDGE { get { return "WORLD_HUMAN_SEAT_LEDGE"; } }
        public static string WORLD_HUMAN_SEAT_LEDGE_EATING { get { return "WORLD_HUMAN_SEAT_LEDGE_EATING"; } }
        public static string WORLD_HUMAN_SEAT_STEPS { get { return "WORLD_HUMAN_SEAT_STEPS"; } }
        public static string WORLD_HUMAN_SEAT_WALL { get { return "WORLD_HUMAN_SEAT_WALL"; } }
        public static string WORLD_HUMAN_SEAT_WALL_EATING { get { return "WORLD_HUMAN_SEAT_WALL_EATING"; } }
        public static string WORLD_HUMAN_SEAT_WALL_TABLET { get { return "WORLD_HUMAN_SEAT_WALL_TABLET"; } }
        public static string WORLD_HUMAN_SECURITY_SHINE_TORCH { get { return "WORLD_HUMAN_SECURITY_SHINE_TORCH"; } }
        public static string WORLD_HUMAN_SIT_UPS { get { return "WORLD_HUMAN_SIT_UPS"; } }
        public static string WORLD_HUMAN_SMOKING { get { return "WORLD_HUMAN_SMOKING"; } }
        public static string WORLD_HUMAN_SMOKING_POT { get { return "WORLD_HUMAN_SMOKING_POT"; } }
        public static string WORLD_HUMAN_STAND_FIRE { get { return "WORLD_HUMAN_STAND_FIRE"; } }
        public static string WORLD_HUMAN_STAND_FISHING { get { return "WORLD_HUMAN_STAND_FISHING"; } }
        public static string WORLD_HUMAN_STAND_IMPATIENT { get { return "WORLD_HUMAN_STAND_IMPATIENT"; } }
        public static string WORLD_HUMAN_STAND_IMPATIENT_UPRIGHT { get { return "WORLD_HUMAN_STAND_IMPATIENT_UPRIGHT"; } }
        public static string WORLD_HUMAN_STAND_MOBILE { get { return "WORLD_HUMAN_STAND_MOBILE"; } }
        public static string WORLD_HUMAN_STAND_MOBILE_UPRIGHT { get { return "WORLD_HUMAN_STAND_MOBILE_UPRIGHT"; } }
        public static string WORLD_HUMAN_STRIP_WATCH_STAND { get { return "WORLD_HUMAN_STRIP_WATCH_STAND"; } }
        public static string WORLD_HUMAN_STUPOR { get { return "WORLD_HUMAN_STUPOR"; } }
        public static string WORLD_HUMAN_SUNBATHE { get { return "WORLD_HUMAN_SUNBATHE"; } }
        public static string WORLD_HUMAN_SUNBATHE_BACK { get { return "WORLD_HUMAN_SUNBATHE_BACK"; } }
        public static string WORLD_HUMAN_SUPERHERO { get { return "WORLD_HUMAN_SUPERHERO"; } }
        public static string WORLD_HUMAN_SWIMMING { get { return "WORLD_HUMAN_SWIMMING"; } }
        public static string WORLD_HUMAN_TENNIS_PLAYER { get { return "WORLD_HUMAN_TENNIS_PLAYER"; } }
        public static string WORLD_HUMAN_TOURIST_MAP { get { return "WORLD_HUMAN_TOURIST_MAP"; } }
        public static string WORLD_HUMAN_TOURIST_MOBILE { get { return "WORLD_HUMAN_TOURIST_MOBILE"; } }
        public static string WORLD_HUMAN_VEHICLE_MECHANIC { get { return "WORLD_HUMAN_VEHICLE_MECHANIC"; } }
        public static string WORLD_HUMAN_WELDING { get { return "WORLD_HUMAN_WELDING"; } }
        public static string WORLD_HUMAN_WINDOW_SHOP_BROWSE { get { return "WORLD_HUMAN_WINDOW_SHOP_BROWSE"; } }
        public static string WORLD_HUMAN_YOGA { get { return "WORLD_HUMAN_YOGA"; } }
        public static string WORLD_BOAR_GRAZING { get { return "WORLD_BOAR_GRAZING"; } }
        public static string WORLD_CAT_SLEEPING_GROUND { get { return "WORLD_CAT_SLEEPING_GROUND"; } }
        public static string WORLD_CAT_SLEEPING_LEDGE { get { return "WORLD_CAT_SLEEPING_LEDGE"; } }
        public static string WORLD_COW_GRAZING { get { return "WORLD_COW_GRAZING"; } }
        public static string WORLD_COYOTE_HOWL { get { return "WORLD_COYOTE_HOWL"; } }
        public static string WORLD_COYOTE_REST { get { return "WORLD_COYOTE_REST"; } }
        public static string WORLD_COYOTE_WANDER { get { return "WORLD_COYOTE_WANDER"; } }
        public static string WORLD_CHICKENHAWK_FEEDING { get { return "WORLD_CHICKENHAWK_FEEDING"; } }
        public static string WORLD_CHICKENHAWK_STANDING { get { return "WORLD_CHICKENHAWK_STANDING"; } }
        public static string WORLD_CORMORANT_STANDING { get { return "WORLD_CORMORANT_STANDING"; } }
        public static string WORLD_CROW_FEEDING { get { return "WORLD_CROW_FEEDING"; } }
        public static string WORLD_CROW_STANDING { get { return "WORLD_CROW_STANDING"; } }
        public static string WORLD_DEER_GRAZING { get { return "WORLD_DEER_GRAZING"; } }
        public static string WORLD_DOG_BARKING_ROTTWEILER { get { return "WORLD_DOG_BARKING_ROTTWEILER"; } }
        public static string WORLD_DOG_BARKING_RETRIEVER { get { return "WORLD_DOG_BARKING_RETRIEVER"; } }
        public static string WORLD_DOG_BARKING_SHEPHERD { get { return "WORLD_DOG_BARKING_SHEPHERD"; } }
        public static string WORLD_DOG_SITTING_ROTTWEILER { get { return "WORLD_DOG_SITTING_ROTTWEILER"; } }
        public static string WORLD_DOG_SITTING_RETRIEVER { get { return "WORLD_DOG_SITTING_RETRIEVER"; } }
        public static string WORLD_DOG_SITTING_SHEPHERD { get { return "WORLD_DOG_SITTING_SHEPHERD"; } }
        public static string WORLD_DOG_BARKING_SMALL { get { return "WORLD_DOG_BARKING_SMALL"; } }
        public static string WORLD_DOG_SITTING_SMALL { get { return "WORLD_DOG_SITTING_SMALL"; } }
        public static string WORLD_FISH_IDLE { get { return "WORLD_FISH_IDLE"; } }
        public static string WORLD_GULL_FEEDING { get { return "WORLD_GULL_FEEDING"; } }
        public static string WORLD_GULL_STANDING { get { return "WORLD_GULL_STANDING"; } }
        public static string WORLD_HEN_PECKING { get { return "WORLD_HEN_PECKING"; } }
        public static string WORLD_HEN_STANDING { get { return "WORLD_HEN_STANDING"; } }
        public static string WORLD_MOUNTAIN_LION_REST { get { return "WORLD_MOUNTAIN_LION_REST"; } }
        public static string WORLD_MOUNTAIN_LION_WANDER { get { return "WORLD_MOUNTAIN_LION_WANDER"; } }
        public static string WORLD_PIG_GRAZING { get { return "WORLD_PIG_GRAZING"; } }
        public static string WORLD_PIGEON_FEEDING { get { return "WORLD_PIGEON_FEEDING"; } }
        public static string WORLD_PIGEON_STANDING { get { return "WORLD_PIGEON_STANDING"; } }
        public static string WORLD_RABBIT_EATING { get { return "WORLD_RABBIT_EATING"; } }
        public static string WORLD_RATS_EATING { get { return "WORLD_RATS_EATING"; } }
        public static string WORLD_SHARK_SWIM { get { return "WORLD_SHARK_SWIM"; } }
        public static string PROP_BIRD_IN_TREE { get { return "PROP_BIRD_IN_TREE"; } }
        public static string PROP_BIRD_TELEGRAPH_POLE { get { return "PROP_BIRD_TELEGRAPH_POLE"; } }
        public static string PROP_HUMAN_ATM { get { return "PROP_HUMAN_ATM"; } }
        public static string PROP_HUMAN_BBQ { get { return "PROP_HUMAN_BBQ"; } }
        public static string PROP_HUMAN_BUM_BIN { get { return "PROP_HUMAN_BUM_BIN"; } }
        public static string PROP_HUMAN_BUM_SHOPPING_CART { get { return "PROP_HUMAN_BUM_SHOPPING_CART"; } }
        public static string PROP_HUMAN_MUSCLE_CHIN_UPS { get { return "PROP_HUMAN_MUSCLE_CHIN_UPS"; } }
        public static string PROP_HUMAN_MUSCLE_CHIN_UPS_ARMY { get { return "PROP_HUMAN_MUSCLE_CHIN_UPS_ARMY"; } }
        public static string PROP_HUMAN_MUSCLE_CHIN_UPS_PRISON { get { return "PROP_HUMAN_MUSCLE_CHIN_UPS_PRISON"; } }
        public static string PROP_HUMAN_PARKING_METER { get { return "PROP_HUMAN_PARKING_METER"; } }
        public static string PROP_HUMAN_SEAT_ARMCHAIR { get { return "PROP_HUMAN_SEAT_ARMCHAIR"; } }
        public static string PROP_HUMAN_SEAT_BAR { get { return "PROP_HUMAN_SEAT_BAR"; } }
        public static string PROP_HUMAN_SEAT_BENCH { get { return "PROP_HUMAN_SEAT_BENCH"; } }
        public static string PROP_HUMAN_SEAT_BENCH_DRINK { get { return "PROP_HUMAN_SEAT_BENCH_DRINK"; } }
        public static string PROP_HUMAN_SEAT_BENCH_DRINK_BEER { get { return "PROP_HUMAN_SEAT_BENCH_DRINK_BEER"; } }
        public static string PROP_HUMAN_SEAT_BENCH_FOOD { get { return "PROP_HUMAN_SEAT_BENCH_FOOD"; } }
        public static string PROP_HUMAN_SEAT_BUS_STOP_WAIT { get { return "PROP_HUMAN_SEAT_BUS_STOP_WAIT"; } }
        public static string PROP_HUMAN_SEAT_CHAIR { get { return "PROP_HUMAN_SEAT_CHAIR"; } }
        public static string PROP_HUMAN_SEAT_CHAIR_DRINK { get { return "PROP_HUMAN_SEAT_CHAIR_DRINK"; } }
        public static string PROP_HUMAN_SEAT_CHAIR_DRINK_BEER { get { return "PROP_HUMAN_SEAT_CHAIR_DRINK_BEER"; } }
        public static string PROP_HUMAN_SEAT_CHAIR_FOOD { get { return "PROP_HUMAN_SEAT_CHAIR_FOOD"; } }
        public static string PROP_HUMAN_SEAT_CHAIR_UPRIGHT { get { return "PROP_HUMAN_SEAT_CHAIR_UPRIGHT"; } }
        public static string PROP_HUMAN_SEAT_CHAIR_MP_PLAYER { get { return "PROP_HUMAN_SEAT_CHAIR_MP_PLAYER"; } }
        public static string PROP_HUMAN_SEAT_COMPUTER { get { return "PROP_HUMAN_SEAT_COMPUTER"; } }
        public static string PROP_HUMAN_SEAT_DECKCHAIR { get { return "PROP_HUMAN_SEAT_DECKCHAIR"; } }
        public static string PROP_HUMAN_SEAT_DECKCHAIR_DRINK { get { return "PROP_HUMAN_SEAT_DECKCHAIR_DRINK"; } }
        public static string PROP_HUMAN_SEAT_MUSCLE_BENCH_PRESS { get { return "PROP_HUMAN_SEAT_MUSCLE_BENCH_PRESS"; } }
        public static string PROP_HUMAN_SEAT_MUSCLE_BENCH_PRESS_PRISON { get { return "PROP_HUMAN_SEAT_MUSCLE_BENCH_PRESS_PRISON"; } }
        public static string PROP_HUMAN_SEAT_SEWING { get { return "PROP_HUMAN_SEAT_SEWING"; } }
        public static string PROP_HUMAN_SEAT_STRIP_WATCH { get { return "PROP_HUMAN_SEAT_STRIP_WATCH"; } }
        public static string PROP_HUMAN_SEAT_SUNLOUNGER { get { return "PROP_HUMAN_SEAT_SUNLOUNGER"; } }
        public static string PROP_HUMAN_STAND_IMPATIENT { get { return "PROP_HUMAN_STAND_IMPATIENT"; } }
        public static string CODE_HUMAN_COWER { get { return "CODE_HUMAN_COWER"; } }
        public static string CODE_HUMAN_CROSS_ROAD_WAIT { get { return "CODE_HUMAN_CROSS_ROAD_WAIT"; } }
        public static string CODE_HUMAN_PARK_CAR { get { return "CODE_HUMAN_PARK_CAR"; } }
        public static string PROP_HUMAN_MOVIE_BULB { get { return "PROP_HUMAN_MOVIE_BULB"; } }
        public static string PROP_HUMAN_MOVIE_STUDIO_LIGHT { get { return "PROP_HUMAN_MOVIE_STUDIO_LIGHT"; } }
        public static string CODE_HUMAN_MEDIC_KNEEL { get { return "CODE_HUMAN_MEDIC_KNEEL"; } }
        public static string CODE_HUMAN_MEDIC_TEND_TO_DEAD { get { return "CODE_HUMAN_MEDIC_TEND_TO_DEAD"; } }
        public static string CODE_HUMAN_MEDIC_TIME_OF_DEATH { get { return "CODE_HUMAN_MEDIC_TIME_OF_DEATH"; } }
        public static string CODE_HUMAN_POLICE_CROWD_CONTROL { get { return "CODE_HUMAN_POLICE_CROWD_CONTROL"; } }
        public static string CODE_HUMAN_POLICE_INVESTIGATE { get { return "CODE_HUMAN_POLICE_INVESTIGATE"; } }
        public static string CODE_HUMAN_STAND_COWER { get { return "CODE_HUMAN_STAND_COWER"; } }
        public static string EAR_TO_TEXT { get { return "EAR_TO_TEXT"; } }
        public static string EAR_TO_TEXT_FAT { get { return "EAR_TO_TEXT_FAT"; } }
#pragma warning restore 1591
        public static void StartScenarioIfNone(this Ped ped, string scenarioName)
        {
            if (!ped.HasScenario())
            {
                ped.StartScenario(scenarioName);
            }
        }

        public static void StartScenario(this Ped ped, string scenarioName)
        {
            NativeFunction.Natives.TASK_START_SCENARIO_IN_PLACE(ped, scenarioName, 0, true);
        }

        public static bool HasScenario(this Ped ped)
        {
            return NativeFunction.Natives.PED_HAS_USE_SCENARIO_TASK<bool>(ped);
        }
    }

    public class Checkpoint : IDeletable, ISpatial
    { //MADE BY PNWPARKSFAN
        private bool _valid;
        private bool _setOnGround = true;

        private Vector3 _position;
        public Vector3 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                ReCreateCheckpoint();
            }
        }

        private Vector3 _nextPosition;
        public Vector3 NextPosition
        {
            get
            {
                return _nextPosition;
            }
            set
            {
                _nextPosition = value;
                ReCreateCheckpoint();
            }
        } // */

        public float Radius { get; private set; }
        public float Height { get; private set; }
        private Color _color;
        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                UpdateColor(value);
            }
        }
        public int Type { get; private set; }
        public int Reserved { get; private set; }
        public int Handle { get; private set; }

        /// <summary>
        /// Create a new checkpoint in-game.
        /// </summary>
        /// <param name="position">Where the checkpoint should sit.</param>
        /// <param name="color">The color of the checkpoint.</param>
        /// <param name="radius">The radius of the checkpoint.</param>
        /// <param name="height">How tall the checkpoint should be.</param>
        /// <param name="type">
        /// The type of the checkpoint.
        /// <para>0-4---------Cylinder: 1 arrow, 2 arrow, 3 arrows, CycleArrow, Checker</para>
        /// <para>5-9---------Cylinder: 1 arrow, 2 arrow, 3 arrows, CycleArrow, Checker</para>
        /// <para>10-14-------Ring: 1 arrow, 2 arrow, 3 arrows, CycleArrow, Checker</para>
        /// <para>15-19-------1 arrow, 2 arrow, 3 arrows, CycleArrow, Checker </para>
        /// <para>20-24-------Cylinder: 1 arrow, 2 arrow, 3 arrows, CycleArrow, Checker </para>
        /// <para>25-29-------Cylinder: 1 arrow, 2 arrow, 3 arrows, CycleArrow, Checker</para>
        /// <para>30-34-------Cylinder: 1 arrow, 2 arrow, 3 arrows, CycleArrow, Checker</para>
        /// <para>35-38-------Ring: Airplane Up, Left, Right, UpsideDown</para>
        /// <para>39----------?</para>
        /// <para>40----------Ring: just a ring</para>
        /// <para>41----------?</para>
        /// <para>42-44-------Cylinder w/ number(uses 'reserved' parameter)</para>
        /// <para>45-47-------Cylinder no arrow or number</para>
        /// </param>
        /// <param name="reserved">For types 42-44, reserved sets the number and shape to display.
        /// <para>0-99------------Just numbers (0-99)</para>
        /// <para>100-109-----------------Arrow (0-9)</para>
        /// <para>110-119------------Two arrows (0-9)</para>
        /// <para>120-129----------Three arrows (0-9)</para>
        /// <para>130-139----------------Circle (0-9)</para>
        /// <para>140-149------------CycleArrow (0-9)</para>
        /// <para>150-159----------------Circle (0-9)</para>
        /// <para>160-169----Circle w/ pointer (0-9)</para>
        /// <para>170-179-------Perforated ring (0-9)</para>
        /// <para>180-189----------------Sphere (0-9)</para>
        /// </param>
        /// <param name="setOnGround">Whether to set the checkpoint on the ground or not.</param>
        public Checkpoint(Vector3 position, Color color, float radius, float height, int type = 47, int reserved = 0, bool setOnGround = true)
        {
            this.Type = type;
            this._position = position;
            this.NextPosition = position;
            this.Radius = radius;
            this.Color = color;
            this.Reserved = reserved;
            this._setOnGround = setOnGround;
            this.Height = height;

            CreateCheckpoint();
        }

        public Checkpoint(Vector3 position, Vector3 nextPosition, Color color, float radius, float height, int type = 0, int reserved = 0, bool setOnGround = true)
        {
            this.Type = type;
            this._position = position;
            this.NextPosition = nextPosition;
            this.Radius = radius;
            this.Color = color;
            this.Reserved = reserved;
            this._setOnGround = setOnGround;
            this.Height = height;

            CreateCheckpoint();
        }

        private void ReCreateCheckpoint()
        {
            this.Delete();
            CreateCheckpoint();
        }

        private void CreateCheckpoint()
        {
            Vector3 placePosition = Position;
            if (_setOnGround)
            {
                placePosition = Util.SetOnGround(Position);
            }

            try
            {
                int handle = NativeFunction.CallByName<int>("CREATE_CHECKPOINT", Type, placePosition.X, placePosition.Y, placePosition.Z, NextPosition.X, NextPosition.Y, NextPosition.Z, Radius, Color.R, Color.G, Color.B, Color.A, Reserved);
                Handle = handle;
                _valid = true;
                SetHeight(Height, Height, Radius);
                Game.LogTrivialDebug("Created checkpoint, handle = " + handle);
            }
            catch (Exception e)
            {
                Game.LogTrivialDebug("Exception trying to create checkpoint: " + e.Message);
                Game.LogTrivialDebug(e.StackTrace);
                _valid = false;
            }
        }

        public void UpdateColor(Color newColor)
        {
            this._color = newColor;
            if (_valid)
                NativeFunction.CallByName<uint>("SET_CHECKPOINT_RGBA", newColor.R, newColor.G, newColor.B, newColor.A);
        }

        public void SetHeight(float near, float far, float radius)
        {
            Height = far;

            if (_valid)
                NativeFunction.CallByName<uint>("SET_CHECKPOINT_CYLINDER_HEIGHT", Handle, near, far, radius);
        }

        public void Delete()
        {
            if (_valid)
                NativeFunction.CallByName<uint>("DELETE_CHECKPOINT", Handle);

            _valid = false;
        }

        public bool IsValid()
        {
            return _valid;
        }

        public float DistanceTo(ISpatial target)
        {
            return Position.DistanceTo(target);
        }

        public float DistanceTo(Vector3 target)
        {
            return Position.DistanceTo(target);
        }

        public float DistanceTo2D(Vector3 target)
        {
            return Position.DistanceTo2D(target);
        }

        public float DistanceTo2D(ISpatial target)
        {
            return Position.DistanceTo2D(target);
        }

        public float TravelDistanceTo(Vector3 target)
        {
            return Position.TravelDistanceTo(target);
        }

        public float TravelDistanceTo(ISpatial target)
        {
            return Position.TravelDistanceTo(target);
        }

        public static implicit operator bool(Checkpoint checkpoint)
        {
            return checkpoint != null && checkpoint.IsValid();
        }
    }

    public static class FriendlyKeys
    { //MADE BY PNWPARKSFAN
        public static string GetFriendlyNames(Keys modifier, Keys key, bool format = true, char formatColor = 'b')
        {
            string output = "";
            if (modifier != null && modifier != Keys.None)
                output += GetFriendlyName(modifier, format, formatColor) + " + ";
            output += GetFriendlyName(key, format, formatColor);
            return output;
        }

        public static string GetFriendlyName(Keys key, bool format = true, char formatColor = 'b')
        {
            string keyName = key.ToString();
            bool keyFound = _keysToFriendlyName.TryGetValue(key, out keyName);
            if (format)
                return "~" + formatColor + "~~h~" + keyName + "~h~~w~";
            else
                return keyName;
        }

        public static string FriendlyName(this Keys key, bool format = true, char formatColor = 'b')
        {
            return GetFriendlyName(key, format, formatColor);
        }

        /// <summary>
        /// A lookup dictionary to take you from a Keys enumeration to a printable friendly name for the key.
        /// </summary>
        internal static Dictionary<Keys, string> _keysToFriendlyName = new Dictionary<Keys, string>
        {
            // Credit to PeterU for creating this list
            {Keys.A,"A"},
            {Keys.Add,"+"},
            {Keys.Alt,"Alt"},
            {Keys.Apps,"Apps"},
            {Keys.Attn,"Attn"},
            {Keys.B,"B"},
            {Keys.Back,"Backspace"},
            {Keys.BrowserBack,"Browser Back"},
            {Keys.BrowserFavorites,"Browser Favorites"},
            {Keys.BrowserForward,"Browser Forward"},
            {Keys.BrowserHome,"Browser Home"},
            {Keys.BrowserRefresh,"Browser Refresh"},
            {Keys.BrowserSearch,"Browser Search"},
            {Keys.BrowserStop,"Browser Stop"},
            {Keys.C,"C"},
            {Keys.Cancel,"Cancel"},
            {Keys.Capital,"Caps Lock"},
            {Keys.Clear,"Clear"},
            {Keys.Control,"Control"},
            {Keys.ControlKey,"Control"},
            {Keys.Crsel,"Crsel"},
            {Keys.D,"D"},
            {Keys.D0,"0"},
            {Keys.D1,"1"},
            {Keys.D2,"2"},
            {Keys.D3,"3"},
            {Keys.D4,"4"},
            {Keys.D5,"5"},
            {Keys.D6,"6"},
            {Keys.D7,"7"},
            {Keys.D8,"8"},
            {Keys.D9,"9"},
            {Keys.Decimal,"Numpad ."},
            {Keys.Delete,"Delete"},
            {Keys.Divide,"/"},
            {Keys.Down,"Down"},
            {Keys.E,"E"},
            {Keys.End,"End"},
            {Keys.Enter,"Enter"},
            {Keys.EraseEof,"Erase EOF"},
            {Keys.Escape,"Esc"},
            {Keys.Execute,"Execute"},
            {Keys.Exsel,"Exsel"},
            {Keys.F,"F"},
            {Keys.F1,"F1"},
            {Keys.F10,"F10"},
            {Keys.F11,"F11"},
            {Keys.F12,"F12"},
            {Keys.F13,"F13"},
            {Keys.F14,"F14"},
            {Keys.F15,"F15"},
            {Keys.F16,"F16"},
            {Keys.F17,"F17"},
            {Keys.F18,"F18"},
            {Keys.F19,"F19"},
            {Keys.F2,"F2"},
            {Keys.F20,"F20"},
            {Keys.F21,"F21"},
            {Keys.F22,"F22"},
            {Keys.F23,"F23"},
            {Keys.F24,"F24"},
            {Keys.F3,"F3"},
            {Keys.F4,"F4"},
            {Keys.F5,"F5"},
            {Keys.F6,"F6"},
            {Keys.F7,"F7"},
            {Keys.F8,"F8"},
            {Keys.F9,"F9"},
            {Keys.FinalMode,"IME Final Mode"},
            {Keys.G,"G"},
            {Keys.H,"H"},
            {Keys.HangulMode,"Hangul Mode"},
            {Keys.HanjaMode,"Hanja Mode"},
            {Keys.Help,"Help"},
            {Keys.Home,"Home"},
            {Keys.I,"I"},
            {Keys.IMEAccept,"IME Accept"},
            {Keys.IMEConvert,"IME Convert"},
            {Keys.IMEModeChange,"IME Mode Change"},
            {Keys.IMENonconvert,"IME Non-convert"},
            {Keys.Insert,"Insert"},
            {Keys.J,"J"},
            {Keys.JunjaMode,"Junja Mode"},
            {Keys.K,"K"},
            {Keys.KeyCode,"Key Code"},
            {Keys.L,"L"},
            {Keys.LaunchApplication1,"Start Application 1"},
            {Keys.LaunchApplication2,"Start Application 2"},
            {Keys.LaunchMail,"Mail"},
            {Keys.LButton,"Left click"},
            {Keys.LControlKey,"Left Ctrl"},
            {Keys.Left,"Left"},
            {Keys.LineFeed,"Line Feed"},
            {Keys.LMenu,"Left Alt"},
            {Keys.LShiftKey,"Left Shift"},
            {Keys.LWin,"Left Windows key"},
            {Keys.M,"M"},
            {Keys.MButton,"Middle click"},
            {Keys.MediaNextTrack,"Next Track"},
            {Keys.MediaPlayPause,"Play Pause"},
            {Keys.MediaPreviousTrack,"Previous Track"},
            {Keys.MediaStop,"Stop"},
            {Keys.Menu,"Alt"},
            {Keys.Modifiers,"Modifiers"},
            {Keys.Multiply,"*"},
            {Keys.N,"N"},
            {Keys.NoName,"NoName"},
            {Keys.None,"[none]"},
            {Keys.NumLock,"Num Lock"},
            {Keys.NumPad0,"Numpad 0"},
            {Keys.NumPad1,"Numpad 1"},
            {Keys.NumPad2,"Numpad 2"},
            {Keys.NumPad3,"Numpad 3"},
            {Keys.NumPad4,"Numpad 4"},
            {Keys.NumPad5,"Numpad 5"},
            {Keys.NumPad6,"Numpad 6"},
            {Keys.NumPad7,"Numpad 7"},
            {Keys.NumPad8,"Numpad 8"},
            {Keys.NumPad9,"Numpad 9"},
            {Keys.O,"O"},
            {Keys.Oem1,"Oem 1"},
            {Keys.Oem102,"Oem 102"},
            {Keys.Oem2,"/"},
            {Keys.Oem3,"'"},
            {Keys.Oem4,"Oem 4"},
            {Keys.Oem5,"\\"},
            {Keys.Oem6,"]"},
            {Keys.Oem7,"Oem 7"},
            {Keys.Oem8,"Oem 8"},
            {Keys.OemClear,"Clear"},
            {Keys.Oemcomma,","},
            {Keys.OemMinus,"-"},
            {Keys.OemPeriod,"."},
            {Keys.Oemplus,"+"},
            {Keys.P,"P"},
            {Keys.Pa1,"Pa1"},
            {Keys.Packet,"Packet"},
            {Keys.PageDown,"Page Down"},
            {Keys.PageUp,"Page Up"},
            {Keys.Pause,"Pause"},
            {Keys.Play,"Play"},
            {Keys.Print,"Print"},
            {Keys.PrintScreen,"Print Screen"},
            {Keys.ProcessKey,"Process Key"},
            {Keys.Q,"Q"},
            {Keys.R,"R"},
            {Keys.RButton,"Right click"},
            {Keys.RControlKey,"Right Ctrl"},
            {Keys.Right,"Right"},
            {Keys.RMenu,"Right Alt"},
            {Keys.RShiftKey,"Right Shift"},
            {Keys.RWin,"Right Windows key"},
            {Keys.S,"S"},
            {Keys.Scroll,"Scroll Lock"},
            {Keys.Select,"Select"},
            {Keys.SelectMedia,"Select Media"},
            {Keys.Separator,"Separator"},
            {Keys.Shift,"Shift"},
            {Keys.ShiftKey,"Shift"},
            {Keys.Sleep,"Sleep"},
            {Keys.Space,"Space"},
            {Keys.Subtract,"-"},
            {Keys.T,"T"},
            {Keys.Tab,"Tab"},
            {Keys.U,"U"},
            {Keys.Up,"Up"},
            {Keys.V,"V"},
            {Keys.VolumeDown,"Volume Down"},
            {Keys.VolumeMute,"Volume Mute"},
            {Keys.VolumeUp,"Volume Up"},
            {Keys.W,"W"},
            {Keys.X,"X"},
            {Keys.XButton1,"Mouse X1"},
            {Keys.XButton2,"Mouse X2"},
            {Keys.Y,"Y"},
            {Keys.Z,"Z"},
            {Keys.Zoom,"Zoom"},
        };
    }
}
