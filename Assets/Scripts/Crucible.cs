using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crucible : MonoBehaviour
{
    public float material;
    public float maxMaterial;
    public Slider fill;
    public Slider pourer;
    public Transform crucible;
    public CrucibleFollowMouse mouseFollower;
    public Mould mould;
    void Start()
    {
        fill.maxValue = maxMaterial;
    }
    void Update()
    {
        material = Mathf.Clamp(material, 0, maxMaterial);
        fill.value = material;
        float angle = mouseFollower.currentAngle;
        float maxAngle = mouseFollower.maxAngle;
        maxAngle -= 360;
        if (mouseFollower.clickedOn && material > 0)
        {
            float takeAwayVal = Mathf.Clamp((pourer.value - 0.60f) * 3 * Time.deltaTime, 0, 1);
            material -= takeAwayVal;
            mould.material += takeAwayVal;
            pourer.value = maxAngle - angle + 2 + material/10;
            
        }
        else
        {
            pourer.value = 0;
        }
    }
}
