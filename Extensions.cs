﻿using LSPD_First_Response.Engine.Scripting;
using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;

namespace ToastyCallouts
{
    class Extensions
    {
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

            return new Vehicle(MathHelper.Choose(selectCarModels), spawnpoint, ClosestVehicleHeading(spawnpoint));
        }

        public static Ped PolicePed(Vector3 spawnpoint)
        { //S_M_Y_COP_01, S_F_Y_COP_01, CSB_COP
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

            return new Ped(MathHelper.Choose(selectPedModels), spawnpoint, ClosestVehicleHeading(spawnpoint));
        }

        public static float ClosestVehicleHeading(Vector3 pos)
        {
            Vector3 tempCoords;
            float tempHeading;

            NativeFunction.Natives.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING(pos.X, pos.Y, pos.Z, out tempCoords, out tempHeading, 0, 3f, 0);
            return tempHeading;
        }

        public static bool IsEntityVisible(Entity ent)
        {
            Entity[] entitiesToIgnore = { Main.Player, ent };
            HitResult result = World.TraceLine(Main.Player.Position, ent.Position, TraceFlags.IntersectWorld, entitiesToIgnore);

            return ent && !result.Hit;
        }

        public static bool IsEntityVisible(Entity ent, Entity ent2)
        {
            Entity[] entitiesToIgnore = { Main.Player, ent, ent2 };
            HitResult result = World.TraceLine(ent.Position, ent2.Position, TraceFlags.IntersectWorld, entitiesToIgnore);

            return ent && ent2 && !result.Hit;
        }

        public static void SetOnGround(Entity ent, float groundLevelIncrement = 0.25f, bool treatWaterAsGround = false, bool anyMeans = true)
        {
            var groundZ = World.GetGroundZ(ent.Position, false, true);
            if (groundZ != null) ent.Position = new Vector3(ent.Position.X, ent.Position.Y, (float)groundZ + groundLevelIncrement);
        }

        public static Vector3 SetOnGround(Vector3 position, float groundLevelIncrement = 0.25f, bool treatWaterAsGround = false, bool anyMeans = true)
        {
            var groundZ = World.GetGroundZ(position, false, true);
            if (groundZ != null) position = new Vector3(position.X, position.Y, (float)groundZ + groundLevelIncrement);
            return position;
        }

        public static void EndAndClean(Entity[] entities, Blip[] blips = null, LHandle[] pursuits = null)
        {
            foreach (Entity ent in entities)
            {
                if (ent)
                {
                    if (!IsEntityVisible(ent)) ent.Delete();
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

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ConvertPropertyToExpressionBody")]
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
                placePosition = Extensions.SetOnGround(Position);
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
}
