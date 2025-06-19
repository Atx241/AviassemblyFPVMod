using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace AviassemblyMod
{
    public static class OBJLoader
    {
        public struct OBJResult
        {
            public Mesh mesh;
            public string code;
            public static OBJResult invalid = new OBJResult { code = "Invalid OBJ file contents!", mesh = null };
        }
        public static OBJResult Load(string path)
        {
            IEnumerable<string> lines = new string[0];
            try
            {  
                lines = File.ReadLines(path);
            } catch {
                return new OBJResult { code = "Failed to load file!", mesh = null };
            }
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            var mesh = new Mesh();
            foreach (var l in lines) {
                var raw = SplitOnFirstWhitespace(l);
                var type = raw.Item1;
                var body = raw.Item2;
                if (body == null)
                {
                    continue;
                }
                var vec = ParseVectorOrNull(body);
                switch (type)
                {
                    case "v":
                        if (vec != null)
                        {
                            vertices.Add(vec.Value);
                        } else
                        {
                            return OBJResult.invalid;
                        }
                        break;
                    case "f":
                        if (vec != null)
                        {
                            triangles.Add((int)vec.Value.x - 1);
                            triangles.Add((int)vec.Value.y - 1);
                            triangles.Add((int)vec.Value.z - 1);
                        } else
                        {
                            return OBJResult.invalid;
                        }
                        break;
                    default:
                        break;
                }
            }
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            return new OBJResult { mesh = mesh, code = "Successfully loaded OBJ file" };
        }
        private static (string,string) SplitOnFirstWhitespace(string input)
        {
            var sb = new StringBuilder();
            foreach (var c in input)
            {
                if (c == ' ')
                {
                    break;
                }
                sb.Append(c);
            }
            var a = sb.ToString();
            string b = null;
            if (sb.Length < input.Length - 1)
            {
                b = input.Substring(sb.Length + 1);
            }
            return (a, b);
        }
        private static Vector3? ParseVectorOrNull(string input)
        {
            if (input == null)
            {
                return null;
            }
            var data = input.Split(' ');
            if (data.Length != 3)
            {
                return null;
            }
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].Contains('/'))
                {
                    data[i] = data[i].Substring(0, data[i].IndexOf('/'));
                }
            }
            if (float.TryParse(data[0], out float x) && float.TryParse(data[1], out float y) && float.TryParse(data[2], out float z))
            {
                return new Vector3(x, y, z);
            }
            return null;
        }
    }
}
