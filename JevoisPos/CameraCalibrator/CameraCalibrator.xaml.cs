﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;   
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExtendedSerialPort;
using System.Windows.Threading;
using System.Globalization;
using System.Collections.Concurrent;
using System.Timers;
using System.IO;
using Microsoft.Win32;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics;

namespace Interfacerobot
{
    public partial class CameraCalibrator : System.Windows.Window
    {

        ReliableSerialPort serialPort1;


        public CameraCalibrator()
        {
            serialPort1 = new ReliableSerialPort("COM3", 460800, Parity.None, 8, StopBits.One);
            serialPort1.DataReceived += SerialPort1_DataReceived;
            serialPort1.Open();
        }


        ConcurrentQueue<byte> receivedData = new ConcurrentQueue<byte>();
        private void SerialPort1_DataReceived(object sender, DataReceivedArgs e)
        {
            foreach (var c in e.Data)
            {
                ProcessJeVoisData(c);
            }
        }

        string jeVoisCurrentFrame = "";
        void ProcessJeVoisData(byte c)
        {
            if(c=='D')
            {
                AnalyzeJeVoisData(jeVoisCurrentFrame);
                jeVoisCurrentFrame = "";            
            }
            jeVoisCurrentFrame += Encoding.UTF8.GetString(new byte[]{c}, 0, 1); 
        }

        void AnalyzeJeVoisData(string s)
        {
            string[] jeVoisArray = s.Split(' ');
            if (jeVoisArray.Length == 11) //size of the JeVois input payload
            {
                AnalyzeJeVoisData(jeVoisArray);
            }
        }

        List<ArucoLutElement> ArucoLut = new List<ArucoLutElement>();


