using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.IO;
using System.Reflection;
using ToastyCallouts.Callouts;

namespace ToastyCallouts
{
    public class Main : Plugin
    {
        internal static Version _currentTCVersion = Assembly.GetExecutingAssembly().GetName().Version;
        internal static Version _currentRPHVersion = Assembly.LoadFile(AssemblyDirectory + "/RAGEPluginHook.exe").GetName().Version;
        internal static Ped Player => Game.LocalPlayer.Character;
        public static Random _rnd = new Random();

        public override void Initialize()
        {
            Util.Log(string.Format("Current TC version is {0}, and the current RPH version is {1}.", _currentTCVersion, _currentRPHVersion), 1);
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
            Utilities.Timing.StartTimer();
        }

        public override void Finally()
        {
            Util.Log("End has been called.", 1);
            Utilities.Timing.EndTimer();
        }

        private static void OnOnDutyStateChangedHandler(bool onDuty)
        {
            if (onDuty)
            {
                PursuitVisual.WaitForPursuit();
                RNUIMenu.Main();

                Functions.RegisterCallout(typeof(PursuitinProgress));
                Functions.RegisterCallout(typeof(PettyTheft));

#if DEBUG
                //DoStuff
#endif
            }
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
