using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicTransformSaver : MonoBehaviour
{

    private Vector3 _Position;
    private Quaternion _Rotation;
 
    public void Save()
    {
        _Position = gameObject.transform.localPosition;
        _Rotation = gameObject.transform.localRotation;
    }
	
    public void Restore()
    {
        gameObject.transform.localPosition = _Position;
        gameObject.transform.localRotation = _Rotation;
    }
}
