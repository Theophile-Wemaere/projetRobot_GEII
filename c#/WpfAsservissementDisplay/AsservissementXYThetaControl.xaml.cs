﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Utilities;

namespace WpfAsservissementDisplay
{
    /// <summary>
    /// Logique d'interaction pour UserControl1.xaml
    /// </summary>
    /// 
    public partial class AsservissementXYThetaControl : UserControl
    {
        int queueSize = 1;
        FixedSizedQueue<double> commandXList;
        FixedSizedQueue<double> commandThetaList;
        FixedSizedQueue<double> commandM1List;
        FixedSizedQueue<double> commandM2List;

        FixedSizedQueue<double> consigneXList;
        FixedSizedQueue<double> consigneThetaList;
        FixedSizedQueue<double> consigneM1List;
        FixedSizedQueue<double> consigneM2List;

        FixedSizedQueue<double> measuredXList;
        FixedSizedQueue<double> measuredThetaList;
        FixedSizedQueue<double> measuredM1List;
        FixedSizedQueue<double> measuredM2List;

        FixedSizedQueue<double> errorXList;
        FixedSizedQueue<double> errorThetaList;
        FixedSizedQueue<double> errorM2List;
        FixedSizedQueue<double> errorM1List;

        FixedSizedQueue<double> corrPXList;
        FixedSizedQueue<double> corrPThetaList;
        FixedSizedQueue<double> corrPM1List;
        FixedSizedQueue<double> corrPM2List;
        FixedSizedQueue<double> corrIXList;
        FixedSizedQueue<double> corrIThetaList;
        FixedSizedQueue<double> corrIM1List;
        FixedSizedQueue<double> corrIM2List;
        FixedSizedQueue<double> corrDXList;
        FixedSizedQueue<double> corrDThetaList;
        FixedSizedQueue<double> corrDM1List;
        FixedSizedQueue<double> corrDM2List;

        double corrLimitPX, corrLimitPTheta, corrLimitPM1, corrLimitPM2;
        double corrLimitIX, corrLimitITheta, corrLimitIM1, corrLimitIM2;
        double corrLimitDX, corrLimitDTheta, corrLimitDM1, corrLimitDM2;

        double KpX, KpTheta, KpM1, KpM2;
        double KiX, KiTheta, KiM1, KiM2;
        double KdX, KdTheta, KdM1, KdM2;

        System.Timers.Timer displayTimer;

        public AsservissementXYThetaControl()
        {
            InitializeComponent();

              commandXList = new Utilities.FixedSizedQueue<double>(queueSize);
              commandThetaList = new Utilities.FixedSizedQueue<double>(queueSize);
              commandM1List = new Utilities.FixedSizedQueue<double>(queueSize);
              commandM2List = new Utilities.FixedSizedQueue<double>(queueSize);

              consigneXList = new Utilities.FixedSizedQueue<double>(queueSize);
              consigneThetaList = new Utilities.FixedSizedQueue<double>(queueSize);
              consigneM1List = new Utilities.FixedSizedQueue<double>(queueSize);
              consigneM2List = new Utilities.FixedSizedQueue<double>(queueSize);

              measuredXList = new Utilities.FixedSizedQueue<double>(queueSize);
              measuredThetaList = new Utilities.FixedSizedQueue<double>(queueSize);
              measuredM1List = new Utilities.FixedSizedQueue<double>(queueSize);
              measuredM2List = new Utilities.FixedSizedQueue<double>(queueSize);

              errorXList = new Utilities.FixedSizedQueue<double>(queueSize);
              errorThetaList = new Utilities.FixedSizedQueue<double>(queueSize);
              errorM1List = new Utilities.FixedSizedQueue<double>(queueSize);
              errorM2List = new Utilities.FixedSizedQueue<double>(queueSize);

              corrPXList = new Utilities.FixedSizedQueue<double>(queueSize);
              corrPThetaList = new Utilities.FixedSizedQueue<double>(queueSize); 
              corrPM1List = new Utilities.FixedSizedQueue<double>(queueSize);
              corrPM2List = new Utilities.FixedSizedQueue<double>(queueSize);

              corrIXList = new Utilities.FixedSizedQueue<double>(queueSize);
              corrIThetaList = new Utilities.FixedSizedQueue<double>(queueSize);
              corrIM1List = new Utilities.FixedSizedQueue<double>(queueSize);
              corrIM2List = new Utilities.FixedSizedQueue<double>(queueSize);


              corrDXList = new Utilities.FixedSizedQueue<double>(queueSize);
              corrDThetaList = new Utilities.FixedSizedQueue<double>(queueSize);
              corrDM1List = new Utilities.FixedSizedQueue<double>(queueSize);
              corrDM2List = new Utilities.FixedSizedQueue<double>(queueSize);

              consigneXList.Enqueue(0);
              consigneThetaList.Enqueue(0);
              consigneM1List.Enqueue(0);
              consigneM2List.Enqueue(0);


              commandXList.Enqueue(0);
              commandThetaList.Enqueue(0);
              commandM1List.Enqueue(0);
              commandM2List.Enqueue(0);

              measuredXList.Enqueue(0);
              measuredThetaList.Enqueue(0);
              measuredM1List.Enqueue(0);
              measuredM2List.Enqueue(0);

              errorXList.Enqueue(0);
              errorThetaList.Enqueue(0);
              errorM1List.Enqueue(0);
              errorM2List.Enqueue(0);

              displayTimer = new Timer(100);
              displayTimer.Elapsed += DisplayTimer_Elapsed;
              displayTimer.Start();
        }

