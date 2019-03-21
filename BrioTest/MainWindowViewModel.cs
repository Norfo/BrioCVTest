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

        public ICommand CountAutomaticallyCommand
        {
            get { return new RelayCommand((object obj) => CountAutomatically()); }
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

            if (source.HaveNewFrame())
            {
                mFrame = source.GetFrame();
                FrameNumber++;
                mWin.ShowImage(mFrame);
                Cv2.WaitKey(33);
            }
        }

        private void CountAutomatically()
        {
            if (mFrame == null)
            {
                return;
            }
            IPreProcessor preProcessor = new KMeansPreProcessor();
            Mat preProcced = preProcessor.Process(mFrame.Clone());
            ContourAnalyzer contourAnalyzer = new ContourAnalyzer(preProcced);

            mImagePoints = contourAnalyzer.GetCorners();
            for (int i = 0; i < mImagePoints.Count; i++)
            {
                if (i == mImagePoints.Count -1)
                {
                    mFrame.Line(mImagePoints[0], mImagePoints[i], Scalar.Red);
                }
                else
                {
                    mFrame.Line(mImagePoints[i], mImagePoints[i + 1], Scalar.Red);
                }

                mFrame.PutText(i.ToString(), mImagePoints[i], HersheyFonts.HersheySimplex, 1, Scalar.Red);
            }

            Process();

            mWin.ShowImage(mFrame);
            Cv2.WaitKey(33);
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
            Point3f pointZero;
            Point3f pointX;
            Point3f pointY;
            Point3f pointZ;
            if (IsLandscapePaperOrientation)
            {
                pointZero = new Point3f(148.5f, 105, 0);
            }
            else
            {
                pointZero = new Point3f(105, 148.5f, 0);
            }

            int axesLenght = 30;
            pointX = new Point3f(pointZero.X + axesLenght, pointZero.Y, pointZero.Z);
            pointY = new Point3f(pointZero.X, pointZero.Y + axesLenght, pointZero.Z);
            pointZ = new Point3f(pointZero.X, pointZero.Y, pointZero.Z - axesLenght);

            Point2f projectedZero = calculator.ProjectPoint(pointZero);
            Point2f projectedX = calculator.ProjectPoint(pointX);
            Point2f projectedY = calculator.ProjectPoint(pointY);
            Point2f projectedZ = calculator.ProjectPoint(pointZ);

            mFrame.Line(projectedZero, projectedX, Scalar.Red);
            mFrame.Line(projectedZero, projectedY, Scalar.Green);
            mFrame.Line(projectedZero, projectedZ, Scalar.Blue);
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
    }
}
