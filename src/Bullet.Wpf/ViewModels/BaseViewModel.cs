using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Bullet.Wpf
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isBusy = false;

        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); OnPropertyChanged(nameof(IsNotBusy)); }
        }

        public bool IsNotBusy
        {
            get
            {
                return !_isBusy;
            }
        }

        protected bool SetProperty<T>(
           ref T backingStore,
           T value,
           [CallerMemberName]string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
            {
                return false;
            }

            backingStore = value;
            OnPropertyChanged(propertyName);

            return true;
        }
        protected void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
