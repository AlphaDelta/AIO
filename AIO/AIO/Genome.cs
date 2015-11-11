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

        public bool Exhausted = false;

        public void Test(float[] input, out float[] output, bool clear = true)
        {
            for (int j = 0; j < this.Input.Length; j++) this.Input[j].Value = input[j];

            this.Process();

            output = new float[this.Output.Length];
            for (int j = 0; j < this.Output.Length; j++) output[j] = this.Output[j].Value;

            if (clear) this.ClearNeurons();
        }

        public void Process()
        {
            foreach (Neuron n in Input)
                foreach (NeuralConnection c in n.Connections)
                    c.Out.Value += n.Value * c.Weight;

            foreach (List<Neuron> layer in HiddenLayers)
                foreach (Neuron n in Input)
                {
                    if (n.Value < n.Threshold) continue;
                    foreach (NeuralConnection c in n.Connections)
                    {
                        if (c.Out == n) continue;

                        c.Out.Value += n.Value * c.Weight;
                    }
                }
        }

        public void Clear()
        {
            Memory = null;
            Fitness = 0;
            Exhausted = false;

            ClearNeurons();
        }

        public void ClearNeurons()
        {
            foreach (Neuron n in Input) n.Value = 0;
            foreach (Neuron n in Output) n.Value = 0;
            foreach (List<Neuron> layer in HiddenLayers)
                foreach (Neuron n in layer) n.Value = 0;
        }

        public void CleanLayers() //Cleans the hidden layers of any neurons with either no input or no output.
        {
            List<List<Neuron>> htoremove = new List<List<Neuron>>();
            foreach (List<Neuron> hlayer in HiddenLayers)
            {
                List<Neuron> toremove = new List<Neuron>();
                foreach (Neuron n in hlayer)
                {
                    bool inpt = false, outp = false;
                    foreach (NeuralConnection c in n.Connections)
                    {
                        if (!inpt && c.Out == n) inpt = true;
                        if (!outp && c.In == n) outp = true;

                        if (inpt && outp) break;
                    }

                    if (inpt && outp) continue;

                    toremove.Add(n);

                    foreach (NeuralConnection c in n.Connections)
                    {
                        Connections.Remove(c);

                        if(c.In != n) c.In.Connections.Remove(c);
                        if (c.Out != n) c.Out.Connections.Remove(c);
                    }
                }

                foreach (Neuron n in toremove)
                    hlayer.Remove(n);

                if (hlayer.Count < 1)
                    htoremove.Add(hlayer);
            }

            foreach (List<Neuron> hlayer in htoremove)
                HiddenLayers.Remove(hlayer);
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
                for (int j = 0; j < this.HiddenLayers[i].Count; j++)
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
