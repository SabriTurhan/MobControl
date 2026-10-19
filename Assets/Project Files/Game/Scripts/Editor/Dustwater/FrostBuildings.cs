using UnityEngine;
using static SquadCommand.Dustwater.DustwaterBuildings;

namespace SquadCommand.Dustwater
{
    // Act 2 (Frostpeak) town pieces and scenery: log cabins under snow, pines, icy rocks, mountains,
    // the White Wolves' camps and fortress walls. Same conventions as DustwaterBuildings (front faces -z).
    public static class FrostBuildings
    {
        private static readonly Vector3 ALONG_Z = new Vector3(90f, 0f, 0f);
        private static readonly Vector3 ALONG_X = new Vector3(0f, 0f, 90f);

        #region Shapes

        // gable roof with a snow blanket that leaves the eaves visible
        private static void SnowRoof(PaletteMesh m, Vector3 position, float width, float depth, float height, int band)
        {
            m.Add(Prism(width, depth, height), position, Vector3.zero, Vector3.one, band, 0.05f, 0.55f);
            m.Add(Prism(width + 0.12f, depth * 0.9f, height * 0.92f), position + new Vector3(0f, height * 0.14f, 0f), Vector3.zero, Vector3.one, Band.Snow, 0f, 0.25f);
        }

        // walls of stacked logs with the log ends poking out at the corners
        private static void LogWalls(PaletteMesh m, Vector3 center, float width, float depth, float height)
        {
            m.Box(new Vector3(width, height, depth), 0.08f, center + new Vector3(0f, height * 0.5f, 0f), Vector3.zero, Band.Log, 0.05f, 0.55f);
            int rows = Mathf.Max(2, Mathf.RoundToInt(height / 0.42f));
            for (int i = 0; i < rows; i++)
            {
                float y = (i + 0.5f) * height / rows;
                m.Cylinder(0.17f, 0.17f, width + 0.36f, 7, center + new Vector3(0f, y, -depth * 0.5f), ALONG_X, i % 2 == 0 ? Band.Log : Band.DarkWood, 0.1f, 0.6f);
                m.Cylinder(0.17f, 0.17f, depth + 0.36f, 7, center + new Vector3(-width * 0.5f, y, 0f), ALONG_Z, i % 2 == 0 ? Band.DarkWood : Band.Log, 0.1f, 0.6f);
                m.Cylinder(0.17f, 0.17f, depth + 0.36f, 7, center + new Vector3(width * 0.5f, y, 0f), ALONG_Z, i % 2 == 0 ? Band.DarkWood : Band.Log, 0.1f, 0.6f);
            }
        }

        private static void Chimney(PaletteMesh m, Vector3 position, float height, bool smoking = false)
        {
            m.Box(new Vector3(0.6f, height, 0.6f), 0.06f, position + new Vector3(0f, height * 0.5f, 0f), Vector3.zero, Band.Slate, 0.1f, 0.6f);
            m.Box(new Vector3(0.72f, 0.16f, 0.72f), 0.04f, position + new Vector3(0f, height, 0f), Vector3.zero, Band.Snow, 0f, 0.25f);
            if (smoking) m.Sphere(0.24f, 3, 8, position + new Vector3(0.1f, height + 0.4f, 0f), Vector3.one, Band.Frost, 0f, 0.3f);
        }

        private static void Firewood(PaletteMesh m, Vector3 position, int columns)
        {
            for (int row = 0; row < 3; row++)
                for (int i = 0; i < columns - row; i++)
                    m.Cylinder(0.13f, 0.13f, 0.8f, 7, position + new Vector3((i - (columns - row - 1) * 0.5f) * 0.27f, 0.13f + row * 0.23f, 0f), ALONG_Z, (i + row) % 2 == 0 ? Band.Log : Band.Wood, 0.1f, 0.55f);
        }

        private static void SnowMound(PaletteMesh m, Vector3 position, float size)
        {
            m.Sphere(size, 3, 10, position, new Vector3(1.4f, 0.4f, 1f), Band.Snow, 0f, 0.3f);
        }

        #endregion

        #region Scenery

        public static Mesh Pine(float height, int seed)
        {
            System.Random r = new System.Random(seed);
            PaletteMesh m = new PaletteMesh()
                .Cylinder(0.18f, 0.14f, height * 0.3f, 7, new Vector3(0f, height * 0.15f, 0f), Vector3.zero, Band.Log, 0.1f, 0.6f);
            int tiers = 3;
            for (int i = 0; i < tiers; i++)
            {
                float t = i / (float)tiers;
                float radius = height * (0.34f - t * 0.09f);
                float y = height * (0.22f + t * 0.24f);
                float tier = height * 0.38f;
                m.Cylinder(radius, 0f, tier, 8, new Vector3(0f, y + tier * 0.5f, 0f), new Vector3(0f, (float)r.NextDouble() * 45f, 0f), i % 2 == 0 ? Band.Pine : Band.PineDark, 0.05f, 0.6f);
                m.Cylinder(radius * 0.62f, 0f, tier * 0.55f, 8, new Vector3(0f, y + tier * 0.73f, 0f), new Vector3(0f, (float)r.NextDouble() * 45f, 0f), Band.Snow, 0f, 0.3f);
            }
            return m.Build("Frost_Pine_" + height + "_" + seed);
        }

