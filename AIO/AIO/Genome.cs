using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace AIO
{
    [Serializable]
    public class Genome
    {
        public uint NeuronIndex = 0;
        public Neuron[] Input, Output;
        public List<List<Neuron>> HiddenLayers = new List<List<Neuron>>();
        public List<NeuralConnection> Connections = new List<NeuralConnection>();

        public float Fitness = 0;
        public object[] Memory;

        public void Process()
        {
            foreach (Neuron n in Input)
                foreach (NeuralConnection c in n.Connections)
                    c.Out.Value += n.Value * c.Weight;

            foreach (List<Neuron> layer in HiddenLayers)
                foreach (Neuron n in Input)
                    foreach (NeuralConnection c in n.Connections)
                    {
                        if (c.Out == n) continue;

                        c.Out.Value += n.Value * c.Weight;
                    }
        }

        public void Clear()
        {
            Memory = null;
            Fitness = 0;

            ClearNeurons();
        }

        public void ClearNeurons()
        {
            foreach (Neuron n in Input) n.Value = 0;
            foreach (Neuron n in Output) n.Value = 0;
            foreach (List<Neuron> layer in HiddenLayers)
                foreach (Neuron n in layer) n.Value = 0;
        }

        public Genome DeepClone()
        {
            /*using (MemoryStream stream = new MemoryStream()) //Old extremely slow deep-clone method
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
                stream.Position = 0;
                return (Genome)formatter.Deserialize(stream);
            }*/

            Genome g = new Genome();
            g.NeuronIndex = this.NeuronIndex;
            g.Input = new Neuron[this.Input.Length];
            g.HiddenLayers = new List<List<Neuron>>(this.HiddenLayers.Capacity);
            g.Output = new Neuron[this.Output.Length];

            for (int i = 0; i < this.Input.Length; i++)
                g.Input[i] = new Neuron(this.Input[i].ID, this.Input[i].Threshold);

            for (int i = 0; i < this.HiddenLayers.Count; i++)
            {
                List<Neuron> layer = new List<Neuron>(this.HiddenLayers[i].Capacity);
                for (int j = 0; j < this.Output.Length; j++)
                    layer.Add(new Neuron(this.HiddenLayers[i][j].ID, this.HiddenLayers[i][j].Threshold));
                g.HiddenLayers.Add(layer);
            }

            for (int i = 0; i < this.Output.Length; i++)
                g.Output[i] = new Neuron(this.Output[i].ID, this.Output[i].Threshold);

            foreach (NeuralConnection connection in this.Connections)
            {
                Neuron from = null, to = null;
                foreach (Neuron n in g.Input)
                    if (connection.In.ID == n.ID) from = n;
                foreach (List<Neuron> layer in g.HiddenLayers)
                    foreach (Neuron n in layer)
                    {
                        if (connection.In.ID == n.ID) from = n;
                        else if (connection.Out.ID == n.ID) to = n;

                        if (from != null && to != null) break;
                    }
                if (to == null)
                    foreach (Neuron n in g.Output)
                        if (connection.Out.ID == n.ID) to = n;

                NeuralConnection newc = new NeuralConnection();
                newc.In = from;
                newc.Out = to;
                newc.Weight = connection.Weight;

                g.Connections.Add(newc);
                from.Connections.Add(newc);
                to.Connections.Add(newc);
            }

            return g;
        }
    }
}
