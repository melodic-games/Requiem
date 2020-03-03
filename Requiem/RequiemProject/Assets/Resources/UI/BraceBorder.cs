using UnityEngine;
using UnityEngine.UI;

public class BraceBorder : MonoBehaviour
{
    public bool overrideInupt = false;

    public Image image;
 
    [Range(0, 1)]
    public float scale = 1;
    [Range(0, 1)]
    public float target = 0;

    private void Update()
    {
              
        if (!overrideInupt)
        if (Input.GetAxisRaw("Focus") == 1 )
            target = 1;
        else
            target = 0;

        if (Time.timeSinceLevelLoad > 5)
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

        image.color = new Color(image.color.r, image.color.g, image.color.b, scale);

    }


}
