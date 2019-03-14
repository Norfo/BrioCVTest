using OpenCvSharp;

namespace BrioTest
{
    public interface ISourceable
    {
        Mat GetFrame();
        bool HaveNewFrame();
    }
}
