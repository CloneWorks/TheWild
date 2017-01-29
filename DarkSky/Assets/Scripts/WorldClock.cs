using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldClock : MonoBehaviour {

    //variables
    [Header("Day/Night Cycle")]
    [Tooltip("Number of seconds in 1 day = 86400")]
    public int dayLength = 60; //holds the length of a day

    [Tooltip("Number of seconds in 1 day = 86400")]
    public int nightLength = 60; //holds the length of a day

    public int CurrentTime; //The current time of the day

    public int updateTimeInterval = 1; //how frequently to update the time

    public int sunrise;
    public int midday;
    public int sunset;
    public int midnight;
    public int fullDay;

    public int wildResetTime = 0;

    public RectTransform clockHand;

    public Material daySkybox;
    public Material nightSkybox;

	// Use this for initialization
	void Start () {
        //get components

        //set variables
        sunrise = 0;                            //beginning of the day
        midday = dayLength / 2;                 //middle of the day
        sunset = dayLength;                     //end of the day
        midnight = dayLength + nightLength / 2; //middle of the night
        fullDay = dayLength + nightLength;    //The total time of a day
        CurrentTime = midday;                   //set time to mid-day

        //rotate clock hand
        rotateClockHand();

        //start coroutines
        StartCoroutine(updateTime(updateTimeInterval));
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public bool IsNewDay()
    {
        if (CurrentTime == wildResetTime)
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }

    IEnumerator updateTime(int updateWait)
    {
        while (true)
        {
            yield return new WaitForSeconds(updateWait);

            //update time
            CurrentTime += updateWait;

            //rotate clock hand
            rotateClockHand();
            
            //full day is over
            if (CurrentTime == fullDay)
            {
                //reset the time
                CurrentTime = 0;
            }

            //starting a new day
            if (CurrentTime == wildResetTime)
            {
                
            }

            //is day time
            if (CurrentTime >= sunrise && CurrentTime < sunset && RenderSettings.skybox != daySkybox)
            {
                RenderSettings.skybox = daySkybox;
                DynamicGI.UpdateEnvironment();
            }
            //is night time
            else if (CurrentTime >= sunset && CurrentTime < fullDay && RenderSettings.skybox != nightSkybox)
            {
                RenderSettings.skybox = nightSkybox;
                DynamicGI.UpdateEnvironment();
            }

        }
    }

    public void rotateClockHand()
    {
        //rotate clock hand
        float handRotation;
        
        //if daytime
        if(CurrentTime <= sunset){
            handRotation = (float)(180 / (float)dayLength) * CurrentTime;
        }
        //is night time
        else
        {
            handRotation = (float)(180 / (float)nightLength) * (CurrentTime - dayLength) + 180;
        }
        
        clockHand.rotation = Quaternion.Euler(0, 0, -handRotation); //new Vector3(0, 0, -handRotation);
    }
}
