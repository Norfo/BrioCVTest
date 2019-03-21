using OpenCvSharp;
using System.Collections.Generic;

namespace BrioTest
{
    interface IPreProcessor
    {
        Mat Process(Mat frame);
    }
}
