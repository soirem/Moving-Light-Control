using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;
using System.Windows;
using System.Media;
using System.Windows.Controls;

namespace KinectPrototyp
{
    class LightControl
    {
        #region Variables
        UDPSender server;
        Vector referenceXZ;
        Vector referenceYZ;
        int lastAngle_XZ;
        int lastAngle_YZ;
        int lastAngle_leftHand;
        TextBlock statusBar;
        Boolean strobeEffect;
        #endregion Variables

        #region Construtor
        public LightControl(TextBlock block)
        {
            server = new UDPSender();
            referenceXZ = new Vector(1, 0);
            referenceYZ = new Vector(0, -1);
            lastAngle_leftHand = 0;
            lastAngle_XZ = 0;
            lastAngle_YZ = 0;
            strobeEffect = false;

            statusBar = block;

            server.sendMessage("open");
            statusBar.Text = "open an Moving Head gesendet";
            server.sendMessage("moveh 146");
            statusBar.Text = "moveh 146 an Moving Head gesendet";
        }
        #endregion Constructor

        #region Methods
        public void stop()
        {
            server.sendMessage("reset");
            statusBar.Text = "reset an Moving Head gesendet";
        }

        /// <summary>
        /// Berechne Winkel zwischen linken Arm und Schulter in YZ-Ebene um Farbe zu ändern
        /// </summary>
        /// <param name="hand"> Joint der linke Hand </param>
        /// <param name="shoulder"> Joint der linke Schulter </param>
        public void changeColor(Joint hand, Joint shoulder)
        {
            int actualAngle_LeftHand = 0;

            Vector leftHand = new Vector(shoulder.Position.Z - hand.Position.Z, hand.Position.Y - shoulder.Position.Y);

            actualAngle_LeftHand = (int)Vector.AngleBetween(referenceYZ, leftHand);

            if (actualAngle_LeftHand > lastAngle_leftHand + 10 || actualAngle_LeftHand < lastAngle_leftHand - 10)
            {
                angleToColor(actualAngle_LeftHand);
                lastAngle_leftHand = actualAngle_LeftHand;
            }
        }


        /// <summary>
        /// Winkel zwischen Arm und Körper in XZ-Ebene und YZ-Ebene werden berechnet um in Pan- und Tilt-Bewegung zu ändern
        /// </summary>
        /// <param name="rightHand"> Joint der rechte Hand</param>
        /// <param name="rightShoulder"> Joint der rechte Schulter</param>
        public void move(Joint rightHand, Joint rightShoulder)
        {
            //Pan-Bewegung
            int actualAngle_XZ = 0;

            //Vektor von Schulter zur Hand
            Vector handXZ = new Vector(rightHand.Position.X - rightShoulder.Position.X, rightHand.Position.Z - rightShoulder.Position.Z);

            actualAngle_XZ = (int)Vector.AngleBetween(handXZ, referenceXZ);
            if (actualAngle_XZ > lastAngle_XZ + 10 || actualAngle_XZ < lastAngle_XZ - 10)
            {
                angleToPan(actualAngle_XZ);
                lastAngle_XZ = actualAngle_XZ;
            }

            //Tilt-Bewegung
            int actualAngle_YZ = 0;
            //Vektor von Schulter zur Hand
            Vector handYZ = new Vector(rightShoulder.Position.Z - rightHand.Position.Z, rightHand.Position.Y - rightShoulder.Position.Y);

            actualAngle_YZ = (int)Vector.AngleBetween(referenceYZ, handYZ);
            if (actualAngle_YZ > lastAngle_YZ + 10 || actualAngle_YZ < lastAngle_YZ - 10)
            {
                angleToTilt(actualAngle_YZ);
                lastAngle_YZ = actualAngle_YZ;
            }


            //strobe an
            if ((actualAngle_XZ >= 35 && actualAngle_XZ <= 55 && actualAngle_YZ >= 35 && actualAngle_YZ <= 55))
            {
                if (strobeEffect == false)
                {
                    server.sendMessage("strobe medium");
                    statusBar.Text = "strobe medium an Moving Head gesendet";
                    strobeEffect = true;
                }
            }
            else if (strobeEffect == true)
            {
                server.sendMessage("strobe off");
                statusBar.Text = "strobe off an Moving Head gesendet";
                strobeEffect = false;
            }
        }

        /// <summary>
        /// Winkel in Farbe umrechnen
        /// </summary>
        /// <param name="angle"> Winkel </param>
        private void angleToColor(int angle)
        {
            if (angle >= 0)
            {
                if (angle <= 36)
                {
                    server.sendMessage("color white");
                    statusBar.Text = "color white an Moving Head gesendet";
                }
                else if (angle <= 54)
                {
                    server.sendMessage("color whitegreen");
                    statusBar.Text = "color whitegreen an Moving Head gesendet";
                }
                else if (angle <= 72)
                {
                    server.sendMessage("color green");
                    statusBar.Text = "color green an Moving Head gesendet";
                }
                else if (angle <= 90)
                {
                    server.sendMessage("color greenblue");
                    statusBar.Text = "color greenblue an Moving Head gesendet";
                }
                else if (angle <= 108)
                {
                    server.sendMessage("color blue");
                    statusBar.Text = "color blue an Moving Head gesendet";
                }
                else if (angle <= 126)
                {
                    server.sendMessage("color bluered");
                    statusBar.Text = "color bluered an Moving Head gesendet";
                }
                else if (angle <= 144)
                {
                    server.sendMessage("color red");
                    statusBar.Text = "color red an Moving Head gesendet";
                }
                else
                {
                    server.sendMessage("color all");
                    statusBar.Text = "color all an Moving Head gesendet";
                }
            }
        }

        /// <summary>
        /// Winkel in Pan-Bewegung des Moving Heads umrechnen
        /// </summary>
        /// <param name="angle"> Winkel </param>
        private void angleToPan(double angle)
        {
            if (angle >= 0){

                    double value = (angle / 180) * 73;
                    String message = "moveh " + (int)value;
                    server.sendMessage(message);
                    statusBar.Text = message + " an Moving Head gesendet";
            }
        }


        /// <summary>
        /// Winkel in Tilt-Bewegung des Moving Heads umrechnen
        /// </summary>
        /// <param name="angle"> Winkel </param>
        private void angleToTilt(double angle)
        {
            if (angle >= 0)
            {
                if (angle >= 135)
                {
                    server.sendMessage("movev 0");
                    statusBar.Text = "movev 0 an Moving Head gesendet";
                }
                else
                {
                    double value = (1 - angle / 135) * 127.5;
                    String message = "movev " + (int)value;
                    server.sendMessage(message);
                    statusBar.Text = message + " an Moving Head gesendet";
                }
            }
        }
        #endregion Methods
    }
}