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
using static System.Net.Mime.MediaTypeNames;

namespace Draw
{
    /// <summary>
    /// Interaction logic for mySlider.xaml
    /// </summary>
    
    public partial class mySlider : UserControl
    {
        Boolean _in = false;

        double max = 100;
        double min = 0;
        public double value = 0;


        // Register a custom routed event using the Bubble routing strategy.
        public static readonly RoutedEvent valueChangedEvent = EventManager.RegisterRoutedEvent(
            name: "valueChanged",
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(mySlider));




        public mySlider()
        {
            InitializeComponent();
            value = min;
        }

        public void set_max(double max)
        {
            this.max = max;
        }

        public void set_min(double min)
        {
            this.min = min;
            value = min;
        }

        public void set_value(double value)
        {
            this.value = value;
            double x = (((value - min) * 145) / (max - min)) + 25;
            grib.Margin = new Thickness(x, 0, 0, 0);
        }
        public event RoutedEventHandler value_changed
        {
            add { AddHandler(valueChangedEvent, value); }
            remove { RemoveHandler(valueChangedEvent, value); }
        }
        void RaiseCustomRoutedEvent()
        {
            // Create a RoutedEventArgs instance.
            RoutedEventArgs routedEventArgs = new(routedEvent: valueChangedEvent);

            // Raise the event, which will bubble up through the element tree.
            RaiseEvent(routedEventArgs);
        }
        private void Rectangle_TouchDown(object sender, TouchEventArgs e)
        {

        }

        private void Rectangle_TouchMove(object sender, TouchEventArgs e)
        {

        }




        private void UserControl_TouchDown(object sender, TouchEventArgs e)
        {

        }

        private void UserControl_TouchMove(object sender, TouchEventArgs e)
        {

        }

        private void Grid_TouchDown(object sender, TouchEventArgs e)
        {

        }

        private void Grid_TouchMove(object sender, TouchEventArgs e)
        {

        }

        private void Rectangle_TouchDown_1(object sender, TouchEventArgs e)
        {
            _in = true;
            TouchPoint x = e.GetTouchPoint(this);
            if(_in)
                if (x.Position.X > 25 && x.Position.X < 170)
                {
                    grib.Margin = new Thickness(x.Position.X, 0, 0, 0);
                    value = min + ((x.Position.X - 25) / 145) * (max - min);
                }
                else if (x.Position.X <= 25)
                {
                    grib.Margin = new Thickness(25, 0, 0, 0);
                    value = min;
                }

                else if (x.Position.X >= 170)
                {
                    grib.Margin = new Thickness(170, 0, 0, 0);
                    value = max;
                }

            RaiseCustomRoutedEvent();
        }

        private void Rectangle_TouchMove_1(object sender, TouchEventArgs e)
        {
            TouchPoint x = e.GetTouchPoint(this);
            if(_in)
                if (x.Position.X > 25 && x.Position.X < 170)
                {
                    grib.Margin = new Thickness(x.Position.X, 0, 0, 0);
                    value = min + ((x.Position.X - 25) / 145) * (max - min);
                }
                else if(x.Position.X <= 25)
                {
                    grib.Margin = new Thickness(25, 0, 0, 0);
                    value = min;
                }

                else if(x.Position.X >= 170)
                {
                    grib.Margin = new Thickness(170, 0, 0, 0);
                    value = max;
                }

            RaiseCustomRoutedEvent();

        }

        private void Rectangle_TouchUp(object sender, TouchEventArgs e)
        {
            _in = false;
        }
    }
}
