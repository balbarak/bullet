using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bullet.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _allowToScroll = true;

        public MainWindow()
        {
            InitializeComponent();

        }

        private void OnLogScroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            ScrollBar sb = e.OriginalSource as ScrollBar;

            if (sb.Orientation == Orientation.Horizontal)
                return;

            if (sb.Value == sb.Maximum)
                _allowToScroll = true;
            else
                _allowToScroll = false;
        }
    }
}