        public static Mesh IceRock(float size, int seed)
        {
            System.Random r = new System.Random(seed);
            PaletteMesh m = new PaletteMesh();
            for (int i = 0; i < 3; i++)
            {
                float s = size * (0.5f + (float)r.NextDouble() * 0.5f);
                Vector3 position = new Vector3((float)(r.NextDouble() - 0.5) * size, s * 0.3f, (float)(r.NextDouble() - 0.5) * size * 0.6f);
                m.Box(new Vector3(s, s * 0.7f, s * 0.9f), s * 0.25f, position, new Vector3(0f, (float)r.NextDouble() * 90f, (float)(r.NextDouble() - 0.5) * 20f), Band.Slate, 0.1f, 0.6f);
                m.Box(new Vector3(s * 0.8f, s * 0.15f, s * 0.7f), s * 0.07f, position + new Vector3(0f, s * 0.36f, 0f), new Vector3(0f, (float)r.NextDouble() * 90f, 0f), Band.Snow, 0f, 0.3f);
            }
            return m.Build("Frost_Rock_" + seed);
        }

        public static Mesh Mountain(float width, float height, int seed)
        {
            System.Random r = new System.Random(seed);
            PaletteMesh m = new PaletteMesh();
            m.Cylinder(width * 0.5f, 0f, height, 6, new Vector3(0f, height * 0.5f, 0f), new Vector3(0f, (float)r.NextDouble() * 60f, 0f), Band.Slate, 0.05f, 0.65f);
            m.Cylinder(width * 0.21f, 0f, height * 0.42f, 6, new Vector3(0f, height * 0.79f + 0.05f, 0f), new Vector3(0f, (float)r.NextDouble() * 60f, 0f), Band.Snow, 0f, 0.3f);
            for (int i = 0; i < 2; i++)
            {
                float side = i * 2 - 1;
                float h = height * (0.5f + (float)r.NextDouble() * 0.2f);
                Vector3 p = new Vector3(side * width * 0.38f, h * 0.5f, (float)(r.NextDouble() - 0.5) * width * 0.2f);
                m.Cylinder(width * 0.3f, 0f, h, 6, p, new Vector3(0f, (float)r.NextDouble() * 60f, 0f), Band.NightBlue, 0.05f, 0.6f);
                m.Cylinder(width * 0.12f, 0f, h * 0.38f, 6, p + new Vector3(0f, h * 0.31f + 0.04f, 0f), Vector3.zero, Band.Snow, 0f, 0.3f);
            }
            return m.Build("Frost_Mountain_" + seed);
        }

        public static Mesh FrozenLake(float radius)
        {
            PaletteMesh m = new PaletteMesh()
                .Cylinder(radius, radius, 0.08f, 20, new Vector3(0f, 0.04f, 0f), Vector3.zero, Band.IceBlue, 0f, 0.35f)
                .Cylinder(radius * 1.08f, radius * 1.08f, 0.05f, 20, new Vector3(0f, 0.02f, 0f), Vector3.zero, Band.Snow, 0f, 0.3f);
            // cracks and frost patches
            for (int i = 0; i < 5; i++)
                m.Box(new Vector3(radius * 0.5f, 0.02f, 0.06f), 0f, new Vector3(Mathf.Cos(i * 1.3f) * radius * 0.4f, 0.09f, Mathf.Sin(i * 1.3f) * radius * 0.4f), new Vector3(0f, i * 47f, 0f), Band.Frost, 0f, 0.2f);
            return m.Build("Frost_Lake_" + radius);
        }

        public static Mesh SnowDrift(float size, int seed)
        {
            System.Random r = new System.Random(seed);
            PaletteMesh m = new PaletteMesh();
            for (int i = 0; i < 3; i++)
                SnowMound(m, new Vector3((float)(r.NextDouble() - 0.5) * size * 1.5f, 0f, (float)(r.NextDouble() - 0.5) * size), size * (0.5f + (float)r.NextDouble() * 0.4f));
            return m.Build("Frost_Drift_" + seed);
        }

