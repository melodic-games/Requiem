using UnityEngine;

public class PIDController
{
    Vector3 error_old;
    //The controller will be more robust if you are using a further back sample
    Vector3 error_old_2;
    Vector3 error_sum;
    //If we want to average an error as input
    Vector3 error_sum2;

    //PID Gains
    public Vector3 gains = Vector3.zero;

    //Use this when experimenting with PID parameters and the gains are stored in a Vector3
    public Vector3 GetFactorFromPIDController(Vector3 gains, Vector3 error)
    {
        this.gains.x = gains.x;
        this.gains.y = gains.y;
        this.gains.z = gains.z;

        Vector3 output = CalculatePIDOutput(error);

        return output;
    }

    private Vector3 CalculatePIDOutput(Vector3 error)
    {
        //Reset Output
        Vector3 output = Vector3.zero;

        //Proportional
        {
            output += gains.x * error;
        }

        //Integral
        {
            error_sum += Time.fixedDeltaTime * error;
            output += gains.y * error_sum;
        }

        //Derivative
        {
            Vector3 d_dt_error = (error - error_old) / Time.fixedDeltaTime;

            //Save the last errors
            error_old_2 = error_old;

            error_old = error;

            output += gains.z * d_dt_error;
        }


        return output;
    }
}