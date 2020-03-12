using CharacterControl;
using UnityEngine;

public class CharacterAIControl : CharacterControlSource<BaseCharacterController>
{

    Vector3 targetLocation = Vector3.zero;
    //Transform targetTransform = null;
    //bool useTransform = false;

    //private float timer = 0;
    public Vector3 dir;

    public override void CollectInput()
    {
     
    }
}