        public static Mesh Crystals(float size, int seed)
        {
            System.Random r = new System.Random(seed);
            PaletteMesh m = new PaletteMesh();
            for (int i = 0; i < 5; i++)
            {
                float h = size * (0.6f + (float)r.NextDouble() * 0.8f);
                float radius = size * 0.16f;
                Vector3 tilt = new Vector3((float)(r.NextDouble() - 0.5) * 50f, 0f, (float)(r.NextDouble() - 0.5) * 50f);
                Vector3 p = new Vector3((float)(r.NextDouble() - 0.5) * size, 0f, (float)(r.NextDouble() - 0.5) * size * 0.6f);
                Quaternion q = Quaternion.Euler(tilt);
                m.Cylinder(radius, radius, h, 6, p + q * new Vector3(0f, h * 0.5f, 0f), tilt, i % 2 == 0 ? Band.Crystal : Band.DeepIce, 0f, 0.4f);
                m.Cylinder(radius, 0f, radius * 1.6f, 6, p + q * new Vector3(0f, h + radius * 0.8f, 0f), tilt, Band.Crystal, 0f, 0.3f);
            }
            m.Box(new Vector3(size * 1.3f, size * 0.25f, size * 0.8f), size * 0.1f, new Vector3(0f, size * 0.08f, 0f), Vector3.zero, Band.Slate, 0.1f, 0.6f);
            return m.Build("Frost_Crystals_" + seed);
        }

        public static Mesh LanternPost()
        {
            return new PaletteMesh()
                .Cylinder(0.08f, 0.08f, 2.6f, 8, new Vector3(0f, 1.3f, 0f), Vector3.zero, Band.Log, 0.1f, 0.5f)
                .Box(new Vector3(0.6f, 0.08f, 0.08f), 0.01f, new Vector3(0.25f, 2.55f, 0f), Vector3.zero, Band.Iron, 0.1f, 0.5f)
                .Box(new Vector3(0.3f, 0.38f, 0.3f), 0.04f, new Vector3(0.5f, 2.3f, 0f), Vector3.zero, Band.Lantern, 0f, 0.25f)
                .Add(Pyramid(0.4f, 0.4f, 0.2f), new Vector3(0.5f, 2.49f, 0f), Vector3.zero, Vector3.one, Band.Snow, 0f, 0.3f)
                .Build("Frost_Lantern");
        }

        public static Mesh Snowman()
        {
            return new PaletteMesh()
                .Sphere(0.55f, 4, 10, new Vector3(0f, 0.5f, 0f), Vector3.one, Band.Snow, 0f, 0.35f)
                .Sphere(0.4f, 4, 10, new Vector3(0f, 1.2f, 0f), Vector3.one, Band.Snow, 0f, 0.35f)
                .Sphere(0.28f, 4, 10, new Vector3(0f, 1.72f, 0f), Vector3.one, Band.Snow, 0f, 0.35f)
                .Cylinder(0.05f, 0f, 0.3f, 6, new Vector3(0f, 1.72f, -0.38f), new Vector3(-90f, 0f, 0f), Band.Orange, 0.05f, 0.4f)
                .Cylinder(0.3f, 0.3f, 0.12f, 10, new Vector3(0f, 1.42f, 0f), Vector3.zero, Band.ScorpionRed, 0.05f, 0.5f)
                .Cylinder(0.2f, 0.2f, 0.3f, 10, new Vector3(0f, 2.05f, 0f), Vector3.zero, Band.Black, 0.1f, 0.5f)
                .Cylinder(0.3f, 0.3f, 0.04f, 10, new Vector3(0f, 1.92f, 0f), Vector3.zero, Band.Black, 0.1f, 0.5f)
                .Build("Frost_Snowman");
        }

        public static Mesh Sled()
        {
            PaletteMesh m = new PaletteMesh();
            for (int side = -1; side <= 1; side += 2)
            {
                m.Box(new Vector3(0.08f, 0.08f, 2.2f), 0.02f, new Vector3(side * 0.55f, 0.05f, 0f), Vector3.zero, Band.Iron, 0.1f, 0.5f);
                m.Box(new Vector3(0.08f, 0.08f, 0.4f), 0.02f, new Vector3(side * 0.55f, 0.18f, -1.18f), new Vector3(-45f, 0f, 0f), Band.Iron, 0.1f, 0.5f);
            }
            m.Box(new Vector3(1.3f, 0.12f, 2.0f), 0.03f, new Vector3(0f, 0.25f, 0.05f), Vector3.zero, Band.Wood, 0.1f, 0.55f);
            Crate(m, new Vector3(-0.2f, 0.31f, 0.5f), 0.6f, 8f);
            Crate(m, new Vector3(0.25f, 0.31f, -0.25f), 0.5f, -12f);
            m.Box(new Vector3(0.9f, 0.3f, 0.6f), 0.1f, new Vector3(0f, 0.45f, -0.6f), Vector3.zero, Band.Canvas, 0.1f, 0.55f);
            return m.Build("Frost_Sled");
        }

