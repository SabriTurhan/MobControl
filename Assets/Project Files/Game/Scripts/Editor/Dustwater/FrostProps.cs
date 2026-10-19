using UnityEngine;

namespace Watermelon.Dustwater
{
    // Act 2 (Frostpeak) props and weapons. Same conventions as DustwaterProps: head props around the Head
    // bone (head ball ~0.46m at y 0.22, face +z), back props on Spine2, guns +z forward.
    public static class FrostProps
    {
        private static readonly Vector3 ALONG_Z = new Vector3(90f, 0f, 0f);
        private static readonly Vector3 ALONG_X = new Vector3(0f, 0f, 90f);

        #region Rangers

        public static Mesh GasMask()
        {
            return new PaletteMesh()
                .Box(new Vector3(0.34f, 0.2f, 0.14f), 0.05f, new Vector3(0f, 0.15f, 0.18f), Vector3.zero, Band.Charcoal, 0.15f, 0.55f)
                .Cylinder(0.06f, 0.06f, 0.05f, 10, new Vector3(-0.08f, 0.22f, 0.25f), ALONG_Z, Band.Lantern, 0f, 0.3f)
                .Cylinder(0.06f, 0.06f, 0.05f, 10, new Vector3(0.08f, 0.22f, 0.25f), ALONG_Z, Band.Lantern, 0f, 0.3f)
                .Cylinder(0.07f, 0.08f, 0.1f, 10, new Vector3(0f, 0.08f, 0.27f), ALONG_Z, Band.Iron, 0.1f, 0.5f)
                .Build("Prop_GasMask");
        }

        public static Mesh FuelTanks()
        {
            PaletteMesh mesh = new PaletteMesh();
            for (int side = -1; side <= 1; side += 2)
            {
                mesh.Cylinder(0.075f, 0.075f, 0.34f, 10, new Vector3(side * 0.08f, 0.0f, -0.2f), Vector3.zero, Band.Ember, 0.05f, 0.55f);
                mesh.Dome(0.075f, 3, 10, new Vector3(side * 0.08f, 0.17f, -0.2f), Vector3.zero, Vector3.one, Band.Ember, 0.05f, 0.4f);
            }
            mesh.Box(new Vector3(0.26f, 0.05f, 0.05f), 0.01f, new Vector3(0f, 0.1f, -0.2f), Vector3.zero, Band.Iron, 0.1f, 0.5f);
            mesh.Cylinder(0.025f, 0.025f, 0.3f, 6, new Vector3(0.12f, -0.12f, -0.12f), new Vector3(60f, 0f, 0f), Band.Charcoal, 0.2f, 0.5f);
            return mesh.Build("Prop_FuelTanks");
        }

        public static Mesh SteelHelmet()
        {
            return new PaletteMesh()
                .Dome(0.255f, 4, 14, new Vector3(0f, 0.26f, 0f), Vector3.zero, new Vector3(1f, 0.95f, 1.05f), Band.Steel, 0.05f, 0.5f)
                .Box(new Vector3(0.34f, 0.07f, 0.04f), 0.015f, new Vector3(0f, 0.3f, 0.24f), new Vector3(-10f, 0f, 0f), Band.Glass, 0f, 0.3f)
                .Cylinder(0.27f, 0.27f, 0.035f, 14, new Vector3(0f, 0.26f, 0f), Vector3.zero, Band.Iron, 0.1f, 0.5f)
                .Build("Prop_SteelHelmet");
        }

