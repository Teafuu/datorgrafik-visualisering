﻿using KnnConsoleAppForUnity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

namespace Assets.KNNAlgorithm
{
    public class KNNController
    {

        private static readonly Lazy<KNNController> lazy = new Lazy<KNNController>(() => new KNNController());
        public static KNNController Instance { get { return lazy.Value; } }

        public DataSet TrainingData { get; set; }
        public DataSet UnkownData { get; set; }

        public DataSet ActiveData { get; set; }
        public List<string[]> data { get; set; } = new List<string[]>();
        public List<string> Labels { get; set; } = new List<string>();
        public List<string> FeatureNames { get; set; } = new List<string>();
        public List<int> AllFeatures { get; set; } = new List<int>();
        public List<int> SelectedFeatures { get; set; } = new List<int>();
        public void StartPrediction(List<int> selectedFeatures, int k, bool shouldUseRegressor, bool shouldUseWeights)
        {
            PrepareData();
            DataPointSpawner.Instance.ShouldResetDataPoints = true;
            foreach (var row in UnkownData.DataRows)
            {
                if (row != null) {
                    double[] values = new double[selectedFeatures.Count];

                    for (int i = 0; i < selectedFeatures.Count; i++)
                        values[i] = row.AllValues[selectedFeatures[i]];
                    RunPrediction(row, selectedFeatures, values, k, shouldUseRegressor, shouldUseWeights, false);
                }
            }
        }

        public int RunPrediction(DataRow dataRow, List<int> features, double[] values, int k, bool shouldUseRegression, bool shouldBeWeighted, bool ShouldUseActiveData)
        {

            DataSet dataSet;
            dataRow.ShouldUseNeighbour = ShouldUseActiveData;
            if (ShouldUseActiveData)
                dataSet = ActiveData;
            else dataSet = TrainingData;

            dataRow.kNNValues = values;
            dataRow.FeatureIDs = features;
            double regressValue = 0;
            if (shouldUseRegression) {
                if (shouldBeWeighted)
                    regressValue = kNN.WeightedRegressedClassify(dataRow, dataSet, k);
                else regressValue = kNN.Regressor(dataRow, dataSet, k);

                if (double.IsNaN(regressValue))
                    regressValue = 0;

                int roundedValue = (int)Math.Round(regressValue);
                if (roundedValue > features[features.Count - 1])
                    roundedValue = features[features.Count - 1];

                if (roundedValue < 0)
                    roundedValue = 0;

                

                dataRow.RegressionValue = regressValue;
                dataRow.LabelID = roundedValue;
                dataRow.Label = Labels[roundedValue];
            }
            else
            {
                int predict = 0;
                if (shouldBeWeighted)
                    predict = kNN.WeightedClassify(dataRow, dataSet, k);
                else predict = kNN.Classify(dataRow, dataSet, k);
                dataRow.RegressionValue = 0;
                dataRow.LabelID = predict;
                dataRow.Label = Labels[predict];
            }
            ActiveData.DataRows.Add(dataRow); // needs to be emptied sometime
            return dataRow.LabelID; 
        }

        public void PlotData()
        {
            DataPointSpawner.Instance.FillBiggestValues();
            foreach(var item in ActiveData.DataRows)
                DataPointSpawner.Instance.DataPoints.Enqueue(item);
        }
        public void LoadData(string csv)
        {
            FeatureNames.Clear();
            AllFeatures.Clear();
            data = LoadCSV(csv);
            ActiveData = new DataSet();
            FeatureNames = data[0].ToList();    
            FeatureNames.Remove(FeatureNames[FeatureNames.Count - 1]);// target feature
            for (int i = 0; i < FeatureNames.Count; i++)
                AllFeatures.Add(i);

            data.RemoveAt(0);
        }

        public void PrepareData()
        {
            List<string[]> trainingData = new List<string[]>();

            int unknowLenght = (data.Count() - 1) / 2;
            Randomize(data);

            for (int i=1; i < unknowLenght; i++)
                trainingData.Add(data[i]);

            TrainingData = FillData(trainingData);
            UnkownData = FillData(data);
            TrainingData.Labels = Labels;
            UnkownData.Labels = Labels;
            ActiveData.Labels = Labels;
        }

        private DataSet FillData(List<string[]> data)
        {
            DataSet newData = new DataSet();

            for (int i = 0; i < data.Count; i++)
            {
                int labelPos = data[i].Count() - 1;
                if (!Labels.Contains(data[i][labelPos]))
                    Labels.Add(data[i][labelPos]);

                newData.DataRows.Add(new DataRow()
                {
                    AllValues = data[i].Take(data[i].Length-1).Select(value => double.Parse(value, CultureInfo.InvariantCulture)).ToList(),
                    LabelID = Labels.IndexOf(data[i][labelPos])
                });

            }
            return newData;
        }


        public List<string[]> LoadCSV(string filename)
        {
            List<string[]> data = new List<string[]>();
            if (File.Exists(filename))
            {
                using (StreamReader reader = new StreamReader(filename))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                        data.Add(CSVParser.Split(line));
                    }
                }
            }
            return data;
        }
        public static void Randomize<T>(List<T> items)
        {
            Random rand = new Random();

            // For each spot in the array, pick
            // a random item to swap into that spot.
            for (int i = 0; i < items.Count - 1; i++)
            {
                int j = rand.Next(i, items.Count);
                T temp = items[i];
                items[i] = items[j];
                items[j] = temp;
            }
        }
    }
}
