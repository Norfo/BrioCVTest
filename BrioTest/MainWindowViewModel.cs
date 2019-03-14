using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace BrioTest
{
    /// <summary>
    /// This class provide buisness logic of this application.
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private DefaultDialogService mDialogService;
        private SourceFabric mSourceFabric;
        private OpenCvSharp.Window mWin;
        private Mat mFrame;
        private List<Point2f> mImagePoints;
        private List<Point3f> mWorldPoints;

        public string XPos { get; set; }
        public string YPos { get; set; }
        public string ZPos { get; set; }
        public string Alpha { get; set; }
        public string Beta { get; set; }
        public string Gamma { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public int FrameNumber { get; set; }

        public bool IsLandscapePaperOrientation { get; set; } = false;

        public MainWindowViewModel()
        {
            mWin = new OpenCvSharp.Window("Source Window");
            CvMouseCallback callback = new CvMouseCallback(CvMouseMove);
            Cv2.SetMouseCallback("Source Window", callback);

            mImagePoints = new List<Point2f>();
            //Initialize source points. 
            //Top left corner of paper list taken as the origin of the coordinate system.
            //Bypassing the points is clockwise.
            mWorldPoints = new List<Point3f>() { new Point3f(0, 0, 0),
                                                      new Point3f(210, 0, 0),
                                                      new Point3f(210, 297, 0),
                                                      new Point3f(0, 297, 0)};

            ResultIsUnknown();
        }

        public ICommand OpenSourceCommand
        {
            get { return new RelayCommand((object obj) => OpenSource()); }
        }

        private void OpenSource()
        {
            mDialogService = new DefaultDialogService();
            if (mDialogService.OpenFileDialog())
            {
                mSourceFabric = new SourceFabric();
                ISourceable source = mSourceFabric.GetSourceObject(mDialogService.FilePath);
                mImagePoints.Clear();
                ResultIsUnknown();

                ShowFrame(source);
            }
        }

        private void ShowFrame(ISourceable source)
        {            
            FrameNumber = 0;

            while (source.HaveNewFrame())
            {
                mFrame = source.GetFrame();
                FrameNumber++;
                mWin.ShowImage(mFrame);
                Cv2.WaitKey(33);
            }
        }

        private void Process()
        {
            //If the paper list have landscape orientation we should change the source coordinates.
            if (IsLandscapePaperOrientation)
            {
                mWorldPoints = new List<Point3f>() { new Point3f(0,0,0),
                                                        new Point3f(297, 0, 0),
                                                        new Point3f(297, 210, 0),
                                                        new Point3f(0, 210, 0)};
            }

            ICoordCalculator coordCalculator = new SimpleCoordCalculator();
            coordCalculator.Process(mWorldPoints, mImagePoints, mFrame.Size());
            double[] posVec = coordCalculator.GetCameraPosition();
            double[] rotVec = coordCalculator.GetCameraRotation();
            ConvertResult(posVec, rotVec);
            CheckCenterPoint(coordCalculator);
        }

        private void CheckCenterPoint(ICoordCalculator calculator)
        {
            Point3f point;
            if (IsLandscapePaperOrientation)
            {
               point = new Point3f(148.5f, 105, 0);
            }
            else
            {
                point = new Point3f(105, 148.5f, 0);
            }
            Point2f projected = calculator.ProjectPoint(point);
            mFrame.DrawMarker((int)projected.X, (int)projected.Y, Scalar.Red);
        }

        private void ConvertResult(double[] trVec, double[] rotVec)
        {
            if (trVec == null || rotVec == null)
            {
                ResultIsUnknown();
            }
            else
            {
                double scale = 1000;
                string meterStr = " m";                
                XPos = (-trVec[0] / scale).ToString() + meterStr;
                YPos = (trVec[2] / scale).ToString() + meterStr;
                ZPos = (-trVec[1] / scale).ToString() + meterStr;

                string degreeStr = "°";
                double radCoef = 180 / Math.PI;
                Alpha = (rotVec[0] * radCoef).ToString() + degreeStr;
                Beta = (rotVec[2] * radCoef).ToString() + degreeStr;
                Gamma = (rotVec[1] * radCoef).ToString() + degreeStr;
            }
        }

        private void ResultIsUnknown()
        {
            string unknown = "?";
            XPos = unknown;
            YPos = unknown;
            ZPos = unknown;
            Alpha = unknown;
            Beta = unknown;
            Gamma = unknown;
        }

        private void CvMouseMove(OpenCvSharp.MouseEvent cvEvent, int x, int y, OpenCvSharp.MouseEvent flags)
        {
            if (mFrame == null)
            {
                return;
            }
            if (cvEvent == OpenCvSharp.MouseEvent.RButtonUp)
            {
                mFrame.DrawMarker(x, y, Scalar.Blue);
                mImagePoints.Add(new Point2f(x, y));

                int pointsNum = mImagePoints.Count;
                if (pointsNum == 4)
                {
                    mFrame.Line(mImagePoints[0], mImagePoints[3], Scalar.Aquamarine);
                    mFrame.Line(mImagePoints[2], mImagePoints[3], Scalar.Aquamarine);
                    Process();
                }
                if (pointsNum > 1)
                {
                    mFrame.Line(mImagePoints[pointsNum - 2], mImagePoints[pointsNum - 1], Scalar.Aquamarine);
                }
                if (pointsNum > 4)
                {
                    return;
                }

                mWin.ShowImage(mFrame);
                Cv2.WaitKey(33);
            }
        }

        //I wanted automatically detect paper list, but it is non-trivial task.
        //There are many ways do it automatically. 
        //The simpliest is to get paperlist contours by Canny or Threshold and morphological manipulations.
        //Unfortunatly this methods uses "magic" coefficients which can be differ for some images.
        //Therefore, I abandoned this idea.
        private void PreProcess()
        {
            Mat gray = mFrame.CvtColor(ColorConversionCodes.BGR2GRAY);
            Mat filtered = gray.BilateralFilter(11, 17, 17);
            Mat thresh = filtered.Threshold(155, 255, ThresholdTypes.Binary);
            Mat canny = thresh.Canny(10, 60);

            Point[][] contours = new Point[][] { };
            HierarchyIndex[] hierarchy = new HierarchyIndex[] { };

            Cv2.FindContours(canny, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

            List<Point[]> rect = new List<Point[]>();
            foreach (var item in contours)
            {
                var p = Cv2.ArcLength(item, true);
                var approx = Cv2.ApproxPolyDP(item, 0.05 * p, true);

                if (approx.Length == 4)
                {
                    rect.Add(approx);
                }
            }

            OrderPoints();

            for (int i = 0; i < rect.Count; i++)
            {
                Cv2.DrawContours(mFrame, rect, i, Scalar.Red);
            }
        }

        private void OrderPoints()
        { }
    }
}
