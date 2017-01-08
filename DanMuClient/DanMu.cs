using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace DanMuClient
{
    /// <summary>
    /// Interactive logic of DanMu Canvas
    /// </summary>
    public class OutlinedDanMu : OutlinedTextBlock
    {
        public OutlinedDanMu(string text)
        {
            FontSize = 40;
            Text = text;
            Fill = Brushes.Red;
            Stroke = Brushes.Black;

            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));
        }
    }

    public class ShadowDanMu : TextBlock
    {
        //TODO: Optimizing the performance of DanMu animation with shadow.
        public ShadowDanMu(string text)
        {
            FontSize = 36;
            Text = text;
            Foreground = Brushes.White;

            Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 2,
                ShadowDepth = 1,
                Opacity = 1,
                RenderingBias = RenderingBias.Performance
            };

            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));
        }
    }


    public class DanMuManager
    {
        private Grid container;

        private int lineHeight = 48;
        private int paddingTop = 8;

        private Boolean[] isOccupy;
        private Boolean enableShadowEffect;
        private int lines;

        public int usableLine()
        {
            for (int line = 0; line < lines; line += 1)
            {
                if (!isOccupy[line])
                {
                    isOccupy[line] = true;
                    return line;
                }
            }
            return -1;
        }

        public void clearLine()
        {
            for (int line = 0; line < lines; line += 1)
            {
                isOccupy[line] = false;
            }
        }

        public int lineLocationY(int line)
        {
            return (line * lineHeight) + paddingTop;
        }

        public DanMuManager(Grid grid, bool enableShadow)
        {
            container = grid;

            lines = (int)(container.RenderSize.Height / lineHeight) - 1;
            isOccupy = new Boolean[lines];

            enableShadowEffect = enableShadow;
        }

        public void Shoot(string text, FrameworkElement DanMu, int entrynum, int speed)
        {
            var line = usableLine();

            if (line == -1)
            {
                clearLine();
                line = usableLine();
            }

            // DanMu initilization and display
            if (enableShadowEffect)
            {
                DanMu = new ShadowDanMu(text);
            }

            //计算弹道间隔
            int screenHeight = (int)System.Windows.SystemParameters.PrimaryScreenHeight;
            int step = (int)screenHeight / entrynum;
            int[] y_offset = new int[entrynum];
            for (int i = 0; i < entrynum; i++)
            {
                y_offset[i] = step * i;
            }

            Random random = new Random();
            int y = random.Next(1000);
            DanMu.Margin = new Thickness(0, y_offset[y%entrynum] + 50, 0, 0); 
            container.Children.Add(DanMu);

            // Initilizing animation
            var anim = new DoubleAnimation();
            anim.From = this.container.RenderSize.Width;
            anim.To = -DanMu.DesiredSize.Width - 1600;
            anim.SpeedRatio = speed>0 ? 0.03*speed : 0.03/(-speed+1);
            TranslateTransform trans = new TranslateTransform();
            DanMu.RenderTransform = trans;

            // Handling the end of DanMu
            anim.Completed += new EventHandler(delegate (Object o, EventArgs a) {
                container.Children.Remove(DanMu);
            });

            // Managing the DanMu lines
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(300);
            timer.Tick += new EventHandler(delegate (Object o, EventArgs a) {
                Point relativePoint = DanMu.TransformToAncestor(container)
                          .Transform(new Point(0, 0));
                if (relativePoint.X < container.ActualWidth - DanMu.DesiredSize.Width - 50)
                {
                    timer.Stop();
                    isOccupy[line] = false;
                }
            });
            timer.Start();

            // Play animation
            trans.BeginAnimation(TranslateTransform.XProperty, anim);
        }
    }
}
