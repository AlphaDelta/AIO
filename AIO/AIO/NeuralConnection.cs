using System;
using System.Collections.Generic;
using System.Text;

namespace AIO
{
    [Serializable]
    public class NeuralConnection
    {
        public float Weight;
        public Neuron In, Out;
    }
}
