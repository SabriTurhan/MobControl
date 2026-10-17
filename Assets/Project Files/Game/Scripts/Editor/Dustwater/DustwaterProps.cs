using UnityEngine;

namespace Watermelon.Dustwater
{
    // Low-poly props and weapons for the new units, built in the local space of the bone/gun they attach to.
    // Head props: origin = Head bone, head (with helmet) is a ~0.46m ball centred at y 0.22, face looks +z.
    // Back props: origin = Spine2 bone, torso ~0.27m wide/deep. Guns: +z forward like the template rifle.
    public static class DustwaterProps
    {
        private static readonly Vector3 ALONG_Z = new Vector3(90f, 0f, 0f);

        #region Hats and gear

        public static Mesh BoonieHat()
        {
            return new PaletteMesh()
                .Cylinder(0.34f, 0.34f, 0.03f, 16, new Vector3(0f, 0.35f, 0f), Vector3.zero, Band.Khaki, 0.2f, 0.45f)
                .Cylinder(0.215f, 0.19f, 0.15f, 14, new Vector3(0f, 0.43f, 0f), Vector3.zero, Band.Khaki, 0.1f, 0.4f)
                .Cylinder(0.218f, 0.218f, 0.035f, 14, new Vector3(0f, 0.385f, 0f), Vector3.zero, Band.Olive, 0.3f, 0.6f)
                .Build("Prop_BoonieHat");
        }

        public static Mesh MedicHelmetCross()
        {
            return new PaletteMesh()
                .Cylinder(0.075f, 0.075f, 0.025f, 14, new Vector3(0f, 0.31f, 0.205f), new Vector3(70f, 0f, 0f), Band.White, 0f, 0.15f)
                .Box(new Vector3(0.1f, 0.03f, 0.02f), 0.004f, new Vector3(0f, 0.313f, 0.222f), new Vector3(-20f, 0f, 0f), Band.ScorpionRed, 0.2f, 0.4f)
                .Box(new Vector3(0.03f, 0.1f, 0.02f), 0.004f, new Vector3(0f, 0.313f, 0.222f), new Vector3(-20f, 0f, 0f), Band.ScorpionRed, 0.2f, 0.4f)
                .Build("Prop_MedicHelmetCross");
        }

        public static Mesh MedicBag()
        {
            return new PaletteMesh()
                .Box(new Vector3(0.24f, 0.24f, 0.13f), 0.03f, new Vector3(0f, 0.0f, -0.19f), Vector3.zero, Band.White, 0.05f, 0.35f)
                .Box(new Vector3(0.26f, 0.05f, 0.15f), 0.015f, new Vector3(0f, 0.1f, -0.19f), Vector3.zero, Band.Bone, 0.3f, 0.6f)
                .Box(new Vector3(0.12f, 0.035f, 0.02f), 0.005f, new Vector3(0f, -0.01f, -0.26f), Vector3.zero, Band.ScorpionRed, 0.2f, 0.4f)
                .Box(new Vector3(0.035f, 0.12f, 0.02f), 0.005f, new Vector3(0f, -0.01f, -0.26f), Vector3.zero, Band.ScorpionRed, 0.2f, 0.4f)
                .Build("Prop_MedicBag");
        }

        public static Mesh Goggles()
        {
            return new PaletteMesh()
                .Cylinder(0.235f, 0.235f, 0.04f, 18, new Vector3(0f, 0.33f, 0f), new Vector3(-12f, 0f, 0f), Band.Black, 0.3f, 0.6f)
                .Cylinder(0.05f, 0.05f, 0.04f, 12, new Vector3(-0.07f, 0.35f, 0.215f), new Vector3(80f, 0f, 0f), Band.Glass, 0f, 0.3f)
                .Cylinder(0.05f, 0.05f, 0.04f, 12, new Vector3(0.07f, 0.35f, 0.215f), new Vector3(80f, 0f, 0f), Band.Glass, 0f, 0.3f)
                .Build("Prop_Goggles");
        }

