using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApplication.MVVM.ViewModels
{
    internal class MainViewModel :ObservableObject
    {
        private object _currentView;
        public RelayCommand KitchenViewCommand { get; set; }
        public RelayCommand BedRoomViewCommand { get; set; }
        public RelayCommand LivingRoomRelayCommand { get; set; }
        public KitchenViewModel KitchenViewModel { get; set; }
        public BedRoomViewModel BedRoomViewModel { get; set; }
        public LivingRoomViewModel LivingRoomViewModel { get; set; }


        public MainViewModel()
        {
            KitchenViewModel = new KitchenViewModel();
            BedRoomViewModel = new BedRoomViewModel();
            LivingRoomViewModel = new LivingRoomViewModel();

            KitchenViewCommand = new RelayCommand(x => { CurrentView = KitchenViewModel; });
            BedRoomViewCommand = new RelayCommand(x => { CurrentView = BedRoomViewModel; });
            LivingRoomRelayCommand = new RelayCommand(x => { CurrentView = LivingRoomViewModel; });
        }

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value; 
                OnPropertyChanged();
            }
        }
    }
}
