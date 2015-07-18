using System;
using System.Collections.Generic;
using System.Text;

namespace AIO
{
    public class NeuralNetworkTestDomain
    {
        public virtual void GenerationInitialization() { }
        public virtual void TestInitialization(int test, int generation, float fitness) { }
        public virtual void GenomeInitialization(Genome g) { }
        public virtual void ChampionSelected(Genome g, float fitness, float[] output, uint generation) { }

        public virtual void Input(Genome g, ref float[] input) { }
        public virtual void Output(Genome g, float[] output) { }
    }
}