        // big riot shield held in front of the chest
        public static Mesh TowerShield()
        {
            PaletteMesh mesh = new PaletteMesh()
                .Box(new Vector3(0.62f, 0.86f, 0.06f), 0.04f, new Vector3(-0.05f, -0.05f, 0.3f), new Vector3(0f, 8f, 0f), Band.Steel, 0f, 0.5f)
                .Box(new Vector3(0.66f, 0.9f, 0.03f), 0.02f, new Vector3(-0.05f, -0.05f, 0.27f), new Vector3(0f, 8f, 0f), Band.Iron, 0.1f, 0.5f)
                .Box(new Vector3(0.4f, 0.1f, 0.03f), 0.01f, new Vector3(-0.05f, 0.18f, 0.335f), new Vector3(0f, 8f, 0f), Band.Glass, 0f, 0.3f);
            // ranger star
            mesh.Cylinder(0.11f, 0.11f, 0.02f, 5, new Vector3(-0.05f, -0.08f, 0.34f), new Vector3(90f, 8f, 0f), Band.RangerBlue, 0.05f, 0.4f);
            return mesh.Build("Prop_TowerShield");
        }

        public static Mesh FurHood()
        {
            PaletteMesh mesh = new PaletteMesh()
                .Dome(0.27f, 4, 14, new Vector3(0f, 0.2f, -0.02f), new Vector3(-8f, 0f, 0f), new Vector3(1.05f, 1.15f, 1.05f), Band.Snow, 0.05f, 0.35f);
            // fur trim around the face
            for (int i = 0; i < 12; i++)
            {
                float a = i / 12f * Mathf.PI * 2f;
                mesh.Sphere(0.06f, 3, 6, new Vector3(Mathf.Cos(a) * 0.2f, 0.22f + Mathf.Sin(a) * 0.2f, 0.2f), new Vector3(1f, 1f, 0.8f), Band.Fur, 0.05f, 0.5f);
            }
            return mesh.Build("Prop_FurHood");
        }

        public static Mesh Scarf(int band)
        {
            return new PaletteMesh()
                .Cylinder(0.17f, 0.16f, 0.09f, 12, new Vector3(0f, 0.08f, 0.0f), Vector3.zero, band, 0.1f, 0.5f)
                .Box(new Vector3(0.09f, 0.22f, 0.04f), 0.015f, new Vector3(0.08f, -0.04f, 0.15f), new Vector3(10f, 0f, -8f), band, 0.1f, 0.55f)
                .Build("Prop_Scarf_" + band);
        }

        #endregion

        #region White Wolves

        // fur hat with ear flaps
        public static Mesh Ushanka(int furBand, int capBand)
        {
            PaletteMesh mesh = new PaletteMesh()
                .Cylinder(0.24f, 0.23f, 0.12f, 14, new Vector3(0f, 0.37f, 0f), Vector3.zero, furBand, 0.05f, 0.5f)
                .Dome(0.22f, 3, 14, new Vector3(0f, 0.42f, 0f), Vector3.zero, new Vector3(1f, 0.7f, 1f), capBand, 0.1f, 0.5f)
                .Box(new Vector3(0.08f, 0.2f, 0.18f), 0.04f, new Vector3(-0.24f, 0.25f, 0f), new Vector3(0f, 0f, -8f), furBand, 0.05f, 0.5f)
                .Box(new Vector3(0.08f, 0.2f, 0.18f), 0.04f, new Vector3(0.24f, 0.25f, 0f), new Vector3(0f, 0f, 8f), furBand, 0.05f, 0.5f)
                .Box(new Vector3(0.3f, 0.09f, 0.08f), 0.03f, new Vector3(0f, 0.38f, 0.21f), Vector3.zero, furBand, 0.05f, 0.4f);
            mesh.Sphere(0.04f, 3, 8, new Vector3(0f, 0.38f, 0.255f), Vector3.one, Band.Gold, 0.1f, 0.4f);
            return mesh.Build("Prop_Ushanka_" + furBand + "_" + capBand);
        }

