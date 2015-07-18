using AIO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TestDomain
{
    class Program
    {
        static void Main(string[] args)
        {
            AIOGenomeViewer viewer = new AIOGenomeViewer();
            NeuralNetwork net = new NeuralNetwork(new NeuralNetworkParameters(2, 1, new XORDomain())
            {
                VolatileChampions = false,
                PopulationDensity = 500,
                TestsPerGeneration = 100,
                //WeightFluctuationChance = 1f,
                WeightFluctuationHigh = 0.5f,
                WeightFluctuationLow = 0.0001f,
                //AddNeuronChance = 0.9f,
                //AddNeuronToNewLayerChance = 0.9f,
                LiveChampionViewer = viewer
            });

            Thread t = new Thread((ThreadStart)delegate
            {
                //while (Console.ReadKey().Key != ConsoleKey.Escape)
                while (!Console.KeyAvailable)
                {
                    Console.WriteLine("Gen={0}, {1:0.00}% correct", net.Generation, net.GenerationFitness);
                    net.TrainGeneration();
                }

                viewer.Invoke((Action)delegate { viewer.Close(); });
            });
            t.Start();

            Application.Run(viewer);
        }

        public delegate void Action();
    }

    class XORDomain : NeuralNetworkTestDomain
    {
        Random rnd;
        int inpt1, inpt2, outpt;
        public override void GenerationInitialization() { rnd = new Random(); }
        public override void TestInitialization(int test)
        {
            inpt1 = (test < 40 ? 1 : 0);
            inpt2 = (test > 20 && test < 80 ? 1 : 0);
            //inpt1 = rnd.Next(2);
            //inpt2 = (inpt1 == 0 ? 1 : rnd.Next(2));
            outpt = inpt1 ^ inpt2;
        }

        public override void GenomeInitialization(Genome g) { }

        public override void Input(Genome g, ref float[] input)
        {
            input[0] = inpt1;
            input[1] = inpt2;
        }

        public override void Output(Genome g, float[] output)
        {
            g.Fitness += 1f - Math.Abs(output[0] - (float)outpt);
        }

        public override void ChampionSelected(Genome g, float fitness, float[] output, uint generation)
        {
            Console.WriteLine("Champion selected : Generation {0} : Fitness {1:0.0000} : {2} ^ {3} = {4}", generation, fitness, inpt1, inpt2, output[0]);
        }
    }
}
