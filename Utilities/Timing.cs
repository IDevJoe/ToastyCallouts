using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;

namespace ToastyCallouts.Utilities
{
    public class Timing
    {
        private static bool _started = false;
        public delegate void PTick();
        public static event PTick Tick;
        private static GameFiber _fiber;

        private Timing()
        {
        }

        public static void StartTimer()
        {
            if (_started) return;
            _fiber = GameFiber.StartNew(delegate
            {
                while (true)
                {
                    GameFiber.Yield();
                    Tick?.Invoke();
                }
            });
            _started = true;
        }

        public static void EndTimer()
        {
            if(_started) _fiber.Abort();
            _started = false;
        }
    }
}
