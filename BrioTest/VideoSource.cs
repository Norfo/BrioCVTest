using OpenCvSharp;
using System;
using System.Diagnostics;

namespace BrioTest
{
    /// <summary>
    /// This class is not necessary in this application, but at the start of it developing I wanted to grab data from my cam to check coord calculator.
    /// </summary>
    public class VideoSource : ISourceable
    {
        private string mPath;
        private bool mHaveNewFrame;
        private VideoCapture mCapture;

        public VideoSource(string path) :
            this(path, 2200)
        { }

        public VideoSource(string path, int startFrameNumber)
        {
            mPath = path;
            if (mPath != null)
            {
                try
                {
                    mCapture = new VideoCapture(path);
                    mCapture.Set(CaptureProperty.PosFrames, startFrameNumber);
                    mHaveNewFrame = true;
                }
                catch (Exception)
                {
                    Debug.WriteLine("Can not open this video file");
                    mHaveNewFrame = false;
                    throw;
                }
            }
            else
            {
                mHaveNewFrame = false;
            }
        }

        public Mat GetFrame()
        {
            Mat frame = new Mat();
            if (mCapture.Read(frame))
            {
                return frame;
            }
            else
            {
                mHaveNewFrame = false;
                return null;
            }
        }

        public bool HaveNewFrame()
        {
            return mHaveNewFrame;
        }
    }
}
