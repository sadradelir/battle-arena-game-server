using System;
using System.Data.SqlTypes;
using System.IO;
using System.ServiceModel.Configuration;
using MobarezooServer.Utilities;

namespace MobarezooServer.AI
{
    public class NlNetwork
    {
        private int inputsCount;
        private int hiddensCount;
        private int outputsCount;
        public Matrix w0;
        public Matrix w1;
        public Matrix bias0;
        public Matrix bias1;
        
        public Matrix hiddenMatrix;
        public Matrix inputsMatrix;
        public NlNetwork(int inputsCount, int hiddensCount, int outputsCount)
        {
            this.inputsCount = inputsCount;
            this.hiddensCount = hiddensCount;
            this.outputsCount = outputsCount;
            bias0 = new Matrix(1, hiddensCount);
            bias1 = new Matrix(1, outputsCount);
            bias0.Randomize();
            bias1.Randomize();
            
            w0 = new Matrix(inputsCount, hiddensCount);
            w1 = new Matrix(hiddensCount, outputsCount);
            w0.Randomize();
            w1.Randomize();
            hiddenMatrix = new Matrix(1 , hiddensCount);
            inputsMatrix = new Matrix(1 , inputsCount);
            
        }
        
        public NlNetwork(NlNetwork n1 , NlNetwork n2)
        {
            this.inputsCount = n1.inputsCount;
            this.hiddensCount = n1.hiddensCount;
            this.outputsCount = n1.outputsCount;
            bias0 = new Matrix(1, hiddensCount);
            bias1 = new Matrix(1, outputsCount);

            w0 = new Matrix(inputsCount, hiddensCount);
            w1 = new Matrix(hiddensCount, outputsCount);

            var r = new Random(IdGenerator.Seed());
            for (int i = 0; i < w0.N; i++)
            {
                for (int j = 0; j < w0.M; j++)
                {
                    if (r.Next( 1000) <  5)
                    {
                        w0[i, j] = r.NextDouble() * 2 - 1;
                    }
                    else
                    {
                        w0[i, j] = r.Next(2) > 0 ? n1.w0[i , j] : n2.w0[i , j];
                    }
                }
            }
            for (int i = 0; i < w1.N; i++)
            {
                for (int j = 0; j < w1.M; j++)
                {
                    if (r.Next( 1000) <  5)
                    {
                        w1[i, j] = r.NextDouble() * 2 - 1;
                    }
                    else
                    {
                        w1[i, j] = r.Next(2) > 0 ? n1.w1[i , j] : n2.w1[i , j];
                    }
                }
            }
            
            for (int j = 0; j < hiddensCount; j++)
            {
                if (r.Next( 1000) <  5)
                {
                    bias0[0, j] = r.NextDouble() * 2 - 1;
                }
                else
                {
                    bias0[0, j] = r.Next(2) > 0 ? n1.bias0[0, j] : n2.bias0[0, j];
                }
            }
            for (int j = 0; j < outputsCount; j++)
            {
                if (r.Next( 1000) <  5)
                {
                    bias1[0, j] = r.NextDouble() * 2 - 1;
                }
                else
                {
                    bias1[0, j] = r.Next(2) > 0 ? n1.bias1[0, j] : n2.bias1[0, j];
                }
            }
            
            hiddenMatrix = new Matrix(1 , hiddensCount);
            inputsMatrix = new Matrix(1 , inputsCount);
            
        }

