using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SharpGL;
using System.Globalization;

namespace OpenGLDemo
{
    class Model
    {
        public List<Point> Vertices { get; private set; }
        public List<List<Vertex>> f;

        public Point Extension { get; private set; }

        public void Import(string fileName)
        {
            Load(fileName);
            CenterModel();
        }

        void Load(string fileName)
        {
            using (StreamReader reader = new StreamReader(fileName))
            {
                Vertices = new List<Point>();
                f = new List<List<Vertex>>();

                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] tokens = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length == 0)
                        continue;
                    switch (tokens[0])
                    {
                        case "v":
                            if (tokens.Length != 4)
                                throw new Exception("wrong row format at vertices: " + line);
                            Point point = new Point(float.Parse(tokens[1], CultureInfo.InvariantCulture),
                                float.Parse(tokens[2], CultureInfo.InvariantCulture),
                                float.Parse(tokens[3], CultureInfo.InvariantCulture));
                            Vertices.Add(point);
                            break;
                        case "f":
                           if (tokens.Length < 4)
                               throw new Exception("wrong row format at flats: " + line);
                            var newFlat = new List<Vertex>();
                            for (int i = 1; i < tokens.Length; i++)
                            {

                                var vertex = new Vertex();
                                string[] vertexTokens = tokens[i].Split('/');

                                if (vertexTokens.Length > 3)
                                    throw new Exception($"Wrong vertex format: {{tokens[i]}}");

                                if (vertexTokens.Length >= 1)
                                    vertex.PointIndex = int.Parse(vertexTokens[0]) - 1;

                                if (vertexTokens.Length >= 2)
                                    vertex.TextureIndex = vertexTokens[1] != "" ? int.Parse(vertexTokens[1]) - 1 : -1;

                                if (vertexTokens.Length == 3)
                                    vertex.NormalIndex = int.Parse(vertexTokens[2]) - 1;

                                newFlat.Add(vertex);
                            }
                            f.Add(newFlat);
                            break;
                        case "o":
                        case "#":
                        case "vt":
                        case "vn":
                        case "usemtl":
                        case "s":
                        case "l":
                        case "mtllib":
                        case "g":
                            break;
                        default:
                            throw new Exception("unknown row type " + line);
                    }
                }
            }
        }

        void CenterModel()
        {
            Point min = new Point(float.MaxValue, float.MaxValue, float.MaxValue);
            Point max = new Point(float.MinValue, float.MinValue, float.MinValue);
            Point center = new Point(0, 0, 0);

            foreach (var point in Vertices)
            {
                min.X = Math.Min(min.X, point.X);
                min.Y = Math.Min(min.Y, point.Y);
                min.Z = Math.Min(min.Z, point.Z);

                max.X = Math.Max(max.X, point.X);
                max.Y = Math.Max(max.Y, point.Y);
                max.Z = Math.Max(max.Z, point.Z);
            }

            center.X = (min.X + max.X) / 2;
            center.Y = (min.Y + max.Y) / 2;
            center.Z = (min.Z + max.Z) / 2;

            foreach (var point in Vertices)
            {
                point.X -= center.X;
                point.Y -= center.Y;
                point.Z -= center.Z;
            }

            Extension = new Point(max.X - center.X, max.Y - center.Y, max.Z - center.Z);
        }
    }
}
