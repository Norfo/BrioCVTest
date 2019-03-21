using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrioTest
{
    /// <summary>
    /// This class extract and trace contour line on input binary image.
    /// </summary>
    public class ContourAnalyzer
    {
        Mat mFrame;
        List<Point2f> mResult;

        public ContourAnalyzer(Mat frame)
        {
            mFrame = frame;
        }

        public List<Point2f> GetCorners()
        {
            if (mFrame != null)
            {
                Point[][] contours = new Point[][] { };
                HierarchyIndex[] hierarchy = new HierarchyIndex[] { };

                Cv2.FindContours(mFrame, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

                Point[] rect = new Point[] { };
                double maxLength = 0;
                foreach (var item in contours)
                {
                    var perimeter = Cv2.ArcLength(item, true);
                    var approx = Cv2.ApproxPolyDP(item, 0.015 * perimeter, true);

                    if (approx.Length == 4 && perimeter > maxLength)
                    {
                        maxLength = perimeter;
                        rect = approx;
                    }
                }

                mResult = new List<Point2f>();
                foreach (var item in rect)
                {
                    mResult.Add(new Point2f(item.X, item.Y));
                }
                
                OrderPoints();

                return mResult;
            }

            return null;
        }

        /// <summary>
        /// Cause points after the contour approximation by a rectangle are arranged in an unknown order, 
        /// they are ordered in the required form.
        /// </summary>
        private void OrderPoints()
        {

            Point2f lb = mResult.Aggregate((i1, i2) => i1.X < i2.X ? i1 : i2);
            Point2f rb = mResult.Aggregate((i1, i2) => i1.X > i2.X ? i1 : i2);

            mResult.Remove(lb);
            mResult.Remove(rb);

            Point lt = mResult[0].X < mResult[1].X ? mResult[0] : mResult[1];
            Point rt = mResult[0].X > mResult[1].X ? mResult[0] : mResult[1];

            mResult.Clear();
            mResult = new List<Point2f>() { lt, rt, rb, lb };
        }
    }
}
