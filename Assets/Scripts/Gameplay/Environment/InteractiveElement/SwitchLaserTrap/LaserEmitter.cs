/*
 * This is the main script of the SwitchLaserTrap that handles its behavior.
 * The SwitchLaserTrap is a trap consisting of a pressure plate with three laser emitters on the sides 
 * that shoot lasers either intermittently or permanently.
 * Upon contact with the pressure plate, all lasers will be activated after a specified delay.
 * See the SwitchLaserPlate class and the LaserInteraction class for more details.
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserEmitter : MonoBehaviour, IStateSaveable
{
    [SerializeField] private List<Laser> laserList = new List<Laser>();
    [SerializeField] private float delayToPermanent = 0.5f;
    private bool permanentlyActive = false;
    private bool initialized = false;

    private void Awake()
    {
        foreach(Laser laser in laserList)
        {
            laser.laserLine.positionCount = 2;
            laser.laserLine.SetPosition(0, laser.laserLine.transform.position);
            laser.laserLine.SetPosition(1, laser.endPoint.position);
        }
    }

    private void Start()
    {
        if (initialized)
            return;

        if(permanentlyActive)
        {
            ActivePermanently();
        }
        else
        {
            for(int i=0; i<laserList.Count;i++)
            {
                StartCoroutine(LaserCoroutine(i));
            }
        }
    }

    //Activates all lasers permanently.
    public void ActiveAllLaser()
    {
        if(permanentlyActive)
            return;

        StopAllCoroutines();
        StartCoroutine(ActivePermanentlyCoroutine());
    }

    private IEnumerator LaserCoroutine(int index)
    {
        Laser laser = laserList[index];

        while(true)
        {
            //The sequence alternates between off and on: off-on-off-on-...
            laser.laserLine.enabled = false;
            laser.interaction.EnableInteraction(false);

            while(laser.timer < laser.durationOff)
            {
                yield return 0;
                laser.timer += Time.deltaTime;
            }

            laser.laserLine.enabled = true;
            laser.interaction.EnableInteraction(true);

            while (laser.timer < (laser.durationOff + laser.durationOn))
            {
                yield return 0;
                laser.timer += Time.deltaTime;
            }

            laser.timer = 0;
        }
    }

    private IEnumerator ActivePermanentlyCoroutine()
    {
        yield return new WaitForSeconds(delayToPermanent);
        ActivePermanently();
    }

    private void ActivePermanently()
    {
        permanentlyActive = true;

        foreach (Laser laser in laserList)
        {
            laser.laserLine.enabled = true;
            laser.interaction.EnableInteraction(true);
        }
    }

    [System.Serializable]
    public class Laser
    {
        [SerializeField] public LineRenderer laserLine;
        [SerializeField] public Transform endPoint;
        [SerializeField] public LaserInteraction interaction;
        [SerializeField] public float durationOn;
        [SerializeField] public float durationOff;
        [SerializeField] public float timer;
    }

    //EntityState
    public IState SaveState()
    {
        CustomEntityState state = new CustomEntityState();

        state.permanentlyActive = permanentlyActive;

        if (permanentlyActive) //Since it is activated permanently, there is no need to store the current timer.
        {
            state.timers = null;
        }
        else
        {
            state.timers = new List<float>();

            for (int i = 0; i < laserList.Count; i++)
            {
                state.timers.Add(laserList[i].timer);
            }
        }

        return state;
    }

    public void LoadState(IState state)
    {
        CustomEntityState _state = state as CustomEntityState;

        initialized = true;
        permanentlyActive = _state.permanentlyActive;

        StopAllCoroutines();

        if(permanentlyActive)
        {
            ActivePermanently();
        }
        else
        {
            if (_state.timers.Count == laserList.Count)
            {
                for (int i = 0; i < _state.timers.Count; i++)
                {
                    laserList[i].timer = _state.timers[i];
                    StartCoroutine(LaserCoroutine(i));
                }
            }
            else //Error: inconsistency between the number of lasers and the number of timers.
            {
                for (int i = 0; i < _state.timers.Count; i++)
                {
                    //Shoots the laser with the timer set to its default value.
                    StartCoroutine(LaserCoroutine(i));
                }
            }
        }
    }

    [System.Serializable]
    private class CustomEntityState : IState
    {
        [SerializeField] public bool permanentlyActive;
        [SerializeField] public List<float> timers;
    }
}
