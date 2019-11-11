namespace SymControl
{
    public abstract class SymControlSource<T>
    {

        public bool invertFocus = false;
        public float focusInput;

        public bool boost;
        public bool boost_ResponceDisabled = false;

        public bool jump = false;
        public bool bounce = false;
        public bool canRun = false;
        public bool crouching = false;
        public float thrustInput = 0;

        public float rollAxisInput;
        public float pitchAxisInput;

        public float horizontalInput;
        public float verticalInput;


        public abstract void CollectInput();

    }
}
