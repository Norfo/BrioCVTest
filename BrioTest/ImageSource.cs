using OpenCvSharp;
using System;
using System.Diagnostics;

namespace BrioTest
{
    /// <summary>
    /// This class provide to open and keep image data of selected file.
    /// </summary>
    public class ImageSource : ISourceable
    {
        private string mPath;
        private Mat mSourceImage;
        private bool mHaveNewFrame;

        public ImageSource(string path) :
            this(path, ImreadModes.Color)
        {
        }

        public ImageSource(string path, ImreadModes readMode)
        {
            mPath = path;
            if (mPath != null)
            {
                try
                {
                    mSourceImage = new Mat(path, readMode);
                    mHaveNewFrame = true;
                }
                catch (Exception)
                {
                    Debug.WriteLine("Can not open this image");
                    mHaveNewFrame = true;
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
            mHaveNewFrame = false;
            return mSourceImage;
        }

        public bool HaveNewFrame()
        {
            return mHaveNewFrame;
        }
    }
}
