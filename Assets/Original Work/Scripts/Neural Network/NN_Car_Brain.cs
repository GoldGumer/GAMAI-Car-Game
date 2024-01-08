using System.Collections;
using System.Collections.Generic;
using UnityEngine.Splines;
using Unity.Mathematics;
using UnityEngine;



public class NN_Car_Brain : MonoBehaviour
{
    static readonly int[] layerCounts = { 27, 7, 5 };

    carTimeDistance carTD;

    Brain carBrain;

    public struct Neuron
    {
        public float value;
        public float bias;

        public Neuron(float _value, float _bias)
        {
            value = _value;
            bias = _bias;
        }
    };

    public struct Pathway
    {
        public int neuronInNum;
        public int neuronOutNum;
        public float weight;

        public Pathway(int _neuronInNum, int _neuronOutNum, float _weight)
        {
            neuronInNum = _neuronInNum;
            neuronOutNum = _neuronOutNum;
            weight = _weight;
        }
    };

    public struct Brain
    {
        public Neuron[] neurons;

        public Pathway[] paths;

        public Brain(int neuronLength, int pathLength)
        {
            neurons = new Neuron[neuronLength];
            paths = new Pathway[pathLength];
        }

        public Brain(int a, int b, int c) : this(a + b + c, a * b + b * c) { }

        public void ClearNeuronValues()
        {
            for (int i = 0; i < neurons.Length; i++)
            {
                neurons[i].value = 0.0f;
            }
        }

        public static Brain[] BreedBrains(Brain parentA, Brain parentB)
        {
            if (parentA.paths.Length == parentB.paths.Length)
            {
                Brain[] brains = new Brain[2];
                brains[0] = new Brain(parentA.neurons.Length, parentA.paths.Length);
                brains[1] = new Brain(parentA.neurons.Length, parentA.paths.Length);
                for (int i = 0; i < parentA.paths.Length; i++)
                {
                    int randInt = UnityEngine.Random.Range(0, 2);
                    if (randInt == 0)
                    {
                        brains[0].paths[i] = parentA.paths[i];
                        brains[1].paths[i] = parentB.paths[i];
                    }
                    else
                    {
                        brains[0].paths[i] = parentB.paths[i];
                        brains[1].paths[i] = parentA.paths[i];
                    }
                }
                for (int i = 0; i < parentA.neurons.Length; i++)
                {
                    int randInt = UnityEngine.Random.Range(0, 2);
                    if (randInt == 0)
                    {
                        brains[0].neurons[i] = parentA.neurons[i];
                        brains[1].neurons[i] = parentB.neurons[i];
                    }
                    else
                    {
                        brains[0].neurons[i] = parentB.neurons[i];
                        brains[1].neurons[i] = parentA.neurons[i];
                    }
                }
                return brains;
            }
            else
            {
                return null;
            }
        }
    };

    public struct carTimeDistance
    {
        public float distance;
        public float time;
        public bool isFinished;
        public int car;

        public carTimeDistance(int _car)
        {
            car = _car;
            time = Mathf.Infinity;
            distance = 0.0f;
            isFinished = false;
        }

        public static IComparer SortCarsByDistance()
        {
            return (IComparer)new SortCarsByDistanceHelper();
        }

        public static IComparer SortCarsByTime()
        {
            return (IComparer)new SortCarsByTimeHelper();
        }
    }

