using System;
using System.Collections.Generic;
using System.Text;

namespace AIO
{
    public class NeuralNetworkParameters
    {
        public bool
        VolatileChampions = true, //Generation dependent champions, just like actual evolution
        FluctuateNewConnections = true,
        ForceComplexify = false; //At least 1 complexify mutation must happen with each mutation loop

        public uint
        InputNeurons = 0,
        OutputNeurons = 0,
        TestsPerGeneration = 10,
        MutationsPerGeneration = 1,
        PopulationDensity = 100;

        public float
        DefaultHiddenNeuronThreshold = 0f,
        DefaultOutputNeuronThreshold = 0f,
        DefaultConnectionWeight = 1f,
        WeightFluctuationHigh = 0.1f, //Fluctuation is +- n, high 0.1 and low 0.01 make the range from 0.1 to 0.01 and -0.01 to -0.1
        WeightFluctuationLow = 0.01f,
        WeightFluctuationChance = 0.1f, //Chances range from 1 (being 100%) to 0 (being 0%)
        AddNeuronChance = 0.002f,
        AddNeuronToNewLayerChance = 0.1f, //This is after AddNeuronChance
        RemoveNeuronChance = 0.001f,
        AddConnectionChance = 0.01f,
        RemoveConnectionChance = 0.005f;

        public NeuralNetworkTestDomain Domain;

        public AIOGenomeViewer LiveChampionViewer = null;

        public NeuralNetworkParameters(uint InputNeurons, uint OutputNeurons, NeuralNetworkTestDomain Domain)
        {
            if (InputNeurons < 0) throw new Exception("Input neurons must be larger than 0");
            if (OutputNeurons < 0) throw new Exception("Output neurons must be larger than 0");

            this.InputNeurons = InputNeurons;
            this.OutputNeurons = OutputNeurons;
            this.Domain = Domain;
        }
    }
}
