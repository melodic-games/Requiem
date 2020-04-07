using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class FloatingOriginManager : MonoBehaviour
{

    public Transform worldAnchorTr;
    public float maxDisplacment = 10;
    private float sqrMaxDisplacment;
    private List<Transform> allTransforms = new List<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] allObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject obj in allObjects)
        {
            allTransforms.Add(obj.transform);
        }

        sqrMaxDisplacment = maxDisplacment * maxDisplacment;
    }

    // Update is called once per frame
    void Update()
    {
        print(Vector3.Distance(worldAnchorTr.position,Vector3.zero));
        print(sqrMaxDisplacment);

        if (worldAnchorTr.position.sqrMagnitude > sqrMaxDisplacment)
        {
            foreach (Transform t in allTransforms)
            {
                t.position -= worldAnchorTr.position;
            }
        }
    }


}
