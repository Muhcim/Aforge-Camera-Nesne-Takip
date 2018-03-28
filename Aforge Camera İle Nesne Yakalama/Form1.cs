using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using AForge;
using AForge.Imaging.Filters;
using AForge.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing.Imaging;
using Point = System.Drawing.Point;

namespace Aforge_Camera_İle_Nesne_Yakalama
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private FilterInfoCollection VideoCapTureDevices;
        private VideoCaptureDevice Finalvideo;

        int R; //Trackbarın değişkeneleri
        int G;
        int B;
        int xoop = 0;
        bool sayma;

        private void Form1_Load(object sender, EventArgs e)
        {
            VideoCapTureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo VideoCaptureDevice in VideoCapTureDevices)
            {

                comboBox1.Items.Add(VideoCaptureDevice.Name);

            }

            comboBox1.SelectedIndex = 0;
        }

        private void BtnBaşlat_Click(object sender, EventArgs e)
        {
            Finalvideo = new VideoCaptureDevice(VideoCapTureDevices[comboBox1.SelectedIndex].MonikerString);
            Finalvideo.NewFrame += new NewFrameEventHandler(Finalvideo_NewFrame);
            Finalvideo.DesiredFrameRate = 20;//saniyede kaç görüntü alsın istiyorsanız. FPS
            Finalvideo.DesiredFrameSize = new Size(640, 480);//görüntü boyutları
            Finalvideo.Start();
        }

        private void Finalvideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
             Bitmap image = (Bitmap)eventArgs.Frame.Clone();
            Bitmap image1 = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = image;


            // create filter
            EuclideanColorFiltering filter = new EuclideanColorFiltering();
            // set center colol and radius
            filter.CenterColor = new RGB(Color.FromArgb(R, G, B));
            filter.Radius = 100;
            // apply the filter
            filter.ApplyInPlace(image1);

            nesnebul(image1);
        }

            

    public void nesnebul(Bitmap image)
        {
            #region
            BlobCounter blobCounter = new BlobCounter();
            blobCounter.MinWidth = 5;
            blobCounter.MinHeight = 5;
            blobCounter.FilterBlobs = true;
            blobCounter.ObjectsOrder = ObjectsOrder.Size;
            //Grayscale griFiltre = new Grayscale(0.2125, 0.7154, 0.0721);
            //Grayscale griFiltre = new Grayscale(0.2, 0.2, 0.2);
            //Bitmap griImage = griFiltre.Apply(image);

            BitmapData objectsData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
            // grayscaling
            Grayscale grayscaleFilter = new Grayscale(0.2125, 0.7154, 0.0721);
            UnmanagedImage grayImage = grayscaleFilter.Apply(new UnmanagedImage(objectsData));
            // unlock image
            image.UnlockBits(objectsData);


            blobCounter.ProcessImage(image);
            Rectangle[] rects = blobCounter.GetObjectsRectangles();
            Blob[] blobs = blobCounter.GetObjectsInformation();
            pictureBox2.Image = image;
            #endregion
            //Multi tracking Çoklu cisim Takibi-------

                for (int i = 0; rects.Length > i; i++)
               {
                    Rectangle objectRect = rects[i];
                    //List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);
                    //Graphics g = Graphics.FromImage(image);
                    Graphics g = pictureBox1.CreateGraphics();
                    using (Pen pen = new Pen(Color.FromArgb(252, 3, 26), 2))
                    {
                        g.DrawRectangle(pen, objectRect);
                        g.DrawString((i+1).ToString() , new Font("Arial", 12), Brushes.Red, objectRect);
                        //g.DrawLines(pen,ToPointsArray(edgePoints));
                                                
                    }
                                    
                    //Cizdirilen Dikdörtgenin Koordinatlari aliniyor.
                    int objectX = objectRect.X + (objectRect.Width / 2);
                    int objectY = objectRect.Y + (objectRect.Height / 2);
                    
                    //  g.DrawString(objectX.ToString() + "X" + objectY.ToString(), new Font("Arial", 12), Brushes.Red, new System.Drawing.Point(250, 1));

                    g.Dispose();

                }

        //TODO: Nesneler arası kapalı çevrim draww
           

                if (rects.Length > 1)
                {
                    for (int j = 0; j < rects.Length - 1; j++)
                    {
                        int ilkx = (rects[j].Right + rects[j].Left) / 2 ;
                        int ilky = (rects[j].Top + rects[j].Bottom) / 2 ;

                        int ikix = (rects[j+1].Right + rects[j+1].Left) / 2 ;
                        int ikiy = (rects[j+1].Top + rects[j+1].Bottom) / 2 ;

                        Graphics g = pictureBox2.CreateGraphics();
                        //g.DrawLine(Pens.Red, rects[j].Location, rects[j + 1].Location);
                        //g.DrawLine(Pens.Blue, rects[0].Location, rects[rects.Length - 1].Location);
                        g.DrawLine(Pens.Red, ilkx, ilky, ikix, ikiy);

                    }

                    int ilkX = rects[0].X + (rects[0].Width / 2);
                    int ilkY = rects[0].Y + (rects[0].Height / 2);

                    int ikiX = rects[1].X + (rects[1].Width / 2);
                    int ikiY = rects[1].X + (rects[1].Height / 2);

                    int uzaklik = System.Math.Abs(ilkX - ikiX);
                    

                   
                    if (uzaklik < 120 && sayma == true)
                    {
                        
                        xoop++;
                        sayma = false;
                        
                        
                    }

                    if (uzaklik > 120)
                    {
                        sayma = true;
                    }
                    else
                    {

                        sayma = false;
                    }

                                                               
                    
              
                    this.Invoke((MethodInvoker)delegate
                    {
                        richTextBox1.Text = xoop.ToString() + "\n" + richTextBox1.Text + "\n"; ;
                        
                    });

                }
        }

    private void BtnDurdur_Click(object sender, EventArgs e)
    {
        if (Finalvideo.IsRunning)
        {
            Finalvideo.Stop();

        }
        Application.Exit();
       }

    private void trackBar1_Scroll(object sender, EventArgs e)
    {
        R = trackBar1.Value;
    }

    private void trackBar2_Scroll(object sender, EventArgs e)
    {
        G = trackBar2.Value;
    }

    private void trackBar3_Scroll(object sender, EventArgs e)
    {
        B = trackBar3.Value;
      }
    private Point[] ToPointsArray(List<IntPoint> points)
    {
        Point[] array = new Point[points.Count];

        for (int i = 0, n = points.Count; i < n; i++)
        {
            array[i] = new Point(points[i].X, points[i].Y);
        }

        return array;
       }

    private void BtnSıfırla_Click(object sender, EventArgs e)
    {
        xoop = 0;
        richTextBox1.Clear();
       }
    }
}
