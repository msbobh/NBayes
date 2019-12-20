using System;
using System.IO;
using Accord.Math;

namespace Utility
{
    class Functions
    {
        static public int parseCommandLine(string[] cLine, int maxArgs, int minArgs)
        {
            int numArgs = cLine.Length;
            if (numArgs > maxArgs | numArgs < minArgs)
            {
                return 0;
            }
            switch (numArgs)
            {
                case 1:
                    return 1;

                case 2:
                    return 2;
                case 3:
                    return 3;
                case 4:
                    return 4;

                default:
                    return 0;

            }

        }
        static public bool checkFile(string fname)
        {
            try
            {
                FileStream fs = File.Open(fname, FileMode.Open, FileAccess.Write, FileShare.None);
                fs.Close();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        static public double[][] convertToJaggedArray(double[,] multiArray)
        {
            int numOfColumns = multiArray.Columns();
            int numOfRows = multiArray.Rows();

            double[][] jaggedArray = new double[numOfRows][];

            for (int r = 0; r < numOfRows; r++)
            {
                jaggedArray[r] = new double[numOfColumns];
                for (int c = 0; c < numOfColumns; c++)
                {
                    jaggedArray[r][c] = multiArray[r, c];
                }
            }

            return jaggedArray;
        }

        static public int[][] convertToJaggedArray(int[,] multiArray)
        {
            int numOfColumns = multiArray.Columns();
            int numOfRows = multiArray.Rows();

            int[][] jaggedArray = new int[numOfRows][];

            for (int r = 0; r < numOfRows; r++)
            {
                jaggedArray[r] = new int[numOfColumns];
                for (int c = 0; c < numOfColumns; c++)
                {
                    jaggedArray[r][c] = multiArray[r, c];
                }
            }

            return jaggedArray;
        }

        static public int[] convertTointArray(int[][] multiArray)
        {
            int numOfColumns = multiArray.Columns();
            int numOfRows = multiArray.Rows();

            int[] returnArray = new int[numOfRows];
            for (int r = 0; r < numOfRows; r++)
            {
                returnArray[r] = multiArray[r][0];
            }

            return returnArray;
        }

    }

   
}