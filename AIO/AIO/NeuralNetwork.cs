using System;
using System.Collections.Generic;
using System.Text;

namespace AIO
{
    public class NeuralNetwork
    {
        readonly NeuralNetworkParameters param;
        Random rnd = new Random();
        public NeuralNetwork(NeuralNetworkParameters param)
        {
            this.param = param;
        }
        public NeuralNetwork(NeuralNetworkParameters param, Genome champ)
        {
            this.param = param;
            _Champion = champ;
        }

        Genome _Champion = null;
        public Genome Champion { get { return _Champion; } }

        float _ChampionFitness = -10000000f;
        public float ChampionFitness { get { return _ChampionFitness; } }

        uint _Generation = 0;
        public uint Generation { get { return _Generation; } }

        float _GenerationFitness = 0f;
        public float GenerationFitness { get { return _GenerationFitness; } }

        public void TrainGeneration()
        {
            bool champ = _Champion != null;

            Genome[] gen = new Genome[param.PopulationDensity];
            if (champ)
                for (int i = 0; i < param.PopulationDensity; i++) gen[i] = _Champion.DeepClone();
            else
            {
                for (int i = 0; i < param.PopulationDensity; i++)
                {
                    Genome genome = new Genome();
                    genome.Input = new Neuron[param.InputNeurons];
                    for (int j = 0; j < param.InputNeurons; j++)
                    {
                        genome.Input[j] = new Neuron(genome.NeuronIndex, 0);
                        genome.NeuronIndex++;
                    }
                    genome.Output = new Neuron[param.OutputNeurons];
                    for (int j = 0; j < param.OutputNeurons; j++)
                    {
                        genome.Output[j] = new Neuron(genome.NeuronIndex, param.DefaultOutputNeuronThreshold);
                        genome.NeuronIndex++;
                    }
                    gen[i] = genome;
                }
            }

            #region Genome mutations
            for (int j = 0; j < param.PopulationDensity; j++)
            {
                Genome g = gen[j];
                param.Domain.GenomeInitialization(g);

                //Remove neuron
                if (g.HiddenLayers.Count > 0 && param.RemoveNeuronChance > rnd.NextDouble())
                {
                    List<Neuron> layer = g.HiddenLayers[rnd.Next(g.HiddenLayers.Count)];
                    Neuron n = layer[rnd.Next(layer.Count)];

                    foreach (NeuralConnection connection in n.Connections)
                    {
                        g.Connections.Remove(connection);
                        if (connection.Out == n)
                            connection.In.Connections.Remove(connection);
                        else
                            connection.Out.Connections.Remove(connection);
                    }
                    layer.Remove(n);
                    n.Connections = null;
                    if (layer.Count < 1) g.HiddenLayers.Remove(layer);

                    layer = null;
                    n = null;
                }

                //Remove connection
                if (g.Connections.Count > 0 && param.RemoveConnectionChance > rnd.NextDouble())
                {
                    NeuralConnection c = g.Connections[rnd.Next(g.Connections.Count)];
                    g.Connections.Remove(c);
                    c.In.Connections.Remove(c);
                    c.Out.Connections.Remove(c);
                }

                //Add neuron
                if (param.AddNeuronChance > rnd.NextDouble())
                {
                    bool newlayer = (g.HiddenLayers.Count < 1 || param.AddNeuronToNewLayerChance > rnd.NextDouble());
                    List<Neuron> layer = (newlayer ? new List<Neuron>() : g.HiddenLayers[rnd.Next(g.HiddenLayers.Count)]);

                    layer.Add(new Neuron(g.NeuronIndex, param.DefaultHiddenNeuronThreshold));
                    g.NeuronIndex++;

                    if (newlayer) g.HiddenLayers.Add(layer);
                }

                //Add connection
                if (!champ || g.Connections.Count < 1 || param.AddConnectionChance > rnd.NextDouble())
                {
                    for(int k = 0; k < 10; k++)
                    {
                        Neuron From, To;
                        bool last = false;
                        if (g.HiddenLayers.Count < 1 || rnd.Next(g.HiddenLayers.Count) == 0)
                            From = g.Input[rnd.Next(g.Input.Length)];
                        else
                        {
                            int index = rnd.Next(g.HiddenLayers.Count);
                            last = (index == g.HiddenLayers.Count - 1);
                            List<Neuron> layer = g.HiddenLayers[index];
                            From = layer[rnd.Next(layer.Count)];
                        }

                        if (last || g.HiddenLayers.Count < 1 || rnd.Next(g.HiddenLayers.Count) == 0)
                            To = g.Output[rnd.Next(g.Output.Length)];
                        else
                        {
                            List<Neuron> layer = g.HiddenLayers[rnd.Next(g.HiddenLayers.Count)];
                            To = layer[rnd.Next(layer.Count)];
                        }

                        bool flag = false;
                        foreach (NeuralConnection c in g.Connections)
                            if (c.In.ID == From.ID && c.Out.ID == To.ID)
                            {
                                flag = true;
                                break;
                            }

                        if (flag) continue;

                        NeuralConnection connection = new NeuralConnection();
                        connection.In = From;
                        connection.Out = To;
                        connection.Weight = param.DefaultConnectionWeight;
                        if (param.FluctuateNewConnections)
                        {
                            float fluc = (float)(rnd.NextDouble() * (param.WeightFluctuationHigh - param.WeightFluctuationLow) + param.WeightFluctuationLow);
                            if (rnd.Next(2) == 1)
                                connection.Weight += fluc;
                            else
                                connection.Weight -= fluc;
                        }

                        g.Connections.Add(connection);
                        From.Connections.Add(connection);
                        To.Connections.Add(connection);
                        break;
                    }
                }

                //Fluctuate weight
                if (param.WeightFluctuationChance > rnd.NextDouble())
                {
                    int index = rnd.Next(g.Connections.Count);

                    float fluc = (float)(rnd.NextDouble() * (param.WeightFluctuationHigh - param.WeightFluctuationLow) + param.WeightFluctuationLow);
                    if (rnd.Next(2) == 1)
                        g.Connections[index].Weight += fluc;
                    else
                        g.Connections[index].Weight -= fluc;
                }
            }
            #endregion

            param.Domain.GenerationInitialization();
            for (int i = 0; i < param.TestsPerGeneration; i++)
            {
                param.Domain.TestInitialization(i);

                /* Testing */
                foreach (Genome g in gen)
                {
                    float[] input = new float[param.InputNeurons];
                    param.Domain.Input(g, ref input);
                    for (int j = 0; j < param.InputNeurons; j++) g.Input[j].Value = input[j];

                    g.Process();

                    float[] output = new float[param.OutputNeurons];
                    for (int j = 0; j < param.OutputNeurons; j++) output[j] = g.Output[j].Value;
                    param.Domain.Output(g, output);

                    if(i != param.TestsPerGeneration - 1)
                        g.ClearNeurons();
                }
            }

            _Generation++;

            /* Champion selection */
            Genome newchamp = null;
            float tempfit = (param.VolatileChampions ? -10000000 : _ChampionFitness);
            float tempgenfit = 0f;
            foreach (Genome g in gen)
            {
                if (g.Fitness > tempgenfit) tempgenfit = g.Fitness;
                if (g.Fitness <= tempfit) continue;

                newchamp = g;
                tempfit = g.Fitness;
            }

            _GenerationFitness = tempgenfit;

            if (newchamp != null)
            {
                _ChampionFitness = newchamp.Fitness;

                float[] output = new float[param.OutputNeurons];
                for (int j = 0; j < param.OutputNeurons; j++) output[j] = newchamp.Output[j].Value;

                newchamp.Clear();
                _Champion = newchamp;

                if (param.LiveChampionViewer != null)
                {
                    param.LiveChampionViewer.Genome = newchamp;
                    param.LiveChampionViewer.Invalidate();
                }

                param.Domain.ChampionSelected(newchamp, _ChampionFitness, output, _Generation);
            }
        }
    }
}
