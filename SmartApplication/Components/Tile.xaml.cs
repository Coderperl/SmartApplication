using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SmartApplication.Components
{
    /// <summary>
    /// Interaction logic for Tile.xaml
    /// </summary>
    public partial class Tile : UserControl
    {
        public static readonly DependencyProperty StateActiveProperty =
            DependencyProperty.Register("StateActive", typeof(string), typeof(Tile));
        public string StateActive
        {
            get { return (string)GetValue(StateActiveProperty); }
            set { SetValue(StateActiveProperty, value); }
        }

        public static readonly DependencyProperty StateInactiveProperty =
            DependencyProperty.Register("StateInactive", typeof(string), typeof(Tile));
        public string StateInactive
        {
            get { return (string)GetValue(StateInactiveProperty); }
            set { SetValue(StateInactiveProperty, value); }
        }



        public static readonly DependencyProperty DeviceNameProperty =
            DependencyProperty.Register("DeviceName", typeof(string), typeof(Tile));
        public string DeviceName
        {
            get { return (string)GetValue(DeviceNameProperty); }
            set { SetValue(DeviceNameProperty, value); }
        }
        public static readonly DependencyProperty DeviceTypeProperty =
            DependencyProperty.Register("DeviceType", typeof(string), typeof(Tile));
        public string DeviceType
        {
            get { return (string)GetValue(DeviceTypeProperty); }
            set { SetValue(DeviceTypeProperty, value); }
        }


        public static readonly DependencyProperty isCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(Tile));
        public bool IsChecked
        {
            get { return (bool)GetValue(isCheckedProperty); }
            set { SetValue(isCheckedProperty, value); }
        }

        public static readonly DependencyProperty ActiveIconProperty =
            DependencyProperty.Register("ActiveIcon", typeof(string), typeof(Tile));
        public string ActiveIcon
        {
            get { return (string)GetValue(ActiveIconProperty); }
            set { SetValue(ActiveIconProperty, value); }
        }

        public static readonly DependencyProperty InActiveIconProperty =
            DependencyProperty.Register("InActiveIcon", typeof(string), typeof(Tile));
        public string InActiveIcon
        {
            get { return (string)GetValue(InActiveIconProperty); }
            set { SetValue(InActiveIconProperty, value); }
        }

        public Tile()
        {
            InitializeComponent();
        }


        private void BtnRemoveDevice_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
