using System.Collections.Generic;
using UnityEngine;

/*
 * Class used to define and manage patrol routes.
 * A patrol route is a collection of Patrol Points (see below) that is followed cyclically.
 * It can be repeated a limited number of times or infinitely.
 * The path can be traversed in one of the following ways: A-B-C-A or A-B-C-B-A
 */

[System.Serializable]
public class PatrolHandler 
{
    [SerializeField] private List<PatrolPoint> points;
    [SerializeField] private int patrolIndex;
    [SerializeField] private bool reversePathPatrol; //false = A-B-C-A, true = A-B-C-B-A 
    [SerializeField] private bool reverseForward;    //Used to manage the index direction (either +1 or -1)
    [SerializeField] private int defaultRepeatTimes;
    //-1: Infinite repetitions, >0: Finite repetitions, 0: Invalid, as it indicates the end of patrol!
    private int repeatTimes;
    //defaultRepeatTimes is the value set for the patrol, while repeatTimes is the current value in use.
    //defaultRepeatTimes is not updated, allowing us to reset the patrol by retrieving its value.
    private bool initialized;

    public PatrolHandler(List<PatrolPoint> points, int patrolIndex, bool reversePathPatrol, bool reverseForward, int defaultRepeatTimes)
    {
        this.points = points;
        this.patrolIndex = patrolIndex;
        this.reversePathPatrol = reversePathPatrol;
        this.reverseForward = reverseForward;
        this.defaultRepeatTimes = defaultRepeatTimes;
        repeatTimes = defaultRepeatTimes;
        initialized = true;
    }

    public bool ReversePathPatrol { get { return reversePathPatrol; } set { reversePathPatrol = value; } }

    //In addition to updating the position in the path, it provides information about reaching the end
    //of the patrol
    public bool SetNextPoint() //false = end of patrol
    {
        if (!initialized)
            InitPatrol();

        GetCurrentPoint().ResetPatrol();

        if (points.Count < 2)
        {
            if (repeatTimes > 0) 
            { 
                repeatTimes--;
                if (repeatTimes == 0) //End of patrol?
                    return false;
            }

            return true;
        }

        if (reversePathPatrol)
        {
            if (reverseForward)
            {
                if (patrolIndex == points.Count - 1)
                {
                    reverseForward = false;
                    patrolIndex--;
                }
                else
                    patrolIndex++;
            }
            else
            {
                if (patrolIndex == 0) //End of the cycle
                {
                    if (repeatTimes > 0)
                    { 
                        repeatTimes--;
                        if (repeatTimes == 0) //End of patrol?
                            return false;
                    }

                    reverseForward = true;
                    patrolIndex++;
                }
                else
                    patrolIndex--;
            }
        }
        else
        {
            if (patrolIndex == points.Count - 1) //End of cycle
            {
                if (repeatTimes > 0)
                {
                    repeatTimes--;
                    if (repeatTimes == 0) //End of patrol?
                        return false;
                }
                patrolIndex = 0;
            }
            else
                patrolIndex++;
        }

        return true;
    }

    public PatrolPoint GetCurrentPoint()
    {
        if(!initialized)
            InitPatrol();

        return points[patrolIndex];
    }

    public void ResetPatrol()
    {
        patrolIndex = 0;
        reverseForward = true;
        InitPatrol();

        foreach(PatrolPoint point in points)
        {
            point.ResetPatrol();
        }
    }

    public void InitPatrol()
    {
        repeatTimes = defaultRepeatTimes;
        initialized = true;
    }

    public enum PatrolState
    {
        CLEAR,
        WARNING,
        ALERT
    }
}

/*
 * Defines a single point in the path/route to follow and specifies the direction to look at 
 * (see PatrolLookAt below) before signaling the completion of the routine at this point 
 * (managed by the method SetNextLookAt).
 */
[System.Serializable]
public class PatrolPoint
{
    [SerializeField] private Vector3 position;
    [SerializeField] private List<PatrolLookAt> lookAts;
    [SerializeField] private int lookAtIndex;

    public PatrolPoint(Vector3 position, List<PatrolLookAt> lookAts, int lookAtIndex)
    {
        this.position = position;
        this.lookAts = lookAts;
        this.lookAtIndex = lookAtIndex;
    }

    public Vector3 Position { get { return position; } }

    public bool SetNextLookAt() //true = has found another look at dir | false = no other look at dir
    {
        if (lookAts.Count < 2)
            return false;

        if (lookAtIndex == lookAts.Count - 1)
            return false;
        else
        {
            lookAtIndex++; 
            return true;
        }
    }

    public PatrolLookAt GetCurrentLookAt()
    {
        return lookAts[lookAtIndex];
    }

    public void ResetPatrol()
    {
        lookAtIndex = 0;
    }
}

/*
 * Specifies the direction to look at and the duration of time to focus.
 */
[System.Serializable]
public class PatrolLookAt
{
    [SerializeField] private Vector3 direction; // Euler angles relative to the world coordinates
    [SerializeField] private float time;

    public PatrolLookAt(Vector3 direction, float time)
    {
        this.direction = direction;
        this.time = time;
    }

    public float Time { get { return time; } set { time = value; } }

    public Vector3 GetEulerAngles()
    {
        return direction;
    }

    public Vector3 GetForward()
    {
        Vector3 lookAtDir = Quaternion.Euler(direction) * Vector3.forward;
        return lookAtDir.normalized;
    }
}
