using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace A001
{
    class NotifyingString : INotifyPropertyChanged
    {
        private string val;

        public string Val
        {
            get { return val; }
            set { 
                val = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Val"));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
    }
}
