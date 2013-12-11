using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using NLog;
using OpenCV.Net;
using Point = System.Drawing.Point;

namespace Admo.classes
{
    public class BodyOutline
    {

        private static Logger Log = LogManager.GetCurrentClassLogger();
        private readonly Mat _edges = new Mat(Constants.KinectHeight, Constants.KinectWidth, Depth.U8, 1);
        public const int DistanceFromuser = 100;
        private Mat _corImage = new Mat(Constants.KinectHeight, Constants.KinectWidth, Depth.U8, 1);
        /// <summary>
        /// Generates an svg path of the based on the depth data from a kinect
        /// </summary>
        /// <param name="corFrame">A byte array of the kinect colorFram</param>
        /// <param name="colorState">An internal object representing the color</param>
        /// <returns>
        /// Returns a sting representing an SVG path
        /// </returns>
        public string GetUserOutline(byte[] corFrame, KinectcolorState colorState)
        {

            //Get byte array of where player is

            _corImage = Mat.FromArray(corFrame);

            _corImage = _corImage.Reshape(4, Constants.KinectHeight);

            const int thresh = 70;
            //Actual edge detection thresholds and kernel width can be further optimised to increase the visual quality of the edges
            CV.Canny(_corImage, _edges, thresh, thresh * 3, 3);
            //Gets all the contours from the edges
            var allContours = GetAllContours(_edges);

            //Turns the contours into an SVG string
            var svgContours = TurnContoursIntoPaths(allContours, colorState, 0, 0, 2.25f, 2.25f);

            //cleanup 

            foreach (Seq seq in allContours)
            {
                seq.Dispose();
            }
            return svgContours;
        }

        ~BodyOutline()
        {
            // class destrutor
            _corImage.Dispose();
            _edges.Dispose();
        }

        /// <summary>
        /// Finds all the contours in an image 
        /// </summary>
        /// <param name="image">Matrix containing a 8bit single channel single depth image</param>
        /// <returns>
        /// Returns a flat list of sequences each representing the points in a contour
        /// </returns>
        public static List<Seq> GetAllContours(Mat image)
        {
            var mem = new MemStorage();
            var allContours = new List<Seq>();

            var contScan = CV.StartFindContours(image, mem, Contour.HeaderSize, ContourRetrieval.External,
            ContourApproximation.ChainApproxSimple, new OpenCV.Net.Point(0, 0));
            var contSeq = contScan.FindNextContour();
            while (contSeq != null && !contSeq.IsInvalid)
            {
                allContours.Add(contSeq);
                contSeq = contScan.FindNextContour();
            }
            contScan.EndFindContours();
            contScan.Dispose();
            return allContours;

        }

        /// <summary>
        /// Generates svg path from a list of contour
        /// </summary>
        /// <param name="allSeqs">A list of all the sequences of eack path</param>
        /// <param name="colorState">The state of all the skeleton colorImage points</param>
        /// <returns>
        /// Returns an svg path string of all the contours 
        /// </returns>
        public static string TurnContoursIntoPaths(List<Seq> allSeqs, KinectcolorState colorState, int biasX = 0, int biasY = 0, float scaleX = 1, float scaleY = 1)
        {

            var svgString = new StringBuilder();

            //Display just the largest sequence

            svgString = (from seq in allSeqs
                         select seq.ToArray<Point>() into points
                         let end = points.Length
                         where end > 20
                         where CheckIfCloseToBody(colorState.AllTogether, points)
                         select ScaleArray(points, biasX, biasY, scaleX, scaleY)).Aggregate(svgString, (current, points) => GenerateCurve(points, current, 3));


            return svgString.ToString();
        }

        ///  <summary>
        ///  Scales a set of 2d points with a bias and a scale
        ///  </summary>
        ///  <param name="points">An array of system.drawing.Drawing2D points</param>
        /// <param name="biasX">Value added to the X coordinate</param>
        /// <param name="biasY">Value added to the Y coordinate</param>
        /// <param name="scaleX">Value the X coordinate is multiplied by</param>
        /// <param name="scaleY">Value the Y coordinate is multiplied by</param>
        /// <returns>
        /// returns transformed set of points
        ///  </returns>
        public static Point[] ScaleArray(Point[] points, int biasX = 0, int biasY = 0, float scaleX = 1, float scaleY = 1)
        {

            for (var i = 0; i < points.Length; i++)
            {
                points[i].X = (int)(points[i].X * scaleX + biasX);
                points[i].Y = (int)(points[i].Y * scaleY + biasY);
            }

            return points;
        }

        /// <summary>
        /// Generates an svg curve from a array of points
        /// </summary>
        /// <param name="points">An array of system.drawing.Drawing2D points</param>
        /// <param name="strBuild">StrinfgBuilder to be appended</param>
        /// <param name="sample">The sampling of the path</param>
        /// <returns>
        /// The same stringbuilder recieved with Appended path string
        /// </returns>
        public static StringBuilder GenerateCurve(Point[] points, StringBuilder strBuild, int sample = 1)
        {
            int end = points.Length;

            GetStartOint(points[0], strBuild);

            for (var i = 0; i < end - sample * 2; i += sample * 3)
            {

                strBuild.Append("C");
                strBuild.Append((points[i + 1 * sample].X));
                strBuild.Append(",");
                strBuild.Append((points[i + 1 * sample].Y));
                strBuild.Append(",");
                strBuild.Append((points[i].X));
                strBuild.Append(",");
                strBuild.Append((points[i].Y));
                strBuild.Append(",");
                strBuild.Append((points[i + 2 * sample].X));
                strBuild.Append(",");
                strBuild.Append((points[i + 2 * sample].Y));

            }

            if ((uint)(Math.Abs(points[0].X - points[end - 1].X) + Math.Abs(points[0].Y - points[end - 1].Y)) < 10)
            {
                strBuild.Append("z"); //close the string
            }
            return strBuild;
        }


