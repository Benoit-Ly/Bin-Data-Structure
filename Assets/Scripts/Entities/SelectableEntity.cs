using UnityEngine;
using System.Collections.Generic;

public abstract class SelectableEntity : MonoBehaviour
{
    protected bool isSelected = false;
    protected Material material;
    protected Color baseColor = Color.white;

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        material.color = isSelected ? Color.red : baseColor;
    }

    virtual protected void Awake()
    {
        material = GetComponent<Renderer>().material;
        baseColor = material.color;
    }
}
