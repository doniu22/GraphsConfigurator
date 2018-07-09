using System;
using System.Windows;

namespace Konfigurator
{
    class Punkt
    {
        private string Name;
        private Point punkt;
        private double pietro;
        private string opis;


        public Punkt(string nazwa, Double X, Double Y, Double Z, string text)
        {
            Name = nazwa;
            punkt.X = X;
            punkt.Y = Y;
            pietro = Z;
            opis = text;

        }
        public string PointName
        {
            get
            {
                return Name;
            }

            set
            {
                Name = value;
            }
        }

        public Point PointXY
        {
            get
            {
                return punkt;
            }

            set
            {
                punkt = value;
            }
        }

        public double PointZ
        {
            get
            {
                return pietro;
            }

            set
            {
                pietro = value;
            }
        }

        public string PointText
        {
            get
            {
                return opis;
            }

            set
            {
                opis = value;
            }
        }
    }
}