        public static Mesh WolfHood()
        {
            PaletteMesh mesh = new PaletteMesh()
                .Dome(0.265f, 4, 14, new Vector3(0f, 0.22f, -0.02f), Vector3.zero, new Vector3(1.05f, 1.1f, 1.05f), Band.WolfGrey, 0.05f, 0.5f);
            // ears
            mesh.Add(DustwaterBuildings.Pyramid(0.12f, 0.08f, 0.16f), new Vector3(-0.13f, 0.47f, 0f), new Vector3(0f, 0f, 12f), Vector3.one, Band.WolfGrey, 0.1f, 0.5f);
            mesh.Add(DustwaterBuildings.Pyramid(0.12f, 0.08f, 0.16f), new Vector3(0.13f, 0.47f, 0f), new Vector3(0f, 0f, -12f), Vector3.one, Band.WolfGrey, 0.1f, 0.5f);
            // snout over the forehead
            mesh.Box(new Vector3(0.14f, 0.1f, 0.16f), 0.04f, new Vector3(0f, 0.4f, 0.2f), new Vector3(-15f, 0f, 0f), Band.Fur, 0.1f, 0.5f);
            mesh.Sphere(0.03f, 3, 6, new Vector3(0f, 0.42f, 0.29f), Vector3.one, Band.Black, 0.1f, 0.4f);
            return mesh.Build("Prop_WolfHood");
        }

        public static Mesh HornedHelmet(int band)
        {
            PaletteMesh mesh = new PaletteMesh()
                .Dome(0.26f, 4, 14, new Vector3(0f, 0.26f, 0f), Vector3.zero, Vector3.one, band, 0.05f, 0.5f)
                .Cylinder(0.27f, 0.27f, 0.05f, 14, new Vector3(0f, 0.27f, 0f), Vector3.zero, Band.Gold, 0.1f, 0.4f)
                .Box(new Vector3(0.05f, 0.18f, 0.04f), 0.01f, new Vector3(0f, 0.22f, 0.26f), Vector3.zero, band, 0.1f, 0.5f);
            for (int side = -1; side <= 1; side += 2)
            {
                mesh.Cylinder(0.06f, 0.035f, 0.2f, 8, new Vector3(side * 0.3f, 0.4f, 0f), new Vector3(0f, 0f, -side * 55f), Band.Bone, 0.05f, 0.4f);
                mesh.Cylinder(0.035f, 0.0f, 0.16f, 8, new Vector3(side * 0.4f, 0.54f, 0f), new Vector3(0f, 0f, -side * 15f), Band.Bone, 0.05f, 0.4f);
            }
            return mesh.Build("Prop_HornedHelmet_" + band);
        }

        public static Mesh IceCrown()
        {
            PaletteMesh mesh = new PaletteMesh()
                .Cylinder(0.24f, 0.24f, 0.08f, 12, new Vector3(0f, 0.42f, 0f), Vector3.zero, Band.DeepIce, 0.05f, 0.5f);
            for (int i = 0; i < 6; i++)
            {
                float a = i / 6f * Mathf.PI * 2f;
                float h = i % 2 == 0 ? 0.26f : 0.17f;
                mesh.Cylinder(0.05f, 0.0f, h, 5, new Vector3(Mathf.Cos(a) * 0.21f, 0.46f + h * 0.5f, Mathf.Sin(a) * 0.21f), Vector3.zero, Band.Crystal, 0f, 0.35f);
            }
            return mesh.Build("Prop_IceCrown");
        }

        public static Mesh Beard(int band)
        {
            return new PaletteMesh()
                .Box(new Vector3(0.3f, 0.16f, 0.12f), 0.05f, new Vector3(0f, 0.1f, 0.19f), new Vector3(-12f, 0f, 0f), band, 0.05f, 0.5f)
                .Add(DustwaterBuildings.Pyramid(0.24f, 0.1f, 0.14f), new Vector3(0f, 0.04f, 0.2f), new Vector3(180f, 0f, 0f), Vector3.one, band, 0.05f, 0.5f)
                .Build("Prop_Beard_" + band);
        }

        public static Mesh FurCape(int band)
        {
            return new PaletteMesh()
                .Box(new Vector3(0.42f, 0.4f, 0.06f), 0.03f, new Vector3(0f, -0.12f, -0.17f), new Vector3(8f, 0f, 0f), band, 0.05f, 0.55f)
                .Cylinder(0.2f, 0.23f, 0.12f, 12, new Vector3(0f, 0.1f, -0.02f), Vector3.zero, Band.Fur, 0.05f, 0.5f)
                .Build("Prop_FurCape_" + band);
        }