        /// <summary>
        /// Checks if a set of point are within range of the skeleton
        /// </summary>
        /// <param name="st">An array of system.drawing.Drawing2D points</param>
        /// <returns>
        /// Return true if contour is near to any of the skeleton points
        /// </returns>
        public static bool CheckIfCloseToBody(List<ColorImagePoint> st, Point[] pts)
        {
            return (from t in st
                    where t.X >= -32768 && t.Y >= -32768
                    where Math.Abs(t.X) <= Constants.KinectWidth && Math.Abs(t.Y) <= Constants.KinectHeight
                    select pts.Aggregate<Point, uint>(0, (current, t1) => current + (uint)(Math.Abs(t1.X - t.X) + Math.Abs(t1.Y - t.Y))) into distance
                    select distance / pts.Length).Any(average => average < DistanceFromuser);
        }

        /// <summary>
        /// Generates an svg curve from
        /// </summary>
        /// <param name="points">An array of system.drawing.Drawing2D points</param>
        /// <param name="strBuild">StrinfgBuilder to be appended</param>
        /// <param name="sample">The sampling of the path</param>
        /// <returns>
        /// The same stringbuilder recieved with Appended path string
        /// </returns>
        public static StringBuilder GenerateLine(Point[] points, StringBuilder strBuild, int sample = 1)
        {
            var end = points.Length;

            GetStartOint(points[0], strBuild);
            for (var i = 0; i < end - sample + 1; i += sample)
            {
                strBuild.Append("L");
                strBuild.Append((points[i].X));
                strBuild.Append(",");
                strBuild.Append((points[i].Y));
            }
            if ((uint)(Math.Abs(points[0].X - points[end - 1].X) + Math.Abs(points[0].Y - points[end - 1].Y)) < 10)
            {
                strBuild.Append("z"); //close the string
            }
            return strBuild;
        }

        /// <summary>
        /// Generates an svg Bezier curve from a set of 
        /// </summary>
        /// <param name="points">An array of system.drawing.Drawing2D points</param>
        /// <param name="strBuild">StrinfgBuilder to be appended</param>
        /// <param name="sample">The sampling of the path</param>
        /// <returns>
        /// The same stringbuilder recieved with Appended path string
        /// </returns>
        public static StringBuilder GenerateBezier(Point[] points, StringBuilder strBuild, int sample = 1)
        {
            var end = points.Length;

            GetStartOint(points[0], strBuild);
            for (var i = 1; i < end - sample + 1; i += sample)
            {
                strBuild.Append("T");
                strBuild.Append((points[i].X));
                strBuild.Append(",");
                strBuild.Append((points[i].Y));
            }

            if ((uint)(Math.Abs(points[0].X - points[end - 1].X) + Math.Abs(points[0].Y - points[end - 1].Y)) < 10)
            {
                strBuild.Append("z"); //close the string
            }

            return strBuild;
        }

        private static void GetStartOint(Point point, StringBuilder strBuild)
        {
            strBuild.Append("M");
            strBuild.Append((point.X));
            strBuild.Append(",");
            strBuild.Append((point.Y));
        }
    }

    public struct KinectcolorState
    {
        public ColorImagePoint Head;
        public ColorImagePoint HandRight;
        public ColorImagePoint HandLeft;
        public ColorImagePoint ElbowRight;
        public ColorImagePoint ElbowLeft;
        public ColorImagePoint ShoulderRight;
        public ColorImagePoint ShoulderLeft;
        public ColorImagePoint ShoulderCenter;
        public ColorImagePoint Spine;
        public ColorImagePoint WristRight;
        public ColorImagePoint WristLeft;

        public List<ColorImagePoint> AllTogether
        {
            get
            {
                var list = new List<ColorImagePoint>
                {
                    Head,
                    HandRight,
                    HandLeft,
                    ElbowRight,
                    ElbowLeft,
                    ShoulderRight,
                    ShoulderLeft,
                    ShoulderCenter,
                    Spine,
                    WristRight,
                    WristLeft
                };

                return list;
            }

        }

        public KinectcolorState(CoordinateMapper cm, Skeleton first)
        {

            //This should have been done in a list.
            Head = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.Head].Position,
                ColorImageFormat.RgbResolution640x480Fps30);
            HandRight = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.HandRight].Position,
                ColorImageFormat.RgbResolution640x480Fps30);
            HandLeft = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.HandLeft].Position,
                ColorImageFormat.RgbResolution640x480Fps30);
            ElbowRight = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.ElbowRight].Position,
                ColorImageFormat.RgbResolution640x480Fps30);
            ElbowLeft = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.ElbowLeft].Position,
                ColorImageFormat.RgbResolution640x480Fps30);
            ShoulderRight = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.ShoulderRight].Position,
                ColorImageFormat.RgbResolution640x480Fps30);
            ShoulderLeft = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.ShoulderLeft].Position,
                ColorImageFormat.RgbResolution640x480Fps30);
            ShoulderCenter = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.ShoulderCenter].Position,
                 ColorImageFormat.RgbResolution640x480Fps30);
            Spine = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.Spine].Position,
                 ColorImageFormat.RgbResolution640x480Fps30);
            WristRight = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.WristRight].Position,
                 ColorImageFormat.RgbResolution640x480Fps30);
            WristLeft = cm.MapSkeletonPointToColorPoint(first.Joints[JointType.WristLeft].Position,
                 ColorImageFormat.RgbResolution640x480Fps30);




        }
    }
}
