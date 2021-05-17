using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Fiz
{

    public partial class MainWindow : Window
    {
        public class SvgCreator
        {
            public string content = "";

            public void DrwaLine(Line line, float xSchift = 0, float ySchift = 0 )
            {
                Color color = ((Color)line.Stroke.GetValue(SolidColorBrush.ColorProperty));
                content += 
                    "<line " +
                    $"x1 = \"{(line.X1+ xSchift).ToString("0.000").Replace(",", ".")}\" " +
                    $"y1 = \"{(line.Y1+ ySchift).ToString("0.000").Replace(",", ".")}\" " +
                    $"x2 = \"{(line.X2+ xSchift).ToString("0.000").Replace(",", ".")}\" " +
                    $"y2 = \"{(line.Y2 + ySchift).ToString("0.000").Replace(",", ".")}\" " +
                    $"style = \"stroke:rgb({color.R},{color.G},{color.B}); " +
                    $"stroke-width:{line.StrokeThickness}\" /> \n";
            }
            public void DrawCircle(float x, float y, int r,Color col)
            {
                content += 
                    $"<circle " +
                    $"cx = \"{x.ToString("0.000").Replace(",", ".")}\" " +
                    $"cy = \"{y.ToString("0.000").Replace(",", ".")}\" " +
                    $"r = \"{r}\" " +
                    $"fill = \"rgb({col.R},{col.G},{col.B})\" /> \n";
            }


            public void SaveSvg(string path,int hegiht,int width)
            {       
             
                string contentFinal =      
                  "<?xml version = \"1.0\" encoding = \"UTF-8\" standalone = \"no\" ?>\n" +
                  "<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 20010904//EN\" \n" +
                  "\"http://www.w3.org/TR/2001/REC-SVG-20010904/DTD/svg10.dtd\">\n" +
                   $"<svg width=\"{width}\" height=\"{hegiht}\"  xmlns = \"http://www.w3.org/2000/svg\">\n"
                    + content +
                  "\n</svg>";
                System.IO.File.WriteAllText(path, contentFinal, Encoding.UTF8);

            } 
        }

        public class Graph
        {
            public double scaleY;
            public double scaleX;
            public double zeroLine;
            
            public double[] samplesArray;
            public Line[]   linesArray;

            public Line     nullLine = null;

            public delegate double Function(double x);
            public delegate double FunctionN(int x);

            public Graph(Grid graphBox, int samples,Brush color,double xAxisRange, bool enableZeroLine = false)
            {
                // scaleXRend:
                // this is for no math implication, splitting graphBox width to linear steps, 
                // so the amount of samples multiplied steps fill all graph box in width

                double scaleXRend = graphBox.Width / (samples-1);

                zeroLine = graphBox.Height / 2; // default zero value is in center of height of graphBox


                scaleX = (xAxisRange / (samples - 1));//this is the range that has the X axis 
                scaleY = graphBox.Height / 4;//default y-axis scale f(x)=y -> scaled = 1/4 graphBox Hegiht * y


                samplesArray = new double[samples];
                linesArray = new Line[samples - 1];
                if (enableZeroLine)
                {
                    var line = new Line();
                    line.Stroke = Brushes.Black;
                    line.X1 = 0;
                    line.X2 = graphBox.Width;
                    line.Y1 = zeroLine;
                    line.Y2 = zeroLine;
                  
                    graphBox.Children.Add(line);
                    nullLine = line;
                }

                for (int i = 0; i < samples-1; i++)
                {

                    var line = new Line();
                    line.Stroke = color;

                    line.X1 = (i)   * scaleXRend;
                    line.X2 = (i+1) * scaleXRend;

                    line.Y1 = zeroLine; 
                    line.Y2 = zeroLine;
                    line.StrokeThickness = 2;


                    linesArray[i] = line;
                    graphBox.Children.Add(line);

                }
                
            }
       
            public void SetLines()
            {
                for (int i = 0; i < linesArray.Length; i++)
                {
                    linesArray[i].Y1 = samplesArray[i];
                    linesArray[i].Y2 = samplesArray[i + 1];
                }
            }

            public void GenGraph(Function function, double schiftX)
            {
                for (int i = 0; i < samplesArray.Length; i++)
                    samplesArray[i] = (function((scaleX * i) + schiftX) * scaleY) + zeroLine;

                SetLines();
            }
            public void GenGraph(Function function)
            {
                for (int i = 0; i < samplesArray.Length; i++)
                    samplesArray[i] = (function((scaleX * i)) * scaleY) + zeroLine;

                SetLines();
            }
            public void GenGraphNoScaled(FunctionN function, double schiftY)
            {
                for (int i = 0; i < samplesArray.Length ; i++)
                    samplesArray[i] = function(i) + schiftY;
                SetLines();
            }
        }
       
        public class Arrow
        {
            private float y;
            private float x;
            private float hegiht;
            private Line body, lHead, rHead;

            public float X 
            { 
                get => x; 
                set 
                { 
                    x = value;
                    refreshArrow(); 
                }
            }
            public float Y 
            { 
                get => y; 
                set 
                {
                    y = value;
                    refreshArrow();
                }
            }

            public float Hegiht 
            { 
                get => hegiht; 
                set 
                {
                    hegiht = value;
                    refreshArrow();
                }
            }

            private void refreshArrow()
            {
                body.X1 = x; body.Y1 = y;
                body.X2 = x; body.Y2 = y + hegiht;

                float v = (hegiht > 0) ? -10 : 10;

                lHead.X1 = x; lHead.Y1 = y+hegiht + 0.5;
                lHead.X2 = x - 5; lHead.Y2 = y + hegiht + v;

                rHead.X1 = x; rHead.Y1 = y+hegiht + 0.5;
                rHead.X2 = x + 5; rHead.Y2 = y + hegiht + v;
            }

            public Arrow(Grid parent,float startX, float startY, float Height, float thiness, Brush brush )
            {
                body = new Line(); 
                lHead = new Line(); 
                rHead = new Line();
                body.Stroke = lHead.Stroke = rHead.Stroke = brush;
                body.StrokeThickness = lHead.StrokeThickness = rHead.StrokeThickness = thiness;
                
                hegiht = Height;
                x = startX;
                y = startY;

                refreshArrow();

                parent.Children.Add(body);
                parent.Children.Add(lHead);
                parent.Children.Add(rHead);
            }
        }

        Graph graph1, graph2, graph3;
        Arrow[] arrows;

        public const double xAxisRange = (Math.PI * 5);
        public const int    samplesCount = 150;

        
        public const bool enableSorceWaves              = true;
        public const bool enableStandingWave            = true;
        public const bool enableVectorsOfEletricalForce = true;

        //recordnig config
        public const bool   enableSavingSvgFrames       = true;
        public const string saveDirLocation             = "frames3";
        public const int    framesRecoredCount          = 200;
        public const int    horizotnalMargin            = 0;

        public const bool   enableNodesDots             = false; // only in recorded framses

        private int waveCycles = (int)(xAxisRange / (Math.PI));

        public MainWindow()
        {
            InitializeComponent();

            if (enableSorceWaves)
            {
                graph1 = new Graph(GridBox, samplesCount, Brushes.Green, xAxisRange, true);
                graph2 = new Graph(GridBox, samplesCount, Brushes.Red, xAxisRange);
            }

            if (enableStandingWave)
            {
                graph3 = new Graph(GridBox, samplesCount, Brushes.Black, xAxisRange,!enableSorceWaves);//Yellow

                //if (enableVectorsOfEletricalForce)
                //{              
                //    float sh = (float)(GridBox.Width / (waveCycles * 2));
                //    arrows = new Arrow[waveCycles];

                //    for (int i = 0; i < waveCycles; i++)
                //        arrows[i] = new Arrow(GridBox, (float)((GridBox.Width / waveCycles) * i) + sh, (float)graph3.zeroLine, 0, 2, Brushes.BlueViolet);
                //}

            }

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            dispatcherTimer.Start();

        }
        int T = 0;
                    
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            double Tscaled = -T * (Math.PI / 100) + (Math.PI / 2);
            double TscaledRevrse = T * (Math.PI / 100) + (Math.PI / 2);

            if (enableSorceWaves)
            {
                graph1.GenGraph((x) => Math.Sin(x) / 2, Tscaled);// (x)=>{} to wyrażenie lambda czyli funkcja bez nazwy którą przekzujemy dalej 
                graph2.GenGraph((x) => -Math.Sin(x) / 2, TscaledRevrse);
            }

            if (enableStandingWave)
           {
                graph3.GenGraph( (x) => (Math.Sin(x + Tscaled) / 2) + (-(Math.Sin(x + TscaledRevrse)) / 2), 0);

                //if (enableVectorsOfEletricalForce)
                //{                 
                //    for (int i = 0; i < waveCycles; i++)
                //    {
                //        arrows[i].Hegiht = (float)graph3.samplesArray[(samplesCount / (waveCycles * 2)) + (samplesCount / waveCycles) * i] - (float)(2 * graph3.scaleY);
                //        if (arrows[i].Hegiht > 0) arrows[i].Hegiht -= 1; else arrows[i].Hegiht += 1;
                //    }
                //}
           }

            if (enableSavingSvgFrames)
            {
                if (T < framesRecoredCount)
                {
                    SvgCreator myGrf = new SvgCreator();
                    foreach (var item in GridBox.Children)// dumping all children's contained in GridBox is should be only Line type
                        myGrf.DrwaLine((Line)item, horizotnalMargin, -50);

                    if(enableNodesDots)
                    {
                        for (int i = 0; i <= waveCycles; i++)
                            myGrf.DrawCircle((float)((GridBox.Width / waveCycles) * i) + horizotnalMargin, (float)graph3.zeroLine - 50, 5, Colors.Red);

                    }
                   

                    myGrf.SaveSvg($"{saveDirLocation}/{T.ToString("000")}frame.svg", (int)GridBox.Height, (int)GridBox.Width + (horizotnalMargin*2));
                    // myGrf.SaveSvg("MyFirstSvgGenrated.svg");

                }
            }
         
            T++;
  
        }
    }
}
