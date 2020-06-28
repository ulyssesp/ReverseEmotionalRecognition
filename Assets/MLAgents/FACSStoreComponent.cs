using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class FACSStoreComponent : MonoBehaviour
{
    public FACSStore facsstore;
    public string FACStoreJSONPath = "facstore.json";
    public Transform Root;
    public string FacName;
}