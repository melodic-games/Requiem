using UnityEngine;

public class SymEffectControl : MonoBehaviour
{
    public ParticleSystem ps;
    public bool isActive;
    public SymBehaviour behaviour;
    public Transform chestAnchor;
    public Object particleExplosion;

    // Start is called before the first frame update
    void Start()
    {        
        var main = ps.main;
        var shape = ps.shape;
        var emission = ps.emission;
        main.maxParticles = 100;
        ps.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);
        main.duration = 3;

        behaviour = GetComponent<SymBehaviour>();

    }

    // Update is called once per frame
    void Update()
    {

        if (behaviour != null)
            if (behaviour.chargingEnergy)
            {
                if (behaviour.energyLevel != 1)
                    isActive = true;
                else
                    isActive = false;
            }
            else
            {
                isActive = false;
            }

        if (behaviour.controlSource.jump && behaviour.controlSource.crouching && behaviour.controlSource.thrustInput == 1 && behaviour.energyLevel == 1)
        {
            Explode();
        }

        ps.gameObject.transform.rotation = Quaternion.identity;
        var main = ps.main;
        var shape = ps.shape;
        var emission = ps.emission;

        if (isActive)
        {            
            main.loop = true;
            main.startLifetime = .5f;
            main.startSpeed = -4;
            shape.radius = 2;
            emission.rateOverTime = Mathf.Lerp(0, 40, 1);//33.42f;
            if (!ps.isPlaying) ps.Play();                           
        }
        else
        {           
            ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);                               
        }

    }

    private void LateUpdate()
    {
        ps.transform.position = chestAnchor.position;
    }

    void Explode()
    {
        if (particleExplosion != null)
            Instantiate(particleExplosion, transform.position, transform.rotation);

        //Explosion force
        SymUtils.ShockWave(transform.position, behaviour.rbVelocityMagnatude, behaviour.rb);
    }

    void ExplodeAtPosition(Vector3 position)
    {
        if (particleExplosion != null)        
            Instantiate(particleExplosion, position, transform.rotation);

        //Explosion force              
        SymUtils.ShockWave(position, behaviour.rbVelocityMagnatude * Vector3.Dot(behaviour.rbVelocityNormalized, -behaviour.groundNormal), behaviour.rb);
       
    }


    private void OnCollisionEnter(Collision collision)
    {
        float speedAlongContactNormal = behaviour.rbVelocityMagnatude * Vector3.Dot(behaviour.rbVelocityNormalized, -behaviour.groundNormal);

        //Visual Effects
        if (speedAlongContactNormal > 20)
        {
            ExplodeAtPosition(collision.contacts[0].point + collision.contacts[0].normal);
            //collision.gameObject.SendMessage("Emit");
        }
    }

}
