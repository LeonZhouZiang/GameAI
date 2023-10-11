using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    public Text maxSpeed;
    public Text acceleration;
    public Text stopDistance;
    public Text slowingRadius;
    public Text currentBehavior;
    static Bot bot;
    void Start()
    {   
        bot =  GetComponent<Bot>();
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        maxSpeed.text = "Max Speed: " + bot.maxSpeed.ToString();
        acceleration.text = "Acceleration: " + bot.acceleration.ToString();
        stopDistance.text = "Stop Distance sqr: " + bot.stopDistanceSqr.ToString();
        slowingRadius.text = "slowingDistance sqr: " + bot.slowingRadiusSqr.ToString();
        
    }
    public void UpdateB()
    {
        currentBehavior.text = "Current behavior: " + bot.currentBehavior.Method.Name;
        if(bot.currentBehavior.Method.Name == "Pursue")
        {
            currentBehavior.text += " use wasd to move agent";
        }
    }
}
