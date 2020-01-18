using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace TestRobot
{
    class MyTick
    {
        public long Id { get; set; }    // 
        public string Name { get; set; }    //
        public string Time { get; set; }
        public decimal Price { get; set; }
        public int Volume { get; set; }
        public string Direction { get; set; }
        public int KolPok { get; set; }     // заявок на покупку?
        public int KolPrd { get; set; }     // заявок на продажу?
        public int OI { get; set; }      //  открытый интерес
    }

    class Position
    {
        public double OpenBal { get; set; }
        public double CurrentBal { get; set; }
        public double Locked { get; set; }
    }

    class DepoLim
    {
        public string SecCode { get; set; }
        public double CurrentBalance { get; set; }
        public double AweragePositionPrice { get; set; }
    }

    /*
    class DepoLim : INotifyPropertyChanged
    {
        public string SecCode { get; set; }

        private long currentBalance;

        public long CurrentBalance
        {
            get { return currentBalance;}

            set
            {
                currentBalance = value;
                OnPropertyChanged("CurrentBalance");
            }
        }

        // https://metanit.com/sharp/wpf/11.2.php

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        */


}

