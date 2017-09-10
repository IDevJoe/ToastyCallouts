using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Reflection;
using ToastyCallouts.Callouts;

namespace ToastyCallouts
{
    public class Main : Plugin
    {
        public static Random _rnd = new Random();
        internal static Ped Player => Game.LocalPlayer.Character;

        public override void Initialize()
        {
            Game.LogTrivial("[TOASTY CALLOUTS]: Current version is " + Assembly.GetExecutingAssembly().GetName().Version);
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
        }

        public override void Finally()
        {
            Game.LogTrivial("[TOASTY CALLOUTS]: End has been called.");
        }

        private static void OnOnDutyStateChangedHandler(bool onDuty)
        {
            if (onDuty)
            {
                Functions.RegisterCallout(typeof(PursuitinProgress));
            }
        }
    }
}
