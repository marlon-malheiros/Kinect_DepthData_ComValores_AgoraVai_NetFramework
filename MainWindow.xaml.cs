﻿using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kinect_DepthData_ComValores_AgoraVai_NetFramework
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Variaveis
        private KinectSensor _Kinect;
        private WriteableBitmap _DepthImageBitmap;
        private Int32Rect _DepthImageBitmapRect;
        private int _DepthImageStride;
        private DepthImageFrame _lastDepthFrame;
        private short[] _depthImagePixelData;        
        public int[] _dadosDeProfundidadeRegistrados;
        // public List<int> dadosProfSemPIndex = new List<int>();
        public List<string> dadosProfSemPIndex = new List<string>();
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += (s, e) => { DiscoverKinectSensor(); };
            this.Unloaded += (s, e) => { this.Kinect = null; };
        }
        
        private void DiscoverKinectSensor()
        {
            KinectSensor.KinectSensors.StatusChanged += kinectSensor_StatusChanged;
            this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
        }
        
        private void kinectSensor_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (this.Kinect == null)
                    {
                        this.Kinect = e.Sensor;
                    }

                    break;
                case KinectStatus.Disconnected:
                    if (this.Kinect == e.Sensor)
                    {
                        this.Kinect = null;
                        // Bloco misterioso
                        this.Kinect =
                            KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
                        if (this.Kinect == null)
                        {
                            
                        }
                    }

                    break;
            }
        }

        private KinectSensor Kinect
        {
            get { return this._Kinect; }
            set
            {
                if (this._Kinect != value)
                {
                    if (this._Kinect == null)
                    {
                        // Desabilita
                        UnitializeKinectSensor(this._Kinect);
                        this._Kinect = null;
                    }

                    if (value != null && value.Status == KinectStatus.Connected)
                    {
                        // Habilita
                        this._Kinect = value;
                        InitializeKinectSensor(this._Kinect);
                    }
                }
            }
        }

        private void InitializeKinectSensor(KinectSensor kinect)
        {
            if (kinect != null)
            {
                DepthImageStream depthStream = kinect.DepthStream;
                kinect.DepthStream.Enable();
                
                this._DepthImageBitmap = new WriteableBitmap(depthStream.FrameWidth, depthStream.FrameHeight, 96, 96, PixelFormats.Gray16, null);
                this._DepthImageBitmapRect = new Int32Rect(0, 0, depthStream.FrameWidth, depthStream.FrameHeight);
                this._DepthImageStride = depthStream.FrameWidth * depthStream.FrameBytesPerPixel;
                DepthImage.Source = this._DepthImageBitmap;

                this._Kinect.DepthFrameReady += Kinect_DepthFrameReady;
                kinect.Start();
            }
            
        }
        
        private void UnitializeKinectSensor(KinectSensor kinect)
        {
            if(kinect != null)
            {
                kinect.Stop();
                this._Kinect.DepthFrameReady += Kinect_DepthFrameReady;
            }
        }

        private void Kinect_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            // using (DepthImageFrame frame = e.OpenDepthImageFrame())
            // {
            //     if (frame != null)
            //     {
            //         short[] pixelData = new short[frame.PixelDataLength];
            //         frame.CopyPixelDataTo(pixelData);
            //         this._DepthImageBitmap.WritePixels(this._DepthImageBitmapRect, pixelData, this._DepthImageStride, 0);
            //     }
            // }

            if (this._lastDepthFrame != null)
            {
                this._lastDepthFrame.Dispose();
                this._lastDepthFrame = null;
            }

            this._lastDepthFrame = e.OpenDepthImageFrame();

            if (this._lastDepthFrame != null)
            {
                // short[] pixelData = new short[this._lastDepthFrame.PixelDataLength];                
                this._depthImagePixelData = new short[this._lastDepthFrame.PixelDataLength];
                this._lastDepthFrame.CopyPixelDataTo(this._depthImagePixelData);
                this._DepthImageBitmap.WritePixels(this._DepthImageBitmapRect, this._depthImagePixelData, this._DepthImageStride, 0);
            }
        }

        private void DepthImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(DepthImage);

            if (this._depthImagePixelData != null && this._depthImagePixelData.Length > 0)
            {
                int pixelIndex = (int) (p.X + ((int) p.Y * this._lastDepthFrame.Width));
                int depth = this._depthImagePixelData[pixelIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                int depthInches = (int) (depth * 0.0393700787);
                int depthFt = depthInches / 12;
                depthInches = depthInches % 12;

                // PixelDepth.Text = string.Format("{0}mm ~ {1}'{2}\"", depth, depthFt, depthInches);
                PixelDepth.Text = string.Format("{0}mm", depth);
            }
        }
        private void Button_SalvarNuvem_OnClick(object sender, RoutedEventArgs e)
        {
            // Verifica se o frame é valido
            if (this._depthImagePixelData != null && this._depthImagePixelData.Length > 0)
            {
                // this._dadosDeProfundidadeRegistrados = this._depthImagePixelData;
                //foreach (int entrada in this._dadosDeProfundidadeRegistrados)
                foreach (int entrada in this._depthImagePixelData)
                {
                    int valor = entrada >> DepthImageFrame.PlayerIndexBitmaskWidth;                    
                    this.dadosProfSemPIndex.Add(Convert.ToString(valor));
                    // this._dadosDeProfundidadeRegistrados.Append<int>(valor);
                    // this.dadosProfSemPIndex.Add(entrada >> DepthImageFrame.PlayerIndexBitmaskWidth);
                }
                // System.IO.File.WriteAllText(@"C:\Users\Administrador\Desktop\Nuvem_teste.txt", this.dadosProfSemPIndex.ToString);
                // System.IO.File.WriteAllBytes(@"C:\Users\Administrador\Desktop\Nuvem_teste.txt", this._depthImagePixelData);
                using(System.IO.StreamWriter arquivo = new System.IO.StreamWriter(@"C:\Users\Administrador\Desktop\Nuvem_teste3.txt", true))
                {
                    foreach(string valor in this.dadosProfSemPIndex)
                    {
                        arquivo.WriteLine(valor);
                    }
                }
            }                
            else
            {
                // Melhor ver outra mensagem
                // AAAAAA
                MessageBox.Show("Erro: this._depthImagePixelData == null ou this._depthImagePixelData.Length < 0");
            }         
        }
    }
}