        public static Mesh StoneWall(float length)
        {
            PaletteMesh m = new PaletteMesh();
            m.Box(new Vector3(length, 2.8f, 1.0f), 0.1f, new Vector3(0f, 1.4f, 0f), Vector3.zero, Band.Slate, 0.05f, 0.6f);
            int blocks = Mathf.RoundToInt(length / 1.0f);
            for (int i = 0; i < blocks; i++)
            {
                float x = -length * 0.5f + (i + 0.5f) * length / blocks;
                if (i % 2 == 0)
                {
                    m.Box(new Vector3(0.7f, 0.6f, 1.1f), 0.06f, new Vector3(x, 3.1f, 0f), Vector3.zero, Band.Slate, 0.05f, 0.6f);
                    m.Box(new Vector3(0.74f, 0.14f, 1.14f), 0.04f, new Vector3(x, 3.44f, 0f), Vector3.zero, Band.Snow, 0f, 0.3f);
                }
                m.Box(new Vector3(length / blocks - 0.06f, 0.06f, 1.02f), 0.01f, new Vector3(x, 0.9f + (i % 2) * 0.9f, 0f), Vector3.zero, Band.Charcoal, 0.2f, 0.6f);
            }
            m.Box(new Vector3(length + 0.1f, 0.16f, 1.1f), 0.04f, new Vector3(0f, 2.82f, 0f), Vector3.zero, Band.Snow, 0f, 0.3f);
            return m.Build("Frost_StoneWall_" + length);
        }

        public static Mesh WolfTower()
        {
            PaletteMesh m = new PaletteMesh();
            m.Cylinder(1.5f, 1.3f, 6f, 10, new Vector3(0f, 3f, 0f), Vector3.zero, Band.Slate, 0.05f, 0.6f);
            m.Cylinder(1.7f, 1.7f, 0.8f, 10, new Vector3(0f, 6.3f, 0f), Vector3.zero, Band.Charcoal, 0.05f, 0.6f);
            for (int i = 0; i < 8; i++)
            {
                float a = i / 8f * Mathf.PI * 2f;
                m.Box(new Vector3(0.5f, 0.5f, 0.5f), 0.05f, new Vector3(Mathf.Cos(a) * 1.5f, 6.95f, Mathf.Sin(a) * 1.5f), new Vector3(0f, -a * Mathf.Rad2Deg, 0f), Band.Slate, 0.05f, 0.6f);
                m.Box(new Vector3(0.55f, 0.1f, 0.55f), 0.03f, new Vector3(Mathf.Cos(a) * 1.5f, 7.23f, Mathf.Sin(a) * 1.5f), new Vector3(0f, -a * Mathf.Rad2Deg, 0f), Band.Snow, 0f, 0.3f);
            }
            // banner with the wolf fang
            m.Box(new Vector3(1.1f, 2.2f, 0.06f), 0.02f, new Vector3(0f, 4.2f, -1.45f), Vector3.zero, Band.NightBlue, 0.05f, 0.45f);
            m.Add(Pyramid(0.6f, 0.06f, 0.9f), new Vector3(0f, 4.6f, -1.5f), new Vector3(180f, 0f, 0f), Vector3.one, Band.White, 0f, 0.25f);
            m.Box(new Vector3(0.7f, 1.2f, 0.12f), 0.04f, new Vector3(0f, 0.6f, -1.42f), Vector3.zero, Band.DarkWood, 0.1f, 0.6f);
            return m.Build("Frost_WolfTower");
        }

        public static Mesh MineEntrance()
        {
            PaletteMesh m = new PaletteMesh();
            m.Box(new Vector3(7f, 5f, 3f), 0.6f, new Vector3(0f, 2.3f, 1.2f), Vector3.zero, Band.Slate, 0.05f, 0.65f);
            m.Box(new Vector3(6.4f, 0.6f, 2.6f), 0.3f, new Vector3(0f, 4.9f, 1.2f), Vector3.zero, Band.Snow, 0f, 0.3f);
            m.Box(new Vector3(2.2f, 2.4f, 0.2f), 0.02f, new Vector3(0f, 1.2f, -0.3f), Vector3.zero, Band.Black, 0.2f, 0.4f);
            Plank(m, new Vector3(0.3f, 2.6f, 0.3f), new Vector3(-1.2f, 1.3f, -0.4f), Band.Log);
            Plank(m, new Vector3(0.3f, 2.6f, 0.3f), new Vector3(1.2f, 1.3f, -0.4f), Band.Log);
            Plank(m, new Vector3(2.8f, 0.3f, 0.34f), new Vector3(0f, 2.65f, -0.4f), Band.Log);
            for (int side = -1; side <= 1; side += 2)
                m.Box(new Vector3(0.08f, 0.08f, 3.5f), 0.01f, new Vector3(side * 0.45f, 0.05f, -2.0f), Vector3.zero, Band.Iron, 0.1f, 0.5f);
            for (int i = 0; i < 5; i++)
                Plank(m, new Vector3(1.2f, 0.06f, 0.18f), new Vector3(0f, 0.03f, -0.6f - i * 0.7f), Band.DarkWood);
            m.Box(new Vector3(1.0f, 0.6f, 1.2f), 0.06f, new Vector3(0f, 0.45f, -2.6f), Vector3.zero, Band.Iron, 0.1f, 0.5f);
            m.Sphere(0.35f, 3, 6, new Vector3(-0.15f, 0.8f, -2.6f), Vector3.one, Band.Crystal, 0f, 0.35f);
            m.Sphere(0.3f, 3, 6, new Vector3(0.2f, 0.8f, -2.5f), Vector3.one, Band.DeepIce, 0f, 0.35f);
            return m.Build("Frost_Mine");
        }