        private void Analyze_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog opfd = new OpenFileDialog();
            if (opfd.ShowDialog() == true)
            {
                ArucoLut = new List<ArucoLutElement>();
                using (var reader = new StreamReader(opfd.FileName))
                {
                    List<int> ThetaAruco = new List<int>();
                    List<int> x1List = new List<int>();
                    List<int> x2List = new List<int>();
                    List<int> x3List = new List<int>();
                    List<int> x4List = new List<int>();
                    List<int> y1List = new List<int>();
                    List<int> y2List = new List<int>();
                    List<int> y3List = new List<int>();
                    List<int> y4List = new List<int>();

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(';');
                        int pos = 0;

                        ArucoLutElement lutElement = new ArucoLutElement();
                        lutElement.xField = int.Parse(values[pos++]);
                        lutElement.yField = int.Parse(values[pos++]);
                        lutElement.thetaField = int.Parse(values[pos++]);

                        double x = int.Parse(values[pos++]);
                        double y = int.Parse(values[pos++]);
                        lutElement.pt1 = new PointD(x, y);
                        x = int.Parse(values[pos++]);
                        y = int.Parse(values[pos++]);
                        lutElement.pt2 = new PointD(x, y);
                        x = int.Parse(values[pos++]);
                        y = int.Parse(values[pos++]);
                        lutElement.pt3 = new PointD(x, y);
                        x = int.Parse(values[pos++]);
                        y = int.Parse(values[pos++]);
                        lutElement.pt4 = new PointD(x, y);

                        lutElement.xMeasured = (lutElement.pt1.X + lutElement.pt2.X + lutElement.pt3.X + lutElement.pt4.X) / 4.0;
                        lutElement.yMeasured = (lutElement.pt1.Y + lutElement.pt2.Y + lutElement.pt3.Y + lutElement.pt4.Y) / 4.0;

                        ArucoLut.Add(lutElement);

                        //Génération du symétrique par rapport à l'axe vertical

                        if (lutElement.xField != 0)
                        {
                            ArucoLutElement lutElementSym = new ArucoLutElement();

                            lutElementSym.xField = -lutElement.xField;
                            lutElementSym.yField = lutElement.yField;
                            lutElementSym.xMeasured = -lutElement.xMeasured;
                            lutElementSym.yMeasured = lutElement.yMeasured;
                            lutElementSym.thetaField = 180 - lutElement.thetaField;
                            lutElementSym.pt1 = new PointD(-lutElement.pt1.X, lutElement.pt1.Y);
                            lutElementSym.pt2 = new PointD(-lutElement.pt2.X, lutElement.pt2.Y);
                            lutElementSym.pt3 = new PointD(-lutElement.pt3.X, lutElement.pt3.Y);
                            lutElementSym.pt4 = new PointD(-lutElement.pt4.X, lutElement.pt4.Y);

                            ArucoLut.Add(lutElementSym);
                        }

                    }
                }
            }
        }

        double x1, x2, x3, x4, y1, y2, y3, y4;

        private void AnalyzeJeVoisData(string[] inputArray)
        {
            for(int i =0; i< inputArray.Length;i++)
            {
                inputArray[i] =  inputArray[i].Replace('.', ',');
            }
            
            x1 = double.Parse(inputArray[3]);
            y1 = double.Parse(inputArray[4]);
            x2 = double.Parse(inputArray[5]);
            y2 = double.Parse(inputArray[6]);
            x3 = double.Parse(inputArray[7]);
            y3 = double.Parse(inputArray[8]);
            x4 = double.Parse(inputArray[9]);            
            y4 = double.Parse(inputArray[10]);

            if(ArucoLut.Count > 0)
            {
               

                double xMeasured = (x1 + x2 + x3 + x4) / 4.0;
                double yMeasured = (y1 + y2 + y3 + y4) / 4.0;

                /// On calcule la distance à chaque element de la LUT
                var ArucoClosestList = ArucoLut.OrderBy(p => Math.Sqrt(Math.Pow(xMeasured - p.xMeasured, 2) + Math.Pow(yMeasured - p.yMeasured, 2))).ToList();

                var closestPoint = ArucoClosestList[0];
                var closestPoint2 = ArucoClosestList[1];
                ArucoLutElement closestPoint3 = ArucoClosestList[2];
                double vectorielProduct;
                int pos = 2;
                do {
                    closestPoint3 = ArucoClosestList[pos];
                    pos++;
                    var V12field = Vector<double>.Build.DenseOfArray(new double[] {
                                    closestPoint.xField - closestPoint2.xField,
                                    closestPoint.yField - closestPoint2.yField });

                    var V13field = Vector<double>.Build.DenseOfArray(new double[] {
                                    closestPoint.xField - closestPoint3.xField,
                                    closestPoint.yField - closestPoint3.yField });
                    vectorielProduct = (double)AlgebraTools.Cross(V12field, V13field);
                }
                while (
                /// On ne garde pas le 3e pt si les 3 sont alignés
                vectorielProduct == 0);

                /// On calcule le gradient de distance mesurée entre le pt le plus proche et le 2e pt le plus proche
                PointD gradient12 = new PointD(
                    closestPoint.xMeasured - closestPoint2.xMeasured,
                    closestPoint.yMeasured - closestPoint2.yMeasured);

                /// On calcule le gradient de distance mesurée entre le pt le plus proche et le 3e pt le plus proche
                PointD gradient13 = new PointD(
                    closestPoint.xMeasured - closestPoint3.xMeasured,
                    closestPoint.yMeasured - closestPoint3.yMeasured);

                /// On calcule l'écart entre le pt le plus proche et le pt détecté
                PointD ecartClosestPoint = new PointD(
                    closestPoint.xMeasured - xMeasured,
                    closestPoint.yMeasured - yMeasured);

                /// On détermine les coeff a et b tq : ecartClosestPoint = a * gradient12 + b * gradient13
                var M = CreateMatrix.Dense(2, 2, new double[] {gradient12.X, gradient12.Y , gradient13.X, gradient13.Y });
                var MInv = M.Inverse();

                Vector<double> v = Vector<double>.Build.DenseOfArray(new double[] { ecartClosestPoint.X, ecartClosestPoint.Y });

                var ab = MInv.Multiply(v);

                /// On calcule les coordonnées du pt mesuré dans le terrain
                PointD posArucoField = new PointD(
                    closestPoint.xField - ab[0] * (closestPoint.xField - closestPoint2.xField) - ab[1] * (closestPoint.xField - closestPoint3.xField),
                    closestPoint.yField - ab[0] * (closestPoint.yField - closestPoint2.yField) - ab[1] * (closestPoint.yField - closestPoint3.yField));

                Console.WriteLine("Pos calculée : Id "+ inputArray[1]+ " X: " + posArucoField.X.ToString("F1") + " Y: " + posArucoField.Y.ToString("F1"));
            }

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

            string path = "C:\\GitHub\\robot_theophile_wemaere\\JeVoisDecoder\\Data\\log.csv";

            string Xreel = tbXreel.Text;
            string Yreel = tbYreel.Text;
            string Theta = tbTheta.Text;

            tbXreel.Text = ""; tbYreel.Text = ""; tbTheta.Text = "";

            string log = Xreel + ";" + Yreel + ";" + Theta + ";" + x1 + ";" + y1 + ";" + x2 + ";" + y2 + ";" + x3 + ";" + y3 + ";" + x4 + ";" + y4;

            using (StreamWriter file = new StreamWriter(path, append: true))
            {
                file.WriteLine(log);
            }
        }
    }
     
    public class ArucoLutElement
    {
        public double xField;
        public double yField;
        public double thetaField;
        public double xMeasured;
        public double yMeasured;
        public PointD pt1;
        public PointD pt2;
        public PointD pt3;
        public PointD pt4;
    }

    public class PointD
    {
        public double X;
        public double Y;
        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public static class AlgebraTools
    {
        public static double? Cross(Vector<double> left, Vector<double> right)
        {
            double? result = null;
            if ((left.Count == 2 && right.Count == 2))
            {                
                result = left[0] * right[1] - left[1] * right[0];
            }

            return result;
        }
    }
}