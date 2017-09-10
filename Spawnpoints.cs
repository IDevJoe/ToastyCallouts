using Rage;
using System.Collections.Generic;
using System.Linq;

namespace ToastyCallouts
{
    class Spawnpoints
    {
        public static Vector3 GetGoodSpawnpoint()
        {
            List<Vector3> sP = new List<Vector3>()
            {
                new Vector3(-155.74f, 6344.41f, 31.16f).Around2D(125f), //paletoBayV1
                new Vector3(-2.63f, 6522.08f, 30.94f).Around2D(130f), //paletoBayV2
                new Vector3(-321.50f, 6213.43f, 31.04f).Around2D(130f), //paletoBayV3
                new Vector3(-571.51f, 5862.00f, 29.79f).Around2D(175f), //paletoForestV4
                new Vector3(-710.56f, 5614.25f, 29.27f).Around2D(130f), //paletoForestV5
                new Vector3(-404.49f, 6041.08f, 30.98f).Around2D(75f), //paletoBayV6
                new Vector3(1950.20f, 4875.28f, 45.14f).Around2D(350f), //grapeseedV1
                new Vector3(2283.23f, 5062.36f, 45.25f).Around2D(275f), //grapeseedV2
                new Vector3(2468.42f, 4722.70f, 33.93f).Around2D(200f), //grapeseedV3
                new Vector3(2585.63f, 4457.40f, 37.64f).Around2D(150f), //grapeseedV4
                new Vector3(2517.11f, 4233.88f, 39.13f).Around2D(130f), //grapeseedV5
                new Vector3(1836.02f, 3796.28f, 32.87f).Around2D(185f), //sandyShoresV1
                new Vector3(1612.86f, 3599.40f, 34.77f).Around2D(200f), //sandyShoresV2
                new Vector3(288.02f, 62.16f, 93.98f).Around2D(400f), //cityV1
                new Vector3(-471.42f, -105.83f, 38.45f).Around2D(350f), //cityV2
                new Vector3(-1269.51f, -350.46f, 36.34f).Around2D(250f), //cityV3
                new Vector3(-674.50f, -805.36f, 32.63f).Around2D(200f), //cityV4
                new Vector3(-23.97f, -877.57f, 32.03f).Around2D(250f), //cityV5
                new Vector3(291.73f, -1625.58f, 31.30f).Around2D(325f), //cityV6
                new Vector3(-90.28f, -1476.92f, 32.37f).Around2D(200f), //cityV7
                new Vector3(931.86f, -1998.72f, 29.83f).Around2D(200f) //cityV8
            };
            
            return Extensions.ClosestVehicleNodePosition(World.GetNextPositionOnStreet(sP.Where(x => x.DistanceTo2D(Main.Player) >= 300f).OrderBy(x => x.DistanceTo2D(Main.Player)).FirstOrDefault()));
        }
    }
}
