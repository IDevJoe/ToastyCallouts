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
            Game.LogTrivial("[TOASTYCALLOUTS]: Initialize() has been called in class Main.");
            Game.LogTrivial("[TOASTYCALLOUTS]: Current TC version is " + Assembly.GetExecutingAssembly().GetName().Version);

            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
        }

        public override void Finally()
        {
            Game.LogTrivial("[TOASTYCALLOUTS]: Finally() has been called in class Main.");
        }

        private static void OnOnDutyStateChangedHandler(bool onDuty)
        {
            Game.LogTrivial("[TOASTYCALLOUTS]: OnOnDutyStateChangedHandler(bool onDuty) has been called in class Main.");

            if (onDuty)
            {
                Game.LogTrivial("[TOASTYCALLOUTS]: bool onDuty is true.");
                Functions.RegisterCallout(typeof(PursuitinProgress));
            }
        }
    }
}
