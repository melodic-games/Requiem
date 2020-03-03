using UnityEngine;
namespace SymBehaviourModule
{
    public class StateMachine<T>
    {
        public Behaviour<T> currentModule { get; private set; }
        public T Owner;

        public StateMachine(T _owner)
        {
            Owner = _owner;
            currentModule = null;
        }

        public void ChangeModule(Behaviour<T> newBehaviour)
        {
            if (currentModule != null)
                currentModule.ExitModule(Owner);
            currentModule = newBehaviour;
            currentModule.EnterModule(Owner);
        }

        public void Locomotion()
        {         
            if (currentModule != null)
                currentModule.Locomotion(Owner);

        }

        public void LateUpdate()
        {
            if (currentModule != null)
                currentModule.LateUpdate(Owner);
        }

        public void CollissionEnter(Collision collision)
        {
            if (currentModule != null)
                currentModule.OnCollisionEnter(Owner, collision);
        }

        public void CollissionStay(Collision collision)
        {
            if (currentModule != null)
                currentModule.OnCollisionStay(Owner, collision);
        }

    }

    public abstract class Behaviour<T>
    {       

        public abstract void EnterModule(T owner);      

        public abstract void Locomotion(T owner);

        public abstract void LateUpdate(T owner);

        public abstract void OnCollisionEnter(T owner, Collision collision);

        public abstract void OnCollisionStay(T owner, Collision collision);

        public abstract void ExitModule(T owner);
    }

    
}