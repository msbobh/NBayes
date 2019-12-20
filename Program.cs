using System;
using System.Collections.Generic;
using Utility;
using System.Text;
using Accord.MachineLearning.Bayes;
using Accord.IO;
using Accord.MachineLearning.Performance;
using Accord.MachineLearning;
using Accord.Math.Optimization.Losses;

namespace NBayes
{
    class Program
    {
        static void Main(string[] args)
        {
            /* 
             * Got basic Nbayes running, need to add command line stuff, prediciton routine (separte program)
             * 
             */

            const int minargs = 2;
            const int maxargs = 3;
            string trainingFname = null;
            string labelFname = null;

            int numArgs = Functions.parseCommandLine (args, maxargs, minargs);
            if (numArgs == 0)
            {
                Console.WriteLine(Strings.resources.usage);
                System.Environment.Exit(1);
            }

            if (numArgs == 2)
            {
                trainingFname = args[0];
                labelFname = args[1];

            }
            if (numArgs == 3) // no use for third parameter yet!
            {
                Console.WriteLine(Strings.resources.usage);
                System.Environment.Exit(1);
            }

            // Check if the files exist and are not locked
            if (!Utility.Functions.checkFile(trainingFname))
            {
                Console.WriteLine("Error opening file{0}", trainingFname);
                System.Environment.Exit(1);

            }
            if (!Functions.checkFile(labelFname))
            {
                Console.WriteLine("Error opening file {0}", labelFname);
                System.Environment.Exit(1);
            }

            // Read in the training and label files
            CsvReader training_samples = new CsvReader(trainingFname, false);
            int[,] MatrixIn = training_samples.ToMatrix<int>();
            int[][] trainingset = Functions.convertToJaggedArray(MatrixIn);
                        
            // Naive Bayes gets trained on integer arrays or arrays of "strings"
            CsvReader label_samples = new CsvReader(labelFname, false);
            
            int[,] labelsIn = label_samples.ToMatrix<int>(); // Need to figure out what i want here
            int[][] LabelSet = Functions.convertToJaggedArray (labelsIn);
            int[] output = Functions.convertTointArray(LabelSet);

            // Create a new Naive Bayes learning
            var learner = new NaiveBayesLearning();

            // Learn a Naive Bayes model from the examples
            NaiveBayes nb = learner.Learn(trainingset, output);

            // Need some stats or predictions
            var cv = CrossValidation.Create(k: 4, learner: (p) => new NaiveBayesLearning(),
                loss: (actual, expected, p) => new ZeroOneLoss(expected).Loss(actual),
                fit: (teacher, x, y, w) => teacher.Learn(x, y, w),
                x: trainingset, y: output);

            var result = cv.Learn(trainingset, output);
            // We can grab some information about the problem:
            Console.WriteLine(" number of samples {0}",result.NumberOfSamples); 
            Console.WriteLine(" number of features: {0}",result.NumberOfInputs);
            Console.WriteLine(" number of outputs {0}", result.NumberOfOutputs);

            Console.WriteLine("Training Error: {0}",result.Training.Mean); // should be 0
            Console.WriteLine("Validation Mean: {0}",result.Validation.Mean);
            nb.Save("NBmodel", compression: SerializerCompression.None);
        }
        

    }
}
