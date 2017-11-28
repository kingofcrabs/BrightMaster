using BrightMaster.Settings;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightMaster.data
{
    class CurveModel:BindableBase
    {
        /// <summary>
        /// Gets the plot model.
        /// </summary>
        static public  PlotModel CreateModel(List<float> vals,bool isX)
        {
            var plot = new PlotModel
            {
                Title = isX ?  "XCurves" :"YCurves"
            };
            double max, min;
            max = vals.Count;
            min = 0;

            var xAxis = new LinearAxis
            {
                Position = isX ? AxisPosition.Bottom : AxisPosition.Right,
                Minimum = min,
                Maximum = max,
                MajorStep = (max - min) / 5,
                MinorStep = (max - min) / 20,
                TickStyle = TickStyle.Inside
            };

           
            plot.Axes.Add(xAxis);

            max = vals.Max();
            min = 0;

            double tmpMax = max;
            int tens = 1;
            while(tmpMax > 10)
            {
                tmpMax /= 10;
                tens *= 10;
            }

            double inTen = max / tens;
            if( inTen > 5)
            {
                inTen = 10;
            }
            else if( inTen < 1.5)
            {
                inTen = 1.5;
            }
            else if( inTen < 2)
            {
                inTen = 2;
            }
            else
            {
                inTen = 5;
            }
            max = inTen * tens;

            var yAxis = new LinearAxis
            {
                Position = isX ? AxisPosition.Left : AxisPosition.Bottom,
                Minimum = min,
                Maximum = max,
                MajorStep = (max - min) / 5,
                MinorStep = (max - min) / 20,

                TickStyle = TickStyle.Inside,
            };
            if (!isX)
            {
                yAxis.StartPosition = 1;
                yAxis.EndPosition = 0;
            }
            plot.Axes.Add(yAxis);
            plot.Series.Add(CreateSeries(vals,isX));
            return plot;
        }

        static public void AdjustYAxis(PlotModel model,List<float>vals)
        {
            var max = vals.Max();
            double tmpMax = max;
            int tens = 1;
            while (tmpMax > 10)
            {
                tmpMax /= 10;
                tens *= 10;
            }

            float inTen = max / tens;
            if (inTen > 5)
            {
                inTen = 10;
            }
            else if (inTen < 1.5)
            {
                inTen = 1.5f;
            }
            else if (inTen < 2)
            {
                inTen = 2;
            }
            else
            {
                inTen = 5;
            }
            max = inTen * tens;
            model.Axes[1].Maximum = max;
            model.Axes[1].MajorStep = (max) / 5;
            model.Axes[1].MinorStep = (max) / 20;
        }

        static public LineSeries CreateSeries(List<float> vals,bool isX = true)
        {
            var ls = new LineSeries();

            for (int i = 0; i < vals.Count; i++)
            {
                double x,y;
                if(isX)
                {
                    x = i;
                    y = vals[i];
                }
                else
                {
                    x = vals[i];
                    y = i;
                }
                ls.Points.Add(new DataPoint(x, y));
            }
           
            return ls;
        }


        
    }


    
}
