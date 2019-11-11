using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymMaterialControl : MonoBehaviour
{
    public Material[] bodyMaterials = { null, null };
    public Material[] emissionMaterials = { null, null };
    public SymBehaviour behaviour;
    [Range(0,3)]
    public float emissionColorIndex = 0;

    private void Start()
    {
        behaviour = GetComponent<SymBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        //Set color of body and emission materials          
        {
            Vector4 color;

            //Calulate color from index
            {
                float t = 0;
                if (behaviour.chargingEnergy || behaviour.energyLevel == 1)
                    t = 1;               
                color = Color.Lerp(Color.black,SignatureColors.colors[(int)emissionColorIndex],t);
            }

            foreach (Material material in bodyMaterials)
            {
                float t = 0;

                material.SetColor("_EmissiveColor", color);
                material.SetFloat("_ShellPower", ((1 - t) * 5) + 1);
            }

            foreach (Material material in emissionMaterials)
            {
                material.SetColor("_EmissiveColor", color);
            }
        }

        //Set shell of body     
        {
            foreach (Material material in bodyMaterials)
            {
                material.SetFloat("_ShellPower", ((1 - behaviour.energyLevel) * 5) + 1);
            }
        }
    }
}
