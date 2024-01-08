using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class NN_Car_Spawner : MonoBehaviour
{
    [SerializeField] GameObject car;
    [SerializeField] int carAmount;
    [SerializeField] float generationLength;

    public int generationCount = 0;

    GameObject[] carArray;
    float generationStartTime;
    bool isCarFinished = false;

    // Start is called before the first frame update
    void Start()
    {
        carArray = new GameObject[carAmount];
        RaycastHit raycastHit;
        Physics.Raycast(GameObject.FindGameObjectWithTag("Road Generator").GetComponent<PR_Road_Gen>().GetSplinePoints()[0] + new Vector3(0, 10.0f, 7.0f), Vector3.down, out raycastHit);
        for (int i = 0; i < carAmount; i++)
        {
            carArray[i] = Instantiate(car, raycastHit.point + Vector3.up * 2, Quaternion.identity, transform);
            carArray[i].GetComponent<NN_Car_Brain>().SetCarTDCar(i);
            carArray[i].GetComponent<NN_Car_Brain>().MutateBrain();
        }
        generationStartTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (generationStartTime + generationLength <= Time.time)
        {
            NN_Car_Brain.carTimeDistance[] carTD = new NN_Car_Brain.carTimeDistance[carAmount];
            for (int i = 0; i < carArray.Length; i++)
            {
                carTD[i] = carArray[i].GetComponent<NN_Car_Brain>().GetCarTimeDistance();
            }

            Array.Sort(carTD, NN_Car_Brain.carTimeDistance.SortCarsByTime());
            List<NN_Car_Brain.carTimeDistance> winnerCars = new List<NN_Car_Brain.carTimeDistance>();

            if (carTD[0].isFinished)
            {
                isCarFinished = true;
                for (int i = 0; i < carTD.Length; i++)
                {
                    if (carTD[i].isFinished)
                    {
                        winnerCars.Add(carTD[i]);
                    }
                }
            }

            Array.Sort(carTD, NN_Car_Brain.carTimeDistance.SortCarsByDistance());

            int j = 0;
            while (winnerCars.Count < (carTD.Length / 2))
            {
                if (!winnerCars.Contains(carTD[j]))
                {
                    winnerCars.Add(carTD[j]);
                }
                j++;
            }

            List<NN_Car_Brain.carTimeDistance> loserCars = new List<NN_Car_Brain.carTimeDistance>();

            j = carTD.Length - 1;
            while (loserCars.Count < (carTD.Length / 2))
            {
                if (!winnerCars.Contains(carTD[j]))
                {
                    loserCars.Add(carTD[j]);
                }
                j--;
            }

            for (int i = 0; i < (carTD.Length / 2); i += 2)
            {
                NN_Car_Brain.Brain[] brains = NN_Car_Brain.Brain.BreedBrains(carArray[winnerCars[i].car].GetComponent<NN_Car_Brain>().GetBrain(), carArray[winnerCars[i + 1].car].GetComponent<NN_Car_Brain>().GetBrain());
                carArray[loserCars[i].car].GetComponent<NN_Car_Brain>().ReplaceBrain(brains[0]);
                carArray[loserCars[i + 1].car].GetComponent<NN_Car_Brain>().ReplaceBrain(brains[1]);
            }

            if (isCarFinished)
            {
                GameObject.FindGameObjectWithTag("Road Generator").GetComponent<PR_Road_Gen>().RegenerateRoad();
            }
            ResetCars();
            generationStartTime = Time.time;
            generationCount++;
        }
    }

    void ResetCars()
    {
        RaycastHit raycastHit;
        Physics.Raycast(GameObject.FindGameObjectWithTag("Road Generator").GetComponent<PR_Road_Gen>().GetSplinePoints()[0] + new Vector3(0, 10.0f, 7.0f), Vector3.down, out raycastHit);
        for (int i = 0; i < carArray.Length; i++)
        {
            carArray[i].transform.position = raycastHit.point + Vector3.up * 2;
            carArray[i].transform.rotation = Quaternion.identity;
            carArray[i].GetComponent<NN_Car_Brain>().SoftMutateBrain();
        }
    }

    public float GetStartTime()
    {
        return generationStartTime;
    }
}
