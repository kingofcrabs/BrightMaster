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

            
            
            plot.Axes.Add(new LinearAxis
            {
                Position = isX ? AxisPosition.Bottom : AxisPosition.Right,
                Minimum = min,
                Maximum = max,
                MajorStep = (max - min) / 5,
                MinorStep = (max - min) /20,
                TickStyle = TickStyle.Inside
            });

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
            else if( inTen < 2)
            {
                inTen = 2;
            }
            else
            {
                inTen = 5;
            }
            max = inTen * tens;


            plot.Axes.Add(new LinearAxis
            {
                Position = isX ? AxisPosition.Left : AxisPosition.Bottom,
                Minimum = min,
                Maximum = max,
                MajorStep = (max - min) / 5,
                MinorStep = (max - min) /20,
                TickStyle = TickStyle.Inside
            });
            plot.Series.Add(CreateSeries(vals,isX));
            return plot;
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
