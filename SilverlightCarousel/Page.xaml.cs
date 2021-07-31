using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace SilverlightCarousel
{
    public partial class Page : UserControl
    {
        public Page()
        {
            InitializeComponent();

            this.carousel1.SelectingItem += new CarouselCanvas.CarouselEventHandler(carousel1_SelectingItem);
            this.carousel1.DeselectedItem += new CarouselCanvas.CarouselEventHandler(carousel1_DeselectedItem);
        }

        void carousel1_DeselectedItem(object sender, CarouselCanvas.CarouselItemEventArgs e)
        {
            Button elm = e.OriginalSource as Button;
        }
        void carousel1_SelectedItem(object sender, CarouselCanvas.CarouselItemEventArgs e)
        {
            Button elm = e.OriginalSource as Button;
        }
        void carousel1_SelectingItem(object sender, CarouselCanvas.CarouselItemEventArgs e)
        {
            this.carousel1.ReverseRotation();

            Button elm = e.OriginalSource as Button;

           // string msg = "Selecting item " + elm.Content;

            //this.AddToList(msg);
        }

        private void AddToList(string message)
        {
            Debug.WriteLine(message);

            this.listBox.Items.Insert(0, message);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button clicked = sender as Button;

            if (clicked != null)
            {
                this.carousel1.SelectItem(clicked, true);
            }

            //this.carousel1.rotating = false;        
        }
        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            this.carousel1.Previous();

            //this.carousel1.rotating = false;
        }
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            this.carousel1.Next();
        //    this.carousel1.ReverseRotation();

        }
        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.carousel1 == null)
                return;

            this.carousel1.RadiusX = e.NewValue;
        }

        private void slider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.carousel1 == null)
                return;

            this.carousel1.RadiusY = e.NewValue;
        }

        private void slider3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.carousel1 == null)
                return;

            this.carousel1.MinOpacity = e.NewValue;
        }

        private void slider4_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.carousel1 == null)
                return;

            this.carousel1.TransitionDelay = e.NewValue;
        }

        private void slider5_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.carousel1 == null)
                return;

            this.carousel1.Elasticity = e.NewValue;
        }

        private void slider6_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.carousel1 == null)
                return;

            this.carousel1.Decelleration = e.NewValue;
        }

        private void slider7_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.carousel1 == null)
                return;

            this.carousel1.MinScale = e.NewValue;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            this.slider1.Value = 200;
            this.slider2.Value = 50;
            this.slider3.Value = 0.10;
            this.slider7.Value = 0.80;
            this.slider4.Value = 20;
            this.slider5.Value = 0;
            this.slider6.Value = 0;
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            this.slider1.Value = 200;
            this.slider2.Value = 50;
            this.slider3.Value = 0.10;
            this.slider7.Value = 0.80;
            this.slider4.Value = 0;
            this.slider5.Value = 0.8;
            this.slider6.Value = 0.2;
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            this.slider1.Value = 180;
            this.slider2.Value = 0;
            this.slider3.Value = 0.20;
            this.slider7.Value = 0.80;
            this.slider4.Value = 0;
            this.slider5.Value = 0.95;
            this.slider6.Value = 0.05;
        }

        private void carousel1_MouseLeave(object sender, MouseEventArgs e)
        {
            this.carousel1.RotationPauseToggle();
        }

        private void carousel1_MouseEnter(object sender, MouseEventArgs e)
        {
            this.carousel1.RotationPauseToggle();
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            this.carousel1.EnableRotation(this.btnRotation.IsChecked.Value);
        }

        private void btnOrientation_Click(object sender, RoutedEventArgs e)
        {
            this.btnOrientation.Content = this.carousel1.Orientation.ToString();

            var newOrientation = this.carousel1.Orientation == Orientation.Vertical ?
                Orientation.Horizontal :
                Orientation.Vertical;

            this.carousel1.Orientation = newOrientation;
        }

        private void btnOpenCarousel_Click(object sender, RoutedEventArgs e)
        {
            this.carousel1.Open();
        }
    }
}
