namespace AI_Lab1._3
{
    using AForge.Video;
    using AForge.Video.DirectShow;
    using Emgu.CV;
    using Emgu.CV.Structure;

    public partial class Form1 : Form
    {
        private static readonly CascadeClassifier EyeClassifier = new CascadeClassifier("haarcascade_eye.xml");
        private static readonly CascadeClassifier FaceClassifier = new CascadeClassifier("haarcascade_frontalface_default.xml");

        private Bitmap? vidDetection = null;
        private FilterInfoCollection camList;
        private VideoCaptureDevice device;

        public Form1()
        {
            this.InitializeComponent();
        }

        // Face detection on images
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog ofd = new OpenFileDialog() { Multiselect = false, Filter = "Image Files (*.jpg;*.png;*.bmp;)|*.jpg;*.png;*.bmp;|All Files (*.*)|*.*;" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        var img = new Image<Bgr, byte>(ofd.FileName);
                        pictureBox1.Image = img.ToBitmap();

                        Bitmap bitmap = new Bitmap(this.pictureBox1.Image);
                        Image<Bgr, byte> grayImage = bitmap.ToImage<Bgr, byte>();

                        Rectangle[] faces = FaceClassifier.DetectMultiScale(grayImage, 1.2, 1);
                        foreach (Rectangle face in faces)
                        {
                            using (Graphics g = Graphics.FromImage(bitmap))
                            {
                                using (Pen pen = new Pen(Color.Red, 2))
                                {
                                    g.DrawRectangle(pen, face);
                                }
                            }
                        }

                        Rectangle[] eyes = EyeClassifier.DetectMultiScale(grayImage, 1.2, 2);
                        foreach (Rectangle eye in eyes)
                        {
                            using (Graphics g = Graphics.FromImage(bitmap))
                            {
                                using (Pen pen = new Pen(Color.Green, 2))
                                {
                                    g.DrawRectangle(pen, eye);
                                }
                            }
                        }

                        pictureBox1.Image = bitmap;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Something went wrong.");
            }
        }

        // Loads available cameras
        private void Form1_Load(object sender, EventArgs e)
        {
            //cams = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            camList = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            foreach (FilterInfo device in camList)
            {
                comboBox1.Items.Add(device.Name);
            }

            if (camList.Count >= 2)
            {
                comboBox1.SelectedIndex = camList.Count - 1;
            }
            else
            {
                comboBox1.SelectedIndex = 0;
            }

            device = new VideoCaptureDevice();
        }

        // Turns on camera
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (camList == null)
                {
                    throw new Exception("No available cameras.");
                }
                else if (camList.Count == 0)
                {
                    throw new Exception("No available cameras.");
                }
                else if (comboBox1.SelectedIndex == null)
                {
                    throw new Exception("Camera is not selected.");
                }
                else
                {
                    device = new VideoCaptureDevice(camList[comboBox1.SelectedIndex].MonikerString);
                    device.NewFrame += Capture_ImageGrabbed;
                    device.Start();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Camera settings
        private void Capture_ImageGrabbed(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                }

                Bitmap frame = (Bitmap)eventArgs.Frame.Clone();
                Image<Bgr, byte> grayImage = frame.ToImage<Bgr, byte>();

                Rectangle[] faces = FaceClassifier.DetectMultiScale(grayImage, 1.2, 1);
                foreach (Rectangle face in faces)
                {
                    using (Graphics g = Graphics.FromImage(frame))
                    {
                        using (Pen pen = new Pen(Color.Red, 2))
                        {
                            g.DrawRectangle(pen, face);
                        }
                    }
                }

                Rectangle[] eyes = EyeClassifier.DetectMultiScale(grayImage, 1.2, 2);
                foreach (Rectangle eye in eyes)
                {
                    using (Graphics g = Graphics.FromImage(frame))
                    {
                        using (Pen pen = new Pen(Color.Green, 2))
                        {
                            g.DrawRectangle(pen, eye);
                        }
                    }
                }

                vidDetection = new Bitmap(frame);
                pictureBox1.Image = vidDetection;
                frame.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Close button
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stopCapera();
            this.Close();
        }

        // Stops capturing on close
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopCapera();
        }

        // Stops capturing
        private void button3_Click(object sender, EventArgs e)
        {
            stopCapera();
        }

        // Function to send stop signal to camera
        private void stopCapera()
        {
            if (device != null)
            {
                if (device.IsRunning)
                {
                    device.SignalToStop();
                    device = null;
                }
            }
        }
    }
}