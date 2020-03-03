using UnityEngine;
using UnityEngine.UI;

public class FadeScreen : MonoBehaviour
{

    public Image image;
    [Range(0, 1)]
    public float alpha = 1;
    [Range(0,1)]
    public float target = 0;

    private void Update()
    {
        if (Time.timeSinceLevelLoad > 2)
        {
            //alpha = Mathf.Lerp(alpha, target, Time.deltaTime * 1);

            float dir = Mathf.Sign(target - alpha);
            float dist = Mathf.Abs(target - alpha);
            if (dist > Time.deltaTime)
            {
                alpha = Mathf.Clamp01(alpha + dir * Time.deltaTime * .5f);
            }
            else
            {
                alpha = target;
            }   
                             
        }

        image.color = new Color(0, 0, 0, alpha);

    }


}
