using System;
using System.Collections.Generic;
using Utility;
using System.Text;
using Accord.MachineLearning.Bayes;
using Accord.IO;
using Accord.MachineLearning.Performance;
using Accord.MachineLearning;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Analysis;

namespace NBayes
{
    class Program
    {
        static void Main(string[] args)
        {
            /* 
             * Takes a csv files as input and trains a naive bayes classfier, if the test flag is set the rountine
             * will calculate the accuracy of the input files using the previous saved model in the exeution directioy
             * If the test flag is set a new classifier is not trainied
             * but the previous model is loaded and used agains the test data.
             * 
             * arg 1 = training file or test file
             * arg 2 = label file
             * arg 3 = test flag (-s or -S)
             * arg 4 = Specify file name of model file
             */

            const int minargs = 2;
            const int maxargs = 4;
            const int Folds = 4;
            Accord.Math.Random.Generator.Seed = 0;
            string trainingFname = null;
            string labelFname = null;
            string modelFname = "NBmodel.sav"; // Default model file name
            bool NoTrain = false;
            Functions.Welcome();
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
                if (args[2] == ("-s") | args[2] == ("-S"))
                {
                    NoTrain = true;
                    trainingFname = args[0];
                    labelFname = args[1];
                }
                else
                {
                    Console.WriteLine(Strings.resources.usage);
                    System.Environment.Exit(1);
                }
            }

            if (numArgs == 4)
            {
                NoTrain = true;
                trainingFname = args[0];
                labelFname = args[1];
                modelFname = args[3];
            }
            //
            // Check if the training and label files exist and are not locked by anohter process
            //

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

            //
            // Read in the training and label files, CSV format
            //
            CsvReader training_samples = new CsvReader(trainingFname, false);
            int[,] MatrixIn = training_samples.ToMatrix<int>();
            int[][] trainingset = Functions.convertToJaggedArray(MatrixIn);
                        
            //
            // Naive Bayes gets trained on integer arrays or arrays of "strings"
            //
            CsvReader label_samples = new CsvReader(labelFname, false);
            
            int[,] labelsIn = label_samples.ToMatrix<int>(); // COnvert the labels to a matrix and then to jagged array
            int[][] LabelSet = Functions.convertToJaggedArray (labelsIn);
            int[] output = Functions.convertTointArray(LabelSet);

            NaiveBayes loaded_nb;   // setup for loading a trained model if one exists
            if (!NoTrain)
            {
                // Create a new Naive Bayes learning instance
                var learner = new NaiveBayesLearning();

                // Create a Naive Bayes classifier and train with the input datasets
                NaiveBayes classifier = learner.Learn(trainingset, output);

                /* Cross-validation is a technique for estimating the performance of a predictive model. 
                 * It can be used to measure how the results of a statistical analysis will generalize to 
                 * an independent data set. It is mainly used in settings where the goal is prediction, and 
                 * one wants to estimate how accurately a predictive model will perform in practice.
                 * 
                 * One round of cross-validation involves partitioning a sample of data into complementary
                 * subsets, performing the analysis on one subset (called the training set), and validating
                 * the analysis on the other subset (called the validation set or testing set). To reduce
                 * variability, multiple rounds of cross-validation are performed using different partitions,
                 * and the validation results are averaged over the rounds
                 */
                
                // Gets results based on performing a k-fold cross validation based on the input training set
                // Create a cross validation instance
                
                
                var cv = CrossValidation.Create(k: Folds, learner: (p) => new NaiveBayesLearning(),
                    loss: (actual, expected, p) => new ZeroOneLoss(expected).Loss(actual),
                    fit: (teacher, x, y, w) => teacher.Learn(x, y, w),
                    x: trainingset, y: output);
                                
                var result = cv.Learn(trainingset, output);
                
                Console.WriteLine("Performing n-fold cross validation where n = {0}", cv.K);

                // We can grab some information about the problem:
                Console.WriteLine("Cross Validation Results");
                Console.WriteLine("     number of samples {0}", result.NumberOfSamples);
                Console.WriteLine("     number of features: {0}", result.NumberOfInputs);
                Console.WriteLine("     number of outputs {0}", result.NumberOfOutputs);
                Console.WriteLine("     Training Error: {0}", result.Training.Mean); // should be 0 or no
                Console.WriteLine("     Validation Mean: {0}\n", result.Validation.Mean);
                
                Console.WriteLine("Creating General Confusion Matrix from Cross Validation");
                GeneralConfusionMatrix gcm = result.ToConfusionMatrix(trainingset,output);
                double accuracy = gcm.Accuracy; // should be 0.625
                Console.WriteLine(" GCM Accuracy {0}\n", accuracy*100);


                ConfusionMatrix cm = ConfusionMatrix.Estimate(classifier, trainingset, output);
                Console.WriteLine("Confusion Error {0}", cm.Error);
                Console.WriteLine("Confusion accuracy {0}", cm.Accuracy);
                double tp = cm.TruePositives;
                double tn = cm.TrueNegatives;
                double fscore = cm.FScore;
                double fp = cm.FalsePositives;
                double fn = cm.FalseNegatives;
                Console.WriteLine("TP = {0},TN = {1}, FP = {2}, FN = {3}, Fscore = {4} ", tp, tn, fp, fn, fscore);
               
                
                // Save the model created from the training set
                                
                classifier.Save("NBmodel.sav", compression: SerializerCompression.None);
                Console.WriteLine("Successfully saved the model");

            }
            else
            {
                // load a previous model
                loaded_nb = Serializer.Load<NaiveBayes>(modelFname); // Load the model
                int[] results = loaded_nb.Decide(trainingset); // Make preditions from the input
                double accuracy = Functions.CalculateAccuraccy(output, results);
                Console.WriteLine ("Accuracy of predictions = {0}%", Math.Round (accuracy * 100, 2)); // Compare the predicions to the labels
            }
            
        }
        

    }
}