        public static Mesh WolfCamp()
        {
            PaletteMesh m = new PaletteMesh();
            m.Add(Prism(2.6f, 2.4f, 1.8f), new Vector3(-0.8f, 0f, 0.6f), new Vector3(0f, 25f, 0f), Vector3.one, Band.WolfGrey, 0.05f, 0.55f);
            m.Add(Prism(2.7f, 2.0f, 1.62f), new Vector3(-0.8f, 0.24f, 0.6f), new Vector3(0f, 25f, 0f), Vector3.one, Band.Snow, 0f, 0.3f);
            // brazier
            m.Cylinder(0.4f, 0.25f, 0.5f, 8, new Vector3(1.2f, 0.25f, -0.6f), Vector3.zero, Band.Iron, 0.1f, 0.5f);
            m.Add(Pyramid(0.45f, 0.45f, 0.6f), new Vector3(1.2f, 0.5f, -0.6f), new Vector3(0f, 45f, 0f), Vector3.one, Band.Fire, 0f, 0.3f);
            for (int i = 0; i < 4; i++)
                m.Box(new Vector3(0.6f, 0.4f, 0.5f), 0.06f, new Vector3(-0.4f + i * 0.62f, 0.2f, -1.9f), new Vector3(0f, (i % 2) * 6f, 0f), Band.Frost, 0f, 0.4f);
            Barrel(m, new Vector3(-2.0f, 0f, -1.2f), Band.NightBlue);
            return m.Build("Frost_WolfCamp");
        }

        public static Mesh WolfFlag(float height = 4f)
        {
            PaletteMesh m = new PaletteMesh()
                .Cylinder(0.07f, 0.06f, height, 8, new Vector3(0f, height * 0.5f, 0f), Vector3.zero, Band.Iron, 0.1f, 0.5f)
                .Cylinder(0.25f, 0.3f, 0.2f, 8, new Vector3(0f, 0.1f, 0f), Vector3.zero, Band.Iron, 0.2f, 0.5f)
                .Cylinder(0.08f, 0f, 0.3f, 6, new Vector3(0f, height + 0.15f, 0f), Vector3.zero, Band.Bone, 0.1f, 0.4f);
            for (int i = 0; i < 3; i++)
                m.Box(new Vector3(0.52f, 1.0f, 0.05f), 0.01f, new Vector3(0.35f + i * 0.5f, height - 0.6f - i * 0.04f, (i % 2) * 0.07f), new Vector3(0f, (i % 2 == 0 ? 8f : -8f), 0f), Band.NightBlue, 0.05f, 0.45f);
            // white fang
            m.Add(Pyramid(0.34f, 0.04f, 0.5f), new Vector3(0.85f, height - 0.38f, -0.08f), new Vector3(180f, 0f, 0f), Vector3.one, Band.White, 0f, 0.2f);
            return m.Build("Frost_Flag_Wolf");
        }

        #endregion

        #region Houses

        public static Mesh Cabin(float width, float depth, float height, int roofBand, int seed)
        {
            System.Random r = new System.Random(seed);
            PaletteMesh m = new PaletteMesh();
            LogWalls(m, Vector3.zero, width, depth, height);
            SnowRoof(m, new Vector3(0f, height, 0f), width + 0.6f, depth + 0.6f, 1.5f, roofBand);
            Door(m, new Vector3(r.NextDouble() < 0.5 ? -width * 0.22f : width * 0.22f, 0.8f, -depth * 0.5f - 0.2f), 0.9f, 1.6f);
            Window(m, new Vector3(r.NextDouble() < 0.5 ? width * 0.25f : -width * 0.25f, 1.5f, -depth * 0.5f - 0.2f), 0.7f, 0.7f, Band.DarkWood);
            Chimney(m, new Vector3(width * 0.3f, height + 0.2f, depth * 0.15f), 1.8f, r.NextDouble() < 0.5);
            SnowMound(m, new Vector3(-width * 0.5f, 0f, -depth * 0.5f), 0.5f);
            return m.Build("Frost_Cabin_" + seed);
        }

