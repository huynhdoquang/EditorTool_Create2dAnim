using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineTool : MonoBehaviour
{
    public static string GetProjectKey(string key)
    {
        return Application.identifier + Application.productName + key;
    }
}
