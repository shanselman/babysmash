using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BabySmash.Properties;

namespace BabySmash
{
    public partial class MainWindow : Window
    {
        private readonly Controller controller;
        public Controller Controller { get { return controller; } }

        public void AddUserControl(UserControl c)
        {
            this.mainCanvas.Children.Add(c);
        }

        public MainWindow(Controller c)
        {
            this.controller = c;
            this.DataContext = controller;
            InitializeComponent();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
           base.OnMouseWheel(e);
           controller.MouseWheel(this, e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
           base.OnMouseUp(e);
           controller.MouseUp(this, e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
           base.OnMouseDown(e);
           controller.MouseDown(this, e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
           base.OnMouseMove(e);
           controller.MouseMove(this, e);
        }
        
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            e.Handled = true;
            controller.ProcessKey(this, e);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);
            controller.LostMouseCapture(this, e);
        }
    }
}