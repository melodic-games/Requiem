// DrawBones.cs
using UnityEngine;

[ExecuteInEditMode]
public class DrawBones : MonoBehaviour
{
    public SkinnedMeshRenderer m_Renderer;


    void Start()
    {
      //  m_Renderer = GetComponentInChildren<SkinnedMeshRenderer>();
       // if (m_Renderer == null)
       // {
       //    
       //     Destroy(this);
       // }
    }

    void Update()
    {
        if (m_Renderer != null)
        {
            var bones = m_Renderer.bones;
            foreach (var B in bones)
            {
                if (B.parent == null)
                    continue;
                Debug.DrawLine(B.position, B.parent.position, Color.green);
            }
        }
        else
        {
            Debug.LogWarning("No SkinnedMeshRenderer found in bone viewer!");
        }




    }
}