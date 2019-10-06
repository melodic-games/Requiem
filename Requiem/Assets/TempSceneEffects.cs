using UnityEngine;

public class TempSceneEffects : MonoBehaviour
{

    public Material mat;
    [ColorUsage(false,true)]
    public Color color;
    private float scale = 0;    

    // Update is called once per frame
    void Update()
    {
        scale =  Mathf.Lerp(scale,0,Time.deltaTime);
        mat.SetColor("_Color", color * scale);
    }

    public void Emit()
    {
        scale = 1;
    }

}
