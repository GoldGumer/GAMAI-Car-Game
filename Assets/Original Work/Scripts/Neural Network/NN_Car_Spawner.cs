using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static NN_Car_Brain;
using System.Globalization;

public class NN_Car_Spawner : MonoBehaviour
{
    [SerializeField] GameObject car;
    [SerializeField] int carAmount;
    [SerializeField] float generationLength;

    string brainDirectory = Directory.GetCurrentDirectory() + "\\Assets\\Original Work\\BrainTXTs";

    public int generationCount = 0;

    GameObject[] carArray;
    float generationStartTime;
    bool isCarFinished = false;

    // Start is called before the first frame update
    void Start()
    {
        if (!Directory.Exists(brainDirectory))
        {
            Directory.CreateDirectory(brainDirectory);
        }

        carArray = new GameObject[carAmount];
        RaycastHit raycastHit;
        Physics.Raycast(GameObject.FindGameObjectWithTag("Road Generator").GetComponent<PR_Road_Gen>().GetSplinePoints()[0] + new Vector3(0, 10.0f, 7.0f), Vector3.down, out raycastHit);
        for (int i = 0; i < carAmount; i++)
        {
            carArray[i] = Instantiate(car, raycastHit.point + Vector3.up * 2, Quaternion.identity, transform);
            Brain brain = ReadBestBrainIn();
            if (brain.neurons.Length == 0)
            {
                carArray[i].GetComponent<NN_Car_Brain>().GenerateNewBrain();
            }
            else
            {
                carArray[i].GetComponent<NN_Car_Brain>().ReplaceBrain(brain);
            }
            
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

            brainDirectory = Directory.GetCurrentDirectory() + "\\Assets\\Original Work\\BrainTXTs";
            SaveBestNN(winnerCars[0].car);
        }
    }

    NN_Car_Brain.Brain ReadBestBrainIn()
    {
        brainDirectory += "\\Load\\Brain To Load.txt";
        if (File.Exists(brainDirectory))
        {
            using (StreamReader reader = File.OpenText(brainDirectory))
            {
                string brainSizes = reader.ReadLine();
                string[] sizes = brainSizes.Split("; ");
                int.TryParse(sizes[0].Split(": ")[1], out int neuronLength);
                int.TryParse(sizes[1].Split(": ")[1], out int pathsLength);
                NN_Car_Brain.Brain brain = new NN_Car_Brain.Brain(neuronLength, pathsLength);

                string neuronData = reader.ReadLine();
                string[] neurons = neuronData.Split("; ");
                foreach (var neuron in neurons)
                {
                    string[] idBias = neuron.Split(", ");
                    int.TryParse(idBias[0], out int id);
                    int.TryParse(idBias[1], out int bias);
                    brain.neurons[id].bias = bias;
                }

                string pathwayData = reader.ReadLine();
                string[] pathways = pathwayData.Split("; ");
                foreach (var path in pathways)
                {
                    string[] pathData = path.Split(", ");
                    int.TryParse(pathData[0], out int id);
                    int.TryParse(pathData[1], out int neuronInNum);
                    int.TryParse(pathData[2], out int neuronOutNum);
                    float.TryParse(pathData[3], out float weight);
                    brain.paths[id].neuronInNum = neuronInNum;
                    brain.paths[id].neuronOutNum = neuronOutNum;
                    brain.paths[id].weight = weight;
                }

                reader.Close();
                return brain;
            }
        }
        else
        {
            return new NN_Car_Brain.Brain(0, 0);
        }
    }

    void SaveBestNN(int carID)
    {
        brainDirectory += ("\\Saves\\Best AI in Gen " + generationCount + ".txt");
        if (File.Exists(brainDirectory))
        {
            File.Delete(brainDirectory);
        }
        if (!File.Exists(brainDirectory))
        {
            using (StreamWriter writer = File.CreateText(brainDirectory))
            {
                NN_Car_Brain.Brain brain = carArray[carID].GetComponent<NN_Car_Brain>().GetBrain();
                writer.WriteLine("Neurons: " + brain.neurons.Length + "; Pathways: " + brain.paths.Length);
                for (int i = 0; i < brain.neurons.Length; i++)
                {
                    writer.Write(i + ", " + brain.neurons[i].bias);
                    if (i != brain.neurons.Length - 1)
                    {
                        writer.Write("; ");
                    }
                    else
                    {
                        writer.Write("\n");
                    }
                }
                for (int i = 0; i < brain.paths.Length; i++)
                {
                    writer.Write(i + ", " + brain.paths[i].neuronInNum + ", " + brain.paths[i].neuronOutNum + ", " + brain.paths[i].weight);
                    if (i != brain.paths.Length - 1)
                    {
                        writer.Write("; ");
                    }
                    else
                    {
                        writer.Write("\n");
                    }
                }
                writer.Close();
            }
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
            carArray[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
            carArray[i].GetComponent<NN_Car_Brain>().SoftMutateBrain();
        }
    }

    public float GetStartTime()
    {
        return generationStartTime;
    }
}
