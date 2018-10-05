using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldObject : MonoBehaviour {

    MeshRenderer rendererComp;

	// Use this for initialization
	void Start () {
        rendererComp = GetComponent<MeshRenderer>();

        if (rendererComp)
        {
            FieldObjectManager.Instance.Register(this, rendererComp.bounds);
            rendererComp.enabled = false;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void EnableRender(bool enable)
    {
        if (rendererComp)
            rendererComp.enabled = enable;
    }
}