        #endregion

        #region Frostpeak buildings (Town tab plots)

        public static Mesh Lodge()
        {
            PaletteMesh m = new PaletteMesh();
            LogWalls(m, Vector3.zero, 5.2f, 3.8f, 2.6f);
            SnowRoof(m, new Vector3(0f, 2.6f, 0f), 5.9f, 4.4f, 1.8f, Band.NightBlue);
            Door(m, new Vector3(0f, 0.85f, -2.1f), 1.1f, 1.7f);
            Window(m, new Vector3(-1.6f, 1.5f, -2.1f));
            Window(m, new Vector3(1.6f, 1.5f, -2.1f));
            // ranger banner over the door and a radio mast
            Plank(m, new Vector3(1.6f, 0.5f, 0.08f), new Vector3(0f, 2.25f, -2.15f), Band.RangerBlue);
            m.Cylinder(0.04f, 0.03f, 3.4f, 6, new Vector3(-2.2f, 3.6f, 1.2f), Vector3.zero, Band.Steel, 0.1f, 0.5f);
            Chimney(m, new Vector3(1.6f, 2.9f, 0.6f), 1.8f, true);
            Firewood(m, new Vector3(3.3f, 0f, -0.6f), 4);
            return m.Build("Frost_Lodge");
        }

        public static Mesh LogBarracks()
        {
            PaletteMesh m = new PaletteMesh();
            LogWalls(m, Vector3.zero, 6.0f, 3.2f, 2.3f);
            SnowRoof(m, new Vector3(0f, 2.3f, 0f), 6.6f, 3.8f, 1.4f, Band.Moss);
            Door(m, new Vector3(0f, 0.85f, -1.8f), 1.0f, 1.7f);
            for (int i = -1; i <= 1; i += 2)
                Window(m, new Vector3(i * 1.8f, 1.4f, -1.8f));
            Plank(m, new Vector3(1.4f, 0.45f, 0.08f), new Vector3(0f, 2.05f, -1.85f), Band.RangerBlue);
            Firewood(m, new Vector3(-3.6f, 0f, 0.4f), 3);
            SnowMound(m, new Vector3(2.6f, 0f, -2.1f), 0.6f);
            return m.Build("Frost_LogBarracks");
        }

        public static Mesh Forge()
        {
            PaletteMesh m = new PaletteMesh();
            m.Box(new Vector3(4.4f, 2.6f, 3.4f), 0.2f, new Vector3(0f, 1.3f, 0f), Vector3.zero, Band.Slate, 0.05f, 0.6f);
            for (int row = 0; row < 3; row++)
                m.Box(new Vector3(4.42f, 0.06f, 3.42f), 0.01f, new Vector3(0f, 0.65f + row * 0.65f, 0f), Vector3.zero, Band.Charcoal, 0.2f, 0.6f);
            SnowRoof(m, new Vector3(0f, 2.6f, 0f), 4.9f, 3.9f, 1.2f, Band.Iron);
            Chimney(m, new Vector3(-1.4f, 2.8f, 0.5f), 2.2f, true);
            // glowing furnace mouth, anvil and a rack of rifles
            m.Box(new Vector3(1.4f, 1.0f, 0.16f), 0.05f, new Vector3(-0.9f, 0.6f, -1.72f), Vector3.zero, Band.Ember, 0f, 0.25f);
            m.Box(new Vector3(1.0f, 0.5f, 0.12f), 0.04f, new Vector3(-0.9f, 0.6f, -1.78f), Vector3.zero, Band.Lantern, 0f, 0.15f);
            m.Box(new Vector3(0.8f, 0.35f, 0.4f), 0.05f, new Vector3(1.3f, 0.75f, -2.3f), Vector3.zero, Band.Iron, 0.1f, 0.5f);
            m.Box(new Vector3(0.4f, 0.55f, 0.32f), 0.04f, new Vector3(1.3f, 0.3f, -2.3f), Vector3.zero, Band.Charcoal, 0.2f, 0.6f);
            Plank(m, new Vector3(1.6f, 0.1f, 0.1f), new Vector3(1.1f, 1.8f, -1.75f), Band.DarkWood);
            for (int i = 0; i < 4; i++)
                Plank(m, new Vector3(0.08f, 1.2f, 0.08f), new Vector3(0.5f + i * 0.4f, 1.3f, -1.8f), Band.Iron, new Vector3(0f, 0f, 6f));
            return m.Build("Frost_Forge");
        }

