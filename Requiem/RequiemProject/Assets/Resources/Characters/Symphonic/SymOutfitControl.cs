using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymOutfitControl : MonoBehaviour
{
    public BaseCharacterController controller;

    public GameObject[] symOutfit;
    public bool modifyOutfit = true;

    public Color baseColor1 = Color.white;
    public Color baseColor2 = Color.black;

    [Range(0, 3)] public float emissionColorIndex = 0;

    public bool updateBodyMaterials = true;

    public Material[] bodyMaterials = { null, null };

    public bool updateEmissiveInMaterials = true;    

    public Material[] solidEmissiveMaterials = { null };

    public bool updateShellPowerInMaterials = true;

    public bool showCapOut = true;

    public Vector4 emissiveColor;
    public Vector4 emissiveSolidColor;

    private float value;

    private void Start()
    {
        controller = GetComponent<BaseCharacterController>();
    }

    private void CostumeChange()
    {
        if (modifyOutfit == false)
            return;


        if(symOutfit.Length > 0)
        if(symOutfit[0] != null)
        foreach (GameObject obj in symOutfit)
        {
            obj.SetActive(!obj.activeSelf);
        }                           
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeSinceLevelLoad > 2)
        {
            //Set color of body and emission materials                                
            Color color = SymColors.colors[(int)emissionColorIndex];

            emissiveColor = Color.Lerp(emissiveColor, color * controller.energyLevel, Time.deltaTime * 5f);
            emissiveSolidColor = Color.Lerp(emissiveSolidColor, color, Time.deltaTime * 5f);

            if (controller.energyLevel == 1 && controller.chargingEnergy)
            {
                if (showCapOut)
                {
                    emissiveColor = SymColors.colors[(int)emissionColorIndex] * 100;             
                    Debug.Log("Capping out!");
                    showCapOut = false;
                    CostumeChange();
                }

            }

            if (controller.energyLevel == 0 && controller.rbVelocityMagnitude < 1)
            {
                if (!showCapOut)
                {
                    emissiveColor = SymColors.colors[(int)emissionColorIndex] * 50;
                    Debug.Log("Power down.");
                    CostumeChange();
                    showCapOut = true;
                }
            }

            if (updateBodyMaterials) {

                foreach (Material material in bodyMaterials)
                {       
                    material.SetColor("_BaseColor1", Color.Lerp(material.GetColor("_BaseColor1"), baseColor1, Time.deltaTime));
                    material.SetColor("_BaseColor2", Color.Lerp(material.GetColor("_BaseColor2"), baseColor2, Time.deltaTime));
                }
            }

            if (updateEmissiveInMaterials) {

                foreach (Material material in bodyMaterials)
                {
                    material.SetColor("_ShellColor", emissiveColor);
                }

                foreach (Material material in solidEmissiveMaterials)
                {
                    material.SetColor("_EmissiveColor", Color.Lerp(material.GetColor("_EmissiveColor"), emissiveSolidColor * 2, Time.deltaTime));
                }
            }

            //Set shell of body   
            if (updateShellPowerInMaterials)
            {        
                value = ((1 - controller.energyLevel) * 8) + 1; 

                foreach (Material material in bodyMaterials)
                {
                    material.SetFloat("_ShellPower", Mathf.Lerp(material.GetFloat("_ShellPower"), value, Time.deltaTime * 2));
                }
            }
        }
    }
}