        // mutate
         public NlNetwork(NlNetwork n1 )
        {
            this.inputsCount = n1.inputsCount;
            this.hiddensCount = n1.hiddensCount;
            this.outputsCount = n1.outputsCount;
            bias0 = new Matrix(1, hiddensCount);
            bias1 = new Matrix(1, outputsCount);

            w0 = new Matrix(inputsCount, hiddensCount);
            w1 = new Matrix(hiddensCount, outputsCount);

            var r = new Random(IdGenerator.Seed());
            for (int i = 0; i < w0.N; i++)
            {
                for (int j = 0; j < w0.M; j++)
                {
                    if (r.Next( 1000) < 20)
                    {
                        w0[i, j] = r.NextDouble() * 2 - 1;
                    }
                    else
                    {
                        w0[i, j] = n1.w0[i , j];
                    }
                }
            }
            for (int i = 0; i < w1.N; i++)
            {
                for (int j = 0; j < w1.M; j++)
                {
                    if (r.Next( 1000) <  20)
                    {
                        w1[i, j] = r.NextDouble() * 2 - 1;
                    }
                    else
                    {
                        w1[i, j] = n1.w1[i , j];
                    }
                }
            }
            
            for (int j = 0; j < hiddensCount; j++)
            {
                if (r.Next( 1000) <  20)
                {
                    bias0[0, j] = r.NextDouble() * 2 - 1;
                }
                else
                {
                    bias0[0, j] =  n1.bias0[0, j] ;
                }
            }
            for (int j = 0; j < outputsCount; j++)
            {
                if (r.Next( 1000) <  20)
                {
                    bias1[0, j] = r.NextDouble() * 2 - 1;
                }
                else
                {
                    bias1[0, j] = n1.bias1[0, j] ;
                }
            }
            
            hiddenMatrix = new Matrix(1 , hiddensCount);
            inputsMatrix = new Matrix(1 , inputsCount);
            
        }

        
 
        public Matrix feedForward(double[] inputsMat)
        {
            inputsMatrix = new Matrix(inputsMat);
            
            hiddenMatrix = inputsMatrix * w0;
            hiddenMatrix = hiddenMatrix + bias0;
            hiddenMatrix.SigmoidAll(); 
            
            Matrix outputsMat = hiddenMatrix * w1;
            outputsMat = outputsMat + bias1;
            //outputsMat.SigmoidAll();
            return outputsMat;    
        }

        public void train(double[] inputArray, double[] target)
        {
            var outputs = feedForward(inputArray);
            var targetsMat = new Matrix(target);
            
            var outputErrors = targetsMat - outputs;
            outputs.DeriveAll();
            var outputDeltas = outputErrors & outputs;

            var w1T = w1.Transpose();
            var hiddenErrors = outputDeltas * w1T;
            var hiddenDerivs = (Matrix)(hiddenMatrix.Clone());
            hiddenDerivs.DeriveAll();
            var hidenDeltas = hiddenErrors & hiddenDerivs;
            
            w1 = w1 + (hiddenMatrix.Transpose() * outputDeltas);
            w0 = w0 + (inputsMatrix.Transpose() * hidenDeltas);

            bias1 = bias1 + outputDeltas;
            bias0 = bias0 + hidenDeltas;

        }
        
        public void writeToFile( int index  = 0)
        {
            var w0S = w0.getSerialized();
            var w1S = w1.getSerialized();
            var b0S = bias0.getSerialized();
            var b1S = bias1.getSerialized();

            if (index == 0)
            {
                File.WriteAllText(@"../w0.txt", w0S);
                File.WriteAllText(@"../w1.txt", w1S);
                File.WriteAllText(@"../b0.txt", b0S);
                File.WriteAllText(@"../b1.txt", b1S);
            }
            else
            {
                File.WriteAllText(@"../BOA/w0.txt", w0S);
                File.WriteAllText(@"../BOA/w1.txt", w1S);
                File.WriteAllText(@"../BOA/b0.txt", b0S);
                File.WriteAllText(@"../BOA/b1.txt", b1S);
                File.WriteAllText(@"../BOA/time.txt", System.DateTime.Now.ToString());
            }

        }
        
        public void readFromFile()
        {
            var w0S = File.ReadAllText(@"../BOA/w0.txt");
            var w1S = File.ReadAllText(@"../BOA/w1.txt");
            var b0S = File.ReadAllText(@"../BOA/b0.txt");
            var b1S = File.ReadAllText(@"../BOA/b1.txt");
            
             w0.readSerialized(w0S);
             w1.readSerialized(w1S);
             bias0.readSerialized(b0S);
             bias1.readSerialized(b1S);
        }

    }
}