    private class SortCarsByDistanceHelper : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            carTimeDistance cD1 = (carTimeDistance)x;
            carTimeDistance cD2 = (carTimeDistance)y;
            if (cD1.distance < cD2.distance)
            {
                return 1;
            }
            else if (cD1.distance > cD2.distance)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }

    private class SortCarsByTimeHelper : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            carTimeDistance cD1 = (carTimeDistance)x;
            carTimeDistance cD2 = (carTimeDistance)y;
            if (cD1.time < cD2.time)
            {
                return 1;
            }
            else if (cD1.time > cD2.time)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        carBrain = new Brain(layerCounts[0], layerCounts[1], layerCounts[2]);
        if (carBrain.neurons[0].bias == 0 && carBrain.paths[0].neuronInNum == carBrain.paths[0].neuronOutNum)
        {
            for (int i = 0; i < carBrain.neurons.Length; i++)
            {
                carBrain.neurons[i] = new Neuron(0.0f, 1.0f);
            }
            for (int i = 0; i < layerCounts[0]; i++)
            {
                for (int j = 0; j < layerCounts[1]; j++)
                {
                    carBrain.paths[i + j * layerCounts[0]] = new Pathway(i, layerCounts[0] + j, 1.0f);
                }
            }
            for (int i = 0; i < layerCounts[1]; i++)
            {
                for (int j = 0; j < layerCounts[2]; j++)
                {
                    carBrain.paths[layerCounts[0] * layerCounts[1] + i + j * layerCounts[1]] = new Pathway(layerCounts[0] + i, layerCounts[0] + layerCounts[1] + j, 1.0f);
                }
            }
        }
        carTD = new carTimeDistance(0);
    }

    public float[] RunNeuralNetwork(float[] inputValues)
    {
        carBrain.ClearNeuronValues();
        for (int i = 0; i < layerCounts[0]; i++)
        {
            carBrain.neurons[i].value = inputValues[i];
        }

        foreach (var path in carBrain.paths)
        {
            carBrain.neurons[path.neuronOutNum].value += (carBrain.neurons[path.neuronInNum].value + carBrain.neurons[path.neuronInNum].bias) * path.weight;
        }

        float[] returnValues = new float[layerCounts[2]];
        for (int i = 0; i < layerCounts[2]; i++)
        {
            returnValues[i] = carBrain.neurons[layerCounts[0] + layerCounts[1] + i].value;
        }
        return returnValues;
    }

    public void SoftMutateBrain()
    {
        for (int i = 0; i < carBrain.paths.Length; i++)
        {
            int randInt = UnityEngine.Random.Range(0, 2);
            switch (randInt)
            {
                case 0:
                    carBrain.paths[i].weight *= UnityEngine.Random.Range(0.5f, 1.5f);
                    break;
                case 1:
                    carBrain.paths[i].weight += UnityEngine.Random.Range(-1.0f, 1.0f);
                    break;
                default:
                    break;
            }
        }
        for (int i = 0; i < carBrain.neurons.Length; i++)
        {
            int randInt = UnityEngine.Random.Range(0, 2);

            switch (randInt)
            {
                case 0:
                    carBrain.neurons[i].bias *= UnityEngine.Random.Range(0.5f, 1.5f);
                    break;
                case 1:
                    carBrain.neurons[i].bias += UnityEngine.Random.Range(-1.0f, 1.0f);
                    break;
                default:
                    break;
            }
        }
    }

    public void MutateBrain()
    {
        MutateWeights();
        MutateBiases();
    }

    void MutateWeights()
    {
        for (int i = 0; i < carBrain.paths.Length; i++)
        {
            int randInt = UnityEngine.Random.Range(0, 4);

            switch (randInt)
            {
                case 0:
                    carBrain.paths[i].weight = UnityEngine.Random.Range(-10.0f, 10.0f);
                    break;
                case 1:
                    carBrain.paths[i].weight *= UnityEngine.Random.Range(0.25f, 1.75f);
                    break;
                case 2:
                    carBrain.paths[i].weight += UnityEngine.Random.Range(-1.0f, 1.0f);
                    break;
                case 3:
                    carBrain.paths[i].weight *= -1;
                    break;
                default:
                    break;
            }
        }
    }

    void MutateBiases()
    {
        for (int i = 0; i < carBrain.neurons.Length; i++)
        {
            int randInt = UnityEngine.Random.Range(0, 4);

            switch (randInt)
            {
                case 0:

                    carBrain.neurons[i].bias = UnityEngine.Random.Range(-10.0f, 10.0f);
                    break;
                case 1:
                    carBrain.neurons[i].bias *= UnityEngine.Random.Range(0.25f, 1.75f);
                    break;
                case 2:
                    carBrain.neurons[i].bias += UnityEngine.Random.Range(-1.0f, 1.0f);
                    break;
                case 3:
                    carBrain.neurons[i].bias *= -1;
                    break;
                default:
                    break;
            }
        }
    }

    public Brain GetBrain()
    {
        return carBrain;
    }

    public carTimeDistance GetCarTimeDistance()
    {
        return carTD;
    }

    public void SetCarTDCar(int car)
    {
        carTD.car = car;
    }

    public void ReplaceBrain(Brain newBrain)
    {
        carBrain = newBrain;
    }

    private void FixedUpdate()
    {
        if (carTD.distance < 1.0f && !carTD.isFinished)
        {
            float3 points;
            float distance;
            SplineUtility.GetNearestPoint(GameObject.FindGameObjectWithTag("Road Generator").transform.GetChild(0).GetComponent<SplineContainer>().Spline, (float3)(transform.position), out points, out distance);
            carTD.distance = distance;
        }
        else if (carTD.distance >= 1.0f && !carTD.isFinished)
        {
            carTD.distance = 1.0f;
            carTD.time = Time.time - GameObject.FindGameObjectWithTag("Car AI Spawner").GetComponent<NN_Car_Spawner>().GetStartTime();
            carTD.isFinished = true;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
