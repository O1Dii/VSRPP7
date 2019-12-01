using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VSRPP5
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Thread> threads;
        private SynchronizationContext context;
        private static object locker = new object();
        private static Point point;
        private static Point lastPoint;
        private static Brush brush;

        public MainWindow()
        {
            InitializeComponent();

            threads = new List<Thread>();

            context = SynchronizationContext.Current;

            for (int i = 0; i < 3; i++)
            {
                Thread thread = new Thread(this.Work);
                Tuple<SynchronizationContext, int> tuple = new Tuple<SynchronizationContext, int>(context, i);
                thread.Start(tuple);
                threads.Add(thread);
            }
        }

        private void Work(object param)
        {
            while (true)
            {
                SynchronizationContext context = ((Tuple<SynchronizationContext, int>)param).Item1;
                int threadId = ((Tuple<SynchronizationContext, int>)param).Item2;

                lock(locker)
                {
                    if (threadId == 0)
                    {
                        brush = Brushes.Red;
                    }
                    if (threadId == 1)
                    {
                        brush = Brushes.Blue;
                    }
                    if (threadId == 2)
                    {
                        brush = Brushes.Green;
                    }

                    if (point != lastPoint)
                    {
                        context.Send(this.DrawEllipse, threadId);
                    }
                }
                
                Thread.Sleep(100);
            }
        }

        private void DrawEllipse(object e)
        {
            if (point.X > 0 && point.Y > 0)
            {
                Ellipse ellipse = new Ellipse()
                {
                    Height = 20,
                    Width = 20,
                    Fill = brush,
                    Margin = new Thickness(
                        point.X - 10,
                        point.Y - 10,
                        0, 0)
                };

                MainCanvas.Children.Add(ellipse);
            }

            lastPoint = point;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            point = e.GetPosition(this);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (Thread thread in threads)
            {
                thread.Abort();
            }
        }
    }
}
