using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rage;

namespace ToastyCallouts.Utilities
{
    class Cameras
    {
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
                                    SpectateCameraNormal(entToSpectate1, defaultCamera);
                                    break;
                                case 2: //Spectating, top view.
                                    SpectateCameraAbove(entToSpectate1, defaultCamera);
                                    break;
                                case 3: //Spectating, top view.
                                    SpectateCameraNormal(entToSpectate2, defaultCamera);
                                    break;
                                case 4: //Spectating, top view.
                                    SpectateCameraAbove(entToSpectate2, defaultCamera);
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
    }
}
