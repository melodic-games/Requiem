using UnityEngine;

public struct Tensor3
{

    public Vector3 forward;
    public Vector3 right;
    public Vector3 up;

    public Tensor3(Vector3 forward, Vector3 right, Vector3 up)
    {
        this.forward = forward;
        this.right = right;
        this.up = up;
    }

}