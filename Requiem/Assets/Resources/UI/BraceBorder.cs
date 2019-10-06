using UnityEngine;


public class BraceBorder : MonoBehaviour
{
    public Texture image;
    [Range(0, 1)]
    public float scale = 1;
    [Range(0, 1)]
    public float target = 0;

    public float height = 0;

    public float displacement;

    private void Update()
    {
              
        if (Input.GetAxisRaw("Focus") == 1)
            target = 1;
        else
            target = 0;
        
        {            
            float dir = Mathf.Sign(target - scale);
            float dist = Mathf.Abs(target - scale);
            if (dist > Time.deltaTime)
            {
                scale =  Mathf.Clamp01(scale + dir *  6 * Time.deltaTime);
            }
            else
            {
                scale = target;
            }
        }
        
    }

    private void OnGUI()
    {
        if(image != null)
        {
            GUI.color = new Color(1,1,1,scale);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), image);            
        }
    }
}
