using UnityEngine;

public class LookDirectionHelper : MonoBehaviour
{
    public BaseCharacterController controller;

    private void OnAnimatorIK(int layerIndex)
    {
        controller.UpdateAnimatorLookPosition(layerIndex);
    }

}
