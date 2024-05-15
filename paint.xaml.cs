using Draw.ColorDemo;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using Color = System.Windows.Media.Color;
using cv = OpenCvSharp;
using Window = System.Windows.Window;
using System.Windows.Ink;
using System.Data;
using Phidget22;
using Phidget22.Events;
using System.Diagnostics;

namespace Draw
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        private DrawingAttributes drawingAttributes = new DrawingAttributes();
        private Color selectedColor = Colors.Transparent;
        private Boolean IsMouseDown = false;


        BitmapSource background;
        BitmapSource texture;
        BitmapSource shape_source;

        Bitmap shape_bit;

        Boolean _shape1 = false;
        System.Drawing.Point pointOrig;
        System.Windows.Point shape_loc;

        TranslateTransform transPoint;

        int selected_shape = 0;

        shape_class sh1;

        Color brush_color = new Color();
        int brush_mode = 1;

        RFID rfid0;

        BlurBitmapEffect myBlurEffect;



        Boolean _insp1 = false, _insp2 = false, _insp3 = false, _insp4 = false;




        public MainWindow()
        {
            InitializeComponent();


            Cursor = Cursors.None;

            sl1.set_min(100);
            sl1.set_max(240);


            opacity_sl.set_min(0.1);
            opacity_sl.set_max(1.0);

            opacity_sl.set_value(1.0);

            blur_sl.set_min(0);
            blur_sl.set_max(35);


            //intilize blur
            myBlurEffect = new BlurBitmapEffect();
            myBlurEffect.Radius = 20;
            myBlurEffect.KernelType = KernelType.Gaussian;


            try
            {
                //Create your Phidget channels
                rfid0 = new RFID();

                //Set addressing parameters to specify which channel to open (if any)

                //Assign any event handlers you need before calling open so that no events are missed.
                rfid0.Tag += Rfid0_Tag;
                rfid0.TagLost += Rfid0_TagLost;
                rfid0.Detach += Rfid0_Detach;

                //Open your Phidgets and wait for attachment
                rfid0.Open(5661);
            }
            catch (Exception ex)
            {
                tag_text.Content = "Failed to Intilize " + ex.Message;
            }



            brush_grid.Visibility = Visibility.Hidden;
            brush_color.R = 0;
            brush_color.G = 0;
            brush_color.B = 0;


            bkg.Width = mainDock.ActualWidth;
            bkg.Height = mainDock.ActualHeight;

            txture.Width = mainDock.ActualWidth;
            txture.Height = mainDock.ActualHeight;


            //pattern(1, ref shape_touch1, @"C:\Users\MALSA\Downloads\tri1.png");
            //pattern(1, ref shape_touch2, @"C:\Users\MALSA\Downloads\tri3.png");
            //pattern(1, ref shape_touch3, @"C:\Users\MALSA\Downloads\fluid.png");
            //pattern(1, ref shape_touch4, @"C:\Users\MALSA\Downloads\fluid-green.png");

            shape_touch1.change_source(@"\tri1.png");
            shape_touch2.change_source(@"\tri3.png");
            shape_touch3.change_source(@"\fluid.png");
            shape_touch4.change_source(@"\fluid-green.png");

            shape_touch1.hide();
            shape_touch2.hide();
            shape_touch3.hide();
            shape_touch4.hide();

            shape1.Opacity = .5;
            shape2.Opacity = .5;
            shape3.Opacity = .5;
            shape4.Opacity = .5;

            sh1 = new shape_class(1, @"C:\Users\MALSA\Downloads\tri1.png", @"\tri1.png", false, false, false, shape_touch1, System.Drawing.Color.FromArgb(255, 0, 0, 0));


            //color picker
            this.selectedColor = brush_color;

        }

        private void Rfid0_Detach(object sender, DetachEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                tag_text.Content = "Disconnected";
                selected_shape = 0;

                shape1.Opacity = .5;
                shape2.Opacity = .5;
                shape3.Opacity = .5;
                shape4.Opacity = .5;

                sh1.touch.IsHitTestVisible = false;

                patern_rect.Opacity = .5;
                single_rect.Opacity = .5;

            });
        }

        private void Rfid0_TagLost(object sender, RFIDTagLostEventArgs e)
        {

            this.Dispatcher.Invoke(() =>
            {
                tag_text.Content = "";
                selected_shape = 0;

                shape1.Opacity = .5;
                shape2.Opacity = .5;
                shape3.Opacity = .5;
                shape4.Opacity = .5;

                sh1.touch.IsHitTestVisible = false;

                patern_rect.Opacity = .5;
                single_rect.Opacity = .5;

            });
        }

        private void Rfid0_Tag(object sender, RFIDTagEventArgs e)
        {

            this.Dispatcher.Invoke(() =>
            {
                tag_text.Content = e.Tag.ToString();
                selected_shape = 1;

                shape1.Opacity = 1.0;
                shape2.Opacity = .5;
                shape3.Opacity = .5;
                shape4.Opacity = .5;

                opacity_sl.set_value(sh1.touch.get_fade());
                blur_sl.set_value(sh1.touch.get_blur());

                sh1.touch.show();
                shape_touch2.IsHitTestVisible = false;
                shape_touch3.IsHitTestVisible = false;
                shape_touch4.IsHitTestVisible = false; ;

                if (sh1._pattern)
                {
                    patern_rect.Opacity = 1.0;
                    single_rect.Opacity = .5;
                }
                else
                {
                    patern_rect.Opacity = .5;
                    single_rect.Opacity = 1.0;
                }

            });

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            Thickness margin = txture.Margin;
            //MessageBox.Show((mainDock.ActualWidth).ToString());

            //margin.Top = -mainDock.ActualWidth;
            //txture.Margin = margin;


            //fhd
            //< ScaleTransform ScaleY = "0.546296296" ScaleX = "0.546875" />

            //maincanv.Width = mainDock.ActualWidth * 1.830508474576271186;
            //maincanv.Height = mainDock.ActualHeight * 1.828571428571428571; 

            //draw_canvas.Width = mainDock.ActualWidth   * 1.830508474576271186;
            //draw_canvas.Height = mainDock.ActualHeight * 1.828571428571428571;

            //bkg.Width = mainDock.ActualWidth* 1.830508474576271186;
            //bkg.Height = mainDock.ActualHeight* 1.828571428571428571;

            //txture.Width = mainDock.ActualWidth* 1.830508474576271186;
            //txture.Height = mainDock.ActualHeight * 1.828571428571428571;


            maincanv.Width = mainDock.ActualWidth * 6.175;
            maincanv.Height = mainDock.ActualHeight * 6.17142858;

            draw_canvas.Width = mainDock.ActualWidth * 6.175;
            draw_canvas.Height = mainDock.ActualHeight * 6.171428571428;

            bkg.Width = mainDock.ActualWidth * 6.175;
            bkg.Height = mainDock.ActualHeight * 6.173633440514469;

            txture.Width = mainDock.ActualWidth * 6.175;
            txture.Height = mainDock.ActualHeight * 6.171428571428;
            //leftdock.Height = mainDock.ActualHeight;


            //menu_grid.Height = mainDock.ActualHeight - tools_grid.Height -20;

            txture.UpdateLayout();
            bkg.UpdateLayout();
            maincanv.UpdateLayout();
            draw_canvas.UpdateLayout();


            //MessageBox.Show(mainDock.ActualHeight.ToString() + " " + shape_touch1.Height.ToString());
            Canvas.SetTop(shape_touch1, mainDock.ActualHeight - shape_touch1.Height / 2);
            Canvas.SetLeft(shape_touch1, mainDock.ActualWidth - shape_touch1.Width / 2);

            Canvas.SetTop(shape_touch2, mainDock.ActualHeight - shape_touch2.Height / 2);
            Canvas.SetLeft(shape_touch2, mainDock.ActualWidth - shape_touch2.Width / 2);

            Canvas.SetTop(shape_touch3, mainDock.ActualHeight - shape_touch3.Height / 2);
            Canvas.SetLeft(shape_touch3, mainDock.ActualWidth - shape_touch3.Width / 2);

            Canvas.SetTop(shape_touch4, mainDock.ActualHeight - shape_touch4.Height / 2);
            Canvas.SetLeft(shape_touch4, mainDock.ActualWidth - shape_touch4.Width / 2);

            UpdateColor((int)color_picker_image.Width / 2, (int)color_picker_image.Width / 2);




            //MessageBox.Show(maincanv.Width.ToString()+" " + draw_canvas.Height.ToString());
            //MessageBox.Show(mainDock.ActualWidth.ToString() + " " + mainDock.ActualHeight.ToString());
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            Color color = new Color();
            color = System.Windows.Media.Color.FromArgb(255, 100, 100, 111);
            background = CreateBitmapSource(color);
            bkg.Source = background;

        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //bkg.Visibility = Visibility.Hidden;
            txture.Source = new BitmapImage(new Uri(@"\txt4.png", UriKind.Relative));



            ////string inputFileName = args[0];
            //string inputFileName = @"C:\Users\MALSA\Dev\assetsVS\bin\Debug\net6.0-windows\Resources\jahez.jpg";
            //using (var image = new Mat(inputFileName))
            //using (var gray = image.CvtColor(ColorConversionCodes.BGR2GRAY))

            //    gray.ConvertTo(gray, 0, 1, 0);
            ////img.Source = gray.ConvertTo();
            ////gray.SaveImage(outputFileName);

        }


        private BitmapSource CreateBitmapSource(System.Windows.Media.Color color)
        {
            int width = 128;
            int height = width;
            int stride = width / 8;
            byte[] pixels = new byte[height * stride];

            List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
            colors.Add(color);
            BitmapPalette myPalette = new BitmapPalette(colors);

            BitmapSource image = BitmapSource.Create(
                width,
                height,
                96,
                96,
                PixelFormats.Indexed1,
                myPalette,
                pixels,
                stride);

            return image;
        }



        private Bitmap blending(Bitmap l = null, BitmapSource u = null)
        {
            if (l == null || u == null)
            {
                Boolean _scaled = false;
                Bitmap upper_scaled = null;
                //using (Bitmap lower = GetBitmap(background))
                using (Bitmap lower = new Bitmap(@"C:\Users\MALSA\Downloads\back.png"))
                using (Bitmap upper = new Bitmap(@"C:\Users\MALSA\Downloads\color.png"))
                using (Bitmap output = new Bitmap(lower.Width, lower.Height))
                {
                    int width = lower.Width;
                    int height = lower.Height;
                    var rect = new System.Drawing.Rectangle(0, 0, width, height);

                    BitmapData upperData;

                    if (lower.Width != upper.Width || lower.Height != upper.Height)
                    {
                        _scaled = true;
                        upper_scaled = new Bitmap(upper, new System.Drawing.Size(width, height));
                        upperData = upper_scaled.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    }
                    else
                        upperData = upper.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                    BitmapData lowerData = lower.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    //upperData = upper.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    BitmapData outputData = output.LockBits(rect, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                    unsafe
                    {
                        byte* lowerPointer = (byte*)lowerData.Scan0;
                        byte* upperPointer = (byte*)upperData.Scan0;
                        byte* outputPointer = (byte*)outputData.Scan0;

                        for (int i = 0; i < height; i++)
                        {
                            for (int j = 0; j < width; j++)
                            {
                                HSLColor lowerColor = new HSLColor(lowerPointer[2], lowerPointer[1], lowerPointer[0]);
                                HSLColor upperColor = new HSLColor(upperPointer[2], upperPointer[1], upperPointer[0]);
                                upperColor.Luminosity = lowerColor.Luminosity;
                                System.Drawing.Color outputColor = (System.Drawing.Color)upperColor;

                                outputPointer[0] = outputColor.B;
                                outputPointer[1] = outputColor.G;
                                outputPointer[2] = outputColor.R;

                                // Moving the pointers by 3 bytes per pixel
                                lowerPointer += 3;
                                upperPointer += 3;
                                outputPointer += 3;
                            }

                            // Moving the pointers to the next pixel row
                            lowerPointer += lowerData.Stride - (width * 3);
                            upperPointer += upperData.Stride - (width * 3);
                            outputPointer += outputData.Stride - (width * 3);
                        }
                    }

                    lower.UnlockBits(lowerData);
                    if (_scaled)
                    {
                        upper_scaled.UnlockBits(upperData);
                        upper_scaled = null;
                    }

                    else
                        upper.UnlockBits(upperData);
                    output.UnlockBits(outputData);

                    // Drawing the output image
                    return output;
                    //txture.Source = ImageSourceFromBitmap(output);
                }

            }
            else
            {
                Boolean _scaled = false;
                Bitmap upper_scaled = null;
                //using (Bitmap lower = GetBitmap(background))
                using (Bitmap lower = l)
                using (Bitmap upper = GetBitmap(u))
                using (Bitmap output = new Bitmap(lower.Width, lower.Height))
                {
                    int width = lower.Width;
                    int height = lower.Height;
                    var rect = new System.Drawing.Rectangle(0, 0, width, height);

                    BitmapData upperData;

                    if (lower.Width != upper.Width || lower.Height != upper.Height)
                    {
                        _scaled = true;
                        upper_scaled = new Bitmap(upper, new System.Drawing.Size(width, height));
                        upperData = upper_scaled.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    }
                    else
                        upperData = upper.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                    BitmapData lowerData = lower.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    //BitmapData upperData = upper.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    BitmapData outputData = output.LockBits(rect, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                    unsafe
                    {
                        byte* lowerPointer = (byte*)lowerData.Scan0;
                        byte* upperPointer = (byte*)upperData.Scan0;
                        byte* outputPointer = (byte*)outputData.Scan0;

                        for (int i = 0; i < height; i++)
                        {
                            for (int j = 0; j < width; j++)
                            {
                                HSLColor lowerColor = new HSLColor(lowerPointer[2], lowerPointer[1], lowerPointer[0]);
                                HSLColor upperColor = new HSLColor(upperPointer[2], upperPointer[1], upperPointer[0]);
                                upperColor.Luminosity = lowerColor.Luminosity;
                                System.Drawing.Color outputColor = (System.Drawing.Color)upperColor;

                                outputPointer[0] = outputColor.B;
                                outputPointer[1] = outputColor.G;
                                outputPointer[2] = outputColor.R;

                                // Moving the pointers by 3 bytes per pixel
                                lowerPointer += 3;
                                upperPointer += 3;
                                outputPointer += 3;
                            }

                            // Moving the pointers to the next pixel row
                            lowerPointer += lowerData.Stride - (width * 3);
                            upperPointer += upperData.Stride - (width * 3);
                            outputPointer += outputData.Stride - (width * 3);
                        }
                    }

                    lower.UnlockBits(lowerData);
                    if (_scaled)
                    {
                        upper_scaled.UnlockBits(upperData);
                        upper_scaled = null;
                    }

                    else
                        upper.UnlockBits(upperData);
                    output.UnlockBits(outputData);

                    // Drawing the output image

                    //shape_touch1.change_source(ImageSourceFromBitmap(output));
                    return output;
                    //return null;
                }
            }

        }

        //If you get 'dllimport unknown'-, then add 'using System.Runtime.InteropServices;'
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        Bitmap GetBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap(
              source.PixelWidth,
              source.PixelHeight,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
              new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
              ImageLockMode.WriteOnly,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            source.CopyPixels(
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        public static Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            var destRect = new System.Drawing.Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private void combine()
        {
            Bitmap bmp1 = new Bitmap(@"C:\Users\MALSA\Downloads\tri1.png");
            Bitmap bmp2 = new Bitmap(@"C:\Users\MALSA\Downloads\tri1.png");
            int width = bmp1.Width + bmp2.Width;
            int height = Math.Max(bmp1.Height, bmp2.Height);
            Bitmap fullBmp = new Bitmap(width, height);
            Graphics gr = Graphics.FromImage(fullBmp);
            gr.DrawImage(bmp1, 0, 0, bmp1.Width, bmp1.Height);
            gr.DrawImage(bmp2, bmp1.Width, 0, bmp2.Width, bmp2.Height);

            //fullBmp = new Bitmap(width, height, gr);

            shape_touch1.change_source(ImageSourceFromBitmap(fullBmp));

            //fullBmp.Save(blah);
        }

        private void pattern(int rows, ref TouchControl x, string source = @"C:\Users\MALSA\Downloads\tri1.png")
        {
            //Bitmap bmp1 = new Bitmap(@"C:\Users\MALSA\Downloads\tri1.png");

            Bitmap bmp1 = new Bitmap(source);
            int width = bmp1.Width * rows;
            int height = bmp1.Height;
            Bitmap fullBmp = new Bitmap(width, height);
            Graphics gr = Graphics.FromImage(fullBmp);
            //gr.DrawImage(bmp1, 0, 0, bmp1.Width, bmp1.Height);

            Bitmap temp;
            for (int i = 0; i < rows; i++)
            {
                bmp1.RotateFlip(RotateFlipType.Rotate180FlipX);
                gr.DrawImage(bmp1, bmp1.Width * i, 0, bmp1.Width, bmp1.Height);

                //if (i%2 == 0)
                //{
                //    temp = bmp1;
                //    temp.RotateFlip(RotateFlipType.Rotate180FlipX);
                //    gr.DrawImage(temp, bmp1.Width * i, 0, bmp1.Width, bmp1.Height);
                //}
                //else
                //    gr.DrawImage(bmp1, bmp1.Width * i, 0, bmp1.Width, bmp1.Height);
            }


            //fullBmp = new Bitmap(width, height, gr);
            x.change_source(ImageSourceFromBitmap(fullBmp));
            //testimg.change_source(ImageSourceFromBitmap(fullBmp));
            //fullBmp.Save(blah);
        }

        private void pattern_col(int rows, int col, ref TouchControl x, string source)
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            TouchControl y = x;
            //Bitmap bmp1 = new Bitmap(@"C:\Users\MALSA\Downloads\tri1.png");
            Bitmap bmp1 = new Bitmap(source);
            int width = bmp1.Width * col;
            int height = bmp1.Height * rows;
            Bitmap fullBmp = new Bitmap(width, height);
            Graphics gr = Graphics.FromImage(fullBmp);
            //gr.DrawImage(bmp1, 0, 0, bmp1.Width, bmp1.Height);

            Bitmap temp;

            int columnHeight = 0;


            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < col; j++)
                {

                    bmp1.RotateFlip(RotateFlipType.Rotate180FlipX);
                    gr.DrawImage(bmp1, bmp1.Width * j, columnHeight, bmp1.Width, bmp1.Height);

                }

                columnHeight = columnHeight + bmp1.Height;
            }


            shape_bit = fullBmp;
            //fullBmp = new Bitmap(width, height, gr);




            this.Dispatcher.Invoke(() =>
            {
                y.change_source(ImageSourceFromBitmap(fullBmp));
                //testimg.change_source(ImageSourceFromBitmap(fullBmp));

            });
            watch.Stop();
            MessageBox.Show($"Execution Time: {watch.ElapsedMilliseconds} ms");
            //fullBmp.Save(blah);
        }


        private void pattern_mix(int rows, int col, ref TouchControl x, string source)
        {
            //var watch = new System.Diagnostics.Stopwatch();
            //watch.Start();

            TouchControl y = x;
            //Bitmap bmp1 = new Bitmap(@"C:\Users\MALSA\Downloads\tri1.png");
            Bitmap bmp1 = new Bitmap(source);
            int width = bmp1.Width * col;
            int height = bmp1.Height * rows;
            Bitmap colbmp = new Bitmap(bmp1.Width, height);
            Bitmap fullBmp = new Bitmap(width, height);
            Graphics gr = Graphics.FromImage(colbmp);
            Graphics full = Graphics.FromImage(fullBmp);
            //gr.DrawImage(bmp1, 0, 0, bmp1.Width, bmp1.Height);

            if (rows > 1 || col > 1)
            {
                int rowHeight = 0;
                int columnWidth = 0;
                for (int i = 0; i < rows; i++)
                {
                    bmp1.RotateFlip(RotateFlipType.Rotate180FlipY);
                    gr.DrawImage(bmp1, 0, rowHeight, bmp1.Width, bmp1.Height);
                    rowHeight += bmp1.Height;
                }
                for (int j = 0; j < col; j++)
                {
                    colbmp.RotateFlip(RotateFlipType.Rotate180FlipX);
                    full.DrawImage(colbmp, columnWidth, 0, colbmp.Width, colbmp.Height);
                    columnWidth = columnWidth + bmp1.Width;
                }
            }
            else
                full.DrawImage(bmp1, 0, 0, bmp1.Width, bmp1.Height);

            shape_bit = fullBmp;
            //fullBmp = new Bitmap(width, height, gr);

            this.Dispatcher.Invoke(() =>
            {
                //if (y.Width < width)
                //    y.Width = width;
                //if (y.Height < height)
                //    y.Height = height;


                y.change_source(ImageSourceFromBitmap(fullBmp));
                //testimg.change_source(ImageSourceFromBitmap(fullBmp));

            });
            //watch.Stop();
            //MessageBox.Show($"Execution Time: {watch.ElapsedMilliseconds} ms");

            //fullBmp.Save(blah);
        }

        private void pattern_shape(int rows, int col, ref shape_class x)
        {
            //var watch = new System.Diagnostics.Stopwatch();
            //watch.Start();

            TouchControl y = x.touch;
            //Bitmap bmp1 = new Bitmap(@"C:\Users\MALSA\Downloads\tri1.png");
            Bitmap bmp1;
            int width;
            int height;
            Bitmap colbmp;
            Bitmap fullBmp;
            Graphics gr;
            Graphics full;
            //gr.DrawImage(bmp1, 0, 0, bmp1.Width, bmp1.Height);

            if (rows > 1 || col > 1)
            {
                bmp1 = x.bitmap;
                width = bmp1.Width * col;
                height = bmp1.Height * rows;
                colbmp = new Bitmap(bmp1.Width, height);
                fullBmp = new Bitmap(width, height);
                gr = Graphics.FromImage(colbmp);
                full = Graphics.FromImage(fullBmp);

                int rowHeight = 0;
                int columnWidth = 0;
                for (int i = 0; i < rows; i++)
                {
                    bmp1.RotateFlip(RotateFlipType.Rotate180FlipY);
                    gr.DrawImage(bmp1, 0, rowHeight, bmp1.Width, bmp1.Height);
                    rowHeight += bmp1.Height;
                }
                for (int j = 0; j < col; j++)
                {
                    colbmp.RotateFlip(RotateFlipType.Rotate180FlipX);
                    full.DrawImage(colbmp, columnWidth, 0, colbmp.Width, colbmp.Height);
                    columnWidth = columnWidth + bmp1.Width;
                }
                x._pattern = true;
            }
            else
            {
                bmp1 = x.original_bitmap;
                width = bmp1.Width;
                height = bmp1.Height;
                fullBmp = new Bitmap(width, height);
                full = Graphics.FromImage(fullBmp);
                full.DrawImage(change_image_color_bitmap(ref sh1, bmp1), 0, 0, bmp1.Width, bmp1.Height);

                x._pattern = false;
            }


            //shape_bit = fullBmp;
            //fullBmp = new Bitmap(width, height, gr);

            this.Dispatcher.Invoke(() =>
            {
                //if (y.Width < width)
                //    y.Width = width;
                //if (y.Height < height)
                //    y.Height = height;


                y.change_source(ImageSourceFromBitmap(fullBmp));

                //testimg.change_source(ImageSourceFromBitmap(fullBmp));

            });
            x.bitmap = fullBmp;
            //watch.Stop();
            //MessageBox.Show($"Execution Time: {watch.ElapsedMilliseconds} ms");

            //fullBmp.Save(blah);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

            //blending();
            //Bitmap bmp1 = new Bitmap(@"C:\Users\MALSA\Downloads\tri1.png");
            pattern(1, ref shape_touch1, @"C:\Users\MALSA\Downloads\tri1.png");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //Task.Delay(0).ContinueWith(t => pattern_col(5, 5, ref testimg, @"C:\Users\MALSA\Downloads\tri1.png"));

            //Task.Delay(0).ContinueWith(t => pattern_mix(15, 15, ref shape_touch1, @"C:\Users\MALSA\Downloads\fluid-green.png"));

            if (selected_shape == 1)
            {
                Task.Delay(0).ContinueWith(t => pattern_shape(1, 5, ref sh1));
            }
            else if (selected_shape == 2)
            {
                Task.Delay(0).ContinueWith(t => pattern_mix(5, 5, ref shape_touch2, @"C:\Users\MALSA\Downloads\tri3.png"));
            }
            else if (selected_shape == 3)
            {
                Task.Delay(0).ContinueWith(t => pattern_mix(5, 5, ref shape_touch3, @"C:\Users\MALSA\Downloads\fluid.png"));
            }
            else if (selected_shape == 4)
            {
                Task.Delay(0).ContinueWith(t => pattern_mix(5, 5, ref shape_touch4, @"C:\Users\MALSA\Downloads\fluid-green.png"));
            }



        }


        private void single_shape_Click(object sender, RoutedEventArgs e)
        {
            if (selected_shape == 1)
            {
                Task.Delay(0).ContinueWith(t => pattern_shape(1, 1, ref sh1));
            }
            else if (selected_shape == 2)
            {
                Task.Delay(0).ContinueWith(t => pattern_mix(1, 1, ref shape_touch2, @"C:\Users\MALSA\Downloads\tri3.png"));
            }
            else if (selected_shape == 3)
            {
                Task.Delay(0).ContinueWith(t => pattern_mix(1, 1, ref shape_touch3, @"C:\Users\MALSA\Downloads\fluid.png"));
            }
            else if (selected_shape == 4)
            {
                Task.Delay(0).ContinueWith(t => pattern_mix(1, 1, ref shape_touch4, @"C:\Users\MALSA\Downloads\fluid-green.png"));
            }
        }

        private void shape_TouchDown(object sender, TouchEventArgs e)
        {

            //_shape1 = true;
            ////shape_loc = shape.TransformToAncestor(stk).Transform(new System.Windows.Point(0, 0));
            //TouchPoint t = e.GetTouchPoint(this);
            //if(pointOrig.IsEmpty)
            //    pointOrig = new System.Drawing.Point((int)t.Position.X, (int) t.Position.Y);
            //cord.Content = pointOrig.X + " " + pointOrig.Y;
        }

        private void shape_TouchMove(object sender, TouchEventArgs e)
        {

            //TouchPoint t = e.GetTouchPoint(this);
            ////pointOrig = new System.Drawing.Point((int)t.Position.X - pointOrig.X, (int)t.Position.Y - pointOrig.Y);
            ////shape.RenderTransform = new TranslateTransform(pointOrig.X, pointOrig.Y);

            //shape.RenderTransform = new TranslateTransform(t.Position.X - pointOrig.X, t.Position.Y - pointOrig.Y);

            //cord_now.Content = t.Position.X  + " " + t.Position.Y ;

        }

        private void shape_TouchLeave(object sender, TouchEventArgs e)
        {
            //TouchPoint t = e.GetTouchPoint(this);
            //pointOrig = new System.Drawing.Point((int)t.Position.X, (int)t.Position.Y);
            ////TouchPoint t = e.GetTouchPoint(stk);
            ////MessageBox.Show(t.Position.X + " " + t.Position.Y + " ," + pointOrig.X + " " + pointOrig.Y);
            ////System.Windows.Point s = shape.;
            ////MessageBox.Show(t.Bounds.X+" "+ t.Bounds.Y + " " +s.X+" " + s.Y);
        }

        private void shape_TouchUp(object sender, TouchEventArgs e)
        {
            //_shape1 = false;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            //var t = blending(background, shape_bit);
            //shape.Source = ImageSourceFromBitmap(t);



            //blur
            BlurBitmapEffect myBlurEffect = new BlurBitmapEffect();

            // Set the Radius property of the blur. This determines how
            // blurry the effect will be. The larger the radius, the more
            // blurring.
            myBlurEffect.Radius = 10;

            // Set the KernelType property of the blur. A KernalType of "Box"
            // creates less blur than the Gaussian kernal type.
            myBlurEffect.KernelType = KernelType.Gaussian;

            // Apply the bitmap effect to the Button.
            shape_touch1.BitmapEffect = myBlurEffect;


            //testimg2.blur(10);
        }

        private void shape_MouseDown(object sender, MouseButtonEventArgs e)
        {

            //System.Windows.Point p = e.GetPosition(shape);

            //pointOrig = new System.Drawing.Point((int)p.X, (int)p.Y);

            //transPoint = new TranslateTransform(pointOrig.X, pointOrig.Y);
        }

        private void shape_MouseMove(object sender, MouseEventArgs e)
        {
            //    mouseLocation = e.GetPosition(ucCanvas)

            //If e.LeftButton = MouseButtonState.Pressed Then
            //    transPoint.X = (mouseLocation.X - pointOrig.X)
            //    transPoint.Y = (mouseLocation.Y - pointOrig.Y)
            //    ucNode.RenderTransform = transPoint

        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            maincanv.UpdateLayout();

            ExportToPng(new Uri(@"C:\Users\MALSA\OneDrive - Craft Group\projects\R&D\res.png"), maincanv);

            //RenderTargetBitmap rtb = new RenderTargetBitmap((int)maincanv.RenderSize.Width,
            //(int)maincanv.RenderSize.Height, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            //rtb.Render(maincanv);

            //var crop = new CroppedBitmap(rtb, new Int32Rect(0, 0, 620, 650));

            //BitmapEncoder pngEncoder = new PngBitmapEncoder();
            //pngEncoder.Frames.Add(BitmapFrame.Create(crop));

            //using (var fs = System.IO.File.OpenWrite(@"C:\Users\MALSA\OneDrive - Craft Group\projects\R&D\res1.png"))
            //{
            //    pngEncoder.Save(fs);
            //}
        }

        public void change_image_color(System.Drawing.Color old_color, System.Drawing.Color new_color)
        {
            Bitmap image = sh1.bitmap;
            //Bitmap bmp1 = new Bitmap(source);

            //Bitmap image = new Bitmap(@"C:\Users\MALSA\Downloads\tri1.png");
            Graphics e = Graphics.FromImage(image);
            ImageAttributes imageAttributes = new ImageAttributes();
            int width = image.Width;
            int height = image.Height;
            ColorMap colorMap = new ColorMap();

            //colorMap.OldColor = System.Drawing.Color.FromArgb(255, 0, 0, 0);  // opaque red
            //colorMap.NewColor = System.Drawing.Color.FromArgb(255, 0, 0, 255);  // opaque blue

            colorMap.OldColor = old_color;
            colorMap.NewColor = new_color;

            ColorMap[] remapTable = { colorMap };

            imageAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

            e.DrawImage(image, 0, 0, width, height);

            e.DrawImage(
               image,
               new System.Drawing.Rectangle(0, 0, width, height),  // destination rectangle
               0, 0,        // upper-left corner of source rectangle
               width,       // width of source rectangle
               height,      // height of source rectangle
               GraphicsUnit.Pixel,
               imageAttributes);


            this.Dispatcher.Invoke(() =>
            {
                shape_touch1.change_source(ImageSourceFromBitmap(image));
                sh1.bitmap = image;
                sh1.current_color = new_color;

            });


        }

        public Bitmap change_image_color_bitmap(ref shape_class sh, Bitmap x)
        {
            Bitmap image = x;
            Graphics e = Graphics.FromImage(image);
            ImageAttributes imageAttributes = new ImageAttributes();
            int width = image.Width;
            int height = image.Height;
            ColorMap colorMap = new ColorMap();

            colorMap.OldColor = sh.original_color;
            colorMap.NewColor = sh.current_color;

            ColorMap[] remapTable = { colorMap };

            imageAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

            e.DrawImage(image, 0, 0, width, height);

            e.DrawImage(
               image,
               new System.Drawing.Rectangle(0, 0, width, height),  // destination rectangle
               0, 0,        // upper-left corner of source rectangle
               width,       // width of source rectangle
               height,      // height of source rectangle
               GraphicsUnit.Pixel,
               imageAttributes);

            //shape_touch1.change_source(ImageSourceFromBitmap(image));
            return image;
        }

        public void ExportToPng(Uri path, Canvas surface)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            this.Dispatcher.Invoke(() =>
            {
                if (path == null) return;

                try
                {
                    // Save current canvas transform
                    Transform transform = surface.LayoutTransform;
                    // reset current transform (in case it is scaled or rotated)
                    surface.LayoutTransform = null;

                    // Get the size of canvas
                    System.Windows.Size size = new System.Windows.Size(surface.Width, surface.Height);
                    // Measure and arrange the surface
                    // VERY IMPORTANT
                    surface.Measure(size);
                    surface.Arrange(new System.Windows.Rect(size));

                    // Create a render bitmap and push the surface to it
                    RenderTargetBitmap renderBitmap =
                      new RenderTargetBitmap(
                        (int)size.Width,
                        (int)size.Height,
                        96d,
                        96d,
                        PixelFormats.Pbgra32);
                    renderBitmap.Render(surface);

                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    Debug.WriteLine(elapsedMs.ToString() + " " + (elapsedMs / 1000).ToString() + "s");

                    // Create a file stream for saving image
                    using (FileStream outStream = new FileStream(path.LocalPath, FileMode.Create))
                    {
                        // Use png encoder for our data
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        // push the rendered bitmap to it
                        encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                        // save the data to the stream
                        encoder.Save(outStream);
                    }


                    Task.Delay(1000).ContinueWith(t => reset_canvas());

                    // Restore previously saved layout
                    surface.LayoutTransform = transform;
                }
                catch (Exception ee)
                {
                    MessageBox.Show("saving error " + ee.Message);
                }
                

            });

            // the code that you want to measure comes here

        }

        //blur
        private void Slider_ValueChanged(object sender, RoutedEventArgs e)
        {
            //if (shape != null)
            //{

            //    shape.fade(1.0-blur_sl.value/12);
            //}

            if (selected_shape == 1)
            {
                shape_touch1.blur((int)blur_sl.value);
            }
            else if (selected_shape == 2)
            {
                shape_touch2.blur((int)blur_sl.value);
            }
            else if (selected_shape == 3)
            {
                shape_touch3.blur((int)blur_sl.value);
            }
            else if (selected_shape == 4)
            {
                shape_touch4.blur((int)blur_sl.value);
            }
        }


        private void opacity_sl_value_changed(object sender, RoutedEventArgs e)
        {
            if (selected_shape == 1)
            {
                shape_touch1.fade(opacity_sl.value);
            }
            else if (selected_shape == 2)
            {
                shape_touch2.fade(opacity_sl.value);
            }
            else if (selected_shape == 3)
            {
                shape_touch3.fade(opacity_sl.value); ;
            }
            else if (selected_shape == 4)
            {
                shape_touch4.fade(opacity_sl.value);
            }

        }


        private void brush_size_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //if (draw_canvas != null)
            //{
            //    draw_canvas.DefaultDrawingAttributes.Width = brush_size_slider.Value;
            //    draw_canvas.DefaultDrawingAttributes.Height = brush_size_slider.Value;


            //    if (erease.Opacity ==1.0)
            //    {
            //        draw_canvas.EraserShape = new EllipseStylusShape(brush_size_slider.Value, brush_size_slider.Value);
            //    }

            //    var editMode = draw_canvas.EditingMode;
            //    draw_canvas.EditingMode = InkCanvasEditingMode.None;
            //    draw_canvas.EditingMode = editMode;
            //}

        }

        private void draw_enable_Click(object sender, RoutedEventArgs e)
        {
            //if (draw_canvas.IsHitTestVisible)
            //{
            //    draw_canvas.IsHitTestVisible = false;
            //    draw_enable.Content = "Enable Drawing";
            //}
            //else
            //{
            //    draw_canvas.IsHitTestVisible = true;

            //    draw_enable.Content = "Disable Drawing";
            //}
        }

        private void color1_TouchDown(object sender, TouchEventArgs e)
        {

            Color color = new Color();
            color = System.Windows.Media.Color.FromRgb(247, 162, 162);

            if (selected_shape == 1)
            {
                change_image_color(sh1.current_color, System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B));
            }
            else
            {
                color1.StrokeThickness = 2;
                color2.StrokeThickness = 0;
                color3.StrokeThickness = 0;
                color4.StrokeThickness = 0;


                background = CreateBitmapSource(color);
                bkg.Source = background;
            }
        }

        private void color2_TouchDown(object sender, TouchEventArgs e)
        {
            Color color = new Color();
            color = System.Windows.Media.Color.FromRgb(62, 102, 103);


            if (selected_shape == 1)
            {
                Task.Delay(0).ContinueWith(t => change_image_color(sh1.current_color, System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B)));
            }
            else
            {
                color1.StrokeThickness = 0;
                color2.StrokeThickness = 2;
                color3.StrokeThickness = 0;
                color4.StrokeThickness = 0;

                background = CreateBitmapSource(color);
                bkg.Source = background;
            }
        }

        private void color3_TouchDown(object sender, TouchEventArgs e)
        {
            Color color = new Color();
            color = System.Windows.Media.Color.FromRgb(44, 91, 166);

            if (selected_shape == 1)
            {
                Task.Delay(0).ContinueWith(t => change_image_color(sh1.current_color, System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B)));

            }
            else
            {
                color1.StrokeThickness = 0;
                color2.StrokeThickness = 0;
                color3.StrokeThickness = 2;
                color4.StrokeThickness = 0;


                background = CreateBitmapSource(color);
                bkg.Source = background;
            }
        }

        private void color4_TouchDown(object sender, TouchEventArgs e)
        {
            Color color = new Color();
            color = System.Windows.Media.Color.FromRgb(32, 72, 5);

            if (selected_shape == 1)
            {
                Task.Delay(0).ContinueWith(t => change_image_color(sh1.current_color, System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B)));
            }
            else
            {
                color1.StrokeThickness = 0;
                color2.StrokeThickness = 0;
                color3.StrokeThickness = 0;
                color4.StrokeThickness = 2;


                background = CreateBitmapSource(color);
                bkg.Source = background;

            }
        }

        private void Image_TouchDown(object sender, TouchEventArgs e)
        {
            brush_mode = 1;

            pen.Opacity = 1.0;
            highligh.Opacity = .5;
            erease.Opacity = .5;

            draw_canvas.EditingMode = InkCanvasEditingMode.Ink;
            draw_canvas.DefaultDrawingAttributes.IsHighlighter = false;
            //draw_canvas.DefaultDrawingAttributes.Color = Color.FromRgb(0, 0, 0);
            draw_canvas.DefaultDrawingAttributes.StylusTip = StylusTip.Ellipse;

            var inkDA = new DrawingAttributes();
            //inkDA.Color = Colors.Red;
            inkDA.Height = sl1.value;
            inkDA.Width = sl1.value;
            inkDA.FitToCurve = true;
            inkDA.StylusTipTransform = new System.Windows.Media.Matrix(.5, 0, 1, .5, 0, 0);
            draw_canvas.DefaultDrawingAttributes = inkDA;

            selectedColor.A = 255;
            draw_canvas.DefaultDrawingAttributes.Color = selectedColor;

            //draw_canvas.DefaultDrawingAttributes.Width = 3;
            //draw_canvas.DefaultDrawingAttributes.Height = 3;
        }

        private void Image_TouchDown_1(object sender, TouchEventArgs e)
        {
            brush_mode = 2;

            pen.Opacity = .5;
            highligh.Opacity = 1.0;
            erease.Opacity = .5;

            draw_canvas.EditingMode = InkCanvasEditingMode.Ink;
            draw_canvas.DefaultDrawingAttributes.FitToCurve = true;
            draw_canvas.DefaultDrawingAttributes.StylusTip = StylusTip.Ellipse;
            draw_canvas.DefaultDrawingAttributes.StylusTipTransform = new System.Windows.Media.Matrix(1, 0, 0, 1, 0, 0);

            selectedColor.A = 100;
            draw_canvas.DefaultDrawingAttributes.Color = selectedColor;

            //draw_canvas.DefaultDrawingAttributes.Width = 20;
            //draw_canvas.DefaultDrawingAttributes.Height = 20;
        }

        private void Image_TouchDown_2(object sender, TouchEventArgs e)
        {
            pen.Opacity = .5;
            highligh.Opacity = .5;
            erease.Opacity = 1.0;

            draw_canvas.EraserShape = new EllipseStylusShape(50, 50);
            draw_canvas.EditingMode = InkCanvasEditingMode.None;
            draw_canvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
            draw_canvas.DefaultDrawingAttributes.Width = 150;
            draw_canvas.DefaultDrawingAttributes.Height = 150;

        }

        //private void brush_color1_TouchDown(object sender, TouchEventArgs e)
        //{

        //    brush_color1.StrokeThickness = 2;
        //    brush_color2.StrokeThickness = 0;
        //    brush_color3.StrokeThickness = 0;

        //    //Color color = new Color();
        //    if (brush_mode == 1)
        //    {
        //        brush_color = System.Windows.Media.Color.FromArgb(255, 118, 43, 43);
        //        draw_canvas.DefaultDrawingAttributes.Color = brush_color;
        //    }
        //    else if(brush_mode == 2) 
        //    { 
        //        brush_color = System.Windows.Media.Color.FromArgb(100, 118, 43, 43);
        //        draw_canvas.DefaultDrawingAttributes.Color = brush_color;
        //    }

        //}

        //private void brush_color2_TouchDown(object sender, TouchEventArgs e)
        //{
        //    brush_color1.StrokeThickness = 0;
        //    brush_color2.StrokeThickness = 2;
        //    brush_color3.StrokeThickness = 0;

        //    //Color color = new Color();
        //    if(brush_mode == 1)
        //    {
        //        brush_color = System.Windows.Media.Color.FromArgb(255, 41, 176, 180);
        //        draw_canvas.DefaultDrawingAttributes.Color = brush_color;
        //    }
        //    else if(brush_mode == 2)
        //    {
        //        brush_color = System.Windows.Media.Color.FromArgb(100, 41, 176, 180);
        //        draw_canvas.DefaultDrawingAttributes.Color = brush_color;
        //    }


        //}

        //private void brush_color3_TouchDown(object sender, TouchEventArgs e)
        //{
        //    brush_color1.StrokeThickness = 0;
        //    brush_color2.StrokeThickness = 0;
        //    brush_color3.StrokeThickness = 2;

        //    //Color color = new Color();
        //    if (brush_mode == 1)
        //    {
        //        brush_color = System.Windows.Media.Color.FromArgb(255, 53, 58, 66);
        //        draw_canvas.DefaultDrawingAttributes.Color = brush_color;
        //    }
        //    else if (brush_mode == 2)
        //    {
        //        brush_color = System.Windows.Media.Color.FromArgb(100, 53, 58, 66);
        //        draw_canvas.DefaultDrawingAttributes.Color = brush_color;
        //    }

        //}

        private void txture1_TouchDown(object sender, TouchEventArgs e)
        {
            txture1.Opacity = 1.0;
            txture2.Opacity = .5;
            txture3.Opacity = .5;
            txture4.Opacity = .5;

            txture.Source = new BitmapImage(new Uri(@"\textures\txt23.png", UriKind.Relative));

            //var f = blending();
            //txture.Source = ImageSourceFromBitmap(f);
        }

        private void txture2_TouchDown(object sender, TouchEventArgs e)
        {
            txture1.Opacity = .5;
            txture2.Opacity = 1.0;
            txture3.Opacity = .5;
            txture4.Opacity = .5;


            txture.Source = new BitmapImage(new Uri(@"\textures\mud wall.png", UriKind.Relative));
            txture.Source = txture2.Source;

        }

        private void texture3_TouchDown(object sender, TouchEventArgs e)
        {
            txture1.Opacity = .5;
            txture2.Opacity = .5;
            txture3.Opacity = 1.0;
            txture4.Opacity = .5;

            txture.Source = new BitmapImage(new Uri(@"\textures\txt22.png", UriKind.Relative));
        }

        private void txture4_TouchDown(object sender, TouchEventArgs e)
        {
            txture1.Opacity = .5;
            txture2.Opacity = .5;
            txture3.Opacity = .5;
            txture4.Opacity = 1.0;

            txture.Source = new BitmapImage(new Uri(@"\txt4.png", UriKind.Relative));
        }

        private void shape1_TouchDown(object sender, TouchEventArgs e)
        {
            if (selected_shape != 1)
            {
                selected_shape = 1;

                shape1.Opacity = 1.0;
                shape2.Opacity = .5;
                shape3.Opacity = .5;
                shape4.Opacity = .5;

                opacity_sl.set_value(sh1.touch.get_fade());
                blur_sl.set_value(sh1.touch.get_blur());
                sh1.touch.show();
                shape_touch2.IsHitTestVisible = false;
                shape_touch3.IsHitTestVisible = false;
                shape_touch4.IsHitTestVisible = false;


                //shape_touch1.change_source(shape1.Source);

                //pattern(1, ref shape_touch1, @"C:\Users\MALSA\Downloads\tri1.png");

                //selected.Content = selected_shape.ToString();
            }
            else
            {
                shape1.Opacity = .5;
                selected_shape = 0;
                sh1.touch.IsHitTestVisible = false;
            }


        }

        private void shape2_TouchDown(object sender, TouchEventArgs e)
        {
            if (selected_shape != 2)
            {
                selected_shape = 2;

                shape1.Opacity = .5;
                shape2.Opacity = 1.0;
                shape3.Opacity = .5;
                shape4.Opacity = .5;


                opacity_sl.set_value(shape_touch2.get_fade());
                blur_sl.set_value(shape_touch2.get_blur());

                shape_touch1.IsHitTestVisible = false;
                shape_touch2.show();
                shape_touch3.IsHitTestVisible = false;
                shape_touch4.IsHitTestVisible = false;

                //shape_touch2.change_source(shape2.Source);

                //selected.Content = selected_shape.ToString();
            }
            else
            {
                shape2.Opacity = .5;
                selected_shape = 0;
                shape_touch2.IsHitTestVisible = false;
            }
        }

        private void shape3_TouchDown(object sender, TouchEventArgs e)
        {
            if (selected_shape != 3)
            {
                selected_shape = 3;

                shape1.Opacity = .5;
                shape2.Opacity = .5;
                shape3.Opacity = 1.0;
                shape4.Opacity = .5;


                opacity_sl.set_value(shape_touch3.get_fade());
                blur_sl.set_value(shape_touch3.get_blur());

                shape_touch1.IsHitTestVisible = false;
                shape_touch2.IsHitTestVisible = false;
                shape_touch3.show();
                shape_touch4.IsHitTestVisible = false;

                //shape_touch3.change_source(shape3.Source);

                //selected.Content = selected_shape.ToString();
            }
            else
            {
                shape3.Opacity = .5;
                selected_shape = 0;
                shape_touch3.IsHitTestVisible = false;
            }
        }

        private void shape4_TouchDown(object sender, TouchEventArgs e)
        {
            if (selected_shape != 4)
            {
                selected_shape = 4;

                shape1.Opacity = .5;
                shape2.Opacity = .5;
                shape3.Opacity = .5;
                shape4.Opacity = 1.0;

                opacity_sl.set_value(shape_touch4.get_fade());
                blur_sl.set_value(shape_touch4.get_blur());

                shape_touch1.IsHitTestVisible = false;
                shape_touch2.IsHitTestVisible = false;
                shape_touch3.IsHitTestVisible = false;
                shape_touch4.show();




                //shape_touch4.change_source(shape4.Source);

                //selected.Content = selected_shape.ToString();
            }
            else
            {
                shape4.Opacity = .5;
                selected_shape = 0;
                shape_touch4.IsHitTestVisible = false;
            }

            //pattern(1, ref shape_touch4, @"C:\Users\MALSA\Downloads\fluid-green.png");
        }


        private void add_shape()
        {
            TouchControl x = new TouchControl();
            x.Height = 150;
            x.Width = 150;

            x.change_source(@"\tri3.png");


            maincanv.Children.Add(x);
            Canvas.SetTop(x, mainDock.ActualHeight / 2 - x.Height / 2);
            Canvas.SetLeft(x, mainDock.ActualWidth / 2 - x.Width / 2);
            Canvas.SetZIndex(x, 3);
        }

        private void Rectangle_TouchDown(object sender, TouchEventArgs e)
        {
            if (draw_canvas.IsHitTestVisible)
            {
                draw_canvas.IsHitTestVisible = false;
                draw_rect.Opacity = .5;
                brush_grid.Visibility = Visibility.Hidden;
            }
            else
            {
                brush_grid.Visibility = Visibility.Visible;
                draw_canvas.IsHitTestVisible = true;
                draw_rect.Opacity = 1.0;


                // default settings for the brush
                brush_mode = 1;

                pen.Opacity = 1.0;
                highligh.Opacity = .5;
                erease.Opacity = .5;

                draw_canvas.EditingMode = InkCanvasEditingMode.Ink;
                draw_canvas.DefaultDrawingAttributes.IsHighlighter = false;
                //draw_canvas.DefaultDrawingAttributes.Color = Color.FromRgb(0, 0, 0);
                draw_canvas.DefaultDrawingAttributes.StylusTip = StylusTip.Ellipse;

                var inkDA = new DrawingAttributes();
                //inkDA.Color = Colors.Red;
                inkDA.Height = sl1.value;
                inkDA.Width = sl1.value;
                inkDA.FitToCurve = true;
                inkDA.StylusTipTransform = new System.Windows.Media.Matrix(.5, 0, 1, .5, 0, 0);
                draw_canvas.DefaultDrawingAttributes = inkDA;

                selectedColor.A = 255;
                draw_canvas.DefaultDrawingAttributes.Color = selectedColor;

                //draw_canvas.DefaultDrawingAttributes.Width = 3;
                //draw_canvas.DefaultDrawingAttributes.Height = 3;
            }

        }

        private void single_rect_TouchDown(object sender, TouchEventArgs e)
        {
            if (single_rect.Opacity == 1.0)
            {
                //single_rect.Opacity = .5;
                //draw_text.Opacity = .5;

                if (selected_shape == 1)
                {
                    Task.Delay(0).ContinueWith(t => pattern_shape(5, 5, ref sh1));
                }
                else if (selected_shape == 2)
                {
                    Task.Delay(0).ContinueWith(t => pattern_mix(5, 5, ref shape_touch2, @"C:\Users\MALSA\Downloads\tri3.png"));
                }
                else if (selected_shape == 3)
                {
                    Task.Delay(0).ContinueWith(t => pattern_mix(5, 5, ref shape_touch3, @"C:\Users\MALSA\Downloads\fluid.png"));
                }
                else if (selected_shape == 4)
                {
                    Task.Delay(0).ContinueWith(t => pattern_mix(5, 5, ref shape_touch4, @"C:\Users\MALSA\Downloads\fluid-green.png"));
                }

                if (selected_shape != 0)
                {
                    patern_rect.Opacity = 1.0;
                    single_rect.Opacity = .5;
                }
            }
            else
            {
                //single_rect.Opacity = 1.0;
                //draw_text.Opacity = 1.0;
                if (selected_shape == 1)
                {
                    Task.Delay(0).ContinueWith(t => pattern_shape(1, 1, ref sh1));
                }
                else if (selected_shape == 2)
                {
                    Task.Delay(0).ContinueWith(t => pattern_mix(1, 1, ref shape_touch2, @"C:\Users\MALSA\Downloads\tri3.png"));
                }
                else if (selected_shape == 3)
                {
                    Task.Delay(0).ContinueWith(t => pattern_mix(1, 1, ref shape_touch3, @"C:\Users\MALSA\Downloads\fluid.png"));
                }
                else if (selected_shape == 4)
                {
                    Task.Delay(0).ContinueWith(t => pattern_mix(1, 1, ref shape_touch4, @"C:\Users\MALSA\Downloads\fluid-green.png"));
                }

                if (selected_shape != 0)
                {
                    patern_rect.Opacity = .5;
                    single_rect.Opacity = 1.0;
                }
            }
        }

        private void patern_rect_TouchDown(object sender, TouchEventArgs e)
        {
            if (patern_rect.Opacity == 1.0)
            {

                //draw_text.Opacity = .5;

                if (selected_shape == 1)
                {
                    Task.Delay(0).ContinueWith(t => pattern_shape(1, 1, ref sh1));
                }
                else if (selected_shape == 2)
                {
                    Task.Delay(0).ContinueWith(t => pattern_mix(1, 1, ref shape_touch2, @"C:\Users\MALSA\Downloads\tri3.png"));
                }
                else if (selected_shape == 3)
                {
                    Task.Delay(0).ContinueWith(t => pattern_mix(1, 1, ref shape_touch3, @"C:\Users\MALSA\Downloads\fluid.png"));
                }
                else if (selected_shape == 4)
                {
                    Task.Delay(0).ContinueWith(t => pattern_mix(1, 1, ref shape_touch4, @"C:\Users\MALSA\Downloads\fluid-green.png"));
                }

                if (selected_shape != 0)
                {
                    patern_rect.Opacity = .5;
                    single_rect.Opacity = 1.0;
                }
            }
            else
            {
                //patern_rect.Opacity = 1.0;
                //draw_text.Opacity = 1.0;
                if (selected_shape == 1)
                {
                    Task.Delay(0).ContinueWith(t => pattern_shape(5, 5, ref sh1));
                }
                else if (selected_shape == 2)
                {
                    Task.Delay(0).ContinueWith(t => pattern_mix(5, 5, ref shape_touch2, @"C:\Users\MALSA\Downloads\tri3.png"));
                }
                else if (selected_shape == 3)
                {
                    Task.Delay(0).ContinueWith(t => pattern_mix(5, 5, ref shape_touch3, @"C:\Users\MALSA\Downloads\fluid.png"));
                }
                else if (selected_shape == 4)
                {
                    Task.Delay(0).ContinueWith(t => pattern_mix(5, 5, ref shape_touch4, @"C:\Users\MALSA\Downloads\fluid-green.png"));
                }

                if (selected_shape != 0)
                {
                    patern_rect.Opacity = 1.0;
                    single_rect.Opacity = .5;
                }


            }



        }

        private void submit_rect_TouchDown(object sender, TouchEventArgs e)
        {
            try
            {
            submit_rect.Fill = new SolidColorBrush(Color.FromRgb(0, 111, 111));

            //submit_rect.Fill = new SolidColorBrush(Color.FromRgb(0, 111, 111));
            myBlurEffect.Radius = 10;
            brush_grid.BitmapEffect = myBlurEffect;
            menu_grid.BitmapEffect = myBlurEffect;


            Random rnd = new Random();
            int x = rnd.Next(1, 10000);
            
            //Task.Delay(100).ContinueWith(t => ExportToPng(new Uri(@"C:\Users\MALSA\OneDrive - Craft Group\projects\R&D\res " + x + ".png"), maincanv));
                Task.Delay(100).ContinueWith(t => ExportToPng(new Uri(@"Z:\submitted-patterns\res " + x + ".png"), maincanv));
                //maincanv.Visibility = Visibility.Hidden;
            }
            catch(Exception ee)
            {
                MessageBox.Show(ee.Message);
            }


        }

        private void submit_rect_TouchUp(object sender, TouchEventArgs e)
        {


        }

        private void reset_canvas()
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    brush_grid.Visibility = Visibility.Hidden;

                    draw_canvas.Strokes.Clear();
                    bkg.Source = null;
                    txture.Source = null;
                    selected_shape = 0;

                    shape_touch1.change_source(@"\tri1.png");
                    shape_touch2.change_source(@"\tri3.png");
                    shape_touch3.change_source(@"\fluid.png");
                    shape_touch4.change_source(@"\fluid-green.png");

                    shape_touch1.hide();
                    shape_touch2.hide();
                    shape_touch3.hide();
                    shape_touch4.hide();

                    shape_touch1.reset();
                    shape_touch2.reset();
                    shape_touch3.reset();
                    shape_touch4.reset();

                    shape1.Opacity = .5;
                    shape2.Opacity = .5;
                    shape3.Opacity = .5;
                    shape4.Opacity = .5;


                    pen.Opacity = .5;
                    highligh.Opacity = .5;
                    erease.Opacity = .5;
                    //submit_rect.Opacity = .5;

                    patern_rect.Opacity = .5;
                    single_rect.Opacity = .5;
                    draw_rect.Opacity = .5;

                    draw_canvas.IsHitTestVisible = false;
                    draw_rect.Opacity = .5;

                    submit_rect.Fill = new SolidColorBrush(Color.FromRgb(240, 204, 100));

                    Canvas.SetTop(shape_touch1, mainDock.ActualHeight - shape_touch1.Height / 2);
                    Canvas.SetLeft(shape_touch1, mainDock.ActualWidth - shape_touch1.Width / 2);

                    Canvas.SetTop(shape_touch2, mainDock.ActualHeight - shape_touch2.Height / 2);
                    Canvas.SetLeft(shape_touch2, mainDock.ActualWidth - shape_touch2.Width / 2);

                    Canvas.SetTop(shape_touch3, mainDock.ActualHeight - shape_touch3.Height / 2);
                    Canvas.SetLeft(shape_touch3, mainDock.ActualWidth - shape_touch3.Width / 2);

                    Canvas.SetTop(shape_touch4, mainDock.ActualHeight - shape_touch4.Height / 2);
                    Canvas.SetLeft(shape_touch4, mainDock.ActualWidth - shape_touch4.Width / 2);


                    //UpdateColor((int)color_picker_image.Width / 2, (int)color_picker_image.Width / 2);
                    UpdateColor((int)color_picker_image.Width / 2, (int)color_picker_image.Width / 2);

                    //color picker
                    //this.selectedColor = brush_color;

                    myBlurEffect.Radius = 0;
                    brush_grid.BitmapEffect = myBlurEffect;
                    menu_grid.BitmapEffect = myBlurEffect;

                });
            }
            catch (Exception er)
            {
                MessageBox.Show("reset Error: " + er.Message);
            }
           


        }
        private void submit_rect_TouchLeave(object sender, TouchEventArgs e)
        {
            //submit_rect.Fill = new SolidColorBrush(Color.FromRgb(240, 204, 100));
            //reset_canvas();
        }

        private void Label_TouchDown(object sender, TouchEventArgs e)
        {


            if (this.Topmost)
            {
                this.WindowStyle = WindowStyle.ThreeDBorderWindow;
                this.WindowState = WindowState.Normal;
                this.Topmost = false;
            }
            else
            {
                this.WindowStyle = WindowStyle.None;
                //this.ResizeMode = ResizeMode.NoResize;
                this.Topmost = true;
                this.WindowState = WindowState.Maximized;


            }
        }




        //color picker functions:
        //private void CreateAlphaLinearBrush()
        //{
        //    Color startColor = Color.FromArgb((byte)0, SelectedColor.R, SelectedColor.G, SelectedColor.B);
        //    Color endColor = Color.FromArgb((byte)255, SelectedColor.R, SelectedColor.G, SelectedColor.B);
        //    LinearGradientBrush alphaBrush = new LinearGradientBrush(startColor, endColor, new Point(0, 0), new Point(1, 0));
        //    AlphaBorder.Background = alphaBrush;
        //}

        /// <summary>
        /// Sets a new Selected Color based on the color of the pixel under the mouse pointer.
        /// </summary>
        private void UpdateColor(int x = 0, int y = 0)
        {
            int imageX;
            int imageY;
            if (x != 0 && y != 0)
            {
                imageX = x;
                imageY = y;
                if ((imageX < 0) || (imageY < 0) || (imageX > color_picker_image.Width - 1) || (imageY > color_picker_image.Height - 1)) return;
                // Get the single pixel under the mouse into a bitmap and copy it to a byte array
                CroppedBitmap cb = new CroppedBitmap(color_picker_image.Source as BitmapSource, new Int32Rect(imageX, imageY, 1, 1));
                byte[] pixels = new byte[4];
                cb.CopyPixels(pixels, 4, 0);
                // Update the mouse cursor position and the Selected Color
                ellipsePixel.SetValue(Canvas.LeftProperty, (double)(x - (ellipsePixel.Width / 2.0)));
                ellipsePixel.SetValue(Canvas.TopProperty, (double)(y - (ellipsePixel.Width / 2.0)));
                color_picker_image.InvalidateVisual();
                // Set the Selected Color based on the cursor pixel and Alpha Slider value
                selectedColor = Color.FromArgb(255, pixels[2], pixels[1], pixels[0]);
                ellipsePixel.Fill = new SolidColorBrush(Color.FromRgb(selectedColor.R, selectedColor.G, selectedColor.B));
            }
            else
            {
                // Test to ensure we do not get bad mouse positions along the edges
                imageX = (int)Mouse.GetPosition(color_picker_image).X;
                imageY = (int)Mouse.GetPosition(color_picker_image).Y;
                if ((imageX < 0) || (imageY < 0) || (imageX > color_picker_image.Width - 1) || (imageY > color_picker_image.Height - 1)) return;
                // Get the single pixel under the mouse into a bitmap and copy it to a byte array
                CroppedBitmap cb = new CroppedBitmap(color_picker_image.Source as BitmapSource, new Int32Rect(imageX, imageY, 1, 1));
                byte[] pixels = new byte[4];
                cb.CopyPixels(pixels, 4, 0);
                // Update the mouse cursor position and the Selected Color
                ellipsePixel.SetValue(Canvas.LeftProperty, (double)(Mouse.GetPosition(color_picker_image).X - (ellipsePixel.Width / 2.0)));
                ellipsePixel.SetValue(Canvas.TopProperty, (double)(Mouse.GetPosition(color_picker_image).Y - (ellipsePixel.Width / 2.0)));
                color_picker_image.InvalidateVisual();
                // Set the Selected Color based on the cursor pixel and Alpha Slider value
                selectedColor = Color.FromArgb(255, pixels[2], pixels[1], pixels[0]);
                ellipsePixel.Fill = new SolidColorBrush(Color.FromRgb(selectedColor.R, selectedColor.G, selectedColor.B));
                draw_canvas.DefaultDrawingAttributes.Color = selectedColor;
            }


        }

        /// <summary>
        /// Update the mouse cursor ellipse position.
        /// </summary>
        private void UpdateCursorEllipse(Color searchColor)
        {
            // Scan the canvas image for a color which matches the search color
            CroppedBitmap cb;
            Color tempColor = new Color();
            byte[] pixels = new byte[4];
            int searchY = 0;
            int searchX = 0;
            searchColor.A = 255;
            for (searchY = 0; searchY <= color_picker_image.Width - 1; searchY++)
            {
                for (searchX = 0; searchX <= color_picker_image.Height - 1; searchX++)
                {
                    cb = new CroppedBitmap(color_picker_image.Source as BitmapSource, new Int32Rect(searchX, searchY, 1, 1));
                    cb.CopyPixels(pixels, 4, 0);
                    tempColor = Color.FromArgb(255, pixels[2], pixels[1], pixels[0]);
                    if (tempColor == searchColor) break;
                }
                if (tempColor == searchColor) break;
            }
            // Default to the top left if no match is found
            if (tempColor != searchColor)
            {
                searchX = 0;
                searchY = 0;
            }
            // Update the mouse cursor ellipse position
            ellipsePixel.SetValue(Canvas.LeftProperty, ((double)searchX - (ellipsePixel.Width / 2.0)));
            ellipsePixel.SetValue(Canvas.TopProperty, ((double)searchY - (ellipsePixel.Width / 2.0)));

        }


        private void color_picker_canv_TouchDown(object sender, TouchEventArgs e)
        {

        }

        private void color_picker_canv_TouchMove(object sender, TouchEventArgs e)
        {

        }

        private void color_picker_canv_TouchUp(object sender, TouchEventArgs e)
        {

        }

        private void color_picker_canv_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show("g");
            IsMouseDown = true;
            UpdateColor();
        }

        private void color_picker_canv_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsMouseDown) UpdateColor();
        }

        private void color_picker_canv_MouseUp(object sender, MouseButtonEventArgs e)
        {
            IsMouseDown = false;
        }

        private void Rectangle_TouchDown_1(object sender, TouchEventArgs e)
        {
            if (shape_selection1.Width == 50)
            {
                shape_selection1.Width = 100;
                shape_selection1.Height = 100;

            }
            else
            {
                shape_selection1.Width = 50;
                shape_selection1.Height = 50;
            }

        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            rfid0.Close();
        }

        private void insp1_TouchDown(object sender, TouchEventArgs e)
        {
            if (!_insp1)
            {
                _insp1 = true;
                insp1.Fill = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(@"pack://application:,,,/inspiration/Asset 4@4x dark.png", UriKind.Absolute)),
                    Stretch = Stretch.Uniform
                };
            }
            else
            {
                _insp1 = false;
                insp1.Fill = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(@"pack://application:,,,/inspiration/Asset 75@4x.png", UriKind.Absolute)),
                    Stretch = Stretch.Uniform
                };

            }
        }

        private void sl1_value_changed(object sender, RoutedEventArgs e)
        {
            tag_text.Content = sl1.value.ToString();
            if (draw_canvas != null)
            {

                draw_canvas.DefaultDrawingAttributes.Width = sl1.value;
                draw_canvas.DefaultDrawingAttributes.Height = sl1.value;


                if (erease.Opacity == 1.0)
                {
                    draw_canvas.EraserShape = new EllipseStylusShape(sl1.value, sl1.value);
                }

                var editMode = draw_canvas.EditingMode;
                draw_canvas.EditingMode = InkCanvasEditingMode.None;
                draw_canvas.EditingMode = editMode;
            }
        }


    private void insp2_TouchDown(object sender, TouchEventArgs e)
    {
        if (!_insp2)
        {
            _insp2 = true;
            insp2.Fill = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(@"pack://application:,,,/inspiration/Asset 2@4x dark.png", UriKind.Absolute)),
                Stretch = Stretch.Uniform
            };
        }
        else
        {
            _insp2 = false;
            insp2.Fill = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(@"pack://application:,,,/inspiration/Asset 77@4x.png", UriKind.Absolute)),
                Stretch = Stretch.Uniform
            };

        }
    }

    private void insp3_TouchDown(object sender, TouchEventArgs e)
    {
        if (!_insp3)
        {
            _insp3 = true;
            insp3.Fill = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(@"pack://application:,,,/inspiration/Asset 1@4x dark.png", UriKind.Absolute)),
                Stretch = Stretch.Uniform
            };
        }
        else
        {
            _insp3 = false;
            insp3.Fill = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(@"pack://application:,,,/inspiration/Asset 76@4x.png", UriKind.Absolute)),
                Stretch = Stretch.Uniform
            };

        }
    }

    private void insp4_TouchDown(object sender, TouchEventArgs e)
    {
        if (!_insp4)
        {
            _insp4 = true;
            insp4.Fill = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(@"pack://application:,,,/inspiration/Asset 3@4x dark.png", UriKind.Absolute)),
                Stretch = Stretch.Uniform
            };
        }
        else
        {
            _insp4 = false;
            insp4.Fill = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(@"pack://application:,,,/inspiration/Asset 78@4x.png", UriKind.Absolute)),
                Stretch = Stretch.Uniform
            };

        }
    }









    }
}
