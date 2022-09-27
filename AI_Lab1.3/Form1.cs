namespace AI_Lab1._3
{
    using DirectShowLib;
    using Emgu.CV;
    using Emgu.CV.Structure;

    public partial class Form1 : Form
    {
        private static readonly CascadeClassifier EyeClassifier = new CascadeClassifier("haarcascade_eye.xml");
        private static readonly CascadeClassifier FaceClassifier = new CascadeClassifier("haarcascade_frontalface_default.xml");
        private VideoCapture? capture = null;
        private DsDevice[]? cams = null;
        private int selectedCamId = 0;

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
                        this.pictureBox1.Image = img.ToBitmap();

                        Bitmap bitmap = new Bitmap(this.pictureBox1.Image);
                        Image<Bgr, byte> grayImage = bitmap.ToImage<Bgr, byte>();
                        //Image<Bgr, byte> grayImage = new Image<Bgr, byte>(bitmap);

                        Rectangle[] ractangles = FaceClassifier.DetectMultiScale(grayImage, 1.4, 0);
                        foreach (Rectangle rectangle in ractangles)
                        {
                            using (Graphics g = Graphics.FromImage(bitmap))
                            {
                                using (Pen pen = new Pen(Color.Red, 3))
                                {
                                    g.DrawRectangle(pen, rectangle);
                                }
                            }
                        }

                        Rectangle[] eyes = EyeClassifier.DetectMultiScale(grayImage, 1.4, 0);
                        foreach (Rectangle rectangle in eyes)
                        {
                            using (Graphics g = Graphics.FromImage(bitmap))
                            {
                                using (Pen pen = new Pen(Color.Green, 3))
                                {
                                    g.DrawRectangle(pen, rectangle);
                                }
                            }
                        }

                        this.pictureBox1.Image = bitmap;
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
            cams = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            for (int i = 0; i < cams.Length; i++)
            {
                comboBox1.Items.Add(cams[i]);
            }
        }

        // Selects correct camera index
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedCamId = comboBox1.SelectedIndex;
        }

        // Turns on camera
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (cams.Length == 0)
                {
                    throw new Exception("No available cameras.");
                }
                else if (comboBox1.SelectedIndex == null)
                {
                    throw new Exception("Camera is not selected.");
                }
                else if (capture != null)
                {
                    capture.Start();
                }
                else
                {
                    capture = new VideoCapture(selectedCamId);
                    capture.ImageGrabbed += Capture_ImageGrabbed;
                    capture.Start();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Camera settings
        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            try
            {
                Mat m = new Mat();
                capture.Retrieve(m);
                pictureBox1.Image = m.ToImage<Bgr, byte>().Flip(Emgu.CV.CvEnum.FlipType.Horizontal).AsBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}