using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Kinect;
using System.IO;

namespace KinectPrototyp
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Variables
        private LightControl control;
        private KinectSensor sensor;
        private WriteableBitmap colorImageBitmap;
        private Int32Rect colorImageBitmapRect;
        private int colorImageStride;
        private DrawingGroup drawingGroup;
        private DrawingImage skeletonImage;
        private Pen drawPen;
        private const float RenderWidth = 640.0f;
        private const float RenderHeight = 480.0f;
        #endregion Variables

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            control = new LightControl(StatusBar);
            drawPen = new Pen(Brushes.Black, 6);
        }
        #endregion Constructor


        #region Methods
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // DrawingGroup erstellen
            this.drawingGroup = new DrawingGroup();
            // ImageSource erstellen
            this.skeletonImage = new DrawingImage(this.drawingGroup);
            //Zeichnung anzeigen
            ImageSkeleton.Source = this.skeletonImage;


            //Finde Kinect, die mit PC verbunden ist
            foreach (var potential in KinectSensor.KinectSensors)
            {
                if (potential.Status == KinectStatus.Connected)
                {
                    this.sensor = potential;
                    break;
                }
            }

            if (null != this.sensor)
            {
                //schalte Streams an
                this.sensor.SkeletonStream.Enable();
                ColorImageStream colorStream = sensor.ColorStream;
                this.sensor.ColorStream.Enable();

                //WriteableBitmap für ColorStream-Daten
                this.colorImageBitmap = new WriteableBitmap(colorStream.FrameWidth, colorStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                this.colorImageBitmapRect = new Int32Rect(0, 0, colorStream.FrameWidth, colorStream.FrameHeight);
                this.colorImageStride = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                ImageColor.Source = this.colorImageBitmap;

                // Eventhaendler
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;
                this.sensor.ColorFrameReady += this.SensorColorFrameReady;

                //Sensor starten
                try
                {
                    this.sensor.Start();
                    StatusBar.Text = ("Kinect ist nun bereit!");
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                StatusBar.Text = ("Keine Kinect gefunden");
            }
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.sensor != null)
            {
                this.sensor.Stop();
                this.sensor.SkeletonFrameReady -= this.SensorSkeletonFrameReady;
                this.sensor.ColorFrameReady -= this.SensorColorFrameReady;
                this.sensor.SkeletonStream.Disable();
                this.sensor.ColorStream.Disable();
            }
            if (this.control != null)
            {
                control.stop();
            }
        }

        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];
            Skeleton closestSkeleton = null;

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                dc.DrawRectangle(Brushes.White, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            Console.WriteLine("Skeleton erkannt!");
                        }
                               if (skel.TrackingState != SkeletonTrackingState.Tracked)
                                {
                                    continue;
                                }

                                if (closestSkeleton == null)
                                {
                                    closestSkeleton = skel;
                                }
                                else if (closestSkeleton.Position.Z > skel.Position.Z)
                                {
                                   closestSkeleton = skel;
                                }
                    }

                        if (closestSkeleton != null)
                        {
                        //Skeletons der am nahsten ist weiterverarbeiten
                        this.sendJoints(closestSkeleton);
                        this.drawSkeleton(closestSkeleton, dc);
                        }
                }
                // nicht ausserhalb des Bereichs zeichnen
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }

        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    byte[] pixelData = new byte[colorFrame.PixelDataLength];
                    colorFrame.CopyPixelDataTo(pixelData);

                    this.colorImageBitmap.WritePixels(this.colorImageBitmapRect, pixelData, this.colorImageStride, 0);
                }
            }
        }

        private void sendJoints(Skeleton skeleton)
        {
            Joint rightHand = skeleton.Joints[JointType.HandRight];
            Joint leftHand = skeleton.Joints[JointType.HandLeft];
            Joint rightShoulder = skeleton.Joints[JointType.ShoulderRight];
            Joint leftShoulder = skeleton.Joints[JointType.ShoulderLeft];

            //Nur Joints senden, deren Status Tracked ist
            if (rightHand.TrackingState == JointTrackingState.Tracked && rightShoulder.TrackingState == JointTrackingState.Tracked)
            {
                this.control.move(rightHand, rightShoulder);
            }

            if (leftHand.TrackingState == JointTrackingState.Tracked && leftShoulder.TrackingState == JointTrackingState.Tracked)
            {
                this.control.changeColor(leftHand, leftShoulder);
            }
        }

        private void drawSkeleton(Skeleton skeleton, DrawingContext drawingContext)
        {
            //Zeichen Koerper
            drawJoints(drawingContext, skeleton, JointType.Head, JointType.ShoulderCenter);
            drawJoints(drawingContext, skeleton, JointType.ShoulderCenter, JointType.Spine);
            drawJoints(drawingContext, skeleton, JointType.Spine, JointType.HipCenter);

            //Zeichne Arme & Schultern
            drawJoints(drawingContext, skeleton, JointType.HandRight, JointType.ElbowRight);
            drawJoints(drawingContext, skeleton, JointType.ElbowRight, JointType.ShoulderRight);
            drawJoints(drawingContext, skeleton, JointType.ShoulderRight, JointType.ShoulderCenter);
            drawJoints(drawingContext, skeleton, JointType.ShoulderCenter, JointType.ShoulderLeft);
            drawJoints(drawingContext, skeleton, JointType.ShoulderLeft, JointType.ElbowLeft);
            drawJoints(drawingContext, skeleton, JointType.ElbowLeft, JointType.HandLeft);

            //Zeichne Beine
            drawJoints(drawingContext, skeleton, JointType.FootRight, JointType.AnkleRight);
            drawJoints(drawingContext, skeleton, JointType.AnkleRight, JointType.KneeRight);
            drawJoints(drawingContext, skeleton, JointType.KneeRight, JointType.HipRight);
            drawJoints(drawingContext, skeleton, JointType.HipRight, JointType.HipCenter);
            drawJoints(drawingContext, skeleton, JointType.HipCenter, JointType.HipLeft);
            drawJoints(drawingContext, skeleton, JointType.HipLeft, JointType.KneeLeft);
            drawJoints(drawingContext, skeleton, JointType.KneeLeft, JointType.AnkleLeft);
            drawJoints(drawingContext, skeleton, JointType.AnkleLeft, JointType.FootLeft);
        }

        private void drawJoints(DrawingContext drawingContext, Skeleton skeleton, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            //Nur zeichnen, wenn beide Joints TrackingState = Tracked haben
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawingContext.DrawLine(drawPen, getJointPoint(joint0), getJointPoint(joint1));
            }
        }

        private Point getJointPoint(Joint joint)
        {
            DepthImagePoint point = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(joint.Position, DepthImageFormat.Resolution640x480Fps30);

            return new Point(point.X, point.Y);
        }
        #endregion Methods

    }
}
