using UnityEngine;
namespace SymBehaviourModule
{
    public class StateMachine<T>
    {
        public Module<T> currentModule { get; private set; }
        public T Owner;

        public StateMachine(T _owner)
        {
            Owner = _owner;
            currentModule = null;
        }

        public void ChangeModule(Module<T> newModule)
        {
            if (currentModule != null)
                currentModule.ExitModule(Owner);
            currentModule = newModule;
            currentModule.EnterModule(Owner);
        }

        public void Update()
        {
            if (currentModule != null)
                currentModule.UpdateModule(Owner);
        }

        public void Locomotion()
        {         
            if (currentModule != null)
                currentModule.Locomotion(Owner);
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

    public abstract class Module<T>
    {       

        public abstract void EnterModule(T owner);

        public abstract void UpdateModule(T owner);

        public abstract void Locomotion(T owner);

        public abstract void OnCollisionEnter(T owner, Collision collision);

        public abstract void OnCollisionStay(T owner, Collision collision);

        public abstract void ExitModule(T owner);
    }

    
}