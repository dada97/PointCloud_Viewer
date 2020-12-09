using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;
using System;

public class PlyLoader
{
    private Vector3[] _vertices;
    private Color[] _colors;
    private int[] _indices;
    private int _verticesCount;

    //x y z r g b a
    private int[] _needInfoIndex = { -1, -1, -1, -1, -1, -1, -1 };

    //true : float, false : uchar
    private bool _colorFormat = true;
    private bool _hasAlpha = false;

    public string fileName;

    enum DataProperty
    {
        Invalid,
        R8, G8, B8, A8,
        R16, G16, B16, A16,
        Nx,Ny,Nz,
        SingleX, SingleY, SingleZ,
        DoubleX, DoubleY, DoubleZ,
        Data8, Data16, Data32, Data64
    }
    enum PlyType
    {
        Binary,
        Ascii
    }

    static int GetPropertySize(DataProperty p)
    {
        switch (p)
        {
            case DataProperty.R8: return 1;
            case DataProperty.G8: return 1;
            case DataProperty.B8: return 1;
            case DataProperty.A8: return 1;
            case DataProperty.R16: return 2;
            case DataProperty.G16: return 2;
            case DataProperty.B16: return 2;
            case DataProperty.A16: return 2;
            case DataProperty.SingleX: return 4;
            case DataProperty.SingleY: return 4;
            case DataProperty.SingleZ: return 4;
            case DataProperty.Nx: return 4;
            case DataProperty.Ny: return 4;
            case DataProperty.Nz: return 4;
            case DataProperty.DoubleX: return 8;
            case DataProperty.DoubleY: return 8;
            case DataProperty.DoubleZ: return 8;
            case DataProperty.Data8: return 1;
            case DataProperty.Data16: return 2;
            case DataProperty.Data32: return 4;
            case DataProperty.Data64: return 8;
        }
        return 0;
    }

    class DataHeader
    {
        public List<DataProperty> properties = new List<DataProperty>();
        public int vertexCount = -1;
        public PlyType pcType;
    }

    public class DataBody
    {
        public List<Vector3> vertices;
        public List<Color32> colors;
        public List<Vector3> normal;

        public DataBody(int vertexCount)
        {
            vertices = new List<Vector3>(vertexCount);
            colors = new List<Color32>(vertexCount);
            normal = new List<Vector3>(vertexCount);
        }

        public void AddPoint(
            float x, float y, float z,
            byte r, byte g, byte b, byte a,
            float nx, float ny, float nz
        )
        {
            vertices.Add(new Vector3(x, y, z));
            colors.Add(new Color32(r, g, b, a));
            normal.Add(new Vector3(nx, ny, nz));
        }
    }

    public (Mesh, List<Vector3>, List<Color32>,List<Vector3>) ImportFile(string filePath)
    {
        if (!File.Exists(filePath))
            return (new Mesh(),null,null,null);
        var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var header = ReadDataHeader(new StreamReader(stream));
        var body = ReadDataBody(header,stream);

        //Set Mesh
        Mesh mesh = new Mesh();
        mesh.indexFormat = header.vertexCount > 65535 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.SetVertices(body.vertices);
        mesh.SetColors(body.colors);
        mesh.SetNormals(body.normal);
        mesh.SetIndices(
            Enumerable.Range(0, header.vertexCount).ToArray(),
            MeshTopology.Points, 0
        );

        mesh.UploadMeshData(true);
        return (mesh,body.vertices,body.colors,body.normal);
    }