        public static Mesh Sawmill()
        {
            PaletteMesh m = new PaletteMesh();
            m.Box(new Vector3(4.8f, 0.2f, 3.4f), 0.05f, new Vector3(0f, 0.1f, 0f), Vector3.zero, Band.Wood, 0.1f, 0.6f);
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    Plank(m, new Vector3(0.22f, 2.8f, 0.22f), new Vector3(x * 2.2f, 1.4f, z * 1.5f), Band.Log);
            SnowRoof(m, new Vector3(0f, 2.8f, 0f), 5.2f, 3.9f, 1.1f, Band.Maroon);
            // saw bench with a big round blade and a log on it
            Plank(m, new Vector3(3.2f, 0.2f, 0.8f), new Vector3(0f, 0.8f, 0f), Band.DarkWood);
            m.Cylinder(0.75f, 0.75f, 0.06f, 16, new Vector3(0f, 1.05f, 0f), ALONG_X, Band.Steel, 0f, 0.4f);
            m.Cylinder(0.15f, 0.15f, 0.1f, 8, new Vector3(0f, 1.05f, 0f), ALONG_X, Band.Iron, 0.1f, 0.5f);
            m.Cylinder(0.3f, 0.3f, 2.6f, 9, new Vector3(-0.2f, 1.2f, 0.3f), ALONG_X, Band.Log, 0.1f, 0.55f);
            Firewood(m, new Vector3(-1.5f, 0f, -2.4f), 4);
            m.Cylinder(0.5f, 0.5f, 0.12f, 12, new Vector3(1.6f, 2.25f, -1.6f), ALONG_Z, Band.Gold, 0.05f, 0.4f);
            return m.Build("Frost_Sawmill");
        }

        public static Mesh Infirmary()
        {
            PaletteMesh m = new PaletteMesh();
            m.Box(new Vector3(4.4f, 2.4f, 3.4f), 0.1f, new Vector3(0f, 1.2f, 0f), Vector3.zero, Band.White, 0.05f, 0.45f);
            for (int i = 0; i < 5; i++)
                m.Box(new Vector3(4.42f, 0.04f, 3.42f), 0.01f, new Vector3(0f, 0.35f + i * 0.45f, 0f), Vector3.zero, Band.Frost, 0.2f, 0.5f);
            SnowRoof(m, new Vector3(0f, 2.4f, 0f), 4.9f, 3.9f, 1.3f, Band.ScorpionRed);
            Door(m, new Vector3(0f, 0.8f, -1.75f), 1.0f, 1.6f, Band.Steel);
            // red cross sign and a lantern
            m.Box(new Vector3(0.9f, 0.9f, 0.08f), 0.03f, new Vector3(1.3f, 1.6f, -1.75f), Vector3.zero, Band.White, 0f, 0.3f);
            m.Box(new Vector3(0.7f, 0.22f, 0.1f), 0.01f, new Vector3(1.3f, 1.6f, -1.78f), Vector3.zero, Band.ScorpionRed, 0.1f, 0.4f);
            m.Box(new Vector3(0.22f, 0.7f, 0.1f), 0.01f, new Vector3(1.3f, 1.6f, -1.78f), Vector3.zero, Band.ScorpionRed, 0.1f, 0.4f);
            m.Box(new Vector3(0.26f, 0.32f, 0.26f), 0.04f, new Vector3(-1.2f, 1.9f, -1.85f), Vector3.zero, Band.Lantern, 0f, 0.25f);
            Plank(m, new Vector3(0.7f, 0.1f, 1.6f), new Vector3(-2.9f, 0.45f, -0.6f), Band.Canvas);
            return m.Build("Frost_Infirmary");
        }

        public static Mesh Tavern()
        {
            PaletteMesh m = new PaletteMesh();
            LogWalls(m, Vector3.zero, 5.0f, 3.8f, 4.0f);
            SnowRoof(m, new Vector3(0f, 4.0f, 0f), 5.6f, 4.4f, 1.6f, Band.Maroon);
            Door(m, new Vector3(0f, 0.85f, -2.1f), 1.2f, 1.7f);
            for (int i = -1; i <= 1; i += 2)
            {
                Window(m, new Vector3(i * 1.7f, 1.3f, -2.1f), 0.8f, 0.8f);
                Window(m, new Vector3(i * 1.3f, 3.0f, -2.1f), 0.8f, 0.8f);
            }
            // hanging sign with a golden mug, porch roof
            m.Box(new Vector3(5.4f, 0.12f, 1.2f), 0.04f, new Vector3(0f, 2.2f, -2.7f), new Vector3(8f, 0f, 0f), Band.DarkWood, 0.1f, 0.55f);
            m.Box(new Vector3(5.5f, 0.1f, 1.1f), 0.04f, new Vector3(0f, 2.3f, -2.7f), new Vector3(8f, 0f, 0f), Band.Snow, 0f, 0.3f);
            for (int i = -1; i <= 1; i += 2)
                Plank(m, new Vector3(0.16f, 2.2f, 0.16f), new Vector3(i * 2.5f, 1.1f, -3.2f), Band.Log);
            Plank(m, new Vector3(1.3f, 0.8f, 0.1f), new Vector3(0f, 3.5f, -2.2f), Band.DarkWood);
            m.Cylinder(0.22f, 0.22f, 0.4f, 10, new Vector3(0f, 3.5f, -2.3f), Vector3.zero, Band.Gold, 0.05f, 0.4f);
            m.Box(new Vector3(0.1f, 0.25f, 0.1f), 0.02f, new Vector3(0.28f, 3.5f, -2.3f), Vector3.zero, Band.Gold, 0.05f, 0.4f);
            Barrel(m, new Vector3(2.9f, 0f, -0.6f), Band.Wood);
            return m.Build("Frost_Tavern");
        }

