using System;

namespace Konfigurator
{
    class Polaczenie
    {
        private Punkt A;
        private Punkt B;
        private Double Waga;


        public Polaczenie(Punkt startPoint, Punkt endPoint, Double koszt)
        {
            this.A = startPoint;
            this.B = endPoint;
            this.Waga = koszt;
        }
        public Punkt A1
        {
            get
            {
                return A;
            }

            set
            {
                A = value;
            }
        }

        public Punkt B1
        {
            get
            {
                return B;
            }

            set
            {
                B = value;
            }
        }

        public Double Koszt
        {
            get
            {
                return Waga;
            }

            set
            {
                Waga = value;
            }
        }
    }
}
