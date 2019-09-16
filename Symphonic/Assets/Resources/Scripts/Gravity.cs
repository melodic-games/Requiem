using System;
using System.Collections.Generic;
using UnityEngine;

//Gravity Type Enumeration
public enum GravityType
{
    Spherical = 0, Planar = 1, Radial = 2, Cylindrical = 3
}

[System.Serializable]
public class GravitySource
{    
    public Vector3 position;
    public Quaternion rotation;
    public float mass;
    public GravityType type;//0 Spherical, 1 Planar, 2 Cylindrical   
    float arcLimit = 360;   
    bool cylindricalTopless = false;
    Vector2 planarSize;
    public float radius;
    public Transform trackingTransform = null;
    

    public float creationTime = 0;
    public float durationTime = Mathf.Infinity;//How long a gravity source last after it is created.
    public float decayTime = 3;// In seconds, linear degredation after duration unpon which a gravity source will phaze out of existance. 
    public float durationScaler = 1;

    public GravitySource()//default
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;
        type = GravityType.Spherical;
        mass = 100f;
        creationTime = 0;        
        durationTime = Mathf.Infinity;
        decayTime = 0;
    }

    public GravitySource(Vector3 position, float mass, float durationTime, float decayTime)//Spherical
    {
        this.position = position;
        rotation = Quaternion.identity;
        type = GravityType.Spherical;
        this.mass = mass;
        creationTime = Time.time;        
        this.durationTime = durationTime;
        this.decayTime = decayTime;
    }

    public GravitySource(Vector3 position, float mass, float durationTime, float decayTime, Vector2 planarSize, Quaternion rotation, bool doubleSided)//Planar
    {
        this.position = position;
        this.rotation = rotation;
        type = GravityType.Planar;
        this.mass = mass;
        creationTime = Time.time;       
        this.durationTime = durationTime;
        this.decayTime = decayTime;
        this.planarSize = planarSize;
    }

    public GravitySource(Vector3 position, float mass, float durationTime, float decayTime, float radius, Quaternion rotation, bool doubleSided)//Radial
    {
        this.position = position;
        this.rotation = rotation;
        type = GravityType.Radial;
        this.mass = mass;
        creationTime = Time.time;       
        this.durationTime = durationTime;
        this.decayTime = decayTime;
        this.radius = radius;
    }

    public GravitySource(Vector3 position, float mass, float durationTime, float decayTime, float arcLimit, bool topless, Quaternion rotation)//Cylinderical
    {
        this.position = position;
        rotation = Quaternion.identity;
        type = GravityType.Spherical;

        this.mass = mass;
        creationTime = Time.time;        
        this.durationTime = durationTime;
        this.decayTime = decayTime;
        this.arcLimit = arcLimit;
        cylindricalTopless = topless;    
    }

}

[System.Serializable]
public class GravityChain
{
    List<GravitySource> chainedGravitysources;

}

