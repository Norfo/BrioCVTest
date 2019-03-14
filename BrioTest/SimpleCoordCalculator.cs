using OpenCvSharp;
using System.Collections.Generic;

namespace BrioTest
{
    /// <summary>
    /// This is an implementation of simple method of camera calibration.
    /// </summary>
    public class SimpleCoordCalculator : ICoordCalculator
    {
        private double[] mRotVectorArray;
        private double[] mTrVectorArray;
        private double[,] mCameraMatrixArray;
        private double[] mDistCoefs;

        public double[] GetCameraPosition()
        {
            return mTrVectorArray;
        }

        public double[] GetCameraRotation()
        {
            return mRotVectorArray;
        }

        public void Process(List<Point3f> worldPoints, List<Point2f> imagePoints, Size size)
        {
            mDistCoefs = new double[5];
            mCameraMatrixArray = new double[3, 3];
            Size screenSize = size;
            Vec3d[] rotVecs = new Vec3d[1];
            Vec3d[] trVecs = new Vec3d[1];

            double errorValue = Cv2.CalibrateCamera(new List<List<Point3f>>() { worldPoints },
                                                    new List<List<Point2f>>() { imagePoints },
                                                    screenSize,
                                                    mCameraMatrixArray,
                                                    mDistCoefs,
                                                    out rotVecs,
                                                    out trVecs,
                                                    CalibrationFlags.ZeroTangentDist | CalibrationFlags.FixK1 | CalibrationFlags.FixK2 | CalibrationFlags.FixK3);


            mRotVectorArray = new double[3] { rotVecs[0].Item0, rotVecs[0].Item1, rotVecs[0].Item2 };
            mTrVectorArray = new double[3] { trVecs[0].Item0, trVecs[0].Item1, trVecs[0].Item2 };
        }

        public Point2f ProjectPoint(Point3f point)
        {
            Point2f[] points2d;
            List<Point3f> points3d = new List<Point3f> { point };
            double[,] jac = new double[3, 3];
            Cv2.ProjectPoints(points3d, mRotVectorArray, mTrVectorArray, mCameraMatrixArray, mDistCoefs, out points2d, out jac);
            return points2d[0];
        }
    }
}
