using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.Dustwater
{
    // Low-poly western town pieces in the toy style of the template (chunky bevels, flat palette colours).
    // Units: metres, y up, the front of every building faces -z (towards the town camera / the road).
    // A soldier is ~1.5m tall, so a one-storey house is ~3.5m.
    public static class DustwaterBuildings
    {
        private static readonly Vector3 ALONG_Z = new Vector3(90f, 0f, 0f);
        private static readonly Vector3 ALONG_X = new Vector3(0f, 0f, 90f);

        #region Shapes

        // triangular prism: ridge along x, apex at the top (tents, gable roofs)
        public static List<Vector3[]> Prism(float width, float depth, float height)
        {
            float w = width * 0.5f, d = depth * 0.5f;
            Vector3 a = new Vector3(-w, 0f, -d), b = new Vector3(-w, 0f, d), c = new Vector3(-w, height, 0f);
            Vector3 e = new Vector3(w, 0f, -d), f = new Vector3(w, 0f, d), g = new Vector3(w, height, 0f);

            return new List<Vector3[]>()
            {
                new[] { a, b, c },
                new[] { e, g, f },
                new[] { a, c, g, e },
                new[] { b, f, g, c },
                new[] { a, e, f, b },
            };
        }

        // four-sided pyramid (tower roofs, tent tops)
        public static List<Vector3[]> Pyramid(float width, float depth, float height)
        {
            float w = width * 0.5f, d = depth * 0.5f;
            Vector3 top = new Vector3(0f, height, 0f);
            Vector3 a = new Vector3(-w, 0f, -d), b = new Vector3(w, 0f, -d), c = new Vector3(w, 0f, d), e = new Vector3(-w, 0f, d);
            return new List<Vector3[]>() { new[] { a, b, top }, new[] { b, c, top }, new[] { c, e, top }, new[] { e, a, top }, new[] { a, e, c, b } };
        }

        internal static PaletteMesh Plank(PaletteMesh m, Vector3 size, Vector3 position, int band = Band.Wood, Vector3? euler = null)
        {
            return m.Box(size, Mathf.Min(0.04f, Mathf.Min(size.x, Mathf.Min(size.y, size.z)) * 0.3f), position, euler ?? Vector3.zero, band, 0.1f, 0.55f);
        }

        internal static void Window(PaletteMesh m, Vector3 position, float width = 0.7f, float height = 0.8f, int frame = Band.DarkWood)
        {
            m.Box(new Vector3(width + 0.16f, height + 0.16f, 0.08f), 0.03f, position, Vector3.zero, frame, 0.2f, 0.5f);
            m.Box(new Vector3(width, height, 0.1f), 0.02f, position + new Vector3(0f, 0f, -0.02f), Vector3.zero, Band.Glass, 0f, 0.4f);
            m.Box(new Vector3(0.06f, height, 0.12f), 0.01f, position + new Vector3(0f, 0f, -0.03f), Vector3.zero, frame, 0.2f, 0.5f);
        }

        internal static void Door(PaletteMesh m, Vector3 position, float width = 0.9f, float height = 1.6f, int band = Band.DarkWood)
        {
            m.Box(new Vector3(width + 0.18f, height + 0.1f, 0.08f), 0.03f, position + new Vector3(0f, 0.05f, 0f), Vector3.zero, Band.Wood, 0.2f, 0.5f);
            m.Box(new Vector3(width, height, 0.12f), 0.03f, position + new Vector3(0f, 0f, -0.02f), Vector3.zero, band, 0.1f, 0.6f);
        }

        internal static void Crate(PaletteMesh m, Vector3 position, float size, float yaw = 0f)
        {
            m.Box(Vector3.one * size, size * 0.06f, position + Vector3.up * size * 0.5f, new Vector3(0f, yaw, 0f), Band.Wood, 0.05f, 0.5f);
            m.Box(new Vector3(size * 1.02f, size * 0.12f, size * 1.02f), 0.01f, position + Vector3.up * size * 0.5f, new Vector3(0f, yaw, 0f), Band.DarkWood, 0.1f, 0.5f);
        }

        internal static void Barrel(PaletteMesh m, Vector3 position, int band = Band.Rust)
        {
            m.Cylinder(0.32f, 0.32f, 0.9f, 10, position + Vector3.up * 0.45f, Vector3.zero, band, 0.1f, 0.55f);
            m.Cylinder(0.335f, 0.335f, 0.08f, 10, position + Vector3.up * 0.25f, Vector3.zero, Band.Iron, 0.2f, 0.5f);
            m.Cylinder(0.335f, 0.335f, 0.08f, 10, position + Vector3.up * 0.65f, Vector3.zero, Band.Iron, 0.2f, 0.5f);
        }

        internal static void Sandbags(PaletteMesh m, Vector3 position, int count, float yaw = 0f)
        {
            Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
            for (int i = 0; i < count; i++)
            {
                Vector3 offset = rotation * new Vector3((i - (count - 1) * 0.5f) * 0.5f, 0.12f, 0f);
                m.Box(new Vector3(0.48f, 0.24f, 0.32f), 0.08f, position + offset, new Vector3(0f, yaw + (i % 3 - 1) * 4f, 0f), i % 2 == 0 ? Band.Khaki : Band.Sand, 0.1f, 0.5f);
            }
        }

        #endregion

        #region Flags and small props

        public static Mesh FlagPole(int flagBand, float height = 4f, bool scorpion = false)
        {
            PaletteMesh m = new PaletteMesh()
                .Cylinder(0.07f, 0.06f, height, 8, new Vector3(0f, height * 0.5f, 0f), Vector3.zero, Band.Steel, 0.1f, 0.5f)
                .Sphere(0.12f, 3, 8, new Vector3(0f, height + 0.05f, 0f), Vector3.one, Band.Gold, 0.1f, 0.4f)
                .Cylinder(0.25f, 0.3f, 0.2f, 8, new Vector3(0f, 0.1f, 0f), Vector3.zero, Band.Iron, 0.2f, 0.5f);

            // waving flag made of three slightly bent panels
            for (int i = 0; i < 3; i++)
            {
                float x = 0.35f + i * 0.5f;
                m.Box(new Vector3(0.52f, 1.0f, 0.05f), 0.01f, new Vector3(x, height - 0.6f - i * 0.04f, (i % 2) * 0.07f), new Vector3(0f, (i % 2 == 0 ? 8f : -8f), 0f), flagBand, 0.05f, 0.45f);
            }

            if (scorpion)
            {
                m.Sphere(0.16f, 3, 8, new Vector3(0.85f, height - 0.65f, -0.07f), new Vector3(1f, 1.2f, 0.4f), Band.Black, 0.2f, 0.5f);
                m.Box(new Vector3(0.08f, 0.32f, 0.04f), 0.01f, new Vector3(0.95f, height - 0.38f, -0.07f), new Vector3(0f, 0f, -30f), Band.Black, 0.2f, 0.5f);
            }
            else
            {
                m.Box(new Vector3(0.36f, 0.08f, 0.04f), 0.01f, new Vector3(0.85f, height - 0.62f, -0.07f), Vector3.zero, Band.White, 0f, 0.2f);
                m.Box(new Vector3(0.08f, 0.36f, 0.04f), 0.01f, new Vector3(0.85f, height - 0.62f, -0.07f), Vector3.zero, Band.White, 0f, 0.2f);
            }

            return m.Build(scorpion ? "Town_Flag_Scorpion" : "Town_Flag_Ranger");
        }

        public static Mesh Fence(float length)
        {
            PaletteMesh m = new PaletteMesh();
            int posts = Mathf.Max(2, Mathf.RoundToInt(length / 1.2f) + 1);
            for (int i = 0; i < posts; i++)
            {
                float x = -length * 0.5f + length * i / (posts - 1);
                Plank(m, new Vector3(0.14f, 1.0f, 0.14f), new Vector3(x, 0.5f, 0f), Band.DarkWood);
            }
            Plank(m, new Vector3(length, 0.12f, 0.06f), new Vector3(0f, 0.75f, 0f));
            Plank(m, new Vector3(length, 0.12f, 0.06f), new Vector3(0f, 0.4f, 0f));
            return m.Build("Town_Fence_" + length);
        }

        public static Mesh Cactus(float height)
        {
            PaletteMesh m = new PaletteMesh()
                .Cylinder(0.26f, 0.22f, height, 8, new Vector3(0f, height * 0.5f, 0f), Vector3.zero, Band.Cactus, 0.1f, 0.6f)
                .Sphere(0.22f, 3, 8, new Vector3(0f, height, 0f), Vector3.one, Band.Cactus, 0.1f, 0.5f)
                .Cylinder(0.16f, 0.14f, height * 0.35f, 8, new Vector3(0.42f, height * 0.62f, 0f), Vector3.zero, Band.Cactus, 0.1f, 0.6f)
                .Cylinder(0.14f, 0.14f, 0.36f, 8, new Vector3(0.24f, height * 0.45f, 0f), ALONG_X, Band.Cactus, 0.1f, 0.6f)
                .Sphere(0.15f, 3, 8, new Vector3(0.42f, height * 0.8f, 0f), Vector3.one, Band.Cactus, 0.1f, 0.5f);
            return m.Build("Town_Cactus_" + height);
        }

        public static Mesh Rock(float size, int seed)
        {
            System.Random r = new System.Random(seed);
            PaletteMesh m = new PaletteMesh();
            for (int i = 0; i < 3; i++)
            {
                float s = size * (0.5f + (float)r.NextDouble() * 0.5f);
                m.Box(new Vector3(s, s * 0.7f, s * 0.9f), s * 0.25f, new Vector3((float)(r.NextDouble() - 0.5) * size, s * 0.3f, (float)(r.NextDouble() - 0.5) * size * 0.6f), new Vector3(0f, (float)r.NextDouble() * 90f, (float)(r.NextDouble() - 0.5) * 20f), Band.Terracotta, 0.1f, 0.6f);
            }
            return m.Build("Town_Rock_" + seed);
        }

        public static Mesh Mesa(float width, float height, int seed)
        {
            System.Random r = new System.Random(seed);
            PaletteMesh m = new PaletteMesh();
            m.Box(new Vector3(width, height, width * 0.5f), height * 0.08f, new Vector3(0f, height * 0.5f, 0f), Vector3.zero, Band.Terracotta, 0.05f, 0.65f);
            m.Box(new Vector3(width * 0.7f, height * 0.25f, width * 0.4f), height * 0.05f, new Vector3(width * 0.1f, height * 1.1f, 0f), Vector3.zero, Band.Terracotta, 0f, 0.5f);
            m.Box(new Vector3(width * 1.15f, height * 0.18f, width * 0.6f), height * 0.05f, new Vector3(0f, height * 0.09f, 0f), Vector3.zero, Band.Rust, 0.1f, 0.6f);
            for (int i = 0; i < 2; i++)
                m.Box(new Vector3(width * 0.25f, height * 0.6f, width * 0.3f), height * 0.06f, new Vector3((i * 2 - 1) * width * 0.48f, height * 0.3f, (float)(r.NextDouble() - 0.5) * 2f), Vector3.zero, Band.Terracotta, 0.1f, 0.6f);
            return m.Build("Town_Mesa_" + seed);
        }

        public static Mesh Lamp()
        {
            return new PaletteMesh()
                .Cylinder(0.08f, 0.08f, 2.6f, 8, new Vector3(0f, 1.3f, 0f), Vector3.zero, Band.Iron, 0.1f, 0.5f)
                .Box(new Vector3(0.6f, 0.08f, 0.08f), 0.01f, new Vector3(0.25f, 2.55f, 0f), Vector3.zero, Band.Iron, 0.1f, 0.5f)
                .Box(new Vector3(0.28f, 0.36f, 0.28f), 0.04f, new Vector3(0.5f, 2.35f, 0f), Vector3.zero, Band.Gold, 0f, 0.3f)
                .Build("Town_Lamp");
        }

        public static Mesh Wagon()
        {
            PaletteMesh m = new PaletteMesh();
            Plank(m, new Vector3(1.6f, 0.5f, 2.6f), new Vector3(0f, 0.85f, 0f));
            Plank(m, new Vector3(1.7f, 0.12f, 2.7f), new Vector3(0f, 0.62f, 0f), Band.DarkWood);
            for (int side = -1; side <= 1; side += 2)
                for (int end = -1; end <= 1; end += 2)
                {
                    m.Cylinder(0.45f, 0.45f, 0.1f, 12, new Vector3(side * 0.9f, 0.45f, end * 0.85f), ALONG_X, Band.DarkWood, 0.1f, 0.5f);
                    m.Cylinder(0.12f, 0.12f, 0.14f, 8, new Vector3(side * 0.9f, 0.45f, end * 0.85f), ALONG_X, Band.Iron, 0.2f, 0.5f);
                }
            // canvas cover
            m.Add(Shapes.Cylinder(0.9f, 0.9f, 2.2f, 10), new Vector3(0f, 1.3f, 0f), ALONG_Z, new Vector3(1f, 1f, 0.8f), Band.Canvas, 0.05f, 0.4f);
            return m.Build("Town_Wagon");
        }

        public static Mesh Truck(int band)
        {
            PaletteMesh m = new PaletteMesh();
            m.Box(new Vector3(2.1f, 1.0f, 4.6f), 0.12f, new Vector3(0f, 1.0f, 0f), Vector3.zero, band, 0.05f, 0.55f);
            m.Box(new Vector3(1.9f, 1.0f, 1.5f), 0.15f, new Vector3(0f, 1.9f, 1.35f), Vector3.zero, band, 0.05f, 0.5f);
            m.Box(new Vector3(1.7f, 0.55f, 0.06f), 0.02f, new Vector3(0f, 2.05f, 2.11f), Vector3.zero, Band.Glass, 0f, 0.3f);
            m.Box(new Vector3(2.2f, 0.25f, 0.3f), 0.06f, new Vector3(0f, 0.65f, 2.35f), Vector3.zero, Band.Iron, 0.1f, 0.5f);
            for (int side = -1; side <= 1; side += 2)
                for (int end = -1; end <= 1; end += 2)
                    m.Cylinder(0.48f, 0.48f, 0.36f, 12, new Vector3(side * 1.0f, 0.48f, end * 1.45f), ALONG_X, Band.Black, 0.2f, 0.5f);
            // open bed rails
            for (int side = -1; side <= 1; side += 2)
                Plank(m, new Vector3(0.1f, 0.4f, 2.8f), new Vector3(side * 0.98f, 1.7f, -0.75f), Band.DarkWood);
            Plank(m, new Vector3(2.0f, 0.4f, 0.1f), new Vector3(0f, 1.7f, -2.2f), Band.DarkWood);
            return m.Build("Town_Truck_" + band);
        }

        #endregion

        #region Buildings

        public static Mesh AdobeHouse(float width, float depth, float height, int seed)
        {
            System.Random r = new System.Random(seed);
            PaletteMesh m = new PaletteMesh();
            m.Box(new Vector3(width, height, depth), 0.18f, new Vector3(0f, height * 0.5f, 0f), Vector3.zero, Band.Adobe, 0.05f, 0.5f);
            m.Box(new Vector3(width + 0.2f, 0.3f, depth + 0.2f), 0.1f, new Vector3(0f, height + 0.05f, 0f), Vector3.zero, Band.Adobe, 0f, 0.35f);

            // roof beams (vigas)
            int beams = Mathf.RoundToInt(width / 0.8f);
            for (int i = 0; i < beams; i++)
                m.Cylinder(0.09f, 0.09f, 0.5f, 6, new Vector3(-width * 0.5f + (i + 0.5f) * width / beams, height - 0.35f, -depth * 0.5f - 0.15f), ALONG_Z, Band.DarkWood, 0.2f, 0.6f);

            Door(m, new Vector3(width * 0.2f * (r.Next(2) * 2 - 1), 0.8f, -depth * 0.5f - 0.03f));
            Window(m, new Vector3(-width * 0.3f * (r.Next(2) * 2 - 1), height * 0.55f, -depth * 0.5f - 0.03f));
            if (r.NextDouble() < 0.6) Crate(m, new Vector3(width * 0.5f - 0.2f, 0f, -depth * 0.5f - 0.7f), 0.6f, 12f);

            return m.Build("Town_Adobe_" + seed);
        }

        // western false-front building: tall flat facade, porch roof on posts, coloured sign
        public static Mesh FalseFront(float width, float depth, float height, int bodyBand, int signBand, int seed)
        {
            System.Random r = new System.Random(seed);
            PaletteMesh m = new PaletteMesh();
            m.Box(new Vector3(width, height, depth), 0.08f, new Vector3(0f, height * 0.5f, 0f), Vector3.zero, bodyBand, 0.05f, 0.5f);
            m.Add(Prism(width + 0.2f, depth + 0.2f, 1.1f), new Vector3(0f, height, 0.1f), Vector3.zero, Vector3.one, Band.Slate, 0.1f, 0.55f);

            // facade sticking above the roof
            float facadeHeight = height + 1.4f;
            m.Box(new Vector3(width + 0.3f, facadeHeight, 0.2f), 0.05f, new Vector3(0f, facadeHeight * 0.5f, -depth * 0.5f - 0.1f), Vector3.zero, bodyBand, 0f, 0.4f);
            m.Box(new Vector3(width + 0.5f, 0.18f, 0.32f), 0.04f, new Vector3(0f, facadeHeight, -depth * 0.5f - 0.1f), Vector3.zero, Band.DarkWood, 0.1f, 0.5f);
            for (int i = 0; i < 4; i++)
                m.Box(new Vector3(width + 0.32f, 0.04f, 0.22f), 0.01f, new Vector3(0f, 0.6f + i * facadeHeight * 0.22f, -depth * 0.5f - 0.11f), Vector3.zero, Band.DarkWood, 0.3f, 0.6f);

            // sign
            m.Box(new Vector3(width * 0.7f, 0.7f, 0.12f), 0.05f, new Vector3(0f, facadeHeight - 0.65f, -depth * 0.5f - 0.24f), Vector3.zero, signBand, 0.05f, 0.45f);
            m.Box(new Vector3(width * 0.74f, 0.8f, 0.08f), 0.03f, new Vector3(0f, facadeHeight - 0.65f, -depth * 0.5f - 0.2f), Vector3.zero, Band.Cream, 0.1f, 0.4f);

            // porch
            float porchDepth = 1.3f;
            m.Box(new Vector3(width + 0.4f, 0.2f, porchDepth), 0.04f, new Vector3(0f, 0.1f, -depth * 0.5f - porchDepth * 0.5f), Vector3.zero, Band.Wood, 0.1f, 0.6f);
            m.Box(new Vector3(width + 0.4f, 0.1f, porchDepth + 0.1f), 0.03f, new Vector3(0f, height * 0.62f, -depth * 0.5f - porchDepth * 0.5f), new Vector3(-6f, 0f, 0f), Band.Wood, 0.1f, 0.55f);
            for (int i = 0; i < 3; i++)
                Plank(m, new Vector3(0.14f, height * 0.62f, 0.14f), new Vector3(-width * 0.5f + i * width * 0.5f, height * 0.31f, -depth * 0.5f - porchDepth + 0.1f), Band.DarkWood);

            Door(m, new Vector3(0f, 0.95f, -depth * 0.5f - 0.22f), 1.0f, 1.6f);
            Window(m, new Vector3(-width * 0.3f, 1.3f, -depth * 0.5f - 0.22f), 0.7f, 0.8f);
            Window(m, new Vector3(width * 0.3f, 1.3f, -depth * 0.5f - 0.22f), 0.7f, 0.8f);
            if (r.NextDouble() < 0.5) Barrel(m, new Vector3(width * 0.5f + 0.1f, 0f, -depth * 0.5f - 0.6f), Band.Wood);

            return m.Build("Town_FalseFront_" + seed);
        }

        public static Mesh CommandPost()
        {
            PaletteMesh m = new PaletteMesh();
            m.Add(Prism(4.6f, 3.6f, 2.6f), new Vector3(0f, 0f, 0f), new Vector3(0f, 90f, 0f), Vector3.one, Band.Olive, 0.05f, 0.6f);
            m.Box(new Vector3(1.1f, 1.6f, 0.1f), 0.02f, new Vector3(0f, 0.8f, -2.32f), Vector3.zero, Band.DarkWood, 0.1f, 0.6f);
            m.Box(new Vector3(0.1f, 2.8f, 0.1f), 0.02f, new Vector3(0f, 1.4f, -2.35f), Vector3.zero, Band.Wood, 0.1f, 0.5f);
            // radio antenna and table
            m.Cylinder(0.04f, 0.03f, 3.2f, 6, new Vector3(1.4f, 2.6f, 1.2f), Vector3.zero, Band.Steel, 0.1f, 0.5f);
            Plank(m, new Vector3(1.2f, 0.1f, 0.7f), new Vector3(-2.3f, 0.75f, -1.6f));
            m.Box(new Vector3(0.4f, 0.3f, 0.3f), 0.04f, new Vector3(-2.3f, 0.95f, -1.6f), Vector3.zero, Band.Olive, 0.1f, 0.5f);
            Sandbags(m, new Vector3(1.9f, 0f, -2.4f), 3);
            Sandbags(m, new Vector3(1.9f, 0.24f, -2.4f), 2);
            Crate(m, new Vector3(-2.6f, 0f, 0.6f), 0.7f, 20f);
            return m.Build("Town_CommandPost");
        }

        public static Mesh Barracks()
        {
            PaletteMesh m = new PaletteMesh();
            m.Box(new Vector3(5.6f, 2.4f, 3.2f), 0.1f, new Vector3(0f, 1.2f, 0f), Vector3.zero, Band.Wood, 0.05f, 0.5f);
            for (int i = 0; i < 5; i++)
                m.Box(new Vector3(5.62f, 0.05f, 3.22f), 0.01f, new Vector3(0f, 0.35f + i * 0.45f, 0f), Vector3.zero, Band.DarkWood, 0.3f, 0.6f);
            m.Add(Prism(6.0f, 3.8f, 1.5f), new Vector3(0f, 2.4f, 0f), Vector3.zero, Vector3.one, Band.Slate, 0.05f, 0.55f);
            Door(m, new Vector3(0f, 0.85f, -1.62f), 1.0f, 1.7f);
            for (int i = -1; i <= 1; i += 2)
            {
                Window(m, new Vector3(i * 1.6f, 1.4f, -1.62f));
                Window(m, new Vector3(i * 2.8f + i * 0.04f, 1.4f, 0f));
            }
            Plank(m, new Vector3(1.4f, 0.5f, 0.08f), new Vector3(0f, 2.15f, -1.64f), Band.RangerBlue);
            Sandbags(m, new Vector3(-2.2f, 0f, -2.3f), 3);
            return m.Build("Town_Barracks");
        }

        public static Mesh Armory()
        {
            PaletteMesh m = new PaletteMesh();
            m.Box(new Vector3(4.4f, 2.8f, 3.4f), 0.15f, new Vector3(0f, 1.4f, 0f), Vector3.zero, Band.Brick, 0.05f, 0.5f);
            m.Box(new Vector3(4.7f, 0.35f, 3.7f), 0.1f, new Vector3(0f, 2.9f, 0f), Vector3.zero, Band.Iron, 0.1f, 0.5f);
            for (int row = 0; row < 3; row++)
                m.Box(new Vector3(4.42f, 0.06f, 3.42f), 0.01f, new Vector3(0f, 0.7f + row * 0.7f, 0f), Vector3.zero, Band.Maroon, 0.2f, 0.5f);
            // steel door with rivets
            m.Box(new Vector3(1.5f, 1.9f, 0.14f), 0.04f, new Vector3(0f, 0.95f, -1.74f), Vector3.zero, Band.Steel, 0.05f, 0.5f);
            m.Box(new Vector3(0.08f, 1.9f, 0.16f), 0.01f, new Vector3(0f, 0.95f, -1.76f), Vector3.zero, Band.Iron, 0.1f, 0.5f);
            // crossed-rifles sign
            m.Box(new Vector3(1.6f, 0.06f, 0.06f), 0.01f, new Vector3(0f, 2.3f, -1.76f), new Vector3(0f, 0f, 25f), Band.DarkWood, 0.1f, 0.5f);
            m.Box(new Vector3(1.6f, 0.06f, 0.06f), 0.01f, new Vector3(0f, 2.3f, -1.76f), new Vector3(0f, 0f, -25f), Band.DarkWood, 0.1f, 0.5f);
            Crate(m, new Vector3(-2.0f, 0f, -2.3f), 0.7f, 10f);
            Crate(m, new Vector3(-1.4f, 0f, -2.5f), 0.55f, -15f);
            Crate(m, new Vector3(-1.8f, 0.7f, -2.3f), 0.5f, 30f);
            Barrel(m, new Vector3(2.1f, 0f, -2.3f), Band.Olive);
            return m.Build("Town_Armory");
        }

        public static Mesh Workshop()
        {
            PaletteMesh m = new PaletteMesh();
            m.Box(new Vector3(4.6f, 0.2f, 3.4f), 0.05f, new Vector3(0f, 0.1f, 0f), Vector3.zero, Band.Wood, 0.1f, 0.6f);
            m.Box(new Vector3(4.6f, 2.6f, 0.2f), 0.05f, new Vector3(0f, 1.3f, 1.6f), Vector3.zero, Band.Wood, 0.05f, 0.5f);
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    Plank(m, new Vector3(0.18f, 2.8f, 0.18f), new Vector3(x * 2.15f, 1.4f, z * 1.5f), Band.DarkWood);
            m.Box(new Vector3(5.0f, 0.15f, 3.9f), 0.04f, new Vector3(0f, 2.85f, 0f), new Vector3(-8f, 0f, 0f), Band.Rust, 0.05f, 0.5f);
            // workbench, anvil, gear sign
            Plank(m, new Vector3(2.0f, 0.12f, 0.8f), new Vector3(-0.8f, 0.9f, 1.0f));
            Plank(m, new Vector3(0.12f, 0.8f, 0.7f), new Vector3(-1.7f, 0.5f, 1.0f), Band.DarkWood);
            Plank(m, new Vector3(0.12f, 0.8f, 0.7f), new Vector3(0.1f, 0.5f, 1.0f), Band.DarkWood);
            m.Box(new Vector3(0.7f, 0.35f, 0.35f), 0.05f, new Vector3(1.2f, 0.75f, -0.2f), Vector3.zero, Band.Iron, 0.1f, 0.5f);
            m.Box(new Vector3(0.35f, 0.5f, 0.3f), 0.04f, new Vector3(1.2f, 0.45f, -0.2f), Vector3.zero, Band.Iron, 0.2f, 0.6f);
            m.Cylinder(0.55f, 0.55f, 0.12f, 8, new Vector3(0f, 2.2f, 1.45f), ALONG_Z, Band.Gold, 0.05f, 0.4f);
            m.Cylinder(0.2f, 0.2f, 0.16f, 8, new Vector3(0f, 2.2f, 1.42f), ALONG_Z, Band.Iron, 0.1f, 0.4f);
            Barrel(m, new Vector3(-2.0f, 0.2f, -1.2f), Band.Steel);
            return m.Build("Town_Workshop");
        }

        public static Mesh FieldHospital()
        {
            PaletteMesh m = new PaletteMesh();
            m.Add(Prism(4.4f, 3.8f, 2.4f), Vector3.zero, Vector3.zero, Vector3.one, Band.White, 0.05f, 0.45f);
            m.Box(new Vector3(1.2f, 1.5f, 0.06f), 0.02f, new Vector3(0f, 0.75f, -1.92f), Vector3.zero, Band.Bone, 0.1f, 0.5f);
            // red crosses on both roof sides
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 c = new Vector3(0f, 1.35f, side * 0.92f);
                Vector3 euler = new Vector3(side * -51f, 0f, 0f);
                m.Box(new Vector3(0.9f, 0.28f, 0.04f), 0.01f, c, euler, Band.ScorpionRed, 0.1f, 0.4f);
                m.Box(new Vector3(0.28f, 0.9f, 0.04f), 0.01f, c, euler, Band.ScorpionRed, 0.1f, 0.4f);
            }
            // cots outside
            for (int i = 0; i < 2; i++)
            {
                Plank(m, new Vector3(0.7f, 0.1f, 1.6f), new Vector3(-2.8f + i * 0.9f, 0.45f, -0.6f), Band.Canvas);
                Plank(m, new Vector3(0.7f, 0.4f, 0.06f), new Vector3(-2.8f + i * 0.9f, 0.2f, -1.35f), Band.Steel);
            }
            return m.Build("Town_FieldHospital");
        }

        public static Mesh Saloon()
        {
            PaletteMesh m = new PaletteMesh();
            float width = 5.2f, depth = 4f, height = 4.6f;
            m.Box(new Vector3(width, height, depth), 0.08f, new Vector3(0f, height * 0.5f, 0f), Vector3.zero, Band.Wood, 0.05f, 0.5f);
            for (int i = 0; i < 8; i++)
                m.Box(new Vector3(width + 0.02f, 0.04f, depth + 0.02f), 0.01f, new Vector3(0f, 0.4f + i * 0.55f, 0f), Vector3.zero, Band.DarkWood, 0.3f, 0.6f);
            m.Add(Prism(width + 0.2f, depth + 0.2f, 1.2f), new Vector3(0f, height, 0f), Vector3.zero, Vector3.one, Band.Brick, 0.05f, 0.55f);

            // tall facade with rounded top piece
            float facade = height + 1.8f;
            m.Box(new Vector3(width + 0.4f, facade, 0.24f), 0.06f, new Vector3(0f, facade * 0.5f, -depth * 0.5f - 0.12f), Vector3.zero, Band.Wood, 0f, 0.4f);
            m.Cylinder(1.3f, 1.3f, 0.24f, 14, new Vector3(0f, facade - 0.2f, -depth * 0.5f - 0.12f), ALONG_Z, Band.Wood, 0f, 0.4f);
            m.Box(new Vector3(3.6f, 0.9f, 0.14f), 0.06f, new Vector3(0f, facade - 0.75f, -depth * 0.5f - 0.3f), Vector3.zero, Band.ScorpionRed, 0.05f, 0.45f);
            m.Box(new Vector3(3.8f, 1.05f, 0.1f), 0.03f, new Vector3(0f, facade - 0.75f, -depth * 0.5f - 0.26f), Vector3.zero, Band.Gold, 0.05f, 0.35f);

            // balcony with rail on posts
            m.Box(new Vector3(width + 0.4f, 0.16f, 1.4f), 0.04f, new Vector3(0f, 2.3f, -depth * 0.5f - 0.8f), Vector3.zero, Band.Wood, 0.1f, 0.55f);
            for (int i = 0; i < 4; i++)
                Plank(m, new Vector3(0.16f, 2.3f, 0.16f), new Vector3(-width * 0.5f + i * width / 3f, 1.15f, -depth * 0.5f - 1.4f), Band.DarkWood);
            Plank(m, new Vector3(width + 0.4f, 0.08f, 0.08f), new Vector3(0f, 2.95f, -depth * 0.5f - 1.45f), Band.DarkWood);
            for (int i = 0; i < 12; i++)
                Plank(m, new Vector3(0.05f, 0.55f, 0.05f), new Vector3(-width * 0.5f + 0.2f + i * (width - 0.4f) / 11f, 2.65f, -depth * 0.5f - 1.45f), Band.DarkWood);

            // swinging doors and windows
            m.Box(new Vector3(1.4f, 2.0f, 0.06f), 0.02f, new Vector3(0f, 1.0f, -depth * 0.5f - 0.25f), Vector3.zero, Band.Black, 0.2f, 0.5f);
            m.Box(new Vector3(0.62f, 0.8f, 0.08f), 0.02f, new Vector3(-0.34f, 0.95f, -depth * 0.5f - 0.3f), Vector3.zero, Band.Wood, 0.1f, 0.5f);
            m.Box(new Vector3(0.62f, 0.8f, 0.08f), 0.02f, new Vector3(0.34f, 0.95f, -depth * 0.5f - 0.3f), Vector3.zero, Band.Wood, 0.1f, 0.5f);
            Window(m, new Vector3(-1.8f, 1.2f, -depth * 0.5f - 0.27f), 0.8f, 0.9f);
            Window(m, new Vector3(1.8f, 1.2f, -depth * 0.5f - 0.27f), 0.8f, 0.9f);
            Window(m, new Vector3(-1.4f, 3.5f, -depth * 0.5f - 0.27f), 0.8f, 0.9f);
            Window(m, new Vector3(1.4f, 3.5f, -depth * 0.5f - 0.27f), 0.8f, 0.9f);
            Barrel(m, new Vector3(width * 0.5f + 0.3f, 0f, -depth * 0.5f - 0.4f), Band.Wood);
            return m.Build("Town_Saloon");
        }

        public static Mesh Watchtower()
        {
            PaletteMesh m = new PaletteMesh();
            float h = 5.5f;
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    m.Box(new Vector3(0.22f, h + 0.3f, 0.22f), 0.04f, new Vector3(x * 1.0f, (h + 0.3f) * 0.5f, z * 1.0f), new Vector3(-z * 3f, 0f, x * 3f), Band.DarkWood, 0.1f, 0.6f);
            for (int i = 0; i < 2; i++)
            {
                float y = 1.6f + i * 1.9f;
                Plank(m, new Vector3(2.2f, 0.1f, 0.1f), new Vector3(0f, y, -1.0f), Band.Wood, new Vector3(0f, 0f, 35f));
                Plank(m, new Vector3(2.2f, 0.1f, 0.1f), new Vector3(0f, y, 1.0f), Band.Wood, new Vector3(0f, 0f, -35f));
                Plank(m, new Vector3(0.1f, 0.1f, 2.2f), new Vector3(-1.0f, y, 0f), Band.Wood, new Vector3(35f, 0f, 0f));
                Plank(m, new Vector3(0.1f, 0.1f, 2.2f), new Vector3(1.0f, y, 0f), Band.Wood, new Vector3(-35f, 0f, 0f));
            }
            m.Box(new Vector3(2.8f, 0.2f, 2.8f), 0.05f, new Vector3(0f, h, 0f), Vector3.zero, Band.Wood, 0.1f, 0.55f);
            for (int side = 0; side < 4; side++)
            {
                Quaternion rotation = Quaternion.Euler(0f, side * 90f, 0f);
                m.Box(new Vector3(2.8f, 0.7f, 0.1f), 0.03f, rotation * new Vector3(0f, h + 0.45f, -1.35f), new Vector3(0f, side * 90f, 0f), Band.Wood, 0.1f, 0.5f);
            }
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    Plank(m, new Vector3(0.14f, 1.6f, 0.14f), new Vector3(x * 1.3f, h + 0.9f, z * 1.3f), Band.DarkWood);
            m.Add(Pyramid(3.4f, 3.4f, 1.3f), new Vector3(0f, h + 1.7f, 0f), Vector3.zero, Vector3.one, Band.Rust, 0.05f, 0.5f);
            // ladder
            for (int i = 0; i < 8; i++)
                Plank(m, new Vector3(0.6f, 0.06f, 0.06f), new Vector3(0f, 0.4f + i * 0.65f, -1.15f), Band.Wood);
            Plank(m, new Vector3(0.07f, h, 0.07f), new Vector3(-0.32f, h * 0.5f, -1.15f), Band.DarkWood);
            Plank(m, new Vector3(0.07f, h, 0.07f), new Vector3(0.32f, h * 0.5f, -1.15f), Band.DarkWood);
            return m.Build("Town_Watchtower");
        }

        public static Mesh WaterTower()
        {
            PaletteMesh m = new PaletteMesh();
            float h = 5f;
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    m.Box(new Vector3(0.22f, h, 0.22f), 0.04f, new Vector3(x * 1.1f, h * 0.5f, z * 1.1f), Vector3.zero, Band.DarkWood, 0.1f, 0.6f);
            Plank(m, new Vector3(2.6f, 0.1f, 0.1f), new Vector3(0f, 2.5f, -1.1f), Band.Wood, new Vector3(0f, 0f, 40f));
            Plank(m, new Vector3(2.6f, 0.1f, 0.1f), new Vector3(0f, 2.5f, 1.1f), Band.Wood, new Vector3(0f, 0f, -40f));
            m.Cylinder(1.7f, 1.7f, 2.4f, 16, new Vector3(0f, h + 1.2f, 0f), Vector3.zero, Band.Wood, 0.05f, 0.5f);
            m.Cylinder(1.75f, 1.75f, 0.12f, 16, new Vector3(0f, h + 0.4f, 0f), Vector3.zero, Band.Iron, 0.1f, 0.5f);
            m.Cylinder(1.75f, 1.75f, 0.12f, 16, new Vector3(0f, h + 2.0f, 0f), Vector3.zero, Band.Iron, 0.1f, 0.5f);
            m.Cylinder(1.85f, 0.2f, 1.2f, 16, new Vector3(0f, h + 3.0f, 0f), Vector3.zero, Band.Slate, 0.05f, 0.5f);
            m.Cylinder(0.12f, 0.12f, 1.4f, 8, new Vector3(1.2f, h - 0.6f, -1.1f), Vector3.zero, Band.Iron, 0.1f, 0.5f);
            return m.Build("Town_WaterTower");
        }

        public static Mesh GasStation()
        {
            PaletteMesh m = new PaletteMesh();
            m.Box(new Vector3(3.2f, 2.6f, 2.8f), 0.1f, new Vector3(-2.6f, 1.3f, 1.0f), Vector3.zero, Band.Cream, 0.05f, 0.5f);
            m.Box(new Vector3(3.5f, 0.25f, 3.1f), 0.06f, new Vector3(-2.6f, 2.7f, 1.0f), Vector3.zero, Band.ScorpionRed, 0.1f, 0.5f);
            Window(m, new Vector3(-2.6f, 1.4f, -0.43f), 1.4f, 0.9f, Band.Steel);
            m.Box(new Vector3(5.0f, 0.25f, 3.0f), 0.08f, new Vector3(1.2f, 3.0f, -0.2f), Vector3.zero, Band.White, 0.05f, 0.4f);
            m.Box(new Vector3(5.04f, 0.3f, 3.04f), 0.02f, new Vector3(1.2f, 2.8f, -0.2f), Vector3.zero, Band.ScorpionRed, 0.1f, 0.4f);
            for (int x = 0; x < 2; x++)
                m.Box(new Vector3(0.22f, 2.8f, 0.22f), 0.04f, new Vector3(-0.6f + x * 3.6f, 1.4f, -0.2f), Vector3.zero, Band.Steel, 0.1f, 0.5f);
            for (int i = 0; i < 2; i++)
            {
                m.Box(new Vector3(0.6f, 1.2f, 0.5f), 0.08f, new Vector3(0.6f + i * 1.3f, 0.6f, -0.2f), Vector3.zero, Band.ScorpionRed, 0.05f, 0.5f);
                m.Box(new Vector3(0.4f, 0.3f, 0.04f), 0.02f, new Vector3(0.6f + i * 1.3f, 0.9f, -0.47f), Vector3.zero, Band.White, 0f, 0.3f);
                m.Cylinder(0.2f, 0.2f, 0.06f, 10, new Vector3(0.6f + i * 1.3f, 1.3f, -0.2f), Vector3.zero, Band.Gold, 0f, 0.4f);
            }
            m.Cylinder(0.1f, 0.1f, 4.2f, 8, new Vector3(3.8f, 2.1f, -1.6f), Vector3.zero, Band.Steel, 0.1f, 0.5f);
            m.Cylinder(0.75f, 0.75f, 0.16f, 14, new Vector3(3.8f, 4.3f, -1.6f), ALONG_Z, Band.Gold, 0.05f, 0.4f);
            m.Cylinder(0.6f, 0.6f, 0.2f, 14, new Vector3(3.8f, 4.3f, -1.6f), ALONG_Z, Band.ScorpionRed, 0.05f, 0.4f);
            return m.Build("Town_GasStation");
        }

        public static Mesh ConstructionSite()
        {
            PaletteMesh m = new PaletteMesh();
            m.Box(new Vector3(4.2f, 0.12f, 3.4f), 0.03f, new Vector3(0f, 0.06f, 0f), Vector3.zero, Band.Khaki, 0.2f, 0.6f);
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    Plank(m, new Vector3(0.16f, 2.4f, 0.16f), new Vector3(x * 1.9f, 1.2f, z * 1.5f), Band.Wood);
            Plank(m, new Vector3(4.0f, 0.12f, 0.12f), new Vector3(0f, 2.3f, -1.5f));
            Plank(m, new Vector3(4.0f, 0.12f, 0.12f), new Vector3(0f, 2.3f, 1.5f));
            Plank(m, new Vector3(0.12f, 0.12f, 3.2f), new Vector3(-1.9f, 2.3f, 0f));
            Plank(m, new Vector3(0.12f, 0.12f, 3.2f), new Vector3(1.9f, 2.3f, 0f));
            Plank(m, new Vector3(4.2f, 0.1f, 0.1f), new Vector3(0f, 1.2f, -1.5f), Band.Wood, new Vector3(0f, 0f, 28f));
            // plank pile and sign with a blue ranger band
            for (int i = 0; i < 4; i++)
                Plank(m, new Vector3(2.0f, 0.12f, 0.35f), new Vector3(0.6f, 0.18f + i * 0.13f, -2.3f + (i % 2) * 0.05f), Band.Wood, new Vector3(0f, (i % 2) * 6f, 0f));
            Plank(m, new Vector3(0.1f, 1.2f, 0.1f), new Vector3(-1.6f, 0.6f, -2.2f), Band.DarkWood);
            Plank(m, new Vector3(0.9f, 0.6f, 0.08f), new Vector3(-1.6f, 1.25f, -2.2f), Band.Cream);
            Plank(m, new Vector3(0.9f, 0.14f, 0.1f), new Vector3(-1.6f, 1.25f, -2.22f), Band.RangerBlue);
            return m.Build("Town_ConstructionSite");
        }

        public static Mesh BanditCamp()
        {
            PaletteMesh m = new PaletteMesh();
            m.Add(Prism(2.6f, 2.4f, 1.8f), new Vector3(-0.8f, 0f, 0.6f), new Vector3(0f, 25f, 0f), Vector3.one, Band.Maroon, 0.05f, 0.55f);
            // campfire
            for (int i = 0; i < 6; i++)
                m.Box(new Vector3(0.2f, 0.18f, 0.24f), 0.05f, new Vector3(Mathf.Cos(i * 1.05f) * 0.45f + 1.2f, 0.09f, Mathf.Sin(i * 1.05f) * 0.45f - 0.6f), Vector3.zero, Band.Steel, 0.1f, 0.6f);
            m.Box(new Vector3(0.7f, 0.1f, 0.12f), 0.02f, new Vector3(1.2f, 0.16f, -0.6f), new Vector3(0f, 40f, 0f), Band.DarkWood, 0.1f, 0.5f);
            m.Box(new Vector3(0.7f, 0.1f, 0.12f), 0.02f, new Vector3(1.2f, 0.16f, -0.6f), new Vector3(0f, -40f, 0f), Band.DarkWood, 0.1f, 0.5f);
            m.Add(Pyramid(0.45f, 0.45f, 0.6f), new Vector3(1.2f, 0.15f, -0.6f), new Vector3(0f, 45f, 0f), Vector3.one, Band.Fire, 0f, 0.3f);
            Sandbags(m, new Vector3(0.4f, 0f, -1.9f), 5, 8f);
            Barrel(m, new Vector3(-2.0f, 0f, -1.2f), Band.ScorpionRed);
            return m.Build("Town_BanditCamp");
        }

        public static Mesh Palisade(float length)
        {
            PaletteMesh m = new PaletteMesh();
            int logs = Mathf.RoundToInt(length / 0.36f);
            for (int i = 0; i < logs; i++)
            {
                float x = -length * 0.5f + (i + 0.5f) * length / logs;
                float h = 2.6f + ((i * 37) % 5) * 0.12f;
                m.Cylinder(0.2f, 0.19f, h, 7, new Vector3(x, h * 0.5f, 0f), Vector3.zero, i % 3 == 0 ? Band.DarkWood : Band.Wood, 0.1f, 0.6f);
                m.Cylinder(0.19f, 0.0f, 0.35f, 7, new Vector3(x, h + 0.17f, 0f), Vector3.zero, Band.Wood, 0.1f, 0.5f);
            }
            Plank(m, new Vector3(length, 0.16f, 0.16f), new Vector3(0f, 1.9f, -0.22f), Band.DarkWood);
            Plank(m, new Vector3(length, 0.16f, 0.16f), new Vector3(0f, 0.7f, -0.22f), Band.DarkWood);
            return m.Build("Town_Palisade_" + length);
        }

        #endregion
    }
}
