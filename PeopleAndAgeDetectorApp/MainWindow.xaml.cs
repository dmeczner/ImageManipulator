using AgeDetectorApp.GoogleDrive;
using ImageManipulatorSharedCommon;
using OpenCvSharp;
using OpenCvSharp.Dnn;
using OpenCvSharp.WpfExtensions;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Window = System.Windows.Window;
using Rect = OpenCvSharp.Rect;
using Size = OpenCvSharp.Size;
using Point = OpenCvSharp.Point;

namespace AgeDetectorApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly VideoCapture _capture;
        private readonly CascadeClassifier _cascadeClassifier;
        private readonly Net? _ageNet;
        private readonly BackgroundWorker bkgWorker;


        public MainWindow()
        {
            InitializeComponent();

            #region Initialize Video Capture
            try
            {
                _capture = new VideoCapture();
                _cascadeClassifier = new CascadeClassifier(Constant.HAARCASCADE_FRONTALFACE_DEFAULT);
                FileDownloader fileDownloader = new();
                fileDownloader.DownloadFile(Constant.CAFFE_MODEL_URL, Constant.CAFFE_MODEL);
                _ageNet = CvDnn.ReadNetFromCaffe(Constant.DEPLOY_AGE, Constant.CAFFE_MODEL);

                bkgWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
                bkgWorker.DoWork += HandleVideoProcessing;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while try to init video capture {ex}");
            }
            #endregion Initialize Video Capture
        }

        public MainWindow(VideoCapture object1, CascadeClassifier object2, Net object3)
        {
            _capture = object1;
            _cascadeClassifier = object2;
            _ageNet = object3;
        }

        public void DetectAgeOnPicture(Mat frame, Rect face)
        {
            try
            {
                // Prepare the face for age estimation
                var faceRegion = new Mat(frame, face);
                Cv2.Resize(faceRegion, faceRegion, new Size(227, 227));
                var blob = CvDnn.BlobFromImage(faceRegion, 1.0, new Size(227, 227), new Scalar(104, 117, 123), false, false);

                if (_ageNet is not null)
                {
                    // Estimate age
                    _ageNet.SetInput(blob);
                    var agePreds = _ageNet.Forward();
                    Cv2.MinMaxLoc(agePreds, out _, out _, out _, out Point maxLoc);
                    var age = Constant.AGES[maxLoc.X];

                    // Put the estimated age on the frame
                    Cv2.PutText(frame, age, new OpenCvSharp.Point(face.X, face.Y - 10), HersheyFonts.HersheySimplex, 0.9, Scalar.Green, 2);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while try to detect faces {ex}");
            }
        }

        public void Window_Closing(object sender, CancelEventArgs e)
        {
            bkgWorker.CancelAsync();

            _capture.Dispose();
            _cascadeClassifier.Dispose();
        }

        public void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _capture.Open(0, VideoCaptureAPIs.ANY);
            if (!_capture.IsOpened())
            {
                Close();
                return;
            }

            bkgWorker.RunWorkerAsync();
        }

        public void HandleVideoProcessing(object sender, DoWorkEventArgs e)
        {
            try
            {
                var worker = (BackgroundWorker)sender;
                while (!worker.CancellationPending)
                {
                    using (var frameMat = _capture.RetrieveMat())
                    {
                        var rects = _cascadeClassifier.DetectMultiScale(frameMat, 1.1, 5, HaarDetectionTypes.ScaleImage, new Size(30, 30));

                        foreach (var rect in rects)
                        {
                            Cv2.Rectangle(frameMat, rect, Scalar.Green);

                            DetectAgeOnPicture(frameMat, rect);
                        }

                        var bitmapSource = frameMat.ToWriteableBitmap();
                        var bitmap = BitmapFromSource(bitmapSource);
                        var orangeBitmap = ImageHandling.MakeOrangeGrayscale(bitmap);

                        // Must create and use WriteableBitmap in the same thread(UI Thread).
                        Dispatcher.Invoke(() =>
                        {
                            RealTimeCameraPicture.Source = BitmapSourceConverter.ToBitmapSource(orangeBitmap);
                        });
                    }

                    Thread.Sleep(30);
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Exception while try to handle video processing {ex}");
            }
        }

        public static Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (MemoryStream outStream = new())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();

                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }
    }
}