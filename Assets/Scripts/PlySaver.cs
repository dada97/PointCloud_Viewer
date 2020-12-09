using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlySaver : MonoBehaviour
{      

    public void SaveOriginalPointCloud(string filepath, UIManager.PointcloudData data,Collider box)
    {
        int vertexCount = data.vertexCount;
        List<Vector3> vertices = data.vertices;
        List<Color32> colors = data.colors;
        List<Vector3> normals = data.normals;

        List<Vector3> final_v = new List<Vector3>();
        List<Color32> final_c = new List<Color32>();
        List<Vector3> final_n = new List<Vector3>();
        int finalCount=0;

        GameObject obj = data.obj;
        Matrix4x4 localToWorld = obj.transform.localToWorldMatrix;
        Quaternion rotate = obj.transform.rotation;

        for (int i = 0; i < vertexCount; ++i)
        {
            Vector3 point = localToWorld.MultiplyPoint3x4(vertices[i]);
            if (box.bounds.Contains(point))
            {
                final_v.Add(vertices[i]);
                final_c.Add(colors[i]);
                final_n.Add(normals[i]);
                finalCount++;
            }
        }

      
        using (StreamWriter sw = File.AppendText(filepath))
        {
            //Write Header
            sw.WriteLine("ply");
            sw.WriteLine("format ascii 1.0");
            string vertex = "element vertex " + finalCount;
            sw.WriteLine(vertex);
            sw.WriteLine("property float x");
            sw.WriteLine("property float y");
            sw.WriteLine("property float z");
            sw.WriteLine("property uchar red");
            sw.WriteLine("property uchar green");
            sw.WriteLine("property uchar blue");
            sw.WriteLine("property uchar alpha");
            sw.WriteLine("property float nx");
            sw.WriteLine("property float ny");
            sw.WriteLine("property float nz");
            sw.WriteLine("end_header");

            for (int i = 0; i < finalCount; ++i)
            {
                string line = final_v[i].x.ToString() + " " + final_v[i].y.ToString() + " " + final_v[i].z.ToString()
                    + " " + final_c[i].r.ToString() + " " + final_c[i].g.ToString() + " " + final_c[i].b.ToString() + " " + final_c[i].a.ToString()
                    + " " + final_n[i].x.ToString() + " " + final_n[i].y.ToString() + " " + final_n[i].z.ToString();
                sw.WriteLine(line);
            }
        }
    }

    public void SaveTransformPointCloud(string filepath, UIManager.PointcloudData data,Collider box)
    {
    
        int vertexCount = data.vertexCount;
        List<Vector3> vertices = data.vertices;
        List<Color32> colors = data.colors;
        List<Vector3> normals = data.normals;
        GameObject obj = data.obj;

        List<Vector3> final_v = new List<Vector3>();
        List<Color32> final_c = new List<Color32>();
        List<Vector3> final_n = new List<Vector3>();
        int finalCount = 0;

        Matrix4x4 localToWorld = obj.transform.localToWorldMatrix;
        Quaternion rotate = obj.transform.rotation;

        for (int i = 0; i < vertexCount; ++i)
        {
            vertices[i] = localToWorld.MultiplyPoint3x4(vertices[i]);
            normals[i] = rotate * normals[i];
            if (box.bounds.Contains(vertices[i]))
            {
                final_v.Add(vertices[i]);
                final_c.Add(colors[i]);
                final_n.Add(normals[i]);
                finalCount++;
            }
        }

        using (StreamWriter sw = File.AppendText(filepath))
        {
            //Write Header
            sw.WriteLine("ply");
            sw.WriteLine("format ascii 1.0");
            string vertex = "element vertex " + finalCount;
            sw.WriteLine(vertex);
            sw.WriteLine("property float x");
            sw.WriteLine("property float y");
            sw.WriteLine("property float z");
            sw.WriteLine("property uchar red");
            sw.WriteLine("property uchar green");
            sw.WriteLine("property uchar blue");
            sw.WriteLine("property uchar alpha");
            sw.WriteLine("property float nx");
            sw.WriteLine("property float ny");
            sw.WriteLine("property float nz");
            sw.WriteLine("end_header");

            for (int i = 0; i < finalCount; ++i)
            {
                string line = final_v[i].x.ToString() + " " + final_v[i].y.ToString() + " " + final_v[i].z.ToString()
                    + " " + final_c[i].r.ToString() + " " + final_c[i].g.ToString() + " " + final_c[i].b.ToString() + " " + final_c[i].a.ToString()
                    + " " + final_n[i].x.ToString() + " " + final_n[i].y.ToString() + " " + final_n[i].z.ToString();
                sw.WriteLine(line);
            }
        }
    }

    public void SaveCameraTransform(string filepath,List<UIManager.PointcloudData> pcList)
    {
        using (StreamWriter sw = File.AppendText(filepath))
        {
            foreach (UIManager.PointcloudData pc in pcList)
            {
                Vector3 pos = pc.obj.transform.position;
                Quaternion rot = pc.obj.transform.rotation;
                string pcname = pc.timeStamp+'.'+pc.cam_id;

                string line = pcname + " " + pos.x.ToString() + " " + pos.y.ToString() + " " + pos.z.ToString() + " " + rot.x.ToString() + " " + rot.y.ToString() + " " + rot.z.ToString() + " " + rot.w.ToString() ;
                sw.WriteLine(line);
            }
        }
    }

    public void WriteShell(string filepath, List<UIManager.PointcloudData> pcList)
    {
        string ShellPath = Path.Combine(filepath);
        using (StreamWriter writer = new StreamWriter(ShellPath))
        {
            writer.Write("cd /home\n");
            writer.Write(ShellStr.FirstHalf);
            for (int i = 0; i < pcList.Count; i++)
                writer.Write(" " + pcList[i].timeStamp+'_'+pcList[i].cam_id);
            writer.Write(ShellStr.SecondHalf);
            writer.Close();
        }
    }

    public void SaveUVpointcloud(string filepath, UIManager.PointcloudData data,Collider box)
    {      
        if (data.hasCamParam)
        {
            int vertexCount = data.vertexCount;
            List<Vector3> vertices = data.vertices;
            GameObject obj = data.obj;
            List<Vector2> uvPC = new List<Vector2>();
            double fx = (double)data.f;
            double cx = (double)(1 - data.cx);
            double cy = (double)data.cy;

            double posz = (double)vertices[vertexCount - 1].z;
            double posy = (double)vertices[vertexCount - 1].y;

            double fy = (1 - cy) * posz / posy;

            List<Vector3> final_v = new List<Vector3>();
            int finalCount = 0;
            Matrix4x4 localToWorld = obj.transform.localToWorldMatrix;

            List<float> ulist = new List<float>();
            List<float> vlist = new List<float>();
            for (int i = 0; i < vertexCount; ++i)
            {
                Vector3 point = localToWorld.MultiplyPoint3x4(vertices[i]);
                if (box.bounds.Contains(point))
                {
                    double u = 1 - ((double)vertices[i].x * fx / (double)vertices[i].z + cx);
                    double v = 1 -((double)vertices[i].y * fy / vertices[i].z + cy);
                    if (u < 0)
                    {
                        u = 0;
                    }
                    else if (u > 1)
                    {
                        u = 1;
                    }
                    if(v < 0)
                    {
                        v = 0;
                    }
                    else if (v > 1)
                    {
                        v = 1;
                    }
                    Vector2 uv = new Vector2((float)u, (float)v);
                    final_v.Add(vertices[i]);
                    uvPC.Add(uv);
                    finalCount++;
                }              
            }      

            using (StreamWriter sw = File.AppendText(filepath))
            {
                //Write Header
                sw.WriteLine("ply");
                sw.WriteLine("format ascii 1.0");
                string vertex = "element vertex " + finalCount;
                sw.WriteLine(vertex);
                sw.WriteLine("property float x");
                sw.WriteLine("property float y");
                sw.WriteLine("property float z");
                sw.WriteLine("property uchar red");
                sw.WriteLine("property uchar green");
                sw.WriteLine("property uchar blue");
                sw.WriteLine("property uchar alpha");
                sw.WriteLine("end_header");

                for (int i = 0; i < finalCount; ++i)
                {
                    string line = final_v[i].x.ToString() + " " + final_v[i].y.ToString() + " " + final_v[i].z.ToString() + " "
                    + uvPC[i].x + " " + uvPC[i].y + " 0 0";
                    sw.WriteLine(line);
                }
            }
        }        
    }
}
