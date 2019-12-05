using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymVisualControl : MonoBehaviour
{
    public SymBehaviour behaviour;

    public GameObject[] symOutfit;     

    public Color baseColor1 = Color.white;
    public Color baseColor2 = Color.black;

    [Range(0, 3)] public float emissionColorIndex = 0;

    public Material[] bodyMaterials = { null, null };
    public Material[] emissionMaterials = { null, null };

    public bool showCapOut = true;

    public Vector4 emissiveColor;

    private float value;

    private void Start()
    {
        behaviour = GetComponent<SymBehaviour>();
    }

    private void CostumeChange()
    {        
        foreach (GameObject obj in symOutfit)
        {
            obj.active = !obj.active;
        }                           
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeSinceLevelLoad > 2)
        {
            //Set color of body and emission materials                                
            Color color = SignatureColors.colors[(int)emissionColorIndex];            
            
            emissiveColor = Color.Lerp(emissiveColor, color * behaviour.energyLevel, Time.deltaTime * 5f);
            
            if (behaviour.energyLevel == 1 && behaviour.chargingEnergy)
            {            
                if (showCapOut)
                {
                    emissiveColor = SignatureColors.colors[(int)emissionColorIndex] * 500;
                    Debug.Log("Capping out!");
                    CostumeChange();
                    showCapOut = false;
                }                           
                
            }

            if (behaviour.energyLevel == 0 && behaviour.rbVelocityMagnatude < 1)
            {
                if (!showCapOut)
                {
                    emissiveColor = SignatureColors.colors[(int)emissionColorIndex] * 500;
                    Debug.Log("Power down.");
                    CostumeChange();
                    showCapOut = true;
                }
            }

            foreach (Material material in bodyMaterials)
            {
                material.SetColor("_ShellColor", emissiveColor);
                material.SetColor("_BaseColor1", Color.Lerp(material.GetColor("_BaseColor1"), baseColor1, Time.deltaTime));
                material.SetColor("_BaseColor2", Color.Lerp(material.GetColor("_BaseColor2"), baseColor2, Time.deltaTime));
            }

            foreach (Material material in emissionMaterials)
            {
                material.SetColor("_EmissiveColor", Color.Lerp(material.GetColor("_EmissiveColor"), emissiveColor * 2, Time.deltaTime));
            }

            //Set shell of body     
            {
        
                 value = ((1 - behaviour.energyLevel) * 8) + 1; 

                foreach (Material material in bodyMaterials)
                {
                    material.SetFloat("_ShellPower", Mathf.Lerp(material.GetFloat("_ShellPower"), value, Time.deltaTime * 2));
                }
            }
        }
    }
}
