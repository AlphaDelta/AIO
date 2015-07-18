using System;
using System.Collections.Generic;
using System.Text;

namespace AIO
{
    [Serializable]
    public class Neuron
    {
        public uint ID;
        public float Value = 0;
        public float Threshold;
        public List<NeuralConnection> Connections = new List<NeuralConnection>();

        public Neuron(uint ID, float Threshold)
        {
            this.ID = ID;
            this.Threshold = Threshold;
        }
    }
}
