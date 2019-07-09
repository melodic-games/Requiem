using System;
using System.Collections.Generic;
using UnityEngine;

//Gravity Type Enumeration
public enum GravityType
{
    Spherical = 0,
    Planar = 1,
    Cylindrical = 2
}

[System.Serializable]
public class GravitySource
{    
    public Vector3 position;
    Quaternion rotation;
    public float mass;
    GravityType type;//0 Spherical, 1 Planar, 2 Cylindrical   
    float cylindricalArcLimit = 360;   
    bool cylindricalTopless = false;
    Vector2 planarSize;
    public Transform trackingTransform = null;

    float duration = Mathf.Infinity;

    public GravitySource()//default
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;
        type = GravityType.Spherical;
        mass = 100f;
    }

    public GravitySource(Vector3 position, float mass, float duration)//Spherical
    {
        this.position = position;
        rotation = Quaternion.identity;
        type = GravityType.Spherical;
        this.mass = mass;
    }

    public GravitySource(Vector3 position, float mass, float duration, Vector2 planarSize, Quaternion rotation)//Planar
    {
        this.position = position;
        this.rotation = rotation;
        type = GravityType.Spherical;
        mass = 100f;
    }

    public GravitySource(Vector3 position, float mass, float duration, float arcLimit, bool topless, Quaternion rotation)//Cylinderical
    {
        this.position = position;
        rotation = Quaternion.identity;
        type = GravityType.Spherical;
        cylindricalArcLimit = arcLimit;     
        cylindricalTopless = topless;        
        mass = 100f;
    }

}

[System.Serializable]
public class GravityChain
{
    List<GravitySource> chainedGravitysources;

}

