using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class NN_Car_Brain : MonoBehaviour
{
    static readonly int[] layerCounts = { 9, 7, 5 };
    const int brainCount = 1;

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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
