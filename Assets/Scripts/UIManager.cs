using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using Battlehub.RTCommon;

public class UIManager : MonoBehaviour
{

    public Shader vertexColorShader;
    public GameObject conePrefabs;
    public SelectPanel selectPanel;
    public GameObject pointcloud;
    public Collider boundingBox;

    private string _rootFolder;
    private string _rootFile;
    List<PointcloudData> pcList;
 
    public class PointcloudData
    {
        public int vertexCount;
        public List<Vector3> vertices;
        public List<Color32> colors;
        public List<Vector3> normals;
        public GameObject obj;
        public string pcname;
        public string cam_id;
        public string timeStamp;
        public bool hasCamParam;
        public float f;
        public float cx;
        public float cy;
       

        public PointcloudData(List<Vector3> v,List<Color32> c,List<Vector3> normal,
            GameObject pcobj,string objname,
            bool cam,float cam_f,float cam_cx,float cam_cy,
            string ts,string camid)
        {
            vertexCount = v.Count;
            vertices = v;
            colors = c;
            normals = normal;
            obj = pcobj;
            pcname = objname;
            hasCamParam = cam;
            f = cam_f;
            cx = cam_cx;
            cy = cam_cy;
            timeStamp = ts;
            cam_id = camid;
        }
    }

    enum BrowserType
    {
       OpenPC,
       SavePC,
       OpenCam,
    }

    private void Start()
    {
        pcList = new List<PointcloudData>();
    }

    private void Init()
    {
        selectPanel.resetToggle();
        for (int i = pcList.Count - 1; i >= 0; i--)
        {
            Destroy(pcList[i].obj);
            pcList.Remove(pcList[i]);
        }     
        pcList = new List<PointcloudData>();
    }

    public void OpenFileBrowser(int type)
    {
        FileBrowser.AddQuickLink("Desktop", "C:\\Users\\Desktop", null);
        StartCoroutine(ShowLoadDialogCorotine((BrowserType) type));
    }

    private void LoadPointCloud(string pcFolder)
    {
        string[] pcFilePath = Directory.GetFiles(pcFolder);
        pcList = new List<PointcloudData>();
        foreach (string path in pcFilePath)
        {
            //check extension is .ply file
            if (Path.GetExtension(path) == ".ply")
            {
                string pcname = Path.GetFileNameWithoutExtension(path);
                PlyLoader plyloader = new PlyLoader();
                Mesh pcMesh = null;
                List<Vector3> vertices = null;
                List<Color32> colors = null;
                List<Vector3> normals = null;
                (pcMesh, vertices, colors, normals) = plyloader.ImportFile(path);

                Material mat = new Material(vertexColorShader);
                GameObject pcObj = new GameObject(plyloader.fileName, typeof(MeshFilter), typeof(MeshRenderer));
                pcObj.name = pcname;            
                pcObj.GetComponent<MeshFilter>().mesh = pcMesh;
                pcObj.GetComponent<MeshRenderer>().material = mat;
                pcObj.AddComponent<ExposeToEditor>();
                pcObj.SetActive(true);

                GameObject cone = GameObject.Instantiate(conePrefabs);
                cone.AddComponent<BoxCollider>();
                cone.SetActive(true);
                cone.transform.parent = pcObj.transform;
                pcObj.transform.parent = pointcloud.transform;

                string camfilepath = pcFolder + "\\cam\\" + pcname + ".cam";
                float f, cx, cy;
                f = cx = cy = 0;
                bool has_cam = false;
                if (File.Exists(camfilepath))
                {
                    has_cam = true;
                    string[] lines = System.IO.File.ReadAllLines(camfilepath);
                    string[] tokens = lines[1].Split(' ');

                    f = float.Parse(tokens[0]);
                    cx = float.Parse(tokens[4]);
                    cy = float.Parse(tokens[5]);
                }

                DateTime dt = DateTime.Now;
                string ts = dt.ToString("yyyyMMdd");
                string[] tmp = pcname.Split('_');
                string camid = tmp[tmp.Length - 1];

                PointcloudData data = new PointcloudData(vertices, colors, normals, pcObj, pcname,has_cam,f,cx,cy,ts,camid);
                pcList.Add(data);
                selectPanel.createToggle(pcname);
            }          
        }      
    }