        public static Mesh Lookout()
        {
            PaletteMesh m = new PaletteMesh();
            float h = 6f;
            m.Box(new Vector3(2.4f, h, 2.4f), 0.2f, new Vector3(0f, h * 0.5f, 0f), Vector3.zero, Band.Slate, 0.05f, 0.6f);
            for (int i = 0; i < 5; i++)
                m.Box(new Vector3(2.42f, 0.06f, 2.42f), 0.01f, new Vector3(0f, 0.9f + i * 1.1f, 0f), Vector3.zero, Band.Charcoal, 0.2f, 0.6f);
            m.Box(new Vector3(3.0f, 0.25f, 3.0f), 0.06f, new Vector3(0f, h, 0f), Vector3.zero, Band.Log, 0.1f, 0.55f);
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    Plank(m, new Vector3(0.16f, 1.7f, 0.16f), new Vector3(x * 1.35f, h + 0.95f, z * 1.35f), Band.Log);
            // beacon lantern and snowy pyramid roof
            m.Box(new Vector3(0.7f, 0.8f, 0.7f), 0.08f, new Vector3(0f, h + 0.55f, 0f), Vector3.zero, Band.Lantern, 0f, 0.2f);
            m.Add(Pyramid(3.6f, 3.6f, 1.4f), new Vector3(0f, h + 1.8f, 0f), Vector3.zero, Vector3.one, Band.Log, 0.05f, 0.55f);
            m.Add(Pyramid(3.3f, 3.3f, 1.3f), new Vector3(0f, h + 1.92f, 0f), Vector3.zero, Vector3.one, Band.Snow, 0f, 0.3f);
            m.Box(new Vector3(0.8f, 1.4f, 0.12f), 0.04f, new Vector3(0f, 0.7f, -1.24f), Vector3.zero, Band.DarkWood, 0.1f, 0.6f);
            Window(m, new Vector3(0f, 3.8f, -1.24f), 0.5f, 0.8f, Band.Log);
            return m.Build("Frost_Lookout");
        }

        public static Mesh SnowSite()
        {
            PaletteMesh m = new PaletteMesh();
            m.Box(new Vector3(4.2f, 0.12f, 3.4f), 0.03f, new Vector3(0f, 0.06f, 0f), Vector3.zero, Band.Frost, 0.2f, 0.6f);
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    Plank(m, new Vector3(0.18f, 2.4f, 0.18f), new Vector3(x * 1.9f, 1.2f, z * 1.5f), Band.Log);
            Plank(m, new Vector3(4.0f, 0.14f, 0.14f), new Vector3(0f, 2.3f, -1.5f), Band.Log);
            Plank(m, new Vector3(4.0f, 0.14f, 0.14f), new Vector3(0f, 2.3f, 1.5f), Band.Log);
            Plank(m, new Vector3(0.14f, 0.14f, 3.2f), new Vector3(-1.9f, 2.3f, 0f), Band.Log);
            Plank(m, new Vector3(0.14f, 0.14f, 3.2f), new Vector3(1.9f, 2.3f, 0f), Band.Log);
            Firewood(m, new Vector3(0.6f, 0f, -2.4f), 4);
            Plank(m, new Vector3(0.1f, 1.2f, 0.1f), new Vector3(-1.6f, 0.6f, -2.2f), Band.DarkWood);
            Plank(m, new Vector3(0.9f, 0.6f, 0.08f), new Vector3(-1.6f, 1.25f, -2.2f), Band.Cream);
            Plank(m, new Vector3(0.9f, 0.14f, 0.1f), new Vector3(-1.6f, 1.25f, -2.22f), Band.RangerBlue);
            SnowMound(m, new Vector3(1.6f, 0f, 1.0f), 0.5f);
            return m.Build("Frost_SnowSite");
        }

        #endregion
    }
}
