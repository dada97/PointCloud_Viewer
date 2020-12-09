using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectPanel : MonoBehaviour
{
    public GameObject content;
    public GameObject togglePrefab;
    public GameObject pointcloud;
    private List<GameObject> toggleList;

    private void Start()
    {
        toggleList = new List<GameObject>();
    }

    public void createToggle(string name)
    {
        GameObject toggle = Object.Instantiate(togglePrefab);
        toggle.GetComponentInChildren<Text>().text = name;
        toggle.transform.parent = content.transform;
        toggle.GetComponent<Toggle>().onValueChanged.AddListener((value) => ToggleValueChanged(toggle));
        toggleList.Add(toggle);
    }

    public void deleteToggle()
    {

    }

    public void ToggleValueChanged(GameObject t)
    {
        string objname = t.GetComponentInChildren<Text>().text;
        GameObject pcobj = null;

        int children = pointcloud.transform.childCount;
        for (int i = 0; i < children; ++i)
        {
            if (pointcloud.transform.GetChild(i).name == objname)
            {
                pcobj = pointcloud.transform.GetChild(i).gameObject;
            }
        }
            
        
        if (t.GetComponent<Toggle>().isOn)
        {
            pcobj.SetActive(true);
        }
        else
        {
            pcobj.SetActive(false);
        }
        
    }

    public void resetToggle()
    {
        foreach (GameObject toggle in toggleList)
        {
            Destroy(toggle);
        }
        toggleList = new List<GameObject>();
    }
}
