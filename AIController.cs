using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public Circuit circuit;
    Moviment ds;
    public float steeringSensitivity = 0.01f;
    Vector3 target;
    Vector3 nextTarget;
    int currentWP = 0;
    float totalDistanceToTarget;
    float brake;
    float accel;

    GameObject tracker;
    int currentTrackerWP = 0;
    float lookAhead = 10;
   
    void Start()
    {
        ds = this.GetComponent<Moviment>();
        target = circuit.waypoints[currentWP].transform.position;
        nextTarget = circuit.waypoints[currentWP+1].transform.position;
        totalDistanceToTarget = Vector3.Distance(target, ds.rb.gameObject.transform.position);

        tracker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        DestroyImmediate(tracker.GetComponent<Collider>());
        tracker.transform.position = ds.rb.gameObject.transform.position;
        tracker.transform.rotation = ds.rb.gameObject.transform.rotation;

    }

    void ProgressTracker()
    {
        //Codigo para ver o progresso dos waypoints que o carro irá seguir
        Debug.DrawLine(ds.rb.gameObject.transform.position, tracker.transform.position);

        if (Vector3.Distance(ds.rb.gameObject.transform.position, tracker.transform.position) > lookAhead) return;

        tracker.transform.LookAt(circuit.waypoints[currentTrackerWP].transform.position);
        tracker.transform.Translate(0, 0, 1.0f);

        if (Vector3.Distance(tracker.transform.position, circuit.waypoints[currentTrackerWP].transform.position) < 1)
        {
            currentTrackerWP++;
            if(currentTrackerWP >= circuit.waypoints.Length)
            {
                currentTrackerWP = 0;
            }
        }
    }
  
    void Update()
    {
        ProgressTracker();
        //Detectando Posição e distancia de todos os targets necessarios
        Vector3 localTarget = ds.rb.gameObject.transform.InverseTransformPoint(target);
        Vector3 nextlocalTarget = ds.rb.gameObject.transform.InverseTransformPoint(nextTarget);
        float distanceToTarget = Vector3.Distance(target, ds.rb.gameObject.transform.position);
        float distanceBTWTargets = Vector3.Distance(localTarget, nextlocalTarget);

        //Detectando angulação dos 2 targets
        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        float nexttargetAngle = Mathf.Atan2(nextlocalTarget.x, nextlocalTarget.z) * Mathf.Rad2Deg;

        //O quão o carro vai virar nas trocas de target
        float steer = Mathf.Clamp(targetAngle * steeringSensitivity, -1, 1) * Mathf.Sign(ds.currentSpeed);

        //Distance e speed factor relacionao aos targets
        float distanceFactor = distanceToTarget / totalDistanceToTarget;
        float speedFactor = ds.currentSpeed / ds.maxSpeed;      

        
        

        //Se o proximo angulo for maior que 20 ou menor que -20
        if (Mathf.Abs(nexttargetAngle) > 20 || Mathf.Abs(nexttargetAngle) < -20)
        {
            //Debug.Log(nexttargetAngle);
            brake += 0.5f;
            accel -= 0.5f;
            Debug.Log(nexttargetAngle);
        }
        // Se a distancia entre os targets for menor que 11
        if (distanceBTWTargets < 11)
        {
            brake += 0.5f;
            accel -= 0.2f;
        }
        else
        {
            brake = 0;
            accel = 1;
            Debug.Log("Diminuiu3");
        }        

        //Debug.Log("Brake:" + brake + "Accel : " + accel + "Speed: " + ds.rb.velocity.magnitude); ;



        //Chamando a função acelerar no outro script
        ds.Acelerar(accel, steer, brake);

        //Se a distancia for menor que 5
        if (distanceToTarget < 5) 
        {
            currentWP++;
            if (currentWP >= circuit.waypoints.Length)
                currentWP = 0;
            target = circuit.waypoints[currentWP].transform.position;
            nextTarget = circuit.waypoints[currentWP+1].transform.position;
            totalDistanceToTarget = Vector3.Distance(target, ds.rb.gameObject.transform.position);
        }
        ds.CheckForSkid();
        ds.CalculateEngineSound();
    }
}