    DataHeader ReadDataHeader(StreamReader reader)
    {
        var data = new DataHeader();
        var readCount = 0;

        // Magic number line ("ply")
        var line = reader.ReadLine();
        readCount += line.Length + 1;
        if (line != "ply")
            throw new ArgumentException("Magic number ('ply') mismatch.");

        // Data format: check if it's binary/little endian.
        line = reader.ReadLine();
        readCount += line.Length + 1;
        if(line == "format binary_little_endian 1.0")
        {
            data.pcType = PlyType.Binary;
        }
        else if(line == "format ascii 1.0")
        {
            data.pcType = PlyType.Ascii;
        }
        else
        {
            throw new ArgumentException(
               "Invalid data format ('" + line + "'). " +
               "Should be binary/little endian/Ascii.");
        }   
        // Read header contents.
        for (var skip = false; ;)
        {
            // Read a line and split it with white space.
            line = reader.ReadLine();
            readCount += line.Length + 1;
            if (line == "end_header") break;
            var col = line.Split();

            // Element declaration (unskippable)
            if (col[0] == "element")
            {
                if (col[1] == "vertex")
                {
                    data.vertexCount = Convert.ToInt32(col[2]);
                    skip = false;
                }
                else
                {
                    // Don't read elements other than vertices.
                    skip = true;
                }
            }

            if (skip) continue;

            // Property declaration line
            if (col[0] == "property")
            {
                var prop = DataProperty.Invalid;

                // Parse the property name entry.
                switch (col[2])
                {
                    case "red": prop = DataProperty.R8; break;
                    case "green": prop = DataProperty.G8; break;
                    case "blue": prop = DataProperty.B8; break;
                    case "alpha": prop = DataProperty.A8; break;
                    case "x": prop = DataProperty.SingleX; break;
                    case "y": prop = DataProperty.SingleY; break;
                    case "z": prop = DataProperty.SingleZ; break;
                    case "nx": prop = DataProperty.Nx; break;
                    case "ny": prop = DataProperty.Ny; break;
                    case "nz": prop = DataProperty.Nz; break;
                }

                // Check the property type.
                if (col[1] == "char" || col[1] == "uchar" ||
                    col[1] == "int8" || col[1] == "uint8")
                {
                    if (prop == DataProperty.Invalid)
                        prop = DataProperty.Data8;
                    else if (GetPropertySize(prop) != 1)
                        throw new ArgumentException("Invalid property type ('" + line + "').");
                }
                else if (col[1] == "short" || col[1] == "ushort" ||
                         col[1] == "int16" || col[1] == "uint16")
                {
                    switch (prop)
                    {
                        case DataProperty.Invalid: prop = DataProperty.Data16; break;
                        case DataProperty.R8: prop = DataProperty.R16; break;
                        case DataProperty.G8: prop = DataProperty.G16; break;
                        case DataProperty.B8: prop = DataProperty.B16; break;
                        case DataProperty.A8: prop = DataProperty.A16; break;
                    }
                    if (GetPropertySize(prop) != 2)
                        throw new ArgumentException("Invalid property type ('" + line + "').");
                }
                else if (col[1] == "int" || col[1] == "uint" || col[1] == "float" ||
                         col[1] == "int32" || col[1] == "uint32" || col[1] == "float32")
                {
                    if (prop == DataProperty.Invalid)
                        prop = DataProperty.Data32;
                    else if (GetPropertySize(prop) != 4)
                        throw new ArgumentException("Invalid property type ('" + line + "').");
                }
                else if (col[1] == "int64" || col[1] == "uint64" ||
                         col[1] == "double" || col[1] == "float64")
                {
                    switch (prop)
                    {
                        case DataProperty.Invalid: prop = DataProperty.Data64; break;
                        case DataProperty.SingleX: prop = DataProperty.DoubleX; break;
                        case DataProperty.SingleY: prop = DataProperty.DoubleY; break;
                        case DataProperty.SingleZ: prop = DataProperty.DoubleZ; break;
                    }
                    if (GetPropertySize(prop) != 8)
                        throw new ArgumentException("Invalid property type ('" + line + "').");
                }
                else
                {
                    throw new ArgumentException("Unsupported property type ('" + line + "').");
                }

                data.properties.Add(prop);
            }
        }

        // Rewind the stream back to the exact position of the reader.
        reader.BaseStream.Position = readCount;

        return data;
    }