        #endregion

        #region Weapons

        public static Mesh Flamethrower()
        {
            return new PaletteMesh()
                .Box(new Vector3(0.08f, 0.14f, 0.3f), 0.03f, new Vector3(0f, 0f, -0.08f), Vector3.zero, Band.Charcoal, 0.1f, 0.5f)
                .Cylinder(0.05f, 0.05f, 0.5f, 10, new Vector3(0f, 0.04f, 0.3f), ALONG_Z, Band.Iron, 0.1f, 0.5f)
                .Cylinder(0.075f, 0.06f, 0.1f, 10, new Vector3(0f, 0.04f, 0.58f), ALONG_Z, Band.Ember, 0.05f, 0.4f)
                .Cylinder(0.06f, 0.06f, 0.2f, 10, new Vector3(0f, -0.07f, 0.1f), ALONG_Z, Band.Ember, 0.05f, 0.5f)
                .Box(new Vector3(0.05f, 0.12f, 0.06f), 0.015f, new Vector3(0f, -0.1f, -0.04f), new Vector3(15f, 0f, 0f), Band.Black, 0.1f, 0.5f)
                .Build("Gun_Flamethrower");
        }

        public static Mesh BurstRifle()
        {
            return new PaletteMesh()
                .Box(new Vector3(0.07f, 0.13f, 0.26f), 0.025f, new Vector3(0f, -0.01f, -0.2f), Vector3.zero, Band.Charcoal, 0.1f, 0.5f)
                .Box(new Vector3(0.08f, 0.12f, 0.42f), 0.02f, new Vector3(0f, 0.03f, 0.12f), Vector3.zero, Band.WolfGrey, 0.05f, 0.5f)
                .Cylinder(0.025f, 0.025f, 0.3f, 8, new Vector3(0f, 0.05f, 0.48f), ALONG_Z, Band.Iron, 0.1f, 0.5f)
                .Box(new Vector3(0.05f, 0.14f, 0.07f), 0.015f, new Vector3(0f, -0.1f, 0.08f), new Vector3(-10f, 0f, 0f), Band.Black, 0.1f, 0.5f)
                .Box(new Vector3(0.04f, 0.05f, 0.16f), 0.01f, new Vector3(0f, 0.12f, 0.08f), Vector3.zero, Band.Black, 0.1f, 0.5f)
                .Box(new Vector3(0.03f, 0.03f, 0.05f), 0.005f, new Vector3(0f, 0.12f, 0.17f), Vector3.zero, Band.Crystal, 0f, 0.3f)
                .Build("Gun_BurstRifle");
        }

        public static Mesh RocketLauncher()
        {
            return new PaletteMesh()
                .Cylinder(0.1f, 0.1f, 0.9f, 12, new Vector3(0f, 0.08f, 0.15f), ALONG_Z, Band.Moss, 0.05f, 0.55f)
                .Cylinder(0.12f, 0.12f, 0.08f, 12, new Vector3(0f, 0.08f, 0.6f), ALONG_Z, Band.Iron, 0.1f, 0.5f)
                .Cylinder(0.12f, 0.12f, 0.08f, 12, new Vector3(0f, 0.08f, -0.3f), ALONG_Z, Band.Iron, 0.1f, 0.5f)
                .Box(new Vector3(0.05f, 0.14f, 0.07f), 0.015f, new Vector3(0f, -0.06f, 0.05f), new Vector3(10f, 0f, 0f), Band.Black, 0.1f, 0.5f)
                .Box(new Vector3(0.06f, 0.1f, 0.12f), 0.02f, new Vector3(-0.1f, 0.14f, 0.15f), Vector3.zero, Band.Iron, 0.1f, 0.5f)
                .Box(new Vector3(0.03f, 0.06f, 0.03f), 0.005f, new Vector3(-0.1f, 0.14f, 0.22f), Vector3.zero, Band.Glass, 0f, 0.3f)
                .Build("Gun_RocketLauncher");
        }

