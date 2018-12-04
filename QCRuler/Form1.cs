using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
//using System.IO;

namespace QCRuler
{
    public partial class Form1 : Form
    //public static class Form1 : Form
    {
        private bool mouseDown; // move
        private Point lastLocation;  //move

        Point lastPoint = Point.Empty;//Point.Empty represents null for a Point object
        bool isMouseDown = new Boolean();//this is used to evaluate whether our mousebutton is down or not
 


        int pixx = 0;
        int pixy = 0;
        int mergecirclecount = 1;  // the circle group start by 1
        //int supermergecirclecount = 1;  // the circle group start by 1
        //int supercircle = 0;
        int largestcircle = 0;
        int smallestcircle = 0;
        Bitmap original = null;
        Bitmap savedraw = null;
        int[,] allcircle = new int[1,4];
        int measurebutton = 0;
        int lasttime_measurebutton = 0;
        //int[,] mergcircle = new int[1, 4];  // [x,y,r,Group]
        //int[,] aftermerge = new int[1, 5];  //  [x,y,r,Group, check]
        int[,] supermerge = new int[1, 4];
        int[,] aftersupermerge = new int[1, 4];  //  [x,y,r,Group]
        List<Point> points = new List<Point>();
        List<PointF> pointsF = new List<PointF>();
        List<PointF> pointsFCriteria = new List<PointF>();
        List<PointF> pointsFCompare1 = new List<PointF>();
        List<PointF> pointsFCompare2 = new List<PointF>();
        List<PointF> pointsFCompare3 = new List<PointF>();
        float zoom = 1f;

        public Form1()
        {
            InitializeComponent();
            panel4.Visible = false;
            panel4.Height = button1.Height;
            label2.Visible = false;
            label2.Text = "na";
            //pictureBox2.Anchor = AnchorStyles.None;
            pictureBox2.Location = new Point(0,0);
            textBox1.ReadOnly = true;
            textBox2.ReadOnly = true;
            textBox3.ReadOnly = true;
            textBox4.ReadOnly = true;
            textBox5.ReadOnly = true;
            textBox6.ReadOnly = true;
            trackBar1.Minimum = 0;
            trackBar1.Maximum = 10;
            trackBar1.TickFrequency = 2;
            //panel11.AutoScroll = true;
            //panel11.Dock
            //pictureBox3.SizeMode = PictureBoxSizeMode.AutoSize;
            //zoom = 1f;
        }

        private void button1_Click(object sender, EventArgs e)   // Load Image
        {
            Bitmap selectedSource = null;
            Bitmap bitmapResult = null;
            Bitmap bitmapResult1 = null;
            panel4.Height = button1.Height;
            panel4.Top = button1.Top;
            opbl.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";
            DialogResult res = opbl.ShowDialog();
            if (res == DialogResult.OK)
            {
                panel4.Visible = true;
                pictureBox2.Image = Image.FromFile(opbl.FileName);
                int w = (int)(pictureBox2.Image.Width * zoom);
                int h = (int)(pictureBox2.Image.Height * zoom);
                pictureBox2.ClientSize = new Size(w, h);

               // pictureBox3.Image = Image.FromFile(opbl.FileName);
                label2.Text = opbl.FileName.ToString();
                label2.Visible = true;


                //Bitmap newbitmap = new Bitmap(pictureBox2.Image);
                selectedSource = new Bitmap(label2.Text);
                original = selectedSource;
                savedraw = selectedSource;
                bitmapResult = selectedSource.KirschFilter(true);  //Laplacian5x5Filter(true), Sobel3x3Filter(true), PrewittFilter(true), 
                //pictureBox3.Image = pictureBox2.Image;
                //
                bitmapResult1 = bitmapResult.Sobel3x3Filter(true);
                pictureBox4.Image = bitmapResult1;
                this.Update();


                points.Clear();
                pointsF.Clear();
                pointsFCriteria.Clear();
                pointsFCompare1.Clear();
                pointsFCompare2.Clear();
                pointsFCompare3.Clear();
                pictureBox2.Invalidate();

            }
        }

        private void button2_Click(object sender, EventArgs e)  // Auto Measure
        {
            
            textBox3.Text = "0";
            //textBox6.Text = "0";
            panel4.Visible = true;
            panel4.Height = button2.Height;
            panel4.Top = button2.Top;
            string image_name = string.Empty;
            Bitmap selectedSource = null;
            Bitmap bitmapResult = null;
            Bitmap bitmapResult1 = null;
            Bitmap bitmapuntouch = null;
            Bitmap bitmapaftermerge = null;
            Bitmap bitmapaftersupermerge = null;
            Color c;
            int row = 0;
            int col = 0;
            int Gcirclecount = 0;
            //int Bcirclecount = 0;
            //int Rcirclecount = 0;
            int minR = 18;
            Int32.TryParse(textBox9.Text.ToString().Trim(), out minR);     // min circle radius
            int maxR = 30;     // max circle radius
            Int32.TryParse(textBox10.Text.ToString().Trim(), out maxR);     // max circle radius
            smallestcircle = maxR;
            int accum = 0;     // vote number
            int thred = 120;   // pixel value
            Int32.TryParse(textBox11.Text.ToString().Trim(), out thred);     // pixel value
            double accuracy = 0.95;
            Double.TryParse(textBox12.Text.ToString().Trim(), out accuracy);
            int min = 1;
            int maximumR = 0;
            int maximumVote = 0;
            double smallratio = 0.9;
            Double.TryParse(textBox7.Text.ToString().Trim(), out smallratio);
            //
            image_name = label2.Text.ToString();
            if (image_name.ToString().Trim() == "na")
            {
                MessageBox.Show("You must choose one picture");
            }
            else
            {
                try
                {
                    pictureBox2.Image = new Bitmap(label2.Text);
                    original = new Bitmap(label2.Text); 
                    selectedSource = new Bitmap(label2.Text);
                    bitmapResult = selectedSource.KirschFilter(true);  //Laplacian5x5Filter(true), Sobel3x3Filter(true), PrewittFilter(true), 

                    //
                    bitmapResult1 = bitmapResult.Sobel3x3Filter(true);
                    bitmapuntouch = bitmapResult.Sobel3x3Filter(true);
                    bitmapaftermerge = bitmapResult.Sobel3x3Filter(true);
                    bitmapaftersupermerge = bitmapResult.Sobel3x3Filter(true);
                    pictureBox5.Image = bitmapResult1;
                    //bitmapResult1 = bitmapuntouch;
                    //bitmapaftermerge = bitmapuntouch;
                    this.Update();

                    //
                    //bitmapResult1 = bitmapResult;  // only use 1st to process
                    row = bitmapResult1.Height;
                    col = bitmapResult1.Width;
                    pixx = col;
                    pixy = row;
                    
                    for (int i = 0; i < row; i = i + 3)   //y
                    {
                        
                        accum = 0;
                        min = maxR + 1;
                        int rowcirclecount = 0;
                        for (int j = 0; j < col; j = j + 3)  //x
                        {
                            accum = 0;
                            int r = minR;
                            
                            maximumR = 0;
                            maximumVote = 0;
                            while (r <= maxR)  // check every Radius
                            {
                                textBox1.Text = "(" + j + "," + i + ")";
                                this.Update();
                              
                                accum = 0;
                                for (int t = r - 1; t <= r + 1; t++)
                                {
                                    int a = 0;
                                    int b = 0;
                                    for (int theta = 0; theta < 360; theta = theta + 3)
                                    {
                                        int pixelval = 0;
                                        a = Convert.ToInt32(j - t * Math.Cos(theta * Math.PI / 180));  //x
                                        b = Convert.ToInt32(i - t * Math.Sin(theta * Math.PI / 180));  //y
                                        if (a >= 0 && b >= 0 && a < col && b < row)
                                        {
                                            //c = bitmapuntouch.GetPixel(a, b);
                                            c = bitmapResult1.GetPixel(a, b);
                                            Int32.TryParse(c.ToString().Split(',', '=')[3], out pixelval);
                                            if (pixelval > thred)
                                            {
                                                accum = accum + 1;
                                            }
                                        }
                                    }
                                }
                               
                                
                                if(accum > Convert.ToInt32(r * Math.PI * 3 * accuracy)){
                                    
                                    if (accum > maximumVote)
                                    {
                                        // store its R and vote
                                        maximumVote = accum;   
                                        maximumR = r;   // the r with maximum votes

                                    }
                                }                                                               
                                r = r + 1;
                                
                            }
                            if (maximumR != 0)
                            {
                                bitmapResult1 = drawcircle(bitmapResult1, j, i, maximumR, "Green");
                                //original = drawcircle(original, j, i, maximumR, "Green");
                                // store the circle in the array  --> allcircle
                                allcircle = AddRow(allcircle, new int[] { j, i, maximumR, 0 });                                //new int[,] { {j, i, maximumR, check } };                                
                                Gcirclecount = Gcirclecount + 1;
                                textBox4.Text = "(" + allcircle[Gcirclecount, 0] + "," + allcircle[Gcirclecount, 1] + "," + allcircle[Gcirclecount, 2] + ")";


                                rowcirclecount = rowcirclecount + 1;
                                textBox3.Text = Gcirclecount.ToString();
                                //textBox6.Text = maximumR.ToString();
                                this.Update();
                                j = j + Convert.ToInt32(1.5 * maximumR * 0.0);   //x
                                if (maximumR < min)
                                {
                                    min = maximumR;

                                }
                                
                            }
                                        
                        }

                        if (min == maxR + 1)
                        {
                            i = i + 0;
                        }
                        else
                        {
                            i = i + Convert.ToInt32(min * 0.0);   //y
                        }              

                        pictureBox5.Image = bitmapResult1;
                        this.Update();
                    }
                    int[,] aftermerge = mergecircle(allcircle, Gcirclecount);
                    int[,] afteraftermerge = mergecircle(aftermerge, aftermerge.GetLength(0) - 1);
                    aftermerge = mergecircle(afteraftermerge, afteraftermerge.GetLength(0) - 1);
                    afteraftermerge = mergecircle(aftermerge, aftermerge.GetLength(0) - 1);

                    // get the biggest circle and the small circle
                    int smallcircle =  Convert.ToInt32(smallratio * largestcircle);
                    
                        //draw the aftermerge

                    for (int m = 1; m <= mergecirclecount; m++)
                    {
                        bitmapaftermerge = drawcircle(bitmapaftermerge, afteraftermerge[m, 0], afteraftermerge[m, 1], afteraftermerge[m, 2], "Blue");
                        if (afteraftermerge[m, 2] == largestcircle)
                        {
                            original = drawcircle(original, afteraftermerge[m, 0], afteraftermerge[m, 1], afteraftermerge[m, 2], "Blue");
                        }
                        else if (afteraftermerge[m, 2] < smallcircle)
                        {
                            original = drawcircle(original, afteraftermerge[m, 0], afteraftermerge[m, 1], afteraftermerge[m, 2], "Red");
                        }
                        else
                        {
                            original = drawcircle(original, afteraftermerge[m, 0], afteraftermerge[m, 1], afteraftermerge[m, 2], "Green");
                        }
                    }

                    //for (int m = 1; m <= supercircle; m++)
                    //{
                    //    //bitmapResult1 = drawcircle(bitmapResult1, j, i, maximumR, "Blue");
                    //    bitmapaftersupermerge = drawcircle(bitmapaftersupermerge, aftersupermerge[m, 0], aftersupermerge[m, 1], aftersupermerge[m, 2], "Blue");
                    //    //original = drawcircle(original, aftersupermerge[m, 0], aftersupermerge[m, 1], aftersupermerge[m, 2], "Green");
                    //}


                    pictureBox6.Image = bitmapaftermerge;  // aftermerge
                    pictureBox5.Image = original;
                    //pictureBox7.Image = bitmapaftersupermerge;  // super merge
                    //pictureBox2.Image = original;
                    textBox2.Text = largestcircle.ToString();
                    textBox6.Text = smallestcircle.ToString();


                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }

        }

        private int[,] mergecircle(int[,] origina, int circlenum)
        {
            int[,] mergcircle = new int[1, 4];  // [x,y,r,Group]
            int[,] aftermerge = new int[1, 4];  //  [x,y,r,Group]
            mergecirclecount = 1;  // the circle group start by 1
            for (int i = 1; i <= circlenum; i++)  // Check all the circle in the array allcircle [x,y,r]
            {
                int x = origina[i, 0];
                int y = origina[i, 1];
                int r = origina[i, 2];

                mergcircle = AddRow(mergcircle, new int[] { x, y, r, mergecirclecount});


                for (int j = 1; j <= circlenum; j++)  // compare with all other circle
                {
                    int otherx = origina[j, 0];
                    int othery = origina[j, 1];
                    int otherr = origina[j, 2];
                    if(j != i) // we don't want it to compare with itself
                    {
                        double distance = Math.Sqrt(Math.Pow(otherx - x, 2) + Math.Pow(othery - y, 2));   // the distance between two center of circle
                        double twoR = (r + otherr) * 0.5;
                        if (distance < twoR)  // they are so close to each other so they should be assigned together
                        {
                            mergcircle = AddRow(mergcircle, new int[] { otherx, othery, otherr, mergecirclecount});
                        }
                    }
                }

                mergecirclecount = mergecirclecount + 1;


            }

            // averaging the group circle
            int supernumber = supermerge.GetLength(0) - 1;
            for (int group = 1; group <= mergecirclecount; group++ )
            {
                int totalx = 0;
                int totaly = 0;
                int totalr = 0;
                int avgx = 0;
                int avgy = 0;
                int avgr = 0;
                int countgroup = 0;
                int arrylen = mergcircle.GetLength(0) - 1;
                for (int k = 1; k <= arrylen; k++)
                {
                    if (mergcircle[k, 3] == group)
                    {
                        int groupx = mergcircle[k, 0];
                        int groupy = mergcircle[k, 1];
                        int groupr = mergcircle[k, 2];

                        totalx = totalx + groupx * groupr;
                        totaly = totaly + groupy * groupr;
                        totalr = totalr + groupr;
                        countgroup = countgroup + 1;
                    }

                }
                if (totalr == 0 || countgroup == 0)
                {
                    //MessageBox.Show(group + "," + totalx + "," + totaly + "," + countgroup);
                    totalr = 1;
                    countgroup = 1;
                }
                //avg the x y r
                avgx = totalx / totalr;
                avgy = totaly / totalr;
                avgr = totalr / countgroup;
                //put in the array aftermerge [x,y,r,group]
                aftermerge = AddRow(aftermerge, new int[] { avgx, avgy, avgr, group});
                if(avgr > largestcircle){
                    largestcircle = avgr;
                }
                if (avgr < smallestcircle && totalr > 1)
                {
                    smallestcircle = avgr;
                }

            }
            //
            int number = aftermerge.GetLength(0) - 1;
            //supercircle = aftersupermerge.GetLength(0) - 1;
            textBox5.Text = number.ToString();
            //textBox8.Text = supercircle.ToString();
            return aftermerge;
        }

        private Bitmap drawcircle(Bitmap bmp, int x, int y, int r, string color)
        {
            Graphics g = Graphics.FromImage(bmp);
            switch (color)
            {
                case "Red":
                    int ra = 0;
                    int rb = 0;
                    for (int theta = 0; theta < 360; theta = theta + 1)
                    {                        
                        ra = Convert.ToInt32(x - r * Math.Cos(theta * Math.PI / 180));  //x
                        rb = Convert.ToInt32(y - r * Math.Sin(theta * Math.PI / 180));  //y
                        if (ra >= 0 && rb >= 0 && ra < pixx && rb < pixy)
                        {
                            bmp.SetPixel(ra, rb, Color.Red);
                        }
                    }                  
                    break;

                case "Blue":
                    int ba = 0;
                    int bb = 0;
                    for (int theta = 0; theta < 360; theta = theta + 1)
                    {
                        ba = Convert.ToInt32(x - r * Math.Cos(theta * Math.PI / 180));  //x
                        bb = Convert.ToInt32(y - r * Math.Sin(theta * Math.PI / 180));  //y
                        if (ba >= 0 && bb >= 0 && ba < pixx && bb < pixy)
                        {
                            bmp.SetPixel(ba, bb, Color.Blue);
                        }
                    }
                    break;
                  
                case "Green":
                    int ga = 0;
                    int gb = 0;
                    for (int theta = 0; theta < 360; theta = theta + 1)
                    {
                        ga = Convert.ToInt32(x - r * Math.Cos(theta * Math.PI / 180));  //x
                        gb = Convert.ToInt32(y - r * Math.Sin(theta * Math.PI / 180));  //y
                        if (ga >= 0 && gb >= 0 && ga < pixx && gb < pixy)
                        {
                            bmp.SetPixel(ga, gb, Color.Green);
                        }
                    }
                    break;
            }
            this.Update();
            return bmp;

        }

        private void button3_Click(object sender, EventArgs e)   // Clear
        {
            panel4.Visible = true;
            panel4.Height = button3.Height;
            panel4.Top = button3.Top;
            pictureBox2.Image = null;
            //pictureBox3.Image = null;
            pictureBox4.Image = null;
            pictureBox5.Image = null;
            pictureBox6.Image = null;
            label2.Text = "na";
            label2.Visible = false;
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            textBox8.Text = "";
            textBox13.Text = "";
            textBox14.Text = "";
            textBox15.Text = "";
            label23.Text = "N/A";
            label24.Text = "N/A";
            label25.Text = "N/A";
            lbl_len.Text = "0 pixels";
            panel6.Enabled = false;
            panel12.Enabled = false;

            pointsF.Clear();
            pictureBox2.Invalidate();
            show_Length();

        }

        //private void pictureBox3_Click(object sender, EventArgs e)
        //{
        //    this.Close();
        //}

        private void button4_Click(object sender, EventArgs e)  // Close form
        {
            this.Close();
        }

        
        private void button5_Click(object sender, EventArgs e)  // Minimize form
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)  // Move form
        {
            mouseDown = true;
            lastLocation = e.Location;
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)  // Move form
        {
            mouseDown = false;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)  // Move form
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }
        }

        private void button6_Click(object sender, EventArgs e)  // Manual measure
        {
            panel4.Visible = true;
            panel4.Height = button6.Height;
            panel4.Top = button6.Top;
            panel6.Enabled = true;
            panel12.Enabled = true;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            SaveFileDialog sF1 = new SaveFileDialog();
            if (sF1.ShowDialog() == DialogResult.OK)
            {
              
               //Bitmap bmp = null;
               //bmp = pictureBox3.Image;
               //pictureBox3.Image.Save(sF1.FileName, ImageFormat.Jpeg);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            SaveFileDialog sF2 = new SaveFileDialog();
            int i = label2.Text.Split('\\').Length - 1;
            sF2.FileName = label2.Text.Split('.', '\\')[i] + "_mergecircle.Png";
            sF2.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";
            if (sF2.ShowDialog() == DialogResult.OK)
            {
                pictureBox5.Image.Save(sF2.FileName, ImageFormat.Png);
            }

        }

        private void button9_Click(object sender, EventArgs e)  // save manual draw bitmap
        {
            //DRAW on the bitmap
            //MessageBox.Show(pointsFCriteria.GetHashCode().ToString());
            if (pointsFCriteria.Count == 2)
            {
                Pen YellowGreen = new Pen(Color.YellowGreen, 2);
                using (var graphics = Graphics.FromImage(savedraw))
                {
                    graphics.DrawLine(YellowGreen, pointsFCriteria[0].X, pointsFCriteria[0].Y, pointsFCriteria[1].X, pointsFCriteria[1].Y);
                    graphics.DrawString("C", new Font("Tahoma", 8), Brushes.YellowGreen, (pointsFCriteria[0].X + pointsFCriteria[1].X) / 2, (pointsFCriteria[0].Y + pointsFCriteria[1].Y) / 2);
                }
            }
            if (pointsFCompare1.Count == 2)
            {
                Pen Blue = new Pen(Color.Blue, 2);
                using (var graphics = Graphics.FromImage(savedraw))
                {
                    graphics.DrawLine(Blue, pointsFCompare1[0].X, pointsFCompare1[0].Y, pointsFCompare1[1].X, pointsFCompare1[1].Y);
                    graphics.DrawString("1", new Font("Tahoma", 8), Brushes.Blue, (pointsFCompare1[0].X + pointsFCompare1[1].X) / 2, (pointsFCompare1[0].Y + pointsFCompare1[1].Y) / 2);
                }
            }
            if (pointsFCompare2.Count == 2)
            {
                Pen Orange = new Pen(Color.Orange, 2);
                using (var graphics = Graphics.FromImage(savedraw))
                {
                    graphics.DrawLine(Orange, pointsFCompare2[0].X, pointsFCompare2[0].Y, pointsFCompare2[1].X, pointsFCompare2[1].Y);
                    graphics.DrawString("2", new Font("Tahoma", 8), Brushes.Orange, (pointsFCompare2[0].X + pointsFCompare2[1].X) / 2, (pointsFCompare2[0].Y + pointsFCompare2[1].Y) / 2);
                }
            }
            if (pointsFCompare3.Count == 2)
            {
                Pen White = new Pen(Color.White, 2);
                using (var graphics = Graphics.FromImage(savedraw))
                {
                    graphics.DrawLine(White, pointsFCompare3[0].X, pointsFCompare3[0].Y, pointsFCompare3[1].X, pointsFCompare3[1].Y);
                    graphics.DrawString("3", new Font("Tahoma", 8), Brushes.White, (pointsFCompare3[0].X + pointsFCompare3[1].X) / 2, (pointsFCompare3[0].Y + pointsFCompare3[1].Y) / 2);                
                }
            }

            SaveFileDialog sF3 = new SaveFileDialog();
            int i = label2.Text.Split('\\').Length - 1;
            sF3.FileName = label2.Text.Split('.', '\\')[i] + "_analysis.Png";
            sF3.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";
            if (sF3.ShowDialog() == DialogResult.OK)
            {
                savedraw.Save(sF3.FileName, ImageFormat.Png);
            }
        }

        //private void label3_Click(object sender, EventArgs e)
        //{

        //}
        static int[,] AddRow(int[,] original, int[] added)
        {
            int lastRow = original.GetUpperBound(0);
            int lastColumn = original.GetUpperBound(1);
            // Create new array.
            int[,] result = new int[lastRow + 2, lastColumn + 1];
            // Copy existing array into the new array.
            for (int i = 0; i <= lastRow; i++)
            {
                for (int x = 0; x <= lastColumn; x++)
                {
                    result[i, x] = original[i, x];
                }
            }
            // Add the new row.
            for (int i = 0; i < added.Length; i++)
            {
                result[lastRow + 1, i] = added[i];
            }
            return result;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            SaveFileDialog sF4 = new SaveFileDialog();
            int i = label2.Text.Split('\\').Length - 1;
            sF4.FileName = label2.Text.Split('.', '\\')[i] + "_analysis.Png";
            sF4.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";
            if (sF4.ShowDialog() == DialogResult.OK)
            {
                pictureBox6.Image.Save(sF4.FileName, ImageFormat.Png);
            }
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            ////
            //if (isMouseDown == true)//check to see if the mouse button is down
            //{

            //    if (lastPoint != null)//if our last point is not null, which in this case we have assigned above
            //    {

            //        if (pictureBox2.Image == null)//if no available bitmap exists on the picturebox to draw on
            //        {
            //            //create a new bitmap
            //            Bitmap bmp = new Bitmap(pictureBox2.Width, pictureBox2.Height);

            //            pictureBox1.Image = bmp; //assign the picturebox.Image property to the bitmap created

            //        }

            //        using (Graphics g = Graphics.FromImage(pictureBox2.Image))
            //        {//we need to create a Graphics object to draw on the picture box, its our main tool

            //            //when making a Pen object, you can just give it color only or give it color and pen size

            //            g.DrawLine(new Pen(Color.YellowGreen, 2), lastPoint, e.Location);
            //            //g.SmoothingMode = SmoothingMode.AntiAliasing;
            //            //this is to give the drawing a more smoother, less sharper look

            //        }

            //        pictureBox2.Invalidate();//refreshes the picturebox

            //        lastPoint = e.Location;//keep assigning the lastPoint to the current mouse position

            //    }

            //}
        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            //
            //lastPoint = e.Location;//we assign the lastPoint to the current mouse position: e.Location ('e' is from the MouseEventArgs passed into the MouseDown event)

            //isMouseDown = true;//we set to true because our mouse button is down (clicked)
            //points.Add(e.Location);
            if (panel6.Enabled)
            {
                switch (lasttime_measurebutton)
                {
                    case 1:
                        if (pointsFCriteria.Count == 1 && lasttime_measurebutton != measurebutton)
                        {
                            pointsFCriteria.Clear();
                        }
                        break;
                    case 2:
                        if (pointsFCompare1.Count == 1 && lasttime_measurebutton != measurebutton)
                        {
                            pointsFCompare1.Clear();
                        }
                        break;
                    case 3:
                        if (pointsFCompare2.Count == 1 && lasttime_measurebutton != measurebutton)
                        {
                            pointsFCompare2.Clear();
                        }
                        break;
                    case 4:
                        if (pointsFCompare3.Count == 1 && lasttime_measurebutton != measurebutton)
                        {
                            pointsFCompare3.Clear();
                        }
                        break;

                }


                if (measurebutton == 1)
                {
                    pointsF.Clear();
                    if (pointsFCriteria.Count >= 2)
                    {
                        pointsFCriteria.Clear();
                    }
                    if (pointsFCriteria.Count < 2)
                    {
                        pointsFCriteria.Add(scaled(e.Location, false));
                        pictureBox2.Invalidate();
                        show_Length();
                        if (pointsFCriteria.Count == 1)
                        {
                            lasttime_measurebutton = measurebutton;
                        }
                    }
                    if (pointsFCriteria.Count == 2)
                    {
                        measurebutton = 0;
                    }
                }
                else if (measurebutton == 2)
                {
                    pointsF.Clear();
                    if (pointsFCompare1.Count >= 2)
                    {
                        pointsFCompare1.Clear();
                    }
                    if (pointsFCompare1.Count < 2)
                    {
                        pointsFCompare1.Add(scaled(e.Location, false));
                        pictureBox2.Invalidate();
                        show_Length();
                        if (pointsFCompare1.Count == 1)
                        {
                            lasttime_measurebutton = measurebutton;
                        }
                    }
                    if (pointsFCompare1.Count == 2)
                    {
                        measurebutton = 0;
                    }
                }
                else if (measurebutton == 3)
                {
                    pointsF.Clear();
                    if (pointsFCompare2.Count >= 2)
                    {
                        pointsFCompare2.Clear();
                    }
                    if (pointsFCompare2.Count < 2)
                    {
                        pointsFCompare2.Add(scaled(e.Location, false));
                        pictureBox2.Invalidate();
                        show_Length();
                        if (pointsFCompare2.Count == 1)
                        {
                            lasttime_measurebutton = measurebutton;
                        }
                    }
                    if (pointsFCompare2.Count == 2)
                    {
                        measurebutton = 0;
                    }
                }
                else if (measurebutton == 4)
                {
                    pointsF.Clear();
                    if (pointsFCompare3.Count >= 2)
                    {
                        pointsFCompare3.Clear();
                    }
                    if (pointsFCompare3.Count < 2)
                    {
                        pointsFCompare3.Add(scaled(e.Location, false));
                        pictureBox2.Invalidate();
                        show_Length();
                        if (pointsFCompare3.Count == 1)
                        {
                            lasttime_measurebutton = measurebutton;
                        }
                    }
                    if (pointsFCompare3.Count == 2)
                    {
                        measurebutton = 0;
                    }
                }
                else
                {
                    if (pointsF.Count >= 2)
                    {
                        pointsF.Clear();
                    }
                    if (pointsF.Count < 2)
                    {
                        pointsF.Add(scaled(e.Location, false));
                        pictureBox2.Invalidate();
                        show_Length();
                    }

                }
                //lasttime_measurebutton = measurebutton;


            }

        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            ////
            //isMouseDown = false;

            //lastPoint = Point.Empty;

            ////set the previous point back to null if the user lets go of the mouse button
        }

        private void button7_Click_1(object sender, EventArgs e)  // clear
        {
            pointsF.Clear();
            pictureBox2.Invalidate();
            show_Length();
        }

        private void button11_Click(object sender, EventArgs e)  // undo
        {
            if (pointsF.Any()) pointsF.Remove(pointsF.Last());
            pictureBox2.Invalidate();
            show_Length();
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {

            if (panel6.Enabled)
            {
                if (measurebutton == 1 || pointsFCriteria.Count > 1)
                {
                    if (pointsFCriteria.Count > 2)
                    {
                        pointsFCriteria.Clear();
                        show_Length();
                        //pictureBox2.Invalidate();
                        //show_Length();
                    }
                    if (pointsFCriteria.Count > 1)
                    {
                        points = pointsFCriteria.Select(x => Point.Round(scaled(x, true))).ToList();
                        e.Graphics.DrawLines(Pens.YellowGreen, points.ToArray());
                        points.Clear();
                        show_Length();
                        //measurebutton = 0;
                    }
                    //if (pointsFCriteria.Count == 2)
                    //{
                    //    measurebutton = 0;
                    //}
                }
                if (measurebutton == 2 || pointsFCompare1.Count > 1)
                {
                    if (pointsFCompare1.Count > 2)
                    {
                        pointsFCompare1.Clear();
                        show_Length();
                        //pictureBox2.Invalidate();
                        //show_Length();
                    }
                    if (pointsFCompare1.Count > 1)
                    {
                        points = pointsFCompare1.Select(x => Point.Round(scaled(x, true))).ToList();
                        e.Graphics.DrawLines(Pens.Blue, points.ToArray());
                        points.Clear();
                        show_Length();
                        //measurebutton = 0;
                    }
                    //if (pointsFCompare1.Count == 2)
                    //{
                    //    measurebutton = 0;
                    //}

                }
                if (measurebutton == 3 || pointsFCompare2.Count > 1)
                {
                    if (pointsFCompare2.Count > 2)
                    {
                        pointsFCompare2.Clear();
                        show_Length();
                        //pictureBox2.Invalidate();
                        //show_Length();
                    }
                    if (pointsFCompare2.Count > 1)
                    {
                        points = pointsFCompare2.Select(x => Point.Round(scaled(x, true))).ToList();
                        e.Graphics.DrawLines(Pens.Orange, points.ToArray());
                        points.Clear();
                        show_Length();
                        //measurebutton = 0;
                    }
                    //if (pointsFCompare2.Count == 2)
                    //{
                    //    measurebutton = 0;
                    //}
                }
                if (measurebutton == 4 || pointsFCompare3.Count > 1)
                {
                    if (pointsFCompare3.Count > 2)
                    {
                        pointsFCompare3.Clear();
                        show_Length();
                        //pictureBox2.Invalidate();
                        //show_Length();
                    }
                    if (pointsFCompare3.Count > 1)
                    {
                        points = pointsFCompare3.Select(x => Point.Round(scaled(x, true))).ToList();
                        e.Graphics.DrawLines(Pens.White, points.ToArray());
                        points.Clear();
                        show_Length();
                        //measurebutton = 0;
                    }
                    //if (pointsFCompare3.Count == 2)
                    //{
                    //    measurebutton = 0;
                    //}
                }
                if (measurebutton == 0)
                {
                    if (pointsF.Count > 2)
                    {
                        pointsF.Clear();
                        pictureBox2.Invalidate();
                        show_Length();
                        //show_Length();
                    }
                    if (pointsF.Count > 1)
                    {
                        points = pointsF.Select(x => Point.Round(scaled(x, true))).ToList();
                        e.Graphics.DrawLines(Pens.Red, points.ToArray());
                        points.Clear();
                        show_Length();

                        //pointsF = pointsF.Select(x => Point.Round(scaled(x, true))).ToList();
                        //e.Graphics.DrawLines(Pens.Red, pointsF.ToArray());
                    }

                }
                
            }
            //pictureBox3.Image = pictureBox2.Image;
        }

        void show_Length()
        {
            //lbl_len.Text = (pointsF.Count) + " point(s), no segments. ";

            //if (!(pointsF.Count > 1) || !(pointsFCriteria.Count > 1) || !(pointsFCompare1.Count > 1) || !(pointsFCompare2.Count > 1) || !(pointsFCompare3.Count > 1)) return;

            double len = 0;


            if (measurebutton == 1)
            {
                if (pointsFCriteria.Count <= 2)
                {
                    for (int i = 1; i < pointsFCriteria.Count; i++)
                    {
                        len += Math.Sqrt((pointsFCriteria[i - 1].X - pointsFCriteria[i].X) * (pointsFCriteria[i - 1].X - pointsFCriteria[i].X)
                                    + (pointsFCriteria[i - 1].Y - pointsFCriteria[i].Y) * (pointsFCriteria[i - 1].Y - pointsFCriteria[i].Y));
                    }
                    //lbl_len.Text = (pointsF.Count - 1) + " segments, " + (int)len + " pixels";
                    lbl_len.Text = (int)len + " pixels";
                    textBox8.Text = (int)len + "";
                }
            }
            else if (measurebutton == 2)
            {
                if (pointsFCompare1.Count <= 2)
                {
                    for (int i = 1; i < pointsFCompare1.Count; i++)
                    {
                        len += Math.Sqrt((pointsFCompare1[i - 1].X - pointsFCompare1[i].X) * (pointsFCompare1[i - 1].X - pointsFCompare1[i].X)
                                    + (pointsFCompare1[i - 1].Y - pointsFCompare1[i].Y) * (pointsFCompare1[i - 1].Y - pointsFCompare1[i].Y));
                    }
                    //lbl_len.Text = (pointsF.Count - 1) + " segments, " + (int)len + " pixels";
                    lbl_len.Text = (int)len + " pixels";
                    textBox13.Text = (int)len + "";
                }

            }
            else if (measurebutton == 3)
            {
                if (pointsFCompare2.Count <= 2)
                {
                    for (int i = 1; i < pointsFCompare2.Count; i++)
                    {
                        len += Math.Sqrt((pointsFCompare2[i - 1].X - pointsFCompare2[i].X) * (pointsFCompare2[i - 1].X - pointsFCompare2[i].X)
                                    + (pointsFCompare2[i - 1].Y - pointsFCompare2[i].Y) * (pointsFCompare2[i - 1].Y - pointsFCompare2[i].Y));
                    }
                    //lbl_len.Text = (pointsF.Count - 1) + " segments, " + (int)len + " pixels";
                    lbl_len.Text = (int)len + " pixels";
                    textBox14.Text = (int)len + "";
                }
            }
            else if (measurebutton == 4)
            {
                if (pointsFCompare3.Count <= 2)
                {
                    for (int i = 1; i < pointsFCompare3.Count; i++)
                    {
                        len += Math.Sqrt((pointsFCompare3[i - 1].X - pointsFCompare3[i].X) * (pointsFCompare3[i - 1].X - pointsFCompare3[i].X)
                                    + (pointsFCompare3[i - 1].Y - pointsFCompare3[i].Y) * (pointsFCompare3[i - 1].Y - pointsFCompare3[i].Y));
                    }
                    //lbl_len.Text = (pointsF.Count - 1) + " segments, " + (int)len + " pixels";
                    lbl_len.Text = (int)len + " pixels";
                    textBox15.Text = (int)len + "";
                }
            }
            else
            {
                if (pointsF.Count <= 2)
                {
                    for (int i = 1; i < pointsF.Count; i++)
                    {
                        len += Math.Sqrt((pointsF[i - 1].X - pointsF[i].X) * (pointsF[i - 1].X - pointsF[i].X)
                                    + (pointsF[i - 1].Y - pointsF[i].Y) * (pointsF[i - 1].Y - pointsF[i].Y));
                    }
                    //lbl_len.Text = (pointsF.Count - 1) + " segments, " + (int)len + " pixels";
                    lbl_len.Text = (int)len + " pixels";
                }
            }

            //if (pointsF.Count <= 2)
            //{
            //    for (int i = 1; i < pointsF.Count; i++)
            //    {
            //        len += Math.Sqrt((pointsF[i - 1].X - pointsF[i].X) * (pointsF[i - 1].X - pointsF[i].X)
            //                    + (pointsF[i - 1].Y - pointsF[i].Y) * (pointsF[i - 1].Y - pointsF[i].Y));
            //    }
            //    //lbl_len.Text = (pointsF.Count - 1) + " segments, " + (int)len + " pixels";
            //    lbl_len.Text = (int)len + " pixels";

            int compare_1, criteria, compare_2, compare_3;


                switch (measurebutton)
                {
                    case 1:
                        //textBox8.Text = (int)len + "";
                        if (pointsFCriteria.Count >= 2)
                        {
                            button12.BackColor = Color.Silver;
                            //measurebutton = 0;
                        }
                        if (int.TryParse(textBox13.Text, out compare_1) && int.TryParse(textBox8.Text, out criteria))
                        {
                            double ratio = measure_ratio(criteria, compare_1);
                            label23.Text = ratio.ToString("0.000");
                        }
                        else
                        {
                            label23.Text = "N/A";
                        }
                        if (int.TryParse(textBox14.Text, out compare_2) && int.TryParse(textBox8.Text, out criteria))
                        {
                            double ratio = measure_ratio(criteria, compare_2);
                            label24.Text = ratio.ToString("0.000");
                        }
                        else
                        {
                            label24.Text = "N/A";
                        }
                        if (int.TryParse(textBox15.Text, out compare_3) && int.TryParse(textBox8.Text, out criteria))
                        {
                            double ratio = measure_ratio(criteria, compare_3);
                            label25.Text = ratio.ToString("0.000");
                        }
                        else
                        {
                            label25.Text = "N/A";
                        }
                        //measurebutton = 0;
                        //button12.BackColor = Color.Silver;
                        break;
                    case 2:
                        if (pointsFCompare1.Count >= 2)
                        {
                            button13.BackColor = Color.Silver;
                            //measurebutton = 0;
                        }
                        //textBox13.Text = (int)len + "";

                        if (int.TryParse(textBox13.Text, out compare_1) && int.TryParse(textBox8.Text, out criteria))
                        {
                            double ratio = measure_ratio(criteria, compare_1);
                            label23.Text = ratio.ToString("0.000");
                        }
                        //else
                        //{
                        //    label23.Text = "N/A";
                        //}
                        //measurebutton = 0;
                        //button13.BackColor = Color.Silver;
                        break;
                    case 3:
                        //textBox14.Text = (int)len + "";
                        if (pointsFCompare2.Count >= 2)
                        {
                            button14.BackColor = Color.Silver;  // turn off the light of the button
                            //measurebutton = 0;
                        }
                        if (int.TryParse(textBox14.Text, out compare_2) && int.TryParse(textBox8.Text, out criteria))
                        {
                            double ratio = measure_ratio(criteria, compare_2);
                            label24.Text = ratio.ToString("0.000");
                        }
                        //else
                        //{
                        //    label24.Text = "N/A";
                        //}
                        //measurebutton = 0;
                        //button14.BackColor = Color.Silver;
                        break;
                    case 4:
                        //textBox15.Text = (int)len + "";
                        if (pointsFCompare3.Count >= 2)
                        {
                            button15.BackColor = Color.Silver;
                            //measurebutton = 0;
                        }
                        if (int.TryParse(textBox15.Text, out compare_3) && int.TryParse(textBox8.Text, out criteria))
                        {
                            double ratio = measure_ratio(criteria, compare_3);
                            label25.Text = ratio.ToString("0.000");
                        }
                        //else
                        //{
                        //    label25.Text = "N/A";
                        //}
                        //measurebutton = 0;
                        //button15.BackColor = Color.Silver;
                        break;

                }
                //measurebutton = 0;

            //}
            lbl_len.Text = (int)len + " pixels";

            
        }

        PointF scaled(PointF p, bool scaled)
        {
            float z = scaled ? 1f * zoom : 1f / zoom;
            return new PointF(p.X * z, p.Y * z);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            try
            {
                //List<float> zooms = new List<float>() { 0.1f, 0.2f, 0.5f, 0.75f, 1f, 2, 3, 4, 6, 8, 10 };
                List<float> zooms = new List<float>() { 1f, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 };
                zoom = zooms[trackBar1.Value];

                int w = (int)(pictureBox2.Image.Width * zoom);
                int h = (int)(pictureBox2.Image.Height * zoom);

                pictureBox2.ClientSize = new Size(w, h);
                //pictureBox2.Location = new Point(0, 0);
                
                lbl_zoom.Text = "zoom: " + (zoom * 100).ToString("0.0");
            }
            catch (Exception ex)
            {
                //
            }
        }

        private void button12_Click(object sender, EventArgs e)  // criteria button
        {
            try
            {
                measurebutton = 1;
                button12.BackColor = Color.GreenYellow;
                pointsF.Clear();
                for (int i = 0; i <= 5; i++)
                {
                    if (i != measurebutton)
                    {
                        closebuttonlight(i);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void button13_Click(object sender, EventArgs e)  // compare 1
        {
            try
            {
                measurebutton = 2;
                button13.BackColor = Color.GreenYellow;
                pointsF.Clear();
                for (int i = 0; i <= 5; i++)
                {
                    if (i != measurebutton)
                    {
                        closebuttonlight(i);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button14_Click(object sender, EventArgs e)  // compare 2
        {
            try
            {
                measurebutton = 3;
                button14.BackColor = Color.GreenYellow;
                pointsF.Clear();
                for (int i = 0; i <= 5; i++)
                {
                    if (i != measurebutton)
                    {
                        closebuttonlight(i);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button15_Click(object sender, EventArgs e)  // compare 3
        {
            try
            {
                measurebutton = 4;
                button15.BackColor = Color.GreenYellow;
                pointsF.Clear();
                for (int i = 0; i <= 5; i++)
                {
                    if (i != measurebutton)
                    {
                        closebuttonlight(i);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private double measure_ratio(int criteria, int compare)
        {
            double ratio = 0.0;

            if (criteria > 0)
            {
               ratio = (double)compare / (double)criteria;
            }

            return ratio;
        }

        private void button12_Leave(object sender, EventArgs e)
        {
            if (pointsFCriteria.Count >= 2)
            {
                button12.BackColor = Color.Silver;
                //measurebutton = 0;
            }
        }

        private void button13_Leave(object sender, EventArgs e)
        {
            if (pointsFCompare1.Count >= 2)
            {
                button13.BackColor = Color.Silver;
                //measurebutton = 0;
            }
        }

        private void button14_Leave(object sender, EventArgs e)
        {
            if (pointsFCompare2.Count >= 2)
            {
                button14.BackColor = Color.Silver;
                //measurebutton = 0;
            }
        }

        private void button15_Leave(object sender, EventArgs e)
        {
            if (pointsFCompare3.Count >= 2)
            {
                button15.BackColor = Color.Silver;
                //measurebutton = 0;
            }
        }

        private void closebuttonlight(int button)
        {
            try
            {
                switch (button)
                {
                    case 1:
                        button12.BackColor = Color.Silver;
                        break;
                    case 2:
                        button13.BackColor = Color.Silver;
                        break;
                    case 3:
                        button14.BackColor = Color.Silver;
                        break;
                    case 4:
                        button15.BackColor = Color.Silver;
                        break;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button16_Click(object sender, EventArgs e)  // Clear criteria
        {
            try
            {
                closebuttonlight(1);
                pointsFCriteria.Clear();
                textBox8.Text = "0";
                measurebutton = 1;
                show_Length();
                measurebutton = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button17_Click(object sender, EventArgs e)  // Clear compare1
        {
            try
            {
                closebuttonlight(2);
                pointsFCompare1.Clear();
                textBox13.Text = "0";
                measurebutton = 2;
                show_Length();
                measurebutton = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button18_Click(object sender, EventArgs e)  // Clear compare2
        {
            try
            {
                closebuttonlight(3);
                pointsFCompare2.Clear();
                textBox14.Text = "0";
                measurebutton = 3;
                show_Length();
                measurebutton = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button19_Click(object sender, EventArgs e)  // Clear compare3
        {
            try
            {
                closebuttonlight(4);
                pointsFCompare3.Clear();
                textBox15.Text = "0";
                measurebutton = 4;
                show_Length();
                measurebutton = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button20_Click(object sender, EventArgs e)  // save
        {
            SaveFileDialog sF6 = new SaveFileDialog();
            int i = label2.Text.Split('\\').Length - 1;
            sF6.FileName = label2.Text.Split('.', '\\')[i] + "_EdgeDetect.Png";
            sF6.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";
            if (sF6.ShowDialog() == DialogResult.OK)
            {
                pictureBox4.Image.Save(sF6.FileName, ImageFormat.Png);
            }
        }

        //private void button11_Click(object sender, EventArgs e)
        //{
        //    SaveFileDialog sF5 = new SaveFileDialog();
        //    if (sF5.ShowDialog() == DialogResult.OK)
        //    {

        //        //Bitmap bmp = null;
        //        //bmp = pictureBox3.Image;
        //        pictureBox7.Image.Save(sF5.FileName, ImageFormat.Jpeg);
        //    }
        //}



    }
}
