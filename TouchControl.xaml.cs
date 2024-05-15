using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Draw
{
    // This class was implemented from https://blogs.msdn.microsoft.com/llobo/2009/12/21/wpf-manipulation-basics/

    /// <summary>
    /// Interaction logic for TouchControl.xaml
    /// </summary>
    public partial class TouchControl : UserControl
    {
        private TransformGroup transformGroup;
        private TranslateTransform translation;
        private ScaleTransform scale;
        private RotateTransform rotation;
        private int blur_value = 0;

        Boolean _move_started = false;

        double oldx;
        double oldy;
        double oldangle;
        double oldscalex;
        double oldscaley;


        double originalx;
        double originaly;
        double originalangle;
        double originalscalex;
        double originalscaley;

        BlurBitmapEffect myBlurEffect;


        public TouchControl()
        {
            InitializeComponent();

            myBlurEffect = new BlurBitmapEffect();

            transformGroup = new TransformGroup();

            translation = new TranslateTransform(0, 0);
            scale = new ScaleTransform(1, 1);
            rotation = new RotateTransform(0);


            transformGroup.Children.Add(rotation);
            transformGroup.Children.Add(scale);
            transformGroup.Children.Add(translation);


            BasicRect.RenderTransform = transformGroup;


            originalx = translation.X;
            originaly = translation.Y;
            originalangle = rotation.Angle;
            originalscalex = scale.ScaleX;
            originalscaley = scale.ScaleY;

            //this.Height = 150;
            //this.Width = 150;

            //BasicRect.Stretch = Stretch.Fill;
        }

        protected override void OnManipulationStarting(ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = this;


            oldx = translation.X;
            oldy = translation.Y;
            oldangle = rotation.Angle;
            oldscalex = scale.ScaleX;
            oldscaley = scale.ScaleY;
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {

            // the center never changes in this sample, although we always compute it.
            Point center = new Point(
                 BasicRect.RenderSize.Width / 2.0, BasicRect.RenderSize.Height / 2.0);

            // apply the rotation at the center of the rectangle if it has changed
            rotation.CenterX = center.X;
            rotation.CenterY = center.Y;
            rotation.Angle += e.DeltaManipulation.Rotation;

            // Scale is always uniform, by definition, so the x and y will always have the same magnitude
            scale.CenterX = center.X;
            scale.CenterY = center.Y;
            scale.ScaleX *= e.DeltaManipulation.Scale.X;
            scale.ScaleY *= e.DeltaManipulation.Scale.Y;

            // apply translation
            //if (translation.X + e.DeltaManipulation.Translation.X > -this.Width/2 *scale.ScaleX)
            
            translation.X += e.DeltaManipulation.Translation.X;
            translation.Y += e.DeltaManipulation.Translation.Y;
        }

        public void change_source(ImageSource x)
        {
            this.BasicRect.Source = x;
        }

        public void change_source(string x)
        {
            this.BasicRect.Source = new BitmapImage(new Uri(x, UriKind.Relative));
        }

        public void blur(int x)
        {
            //blur
            //BlurBitmapEffect myBlurEffect = new BlurBitmapEffect();

            // Set the Radius property of the blur. This determines how
            // blurry the effect will be. The larger the radius, the more
            // blurring.
            myBlurEffect.Radius = x;
            this.blur_value = x;
            // Set the KernelType property of the blur. A KernalType of "Box"
            // creates less blur than the Gaussian kernal type.
            myBlurEffect.KernelType = KernelType.Gaussian;

            // Apply the bitmap effect to the Button.
            this.BasicRect.BitmapEffect = myBlurEffect;
        }

        public void fade(double x)
        {
            this.BasicRect.Opacity = x;
        }

        public double get_fade()
        {
            return this.BasicRect.Opacity;
        }

        public int get_blur()
        {
            return this.blur_value;
        }

        public void blend()
        {
            // the center never changes in this sample, although we always compute it.
            Point center = new Point(
                 BasicRect.RenderSize.Width / 2.0, BasicRect.RenderSize.Height / 2.0);

            // apply the rotation at the center of the rectangle if it has changed
            rotation.CenterX = center.X;
            rotation.CenterY = center.Y;
            rotation.Angle = oldangle;

            // Scale is always uniform, by definition, so the x and y will always have the same magnitude
            scale.CenterX = center.X;
            scale.CenterY = center.Y;
            scale.ScaleX = oldscalex;
            scale.ScaleY = oldscaley;

            // apply translation
            //if (translation.X + e.DeltaManipulation.Translation.X > -this.Width/2 *scale.ScaleX)

            translation.X = oldx;
            translation.Y = oldy;
            // Apply the bitmap effect to the Button.
            //this.BasicRect.ima
        }

        public void hide()
        {
            this.Visibility = Visibility.Hidden;
            this.IsHitTestVisible = false;
        }

        public void show()
        {
            this.Visibility = Visibility.Visible;
            this.IsHitTestVisible = true;
        }

        public void reset()
        {
            // the center never changes in this sample, although we always compute it.
            Point center = new Point(
                 BasicRect.RenderSize.Width / 2.0, BasicRect.RenderSize.Height / 2.0);

            // apply the rotation at the center of the rectangle if it has changed
            rotation.CenterX = center.X;
            rotation.CenterY = center.Y;
            rotation.Angle = originalangle;

            // Scale is always uniform, by definition, so the x and y will always have the same magnitude
            scale.CenterX = center.X;
            scale.CenterY = center.Y;
            scale.ScaleX = originalscalex;
            scale.ScaleY = originalscaley;

            // apply translation
            //if (translation.X + e.DeltaManipulation.Translation.X > -this.Width/2 *scale.ScaleX)

            translation.X = originalx;
            translation.Y = originaly;
            // Apply the bitmap effect to the Button.
            //this.BasicRect.ima


            //blur


            // Set the Radius property of the blur. This determines how
            // blurry the effect will be. The larger the radius, the more
            // blurring.
            myBlurEffect.Radius = 0;
            this.blur_value = 0;
            // Set the KernelType property of the blur. A KernalType of "Box"
            // creates less blur than the Gaussian kernal type.
            myBlurEffect.KernelType = KernelType.Gaussian;

            // Apply the bitmap effect to the Button.
            this.BasicRect.BitmapEffect = myBlurEffect;

            this.BasicRect.Opacity = 1.0;
        }


    }
}
