using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class NN_Car_Brain : MonoBehaviour
{
    static readonly int[] layerCounts = { 9, 7, 5 };

    Brain carBrain;

    struct Neuron
    {
        public float value;
        public float bias;

        public Neuron(float _value, float _bias)
        {
            value = _value;
            bias = _bias;
        }
    };

    struct Pathway
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

    struct Brain
    {
        public Neuron[] neurons;

        public Pathway[] paths;

        public Brain(int a, int b, int c)
        {
            neurons = new Neuron[a + b + c];
            paths = new Pathway[a * b + b * c];
        }

        public void ClearNeuronValues()
        {
            for (int i = 0; i < (layerCounts[1] + layerCounts[2]); i++)
            {
                neurons[layerCounts[0] + i].value = 0.0f;
            }
        }
    };

    // Start is called before the first frame update
    void Start()
    {
        carBrain = new Brain(layerCounts[0], layerCounts[1], layerCounts[2]);
        if (carBrain.neurons[0].bias == 0 && carBrain.paths[0].neuronInNum == carBrain.paths[0].neuronOutNum)
        {
            for (int i = 0; i < carBrain.neurons.Length; i++)
            {
                carBrain.neurons[i] = new Neuron(0.0f, 1.0f);
            }
            for (int i = 0; i < layerCounts[1]; i++)
            {
                for (int j = 0; j < layerCounts[0]; j++)
                {
                    carBrain.paths[i * layerCounts[0] + j] = new Pathway(j, i, 0.5f);
                }
            }
        }

        //transform.position = GameObject.FindGameObjectWithTag("Road Generator").GetComponent<PR_Road_Gen>().GetSplinePoints;
    }

    private void FixedUpdate()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