        public static Mesh GrenadeBelt()
        {
            PaletteMesh mesh = new PaletteMesh()
                .Box(new Vector3(0.06f, 0.34f, 0.03f), 0.01f, new Vector3(0f, 0.0f, 0.145f), new Vector3(0f, 0f, 38f), Band.DarkWood, 0.3f, 0.6f);
            for (int i = 0; i < 3; i++)
            {
                float t = (i - 1) * 0.085f;
                Vector3 position = new Vector3(-t * 0.62f, t * 0.79f, 0.17f);
                mesh.Sphere(0.042f, 4, 8, position, Vector3.one, Band.Olive, 0.15f, 0.6f);
                mesh.Cylinder(0.014f, 0.014f, 0.03f, 6, position + new Vector3(0f, 0.045f, 0f), Vector3.zero, Band.Iron, 0.2f, 0.4f);
            }

            return mesh.Build("Prop_GrenadeBelt");
        }

        public static Mesh Bandana(int band)
        {
            return new PaletteMesh()
                .Box(new Vector3(0.4f, 0.12f, 0.12f), 0.03f, new Vector3(0f, 0.135f, 0.17f), new Vector3(-8f, 0f, 0f), band, 0.15f, 0.45f)
                .Box(new Vector3(0.16f, 0.1f, 0.06f), 0.02f, new Vector3(0f, 0.07f, 0.22f), new Vector3(-30f, 0f, 45f), band, 0.2f, 0.55f)
                .Build("Prop_Bandana_" + band);
        }

        public static Mesh CowboyHat(int hatBand, int ribbonBand, float scale)
        {
            Vector3 s = Vector3.one * scale;
            PaletteMesh mesh = new PaletteMesh();
            // curled brim: centre plate + two raised side plates; the crown swallows the helmet top
            mesh.Add(Shapes.Cylinder(0.31f, 0.31f, 0.03f, 18), new Vector3(0f, 0.33f, 0.02f) * scale, Vector3.zero, s, hatBand, 0.25f, 0.5f);
            mesh.Add(Shapes.Box(new Vector3(0.12f, 0.025f, 0.42f), 0.01f), new Vector3(-0.31f, 0.36f, 0.02f) * scale, new Vector3(0f, 0f, 28f), s, hatBand, 0.25f, 0.5f);
            mesh.Add(Shapes.Box(new Vector3(0.12f, 0.025f, 0.42f), 0.01f), new Vector3(0.31f, 0.36f, 0.02f) * scale, new Vector3(0f, 0f, -28f), s, hatBand, 0.25f, 0.5f);
            mesh.Add(Shapes.Cylinder(0.225f, 0.185f, 0.21f, 12), new Vector3(0f, 0.435f, 0.0f) * scale, Vector3.zero, s, hatBand, 0.1f, 0.45f);
            mesh.Add(Shapes.Box(new Vector3(0.08f, 0.05f, 0.36f), 0.02f), new Vector3(0f, 0.545f, 0f) * scale, Vector3.zero, s, hatBand, 0.05f, 0.3f);
            mesh.Add(Shapes.Cylinder(0.228f, 0.228f, 0.045f, 12), new Vector3(0f, 0.37f, 0f) * scale, Vector3.zero, s, ribbonBand, 0.15f, 0.45f);

            return mesh.Build("Prop_CowboyHat_" + hatBand + "_" + ribbonBand);
        }

        public static Mesh SteelJaw()
        {
            return new PaletteMesh()
                .Box(new Vector3(0.36f, 0.13f, 0.1f), 0.03f, new Vector3(0f, 0.11f, 0.19f), Vector3.zero, Band.Steel, 0f, 0.45f)
                .Box(new Vector3(0.3f, 0.03f, 0.03f), 0.005f, new Vector3(0f, 0.135f, 0.245f), Vector3.zero, Band.White, 0f, 0.2f)
                .Box(new Vector3(0.3f, 0.03f, 0.03f), 0.005f, new Vector3(0f, 0.085f, 0.245f), Vector3.zero, Band.White, 0f, 0.2f)
                .Build("Prop_SteelJaw");
        }

        public static Mesh ShoulderSpikes()
        {
            PaletteMesh mesh = new PaletteMesh();
            for (int side = -1; side <= 1; side += 2)
            {
                mesh.Dome(0.13f, 3, 10, new Vector3(0.24f * side, 0.12f, 0f), new Vector3(0f, 0f, -25f * side), Vector3.one, Band.Iron, 0.15f, 0.55f);
                for (int i = 0; i < 2; i++)
                    mesh.Cylinder(0.035f, 0.0f, 0.13f, 6, new Vector3((0.26f + 0.02f * i) * side, 0.22f, -0.04f + 0.08f * i), new Vector3(0f, 0f, -25f * side), Band.Steel, 0f, 0.3f);
            }

            return mesh.Build("Prop_ShoulderSpikes");
        }

