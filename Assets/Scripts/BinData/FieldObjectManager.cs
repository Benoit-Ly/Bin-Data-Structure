using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldObjectManager : MonoBehaviour {

    #region INSTANCE
    static private FieldObjectManager p_Instance = null;
    static public FieldObjectManager Instance { get { return p_Instance; } }
    #endregion

    #region Events
    public event Action<FieldObject, Bounds> OnNewObject;
    #endregion

    private void Awake()
    {
        if (FieldObjectManager.Instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }

        p_Instance = this;
    }

    public void Register(FieldObject newObject, Bounds bounds)
    {
        if (OnNewObject != null)
            OnNewObject(newObject, bounds);
    }
}
