using OpenCvSharp;
using System.Collections.Generic;

namespace BrioTest
{
    interface ICoordCalculator
    {
        void Process(List<Point3f> worldPoints, List<Point2f> imagePoints, Size size);
        double[] GetCameraPosition();
        double[] GetCameraRotation();
        Point2f ProjectPoint(Point3f point);
    }
}
