﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KNNAlgorithm
{
    public class DataRow
    {
        public List<double> AllValues { get; set; } = new List<double>();
        public double[] kNNValues { get; set; }

        public List<int> FeatureIDs { get; set; } = new List<int>();
        public int LabelID { get; set; }
        public string Label { get; set; }
    }
}