          public void SetTitle(string titre)
          {
              LabelTitre.Content = titre;
          }

          private void DisplayTimer_Elapsed(object sender, ElapsedEventArgs e)
          {
              Dispatcher.BeginInvoke(new Action(delegate ()
              {
                  UpdateDisplay();
              }));
          }

          public void UpdateDisplay()
          {
              LabelConsigneX.Content = consigneXList.Average().ToString("N3");
              LabelConsigneTheta.Content = consigneThetaList.Average().ToString("N3");
              LabelConsigneM1.Content = consigneM1List.Average().ToString("N3");
              LabelConsigneM2.Content = consigneM2List.Average().ToString("N3");

              LabelMeasureX.Content = measuredXList.Average().ToString("N3");
              LabelMeasureTheta.Content = measuredThetaList.Average().ToString("N3");
              LabelMeasureM1.Content = measuredM1List.Average().ToString("N3");
              LabelMeasureM2.Content = measuredM2List.Average().ToString("N3");

              LabelErreurX.Content = errorXList.Average().ToString("N3");
              LabelErreurTheta.Content = errorThetaList.Average().ToString("N3");
              LabelErreurM1.Content = errorM1List.Average().ToString("N3");
              LabelErreurM2.Content = errorM2List.Average().ToString("N3");

              LabelCommandX.Content = commandXList.Average().ToString("N3");
              LabelCommandTheta.Content = commandThetaList.Average().ToString("N3");
              LabelCommandM1.Content = commandM1List.Average().ToString("N3");
              LabelCommandM2.Content = commandM2List.Average().ToString("N3");

              LabelKpX.Content = KpX.ToString("N3");
              LabelKpTheta.Content = KpTheta.ToString("N3");
              LabelKpM1.Content = KpM1.ToString("N3");
              LabelKpM2.Content = KpM2.ToString("N3");

              LabelKiX.Content = KiX.ToString("N3");
              LabelKiTheta.Content = KiTheta.ToString("N3");
              LabelKiM1.Content = KiM1.ToString("N3");
              LabelKiM2.Content = KiM2.ToString("N3");

              LabelKdX.Content = KdX.ToString("N3");
              LabelKdTheta.Content = KdTheta.ToString("N3");
              LabelKdM1.Content = KdM1.ToString("N3");
              LabelKdM2.Content = KdM2.ToString("N3");

              LabelCorrMaxPX.Content = corrLimitPX.ToString("N3");
              LabelCorrMaxPTheta.Content = corrLimitPTheta.ToString("N3");
              LabelCorrMaxPM1.Content = corrLimitPM1.ToString("N3");
              LabelCorrMaxPM2.Content = corrLimitPM2.ToString("N3");

              LabelCorrMaxIX.Content = corrLimitIX.ToString("N3");
              LabelCorrMaxITheta.Content = corrLimitITheta.ToString("N3");
              LabelCorrMaxIM1.Content = corrLimitIM1.ToString("N3");
              LabelCorrMaxIM2.Content = corrLimitIM2.ToString("N3");

              LabelCorrMaxDX.Content = corrLimitDX.ToString("N3");
              LabelCorrMaxDTheta.Content = corrLimitDTheta.ToString("N3");
              LabelCorrMaxDM1.Content = corrLimitDM1.ToString("N3");
              LabelCorrMaxDM2.Content = corrLimitDM2.ToString("N3");


              if (corrPXList.Count > 0)
              {
                  LabelCorrPX.Content = corrPXList.Average().ToString("N3");
                  LabelCorrPTheta.Content = corrPThetaList.Average().ToString("N3");

                  LabelCorrIX.Content = corrIXList.Average().ToString("N3");
                  LabelCorrITheta.Content = corrIThetaList.Average().ToString("N3");

                  LabelCorrDX.Content = corrDXList.Average().ToString("N3");
                  LabelCorrDTheta.Content = corrDThetaList.Average().ToString("N3");
              }

              if (corrPM1List.Count > 0)
              {
                  LabelCorrPM1.Content = corrPM1List.Average().ToString("N3");
                  LabelCorrPM2.Content = corrPM2List.Average().ToString("N3");

                  LabelCorrIM1.Content = corrIM1List.Average().ToString("N3");
                  LabelCorrIM2.Content = corrIM2List.Average().ToString("N3");

                  LabelCorrDM1.Content = corrDM1List.Average().ToString("N3");
                  LabelCorrDM2.Content = corrDM2List.Average().ToString("N3");
              }
          }