    private void SavePointCloud(string saveFolder)
    {

        PlySaver plySaver = new PlySaver();
        List<PointcloudData> camLsit = new List<PointcloudData>();

        string outputPC_path = "/Output";
        string output_SLAM_path = "/Output_SLAM";
        string output_UV_path = "/Output_UV_PointCloud";
        string ICP_path = "/ICP";


        var folder = Directory.CreateDirectory(saveFolder + outputPC_path);
        folder = Directory.CreateDirectory(saveFolder + output_SLAM_path);
        folder = Directory.CreateDirectory(saveFolder + output_UV_path);
        folder = Directory.CreateDirectory(saveFolder + ICP_path);

        
        foreach (PointcloudData pc in pcList)
        {
            string filename = pc.timeStamp + '_' + pc.cam_id;
            if (pc.obj.activeSelf)
            {
                string filepath = saveFolder + outputPC_path + '\\' + filename + ".ply";
                Debug.Log(filepath);
                plySaver.SaveOriginalPointCloud(filepath, pc, boundingBox);
                camLsit.Add(pc);
            }
        }

        foreach (PointcloudData pc in pcList)
        {
            if (pc.obj.activeSelf)
            {
                string filename = pc.timeStamp + '_' + pc.cam_id;
                string filepath = saveFolder + output_UV_path + '\\' + filename + ".ply";
                Debug.Log(filepath);
                plySaver.SaveUVpointcloud(filepath, pc, boundingBox);
            }
        }

        foreach (PointcloudData pc in pcList)
        {
            if (pc.obj.activeSelf)
            {
                string filename = pc.timeStamp + '_' + pc.cam_id;
                string filepath = saveFolder + ICP_path + '\\' + filename + ".ply";
                Debug.Log(filepath);               
                plySaver.SaveTransformPointCloud(filepath, pc,boundingBox);
            }           
        }     
        string camerapath = saveFolder + output_SLAM_path+ '\\' + "CameraTrajectory_withScale.txt";
       
        plySaver.SaveCameraTransform(camerapath, camLsit);
        plySaver.WriteShell(saveFolder+"\\Recon.sh",camLsit);
        Debug.Log("Save!");
    }

    private void LoadCamera(string filepath)
    {
        string[] lines = System.IO.File.ReadAllLines(filepath);
        foreach (string line in lines){
            string[] tokens = line.Split(' ');
            string cam_name = tokens[0];
            Debug.Log(tokens[0]);
            for (int i = 1; i < tokens.Length; i++){
              
                Debug.Log(i+": "+float.Parse(tokens[i]));
            }
            Debug.Log(tokens[0] + " "+tokens[1] + " "+tokens[2] + " "+tokens[3] + " " + tokens[4]+" "+tokens[5]+" "+tokens[6]+" "+tokens[7]);
            Vector3 pos = new Vector3(float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3]));
            Quaternion rot = new Quaternion(float.Parse(tokens[4]), float.Parse(tokens[5]), float.Parse(tokens[6]), float.Parse(tokens[7]));
             
            for(int i = 0; i < pcList.Count; i++)
            {
                if(pcList[i].pcname == cam_name)
                {
                    pcList[i].obj.transform.position = pos;
                    pcList[i].obj.transform.rotation = rot;
                    break;
                }
            }
        }
    }

    IEnumerator ShowLoadDialogCorotine(BrowserType type)
    {
        switch (type)
        {
            case BrowserType.OpenPC:
                yield return FileBrowser.WaitForLoadDialog(true, false, "Select Folder", "Select");
                break;
            case BrowserType.SavePC:
                yield return FileBrowser.WaitForLoadDialog(true, false, "Select Folder", "Save");
                break;
            case BrowserType.OpenCam:
                yield return FileBrowser.WaitForLoadDialog(false, false, "Select File", "Select");
                break;

        }

        if (FileBrowser.Result != null)
        {
            switch (type)
            {
                case BrowserType.OpenPC:
                    Init();
                    _rootFolder = FileBrowser.Result[0];
                    LoadPointCloud(_rootFolder);
                    break;

                case BrowserType.SavePC:
                    _rootFolder = FileBrowser.Result[0];
                    SavePointCloud(_rootFolder);
                    break;

                case BrowserType.OpenCam:
                    _rootFile = FileBrowser.Result[0];
                    LoadCamera(_rootFile);
                    break;
            }
        }      
    }
}
