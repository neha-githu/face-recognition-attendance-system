using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Face;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Face_Recognition_Attendance_System
{
    public partial class Form1 : Form
    {
        //Declare variables to use them in all project
        MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_TRIPLEX, 0.6d, 0.6d);
        HaarCascade faceDetected;
        Image<Bgr, Byte> frame, img;
        Capture camera;
        Image<Gray, byte> result = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        int Count, Numlabels;

        public Form1()
        {
            InitializeComponent();
            //HaarCascade is for face Detection
            faceDetected = new HaarCascade("haarcascade_frontalface_default.xml");
            try
            {
                string Labelsinf = File.ReadAllText(Application.StartupPath + "/Faces/Faces.txt");
                string[] Labels = Labelsinf.Split(',');
                //The first label before , will be the number of faces saved.
                Numlabels = Convert.ToInt32(Labels[0]);
                Count = Numlabels;
                string FacesLoad;
                for (int i = 1; i < Numlabels + 1; i++)
                {
                    FacesLoad = "face" + i + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/Faces/" + FacesLoad));
                    labels.Add(Labels[i]);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Nothing in the Database");
            }
        }

        private void cameraBox_Click(object sender, EventArgs e)
        {
            cameraBox.Invalidate();
        }


        private void start_Click(object sender, EventArgs e)
        {
            camera = new Capture();
             img = camera.QueryFrame();
            cameraBox.Image = img.ToBitmap(); //comment this line after checking image
            Application.Idle += new EventHandler(FrameProcedure); // uncomment this line later on.after checking image.
        }

        private void FrameProcedure(object sender, EventArgs e)
        {
            frame = camera.QueryFrame();
            if (frame != null && (frame.Size.Width * frame.Size.Height) > 0)
            {
                frame = frame.Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                Image<Gray, byte> grayFace = frame.Convert<Gray, byte>();

                if (grayFace != null)
                {
                    MCvAvgComp[][] facesDetectedNow = grayFace.DetectHaarCascade(faceDetected, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
                    foreach (MCvAvgComp f in facesDetectedNow[0])
                    {
                        result = frame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                        frame.Draw(f.rect, new Bgr(Color.Red), 2);

                        if (trainingImages.ToArray().Length != 0)
                        {
                            //TermCriteria for face recognition with numbers of trained images like maxIteration
                            MCvTermCriteria termCrit = new MCvTermCriteria(Count, 0.001);

                            //Eigen face recognizer
                            EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), 3000, ref termCrit);

                            Name = recognizer.Recognize(result);
                            frame.Draw(message: Name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.LightGreen));
                        }
                    }
                }
            }
        }

        private void btn_Capture_Click(object sender, EventArgs e)
        {

        }
    }
}