        public static Mesh ScorpionBadge()
        {
            PaletteMesh mesh = new PaletteMesh()
                .Cylinder(0.075f, 0.075f, 0.025f, 14, new Vector3(0f, 0.02f, 0.15f), ALONG_Z, Band.Gold, 0.1f, 0.4f)
                .Sphere(0.03f, 3, 8, new Vector3(0f, 0.0f, 0.168f), new Vector3(1f, 1.3f, 0.6f), Band.ScorpionRed, 0.1f, 0.5f);
            for (int i = 0; i < 3; i++)
                mesh.Sphere(0.016f, 3, 6, new Vector3(0f, 0.035f + i * 0.02f, 0.168f), new Vector3(1f, 1f, 0.6f), Band.ScorpionRed, 0.1f, 0.5f);

            return mesh.Build("Prop_ScorpionBadge");
        }

        #endregion

        #region Weapons

        public static Mesh SniperRifle()
        {
            return new PaletteMesh()
                .Box(new Vector3(0.07f, 0.15f, 0.3f), 0.02f, new Vector3(0f, -0.01f, -0.22f), Vector3.zero, Band.Wood, 0.1f, 0.5f)
                .Box(new Vector3(0.08f, 0.11f, 0.42f), 0.02f, new Vector3(0f, 0.04f, 0.1f), Vector3.zero, Band.Iron, 0.1f, 0.5f)
                .Box(new Vector3(0.05f, 0.12f, 0.06f), 0.015f, new Vector3(0f, -0.06f, 0.0f), new Vector3(15f, 0f, 0f), Band.DarkWood, 0.1f, 0.5f)
                .Cylinder(0.024f, 0.022f, 0.62f, 8, new Vector3(0f, 0.06f, 0.6f), ALONG_Z, Band.Iron, 0.1f, 0.5f)
                .Cylinder(0.036f, 0.036f, 0.06f, 8, new Vector3(0f, 0.06f, 0.9f), ALONG_Z, Band.Black, 0.2f, 0.5f)
                .Cylinder(0.042f, 0.042f, 0.3f, 10, new Vector3(0f, 0.15f, 0.08f), ALONG_Z, Band.Black, 0.1f, 0.4f)
                .Cylinder(0.05f, 0.05f, 0.04f, 10, new Vector3(0f, 0.15f, 0.24f), ALONG_Z, Band.Glass, 0f, 0.2f)
                .Cylinder(0.05f, 0.05f, 0.04f, 10, new Vector3(0f, 0.15f, -0.08f), ALONG_Z, Band.Black, 0.2f, 0.5f)
                .Build("Gun_SniperRifle");
        }

        public static Mesh GrenadeLauncher()
        {
            PaletteMesh mesh = new PaletteMesh()
                .Box(new Vector3(0.08f, 0.14f, 0.26f), 0.025f, new Vector3(0f, -0.01f, -0.2f), Vector3.zero, Band.DarkWood, 0.1f, 0.5f)
                .Cylinder(0.11f, 0.11f, 0.18f, 10, new Vector3(0f, 0.04f, 0.04f), ALONG_Z, Band.Olive, 0.1f, 0.55f)
                .Cylinder(0.075f, 0.075f, 0.44f, 10, new Vector3(0f, 0.06f, 0.34f), ALONG_Z, Band.Iron, 0.1f, 0.5f)
                .Cylinder(0.09f, 0.09f, 0.06f, 10, new Vector3(0f, 0.06f, 0.56f), ALONG_Z, Band.Black, 0.2f, 0.5f)
                .Box(new Vector3(0.05f, 0.13f, 0.06f), 0.015f, new Vector3(0f, -0.08f, -0.04f), new Vector3(15f, 0f, 0f), Band.Black, 0.1f, 0.5f)
                .Box(new Vector3(0.04f, 0.04f, 0.14f), 0.01f, new Vector3(0f, 0.17f, 0.2f), Vector3.zero, Band.Black, 0.1f, 0.5f);
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f * Mathf.Deg2Rad;
                mesh.Cylinder(0.025f, 0.025f, 0.19f, 6, new Vector3(Mathf.Cos(angle) * 0.07f, 0.04f + Mathf.Sin(angle) * 0.07f, 0.04f), ALONG_Z, Band.Iron, 0.2f, 0.6f);
            }

