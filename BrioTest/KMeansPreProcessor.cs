using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace BrioTest
{
    /// <summary>
    /// This class make segmentation image by K-means method and then extract contour image.
    /// </summary>
    public class KMeansPreProcessor : IPreProcessor
    {
        private Mat mFrame;
        //The number of image segmentation classes. 
        private int mClustNum = 2;
        //The size of bluring filter window.
        private int mBlurWinWidth = 40;
        private int mBlurWinHeight = 40;

        public Mat Process(Mat frame)
        {
            mFrame = frame;

            Size blurWinSize = new Size(mBlurWinWidth, mBlurWinHeight);
            mFrame.Blur(blurWinSize);

            var reshaped = mFrame.Reshape(cn: 3, rows: mFrame.Rows * mFrame.Cols);
            var samples = new Mat();

            reshaped.ConvertTo(samples, MatType.CV_32FC3);

            var bestLabels = new Mat();
            var centers = new Mat();

            Cv2.Kmeans(samples,
                mClustNum,
                bestLabels,
                new TermCriteria(type: CriteriaType.Eps | CriteriaType.MaxIter, maxCount: 10, epsilon: 1.0),
                3,
                KMeansFlags.PpCenters,
                centers);

            //This is not optimal solution, but it works in this case.
            Mat clusteredImage = new Mat(mFrame.Rows, mFrame.Cols, mFrame.Type());
            for (var size = 0; size < mFrame.Cols * mFrame.Rows; size++)
            {
                var clusterIndex = bestLabels.At<int>(0, size);
                var newPixel = new Vec3b
                {
                    Item0 = (byte)(centers.At<float>(clusterIndex, 0)), // B
                    Item1 = (byte)(centers.At<float>(clusterIndex, 1)), // G
                    Item2 = (byte)(centers.At<float>(clusterIndex, 2)) // R
                };
                clusteredImage.Set(size / mFrame.Cols, size % mFrame.Cols, newPixel);
            }

            mFrame = clusteredImage;

            Mat gray = mFrame.CvtColor(ColorConversionCodes.BGR2GRAY);
            Mat thresh = gray.Threshold(140, 255, ThresholdTypes.Otsu);

            Mat erode = thresh.Erode(new Mat());
            Mat dilate = erode.Dilate(new Mat());

            Mat morph = new Mat();
            Cv2.BitwiseXor(erode, dilate, morph);

            return morph;         
        }
    }
}
