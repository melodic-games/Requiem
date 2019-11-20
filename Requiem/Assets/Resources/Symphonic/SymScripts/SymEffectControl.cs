using UnityEngine;

public class SymEffectControl : MonoBehaviour
{
    public ParticleSystem chargeParticleSystem;
    public bool isActive;
    public SymBehaviour behaviour;
    public Transform chestAnchor;
    public Object particleExplosion;

    // Start is called before the first frame update
    void Start()
    {
        chargeParticleSystem.transform.parent = null;
        var main = chargeParticleSystem.main;
        var shape = chargeParticleSystem.shape;
        var emission = chargeParticleSystem.emission;
        main.maxParticles = 100;
        chargeParticleSystem.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);
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

        if (chargeParticleSystem != null)
        {
            chargeParticleSystem.gameObject.transform.rotation = Quaternion.identity;
            var main = chargeParticleSystem.main;
            var shape = chargeParticleSystem.shape;
            var emission = chargeParticleSystem.emission;

            if (isActive)
            {
                main.loop = true;
                main.startLifetime = .5f;
                main.startSpeed = -4;
                shape.radius = 2;
                emission.rateOverTime = Mathf.Lerp(0, 40, 1);//33.42f;
                if (!chargeParticleSystem.isPlaying) chargeParticleSystem.Play();
            }
            else
            {
                chargeParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
        }

    }

    private void LateUpdate()
    {
        if (chargeParticleSystem != null)
            chargeParticleSystem.transform.position = chestAnchor.position;
    }

    void Explode()
    {
        if (particleExplosion != null)
            Instantiate(particleExplosion, transform.position + transform.up * behaviour.playerHeight * .5f, transform.rotation);

        //Explosion force
        SymUtils.ShockWave(transform.position, behaviour.rbVelocityMagnatude, behaviour.rb);
    }

    void ExplodeAtPosition(Vector3 position)
    {
        if (particleExplosion != null)        
            Instantiate(particleExplosion, position, transform.rotation);

        //Explosion force              
        SymUtils.ShockWave(position, behaviour.rbVelocityMagnatude * Vector3.Dot(behaviour.rbVelocityNormalized, -behaviour.surfaceNormal), behaviour.rb);
       
    }


    private void OnCollisionEnter(Collision collision)
    {
        float speedAlongContactNormal = behaviour.rbVelocityMagnatude * Vector3.Dot(behaviour.rbVelocityNormalized, -behaviour.surfaceNormal);

        //Visual Effects
        if (speedAlongContactNormal > 20)
        {
            ExplodeAtPosition(collision.contacts[0].point + collision.contacts[0].normal);
            //collision.gameObject.SendMessage("Emit");
        }
    }

}
