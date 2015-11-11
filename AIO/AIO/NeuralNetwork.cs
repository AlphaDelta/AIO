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

            for (int i = 0; i < param.MutationsPerGeneration; i++)
            {
                #region Genome mutations
                for (int j = 0; j < param.PopulationDensity; j++)
                {
                    Genome g = gen[j];
                    param.Domain.GenomeInitialization(g);

                    int force = (param.ForceComplexify ? rnd.Next(4) : -1);

                    //Remove neuron
                    if (g.HiddenLayers.Count > 0 && param.RemoveNeuronChance > (float)rnd.NextDouble())
                    {
                        List<Neuron> layer = g.HiddenLayers[rnd.Next(g.HiddenLayers.Count)];
                        if (layer.Count < 1)
                            g.HiddenLayers.Remove(layer);
                        else
                        {
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
                    }

                    //Remove connection
                    if (g.Connections.Count > 0 && param.RemoveConnectionChance > (float)rnd.NextDouble())
                    {
                        NeuralConnection c = g.Connections[rnd.Next(g.Connections.Count)];
                        g.Connections.Remove(c);
                        c.In.Connections.Remove(c);
                        c.Out.Connections.Remove(c);

                        List<List<Neuron>> layerstoremove = new List<List<Neuron>>();
                        foreach (List<Neuron> layer in g.HiddenLayers)
                        {
                            List<Neuron> toremove = new List<Neuron>();
                            foreach (Neuron n in layer)
                                if (n.Connections.Count < 1) toremove.Add(n);
                            foreach (Neuron n in toremove)
                                layer.Remove(n);
                            if (layer.Count < 1) layerstoremove.Add(layer);
                        }
                        foreach (List<Neuron> layer in layerstoremove)
                            g.HiddenLayers.Remove(layer);
                    }

                    //Add neuron
                    float rand = (float)rnd.NextDouble();
                    bool fhlayer = false;
                    if (force == 0 || param.AddNeuronChance > rand || (fhlayer = (g.HiddenLayers.Count < 1 && param.ForceHiddenLayer)))
                    {
                        bool newlayer = (g.HiddenLayers.Count < 1 || param.AddNeuronToNewLayerChance > (float)rnd.NextDouble());
                        int index = rnd.Next(g.HiddenLayers.Count);
                        List<Neuron> layer = (newlayer || fhlayer ? new List<Neuron>() : g.HiddenLayers[index]);

                        Neuron newn = new Neuron(g.NeuronIndex, param.DefaultHiddenNeuronThreshold);
                        layer.Add(newn);
                        g.NeuronIndex++;

                        NeuralConnection cin = new NeuralConnection();
                        cin.In = newn;
                        cin.Weight = param.DefaultConnectionWeight;
                        NeuralConnection cout = new NeuralConnection();
                        cout.Out = newn;
                        cout.Weight = param.DefaultConnectionWeight;
                        if (newlayer)
                            g.HiddenLayers.Add(layer);

                        if (newlayer || index == g.HiddenLayers.Count - 1 || rnd.Next(g.HiddenLayers.Count) == 0)
                            cin.Out = g.Output[rnd.Next(g.Output.Length)];
                        else
                        {
                            List<Neuron> layercin = g.HiddenLayers[rnd.Next(index + 1, g.HiddenLayers.Count)];
                            cin.Out = layercin[rnd.Next(layercin.Count)];
                        }

                        if (index == 0 || rnd.Next(g.HiddenLayers.Count) == 0)
                            cout.In = g.Input[rnd.Next(g.Input.Length)];
                        else
                        {
                            List<Neuron> layercout = g.HiddenLayers[rnd.Next(0, index)];
                            cout.In = layercout[rnd.Next(layercout.Count)];
                        }

                        if (param.FluctuateNewConnections)
                        {
                            float fluc = (float)(rnd.NextDouble() * (param.WeightFluctuationHigh - param.WeightFluctuationLow) + param.WeightFluctuationLow);
                            float fluc2 = (float)(rnd.NextDouble() * (param.WeightFluctuationHigh - param.WeightFluctuationLow) + param.WeightFluctuationLow);
                            if (rnd.Next(2) == 1)
                                cin.Weight += fluc;
                            else
                                cin.Weight -= fluc;
                            if (rnd.Next(2) == 1)
                                cout.Weight += fluc2;
                            else
                                cout.Weight -= fluc2;
                        }

                        newn.Connections.Add(cin);
                        newn.Connections.Add(cout);
                        cin.Out.Connections.Add(cin);
                        cout.In.Connections.Add(cout);

                        g.Connections.Add(cin);
                        g.Connections.Add(cout);
                    }

                    //Add connection
                    if (!champ || g.Connections.Count < 1 || force == 1 || param.AddConnectionChance > (float)rnd.NextDouble())
                    {
                        for (int k = 0; k < 10; k++)
                        {
                            Neuron From, To;
                            bool last = false;
                            int index = -1;
                            if (g.HiddenLayers.Count < 1 || rnd.Next(g.HiddenLayers.Count) == 0)
                                From = g.Input[rnd.Next(g.Input.Length)];
                            else
                            {
                                index = rnd.Next(g.HiddenLayers.Count);
                                last = (index == g.HiddenLayers.Count - 1);
                                List<Neuron> layer = g.HiddenLayers[index];
                                From = layer[rnd.Next(layer.Count)];
                            }

                            if (last || g.HiddenLayers.Count < 1 || rnd.Next(g.HiddenLayers.Count) == 0)
                                To = g.Output[rnd.Next(g.Output.Length)];
                            else
                            {
                                List<Neuron> layer = g.HiddenLayers[rnd.Next(index + 1, g.HiddenLayers.Count)];
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
                    if (force == 2 || param.WeightFluctuationChance > (float)rnd.NextDouble())
                    {
                        int index = rnd.Next(g.Connections.Count);

                        float fluc = (float)(rnd.NextDouble() * (param.WeightFluctuationHigh - param.WeightFluctuationLow) + param.WeightFluctuationLow);
                        if (rnd.Next(2) == 1)
                            g.Connections[index].Weight += fluc;
                        else
                            g.Connections[index].Weight -= fluc;
                    }

                    //Fluctuate threshold
                    if (g.HiddenLayers.Count > 0 && (force == 3 || param.ThresholdFluctuationChance > (float)rnd.NextDouble()))
                    {
                        List<Neuron> hlayer = g.HiddenLayers[rnd.Next(g.HiddenLayers.Count)];
                        int index = rnd.Next(hlayer.Count);

                        float fluc = (float)(rnd.NextDouble() * (param.ThresholdFluctuationHigh - param.ThresholdFluctuationLow) + param.ThresholdFluctuationLow);
                        if (hlayer[index].Threshold <= 0 || rnd.Next(2) == 1)
                            hlayer[index].Threshold += fluc;
                        else
                            hlayer[index].Threshold -= fluc;

                        if (hlayer[index].Threshold < 0) hlayer[index].Threshold = 0;
                    }
                }
                #endregion
            }

            param.Domain.GenerationInitialization();
            for (int i = 0; i < param.TestsPerGeneration; i++)
            {
                param.Domain.TestInitialization(i, (int)_Generation, _GenerationFitness);

                /* Testing */
                foreach (Genome g in gen)
                {
                    if (g.Exhausted) continue;

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
                newchamp.CleanLayers();
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