            return mesh.Build("Gun_GrenadeLauncher");
        }

        public static Mesh Grenade()
        {
            return new PaletteMesh()
                .Sphere(0.11f, 4, 10, Vector3.zero, new Vector3(1f, 1f, 1.25f), Band.Olive, 0.05f, 0.6f)
                .Cylinder(0.04f, 0.04f, 0.06f, 8, new Vector3(0f, 0f, -0.14f), ALONG_Z, Band.Iron, 0.1f, 0.5f)
                .Cylinder(0.106f, 0.106f, 0.018f, 10, new Vector3(0f, 0f, 0.03f), ALONG_Z, Band.Gold, 0.1f, 0.4f)
                .Build("Bullet_Grenade");
        }

        #endregion

        #region Level props

        public static Mesh Sandbags(int columns, int rows)
        {
            PaletteMesh mesh = new PaletteMesh();
            const float BAG_W = 0.5f, BAG_H = 0.24f, BAG_D = 0.34f;
            for (int r = 0; r < rows; r++)
            {
                int count = columns - (r % 2);
                float offset = (r % 2) * BAG_W * 0.5f;
                for (int c = 0; c < count; c++)
                {
                    float x = (c - (columns - 1) * 0.5f) * BAG_W + offset;
                    float tilt = ((c * 7 + r * 3) % 5 - 2) * 2f;
                    int band = (c + r) % 3 == 0 ? Band.Sand : Band.Khaki;
                    mesh.Box(new Vector3(BAG_W * 0.96f, BAG_H, BAG_D), 0.08f, new Vector3(x, BAG_H * 0.5f + r * BAG_H * 0.9f, 0f), new Vector3(0f, tilt, tilt * 0.5f), band, 0.1f, 0.5f);
                }
            }

            return mesh.Build("Obstacle_Sandbags_" + columns + "x" + rows);
        }

        public static Mesh SupplyCrate()
        {
            PaletteMesh mesh = new PaletteMesh()
                .Box(new Vector3(1.0f, 0.9f, 0.9f), 0.04f, new Vector3(0f, 0.45f, 0f), Vector3.zero, Band.Wood, 0.05f, 0.5f);
            // planks and corner frame
            for (int side = -1; side <= 1; side += 2)
            {
                mesh.Box(new Vector3(0.1f, 0.94f, 0.94f), 0.02f, new Vector3(side * 0.47f, 0.45f, 0f), Vector3.zero, Band.DarkWood, 0.1f, 0.5f);
                mesh.Box(new Vector3(1.04f, 0.1f, 0.94f), 0.02f, new Vector3(0f, 0.45f + side * 0.42f, 0f), Vector3.zero, Band.DarkWood, 0.1f, 0.5f);
            }
            mesh.Box(new Vector3(1.0f, 0.08f, 0.04f), 0.01f, new Vector3(0f, 0.45f, 0.46f), new Vector3(0f, 0f, 42f), Band.DarkWood, 0.1f, 0.5f);
            // blue ranger band around the crate
            mesh.Box(new Vector3(0.22f, 0.96f, 0.96f), 0.02f, new Vector3(0.2f, 0.45f, 0f), Vector3.zero, Band.RangerBlue, 0.05f, 0.4f);

            return mesh.Build("Obstacle_SupplyCrate");
        }

        public static Mesh SuppliesPickup()
        {
            return new PaletteMesh()
                .Box(new Vector3(0.36f, 0.26f, 0.3f), 0.03f, new Vector3(0f, 0.13f, 0f), Vector3.zero, Band.Wood, 0.05f, 0.45f)
                .Box(new Vector3(0.38f, 0.05f, 0.32f), 0.01f, new Vector3(0f, 0.2f, 0f), Vector3.zero, Band.DarkWood, 0.1f, 0.5f)
                .Box(new Vector3(0.06f, 0.28f, 0.32f), 0.01f, new Vector3(0f, 0.13f, 0f), Vector3.zero, Band.RangerBlue, 0.1f, 0.4f)
                .Build("Drop_Supplies");
        }

        #endregion
    }
}