        public static Mesh Rocket()
        {
            PaletteMesh mesh = new PaletteMesh()
                .Cylinder(0.06f, 0.06f, 0.36f, 10, Vector3.zero, ALONG_Z, Band.Moss, 0.05f, 0.5f)
                .Cylinder(0.06f, 0.0f, 0.14f, 10, new Vector3(0f, 0f, 0.25f), ALONG_Z, Band.ScorpionRed, 0.05f, 0.4f)
                .Cylinder(0.045f, 0.05f, 0.06f, 10, new Vector3(0f, 0f, -0.2f), ALONG_Z, Band.Lantern, 0f, 0.2f);
            for (int i = 0; i < 4; i++)
                mesh.Box(new Vector3(0.02f, 0.1f, 0.1f), 0.005f, Quaternion.Euler(0f, 0f, i * 90f) * new Vector3(0f, 0.07f, 0f) + new Vector3(0f, 0f, -0.14f), new Vector3(0f, 0f, i * 90f), Band.Iron, 0.1f, 0.5f);
            return mesh.Build("Bullet_Rocket");
        }

        public static Mesh FlameBall()
        {
            return new PaletteMesh()
                .Sphere(0.16f, 4, 10, Vector3.zero, new Vector3(1f, 1f, 1.3f), Band.Ember, 0f, 0.4f)
                .Sphere(0.1f, 3, 8, new Vector3(0f, 0.02f, 0.06f), Vector3.one, Band.Lantern, 0f, 0.2f)
                .Build("Bullet_Flame");
        }

        #endregion

        #region Obstacles

        public static Mesh IceWall(int columns, int rows)
        {
            PaletteMesh mesh = new PaletteMesh();
            const float W = 0.55f, H = 0.36f, D = 0.42f;
            for (int r = 0; r < rows; r++)
            {
                int count = columns - (r % 2);
                for (int c = 0; c < count; c++)
                {
                    float x = (c - (columns - 1) * 0.5f) * W + (r % 2) * W * 0.5f;
                    float tilt = ((c * 5 + r * 7) % 5 - 2) * 2f;
                    mesh.Box(new Vector3(W * 0.95f, H, D), 0.05f, new Vector3(x, H * 0.5f + r * H * 0.95f, 0f), new Vector3(0f, tilt, 0f), (c + r) % 3 == 0 ? Band.IceBlue : Band.Frost, 0f, 0.5f);
                }
            }
            // snow cap
            mesh.Box(new Vector3(columns * W, 0.1f, D * 1.05f), 0.04f, new Vector3(0f, rows * H * 0.95f + 0.02f, 0f), Vector3.zero, Band.Snow, 0f, 0.2f);
            return mesh.Build("Obstacle_IceWall_" + columns + "x" + rows);
        }

        public static Mesh IceBarrel()
        {
            return new PaletteMesh()
                .Cylinder(0.62f, 0.62f, 1.25f, 12, new Vector3(0f, 0.65f, 0f), ALONG_X, Band.Log, 0.05f, 0.55f)
                .Cylinder(0.64f, 0.64f, 0.1f, 12, new Vector3(-0.35f, 0.65f, 0f), ALONG_X, Band.Iron, 0.1f, 0.5f)
                .Cylinder(0.64f, 0.64f, 0.1f, 12, new Vector3(0.35f, 0.65f, 0f), ALONG_X, Band.Iron, 0.1f, 0.5f)
                .Box(new Vector3(1.0f, 0.14f, 0.7f), 0.05f, new Vector3(0f, 1.25f, 0f), Vector3.zero, Band.Snow, 0f, 0.25f)
                .Build("Obstacle_SnowBarrel");
        }

        #endregion
    }
}
