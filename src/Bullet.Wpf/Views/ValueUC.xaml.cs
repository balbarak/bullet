using System;
using System.Collections.Generic;
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

namespace Bullet.Wpf
{
    /// <summary>
    /// Interaction logic for ValueUC.xaml
    /// </summary>
    public partial class ValueUC : UserControl
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), 
            typeof(string),
            typeof(ValueUC), 
            new PropertyMetadata(""));

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
           nameof(Label),
           typeof(string),
           typeof(ValueUC),
           new PropertyMetadata(""));

        public static readonly DependencyProperty HintProperty = DependencyProperty.Register(
          nameof(Hint),
          typeof(string),
          typeof(ValueUC),
          new PropertyMetadata(""));

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        public string Hint
        {
            get { return (string)GetValue(HintProperty); }
            set { SetValue(HintProperty, value); }
        }

        public ValueUC()
        {
            InitializeComponent();
        }
    }
}