          public void UpdatePolarSpeedConsigneValues(double consigneX, double consigneTheta)
          {
              consigneXList.Enqueue(consigneX);
              consigneThetaList.Enqueue(consigneTheta);
          }
          public void UpdateIndependantSpeedConsigneValues(double consigneM1, double consigneM2)
          {
              consigneM1List.Enqueue(consigneM1);
              consigneM2List.Enqueue(consigneM2);
          }

          public void UpdatePolarSpeedCommandValues(double commandX, double commandTheta)
          {
              commandXList.Enqueue(commandX);
              commandThetaList.Enqueue(commandTheta);
          }
          public void UpdateIndependantSpeedCommandValues(double commandM1, double commandM2)
          {
              commandM1List.Enqueue(commandM1);
              commandM2List.Enqueue(commandM2);
          }

          public void UpdatePolarOdometrySpeed(double valueX, double valueTheta)
          {
              measuredXList.Enqueue(valueX);
              measuredThetaList.Enqueue(valueTheta);
          }
          public void UpdateIndependantOdometrySpeed(double valueM1, double valueM2)
          {
              measuredM1List.Enqueue(valueM1);
              measuredM2List.Enqueue(valueM2);
          }

          public void UpdatePolarSpeedErrorValues(double errorX, double errorTheta)
          {
              errorXList.Enqueue(errorX);
              errorThetaList.Enqueue(errorTheta);
          }
          public void UpdateIndependantSpeedErrorValues(double errorM1, double errorM2)
          {
              errorM1List.Enqueue(errorM1);
              errorM2List.Enqueue(errorM2);
          }

          public void UpdatePolarSpeedCorrectionValues(double corrPX, double corrPTheta, double corrIX, double corrITheta, 
              double corrDX, double corrDTheta)
          {
              corrPXList.Enqueue(corrPX);
              corrPThetaList.Enqueue(corrPTheta);
              corrIXList.Enqueue(corrIX);
              corrIThetaList.Enqueue(corrITheta);
              corrDXList.Enqueue(corrDX);
              corrDThetaList.Enqueue(corrDTheta);
          }
          public void UpdateIndependantSpeedCorrectionValues(double corrPM1, double corrPM2, double corrIM1, double corrIM2, double corrDM1, double corrDM2)
          {
              corrPM1List.Enqueue(corrPM1);
              corrPM2List.Enqueue(corrPM2);
              corrIM1List.Enqueue(corrIM1);
              corrIM2List.Enqueue(corrIM2);
              corrDM1List.Enqueue(corrDM1);
              corrDM2List.Enqueue(corrDM2);
          }


          public void UpdatePolarSpeedCorrectionGains(double KpX, double KpTheta, double KiX, double KiTheta, double KdX, double KdTheta)
          {
              this.KpX = KpX;
              this.KpTheta = KpTheta;
              this.KiX = KiX;
              this.KiTheta = KiTheta;
              this.KdX = KdX;
              this.KdTheta = KdTheta;
          }
          public void UpdateIndependantSpeedCorrectionGains(double KpM1, double KpM2, double KiM1, double KiM2, double KdM1, double KdM2)
          {
              this.KpM1 = KpM1;
              this.KpM2 = KpM2;
              this.KiM1 = KiM1;
              this.KiM2 = KiM2;
              this.KdM1 = KdM1;
              this.KdM2 = KdM2;
          }

          public void UpdatePolarSpeedCorrectionLimits(double corrLimitPX, double corrLimitPTheta, double corrLimitIX, double corrLimitITheta,
              double corrLimitDX, double corrLimitDTheta)
          {
              this.corrLimitPX = corrLimitPX;
              this.corrLimitPTheta = corrLimitPTheta;
              this.corrLimitIX = corrLimitIX;
              this.corrLimitITheta = corrLimitITheta;
              this.corrLimitDX = corrLimitDX;
              this.corrLimitDTheta = corrLimitDTheta;
          }
          public void UpdateIndependantSpeedCorrectionLimits(double corrLimitPM1, double corrLimitPM2, double corrLimitIM1, double corrLimitIM2,
              double corrLimitDM1, double corrLimitDM2)
          {
              this.corrLimitPM1 = corrLimitPM1;
              this.corrLimitPM2 = corrLimitPM2;
              this.corrLimitIM1 = corrLimitIM1;
              this.corrLimitIM2 = corrLimitIM2;
              this.corrLimitDM1 = corrLimitDM1;
              this.corrLimitDM2 = corrLimitDM2;
          }
        
    }
}