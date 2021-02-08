using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moviment : MonoBehaviour
{
    public WheelCollider[] WheelC;
    public GameObject[] Wheels;
    public GameObject Steer;
    public float torque = 200f;
    public float maxSteerAngle = 30;
    public float maxBrakeTorque = 500;

    public AudioSource BrakeSound;
    public AudioSource highAccel;

    public Transform SkidTrailPrefab;
    Transform[] skidTrails = new Transform[4];

    public ParticleSystem SmokePrefab;
    ParticleSystem[] smokeTire = new ParticleSystem[4];

    public GameObject BrakeLight;

    public Rigidbody rb;
    public float gearLength = 3;
    public float currentSpeed { get { return rb.velocity.magnitude * gearLength; } }
    public float lowPitch = 1f;
    public float highPitch = 6f;
    public int numGears = 5;
    float rpm;
    int currentGear = 1;
    float currentGearPerc;
    public float maxSpeed = 200;



    public void Start()
    {
        for (int i = 0; i < 2; i++)
        {
            smokeTire[i] = Instantiate(SmokePrefab);
            smokeTire[i].Stop();
        }
        BrakeLight.SetActive(false);
    }
    public void CalculateEngineSound()
    {
        float gearPercentage = (1 / (float)numGears);
        float targetGearFactor = Mathf.InverseLerp(gearPercentage * currentGear, gearPercentage * (currentGear + 1),
                                                    Mathf.Abs(currentSpeed / maxSpeed));
        currentGearPerc = Mathf.Lerp(currentGearPerc, targetGearFactor, Time.deltaTime * 5f);

        var gearNumFactor = currentGear / (float)numGears;
        rpm = Mathf.Lerp(gearNumFactor, 1, currentGearPerc);

        float speedPercentage = Mathf.Abs(currentSpeed / maxSpeed);
        float upperGearMax = (1 / (float)numGears) * (currentGear + 1);
        float downGearMax = (1 / (float)numGears) * currentGear;

        if (currentGear > 0 && speedPercentage < downGearMax)
            currentGear--;
        if (speedPercentage > upperGearMax && (currentGear < (numGears - 1)))
            currentGear++;

        float pitch = Mathf.Lerp(lowPitch, highPitch, rpm);
        highAccel.pitch = Mathf.Min(highPitch, pitch) * 0.25f;

    }

    public void Acelerar(float acceleration, float angle, float brake)
    {
        //Dizer o maximo e minimo e aceleração e angulo     
        acceleration = Mathf.Clamp(acceleration, -1, 1);
        angle = Mathf.Clamp(angle, -1, 1) * maxSteerAngle;
        brake = Mathf.Clamp(brake, 0, 1) * maxBrakeTorque;
        if(brake != 0)
        {
            BrakeLight.SetActive(true);            
        }
        else
        {
            BrakeLight.SetActive(false);
        }
        float thrustTorque = 0;
        if (currentSpeed < maxSpeed)
        {
            thrustTorque = acceleration * torque;
            //Debug.Log(acceleration);
        }
        //Reconheçer 4Rodas e as 2 da frente sendo as primeiras
        for (int i = 0; i < 4; i++)
        {
            WheelC[i].motorTorque = thrustTorque;
            if (i < 2)
            {
                //Mudar o Angulo
                //Changing
                WheelC[i].steerAngle = angle;
            }
            else
            {
                WheelC[i].brakeTorque = brake;
            }

            //Variaveis para mudar o Angulo
            Quaternion quat;
            Vector3 position;
            //Mudar o Angulos dos Shader e Collider (Roda e Volante)
            //Changing Angle Steer and Wheel Shader  
            WheelC[i].GetWorldPose(out position, out quat);
            Wheels[i].transform.position = position;
            Wheels[i].transform.rotation = quat;
            Steer.transform.localRotation = Quaternion.Euler(-angle, angle, -41);

        }

    }

    public void CheckForSkid()
    {
        //Check para ver se está derrapando o peneu
        int numSkidding = 0;
        for (int i = 0; i < 2; i++)
        {
            WheelHit wheelHit;
            WheelC[i].GetGroundHit(out wheelHit);
            if (Mathf.Abs(wheelHit.forwardSlip) >= 0.4f || Mathf.Abs(wheelHit.sidewaysSlip) >= 0.4f)
            {
                numSkidding++;
                //Se não tiver tocando o som
                if (!BrakeSound.isPlaying)
                {
                    BrakeSound.Play();
                }
                //StartSkidTrail(i);
                smokeTire[i].transform.position = WheelC[i].transform.position - WheelC[i].transform.up * WheelC[i].radius;
                smokeTire[i].Emit(1);
            }
            else
            {
                //EndSkidTrail(i);
            }
        }
        //Se tiver tocando o som e parou de derrapar
        if (numSkidding == 0 && BrakeSound.isPlaying)
        {
            BrakeSound.Stop();
        }
    }

    public void Update()
    {

        CheckForSkid();
    }
}