    DataBody ReadDataBody(DataHeader header, FileStream stream)
    {
        var data = new DataBody(header.vertexCount);
        float x = 0, y = 0, z = 0;
        float nx = 0, ny = 0, nz = 0;
        Byte r = 255, g = 255, b = 255, a = 255;

        if (header.pcType == PlyType.Binary)
        {
            for (var i = 0; i < header.vertexCount; i++)
            {
                (x, y, z, r, g, b, a, nx, ny, nz) = ParseBinaryData(header, new BinaryReader(stream));
                data.AddPoint(x, y, z, r, g, b, a, nx, ny, nz);
            }
        }
        else if(header.pcType == PlyType.Ascii)
        {
            StreamReader reader = new StreamReader(stream);
            reader.ReadLine();
            reader.ReadLine();
            for (var i = 0; i < header.vertexCount; i++)
            {
                (x, y, z, r, g, b, a, nx, ny, nz) = ParseAsciiData(header, reader);
                data.AddPoint(x, y, z, r, g, b, a, nx, ny, nz);
            }
        }
        return data;
    }


    (float,float,float, Byte, Byte, Byte, Byte, float,float,float) ParseBinaryData(DataHeader header, BinaryReader reader)
    {
        float x = 0, y = 0, z = 0;
        float nx = 0, ny = 0, nz = 0;
        Byte r = 255, g = 255, b = 255, a = 255;

        foreach (var prop in header.properties)
        {
            switch (prop)
            {
                case DataProperty.R8: r = reader.ReadByte(); break;
                case DataProperty.G8: g = reader.ReadByte(); break;
                case DataProperty.B8: b = reader.ReadByte(); break;
                case DataProperty.A8: a = reader.ReadByte(); break;

                case DataProperty.R16: r = (byte)(reader.ReadUInt16() >> 8); break;
                case DataProperty.G16: g = (byte)(reader.ReadUInt16() >> 8); break;
                case DataProperty.B16: b = (byte)(reader.ReadUInt16() >> 8); break;
                case DataProperty.A16: a = (byte)(reader.ReadUInt16() >> 8); break;

                case DataProperty.SingleX: x = reader.ReadSingle(); break;
                case DataProperty.SingleY: y = reader.ReadSingle(); break;
                case DataProperty.SingleZ: z = reader.ReadSingle(); break;

                case DataProperty.Nx: nx = reader.ReadSingle(); break;
                case DataProperty.Ny: ny = reader.ReadSingle(); break;
                case DataProperty.Nz: nz = reader.ReadSingle(); break;

                case DataProperty.DoubleX: x = (float)reader.ReadDouble(); break;
                case DataProperty.DoubleY: y = (float)reader.ReadDouble(); break;
                case DataProperty.DoubleZ: z = (float)reader.ReadDouble(); break;

                case DataProperty.Data8: reader.ReadByte(); break;
                case DataProperty.Data16: reader.BaseStream.Position += 2; break;
                case DataProperty.Data32: reader.BaseStream.Position += 4; break;
                case DataProperty.Data64: reader.BaseStream.Position += 8; break;
            }
        }

        return(x,y,z,r,g,b,a,nx,ny,nz);
    }

    (float, float, float, Byte, Byte, Byte, Byte, float, float, float) ParseAsciiData(DataHeader header, StreamReader reader)
    {
        float x = 0, y = 0, z = 0;
        float nx = 0, ny = 0, nz = 0;
        Byte r = 255, g = 255, b = 255, a = 255;
        string line = reader.ReadLine();
        string[] tokens = line.Split(' ');
        x = float.Parse(tokens[0]);
        y = float.Parse(tokens[1]);
        z = float.Parse(tokens[2]);
        r = Byte.Parse(tokens[3]);
        g = Byte.Parse(tokens[4]);
        b = Byte.Parse(tokens[5]);
        a = Byte.Parse(tokens[6]);
        nx = float.Parse(tokens[7]);
        ny = float.Parse(tokens[8]);
        nz = float.Parse(tokens[9]);
        return (x, y, z, r, g, b, a, nx, ny, nz);
    }
}
