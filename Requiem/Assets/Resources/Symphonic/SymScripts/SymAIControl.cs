using SymControl;
using UnityEngine;

public class SymAIControl : SymControlSource<SymBehaviour>
{

    Vector3 targetLocation = Vector3.zero;
    Transform targetTransform = null;
    bool useTransform = false;

    private float timer = 0;
    public Vector3 dir;

    public override void CollectInput()
    {
     
    }
}
