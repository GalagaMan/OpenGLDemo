using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.WPF;
using SharpGL.Serialization;
using SharpGL.Controls;
using SharpGL.OpenGLAttributes;
using SharpGL.RenderContextProviders;
using SharpGL.Enumerations;
using SharpGL.VertexBuffers;
using System.IO;
using System.Runtime.InteropServices;


namespace OpenGLDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    { 
        public double heightCoeficient = 1;
        public double widthCoeficient = 1;
        Model model;

        
        const double zNear = 1;
        const double zFar = 1000;
        const double sizeCorrectionFactor = 2;

        const double stepAlpha = 1 * Math.PI / 180;
        const double stepBeta = 1 * Math.PI / 180;
        const double stepR = 1;
        const double stepFov = 1 * Math.PI / 180;
        const double defaultFov = 30 * Math.PI / 180;
        const double minFov = 1 * Math.PI / 180;
        const double maxFov = 80 * Math.PI / 180;

        const double maxBeta = 89.5 * Math.PI / 180;

        double fov = defaultFov;
        double cameraR;
        double cameraAlpha;
        double cameraBeta;

        bool axesVisible = true;

        double[] maxExtension;

        enum RotationAxis
        {
            Z = 0,
            X = 1,
            Y = 2
        }

        RotationAxis rotationAxis;



        public delegate void Keys();

        public event Keys WindowGotResized;

        public MainWindow()
        {
            InitializeComponent();

            model = new Model();
            model.Import(@"cube.obj");

            maxExtension = new double[3]
                {
                    Math.Max(model.Extension.Z, model.Extension.Y),
                    Math.Max(model.Extension.X, model.Extension.Z),
                    Math.Max(model.Extension.Y, model.Extension.X)
                };

            InitRotation(RotationAxis.Z);
        }

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        void OpenGLControl_OpenGLInitialized(object sender, OpenGLRoutedEventArgs args)
        {
            var gl = args.OpenGL;
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.FrontFace(OpenGL.GL_CCW);
            gl.CullFace(OpenGL.GL_BACK);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_TEXTURE);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Enable(OpenGL.GL_MULTISAMPLE);
            gl.ClearColor(0.3f, 0.3f, 0.3f, 0.3f);
        }
          
        public void OpenGLControl_OpenGLDraw(object sender, OpenGLRoutedEventArgs args)
        {
            var gl = args.OpenGL;

            gl.MatrixMode(MatrixMode.Projection);

            PositionCamera(gl);

            gl.MatrixMode(MatrixMode.Modelview);
            //gl.Viewport(0, 0, (int)this.Width, (int)thisht);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);  
            if (axesVisible)            
                DrawLineAxes(gl);

            DrawModel(gl);
        }

        void PositionCamera(OpenGL gl)
        {
            float[] coord = new float[3];

            coord[(int)rotationAxis] = (float)(cameraR * Math.Cos(cameraAlpha) * Math.Cos(cameraBeta));
            coord[((int)rotationAxis + 1) % 3] = (float)(cameraR * Math.Sin(cameraAlpha) * Math.Cos(cameraBeta));
            coord[((int)rotationAxis + 2) % 3] = (float)(cameraR * Math.Sin(cameraBeta));

            gl.LoadIdentity();
            double nearSize = zNear * Math.Tan(fov);
            gl.Frustum(-nearSize, nearSize, -nearSize, nearSize, zNear, zFar);
            gl.LookAt(coord[0], coord[1], coord[2], 0, 0, 0,
                rotationAxis == RotationAxis.X ? 1 : 0,
                rotationAxis == RotationAxis.Y ? 1 : 0,
                rotationAxis == RotationAxis.Z ? 1 : 0);
        }

        void DrawModel(OpenGL gl)
        {
            gl.Begin(BeginMode.Points);

            gl.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Filled);
            float color = 0;
            float color2 = 0;
            float color3 = 0;
            foreach (var flat in model.f)
            {
                gl.Begin(OpenGL.GL_POLYGON);
                gl.LineWidth(5f);
                gl.Color(color/100, color2/100, color3/100);
                color++;
                color2++;
                color3++;
                if (color > 100)
                    color = 0;
                if (color2 > 100)
                    color2 = 0;
                if (color3 > 100)
                    color3 = 0;

                foreach (var vertex in flat)
                {
                    var p = model.Vertices[vertex.PointIndex];
                    gl.Vertex(p.X, p.Y, p.Z);

                }
                //foreach (var point in points)
                //{
                //    gl.Vertex(point.X, point.Y, point.Z-40);
                //}
                gl.End();
            }

            gl.End();
        }

        void DrawLineAxes(OpenGL gl)
        {
            gl.Begin(BeginMode.Lines);
            gl.LineWidth(5f);

            float length = 1.5f * Math.Max(model.Extension.X, Math.Max(model.Extension.Y, model.Extension.Z));

            gl.Color(1f, 0, 0);
            gl.Vertex(0, 0, 0);
            gl.Vertex(length, 0, 0);

            gl.Color(0, 1f, 0);
            gl.Vertex(0, 0, 0);
            gl.Vertex(0, length, 0);

            gl.Color(0, 0, 1f);
            gl.Vertex(0, 0, 0);
            gl.Vertex(0, 0, length);

            gl.End();
        }

        void InitRotation(RotationAxis axis)
        {
            rotationAxis = axis;
            fov = defaultFov;
            cameraR = maxExtension[(int)axis] * sizeCorrectionFactor / Math.Tan(fov);
            cameraAlpha = 0;
            cameraBeta = 0;
        }

        public void Key_Capture(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    if (cameraBeta + stepBeta <= maxBeta)
                        cameraBeta += stepBeta;
                    break;
                case Key.Down:
                    if (cameraBeta - stepBeta >= -maxBeta)
                        cameraBeta -= stepBeta;
                    break;
                case Key.Left:
                    cameraAlpha -= stepAlpha;
                    break;
                case Key.Right:
                    cameraAlpha += stepAlpha;
                    break;
                case Key.Add:
                    cameraR -= stepR;
                    break;
                case Key.Subtract:
                    cameraR += stepR;
                    break;
                case Key.X:
                    InitRotation(RotationAxis.X);
                    break;
                case Key.Y:
                    InitRotation(RotationAxis.Y);
                    break;
                case Key.Z:
                    InitRotation(RotationAxis.Z);
                    break;
                case Key.A:
                    axesVisible = !axesVisible;
                    break;
                case Key.PageUp:
                    if (fov - stepFov >= minFov)
                        fov -= stepFov;
                    break;
                case Key.PageDown:
                    if (fov + stepFov <= maxFov)
                        fov += stepFov;
                    break;
            }
		/*
            
            double x = -1, z = -1;
            if (e != null && Keyboard.IsKeyDown(Key.LeftAlt) & Keyboard.IsKeyDown(Key.Enter))
                {
                if (WindowState != WindowState.Maximized)
                {
                    WindowStyle = WindowStyle.None;
                    WindowState = WindowState.Maximized;
                }
                else
                {
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    WindowState = WindowState.Normal;
                }
            }
            if (e != null && Keyboard.IsKeyDown(Key.LeftAlt) & Keyboard.IsKeyDown(Key.F4))
            {
                this.Close();
            }
            var forward = Keyboard.GetKeyStates(Key.W);
            var backward = Keyboard.GetKeyStates(Key.S);
            var left = Keyboard.GetKeyStates(Key.A);
            var right = Keyboard.GetKeyStates(Key.D);
            var fwrotate = Keyboard.GetKeyStates(Key.Up);
            var fwrotatebck = Keyboard.GetKeyStates(Key.Down);
            var siderotatergh = Keyboard.GetKeyStates(Key.Right);
            var siderotatelft = Keyboard.GetKeyStates(Key.Left);


            if(forward == KeyStates.Down)
            {
                Z += z;
            }
            if (backward == KeyStates.Down)
            {
                Z += -z;
            }
            if (left == KeyStates.Down)
            {
                X += x;
            }
            if (right == KeyStates.Down)
            {
                X += -x;
            }

            if (siderotatergh == KeyStates.Down)
                AngleZ = (float)Math.Atan(z / 20);
            if (siderotatelft == KeyStates.Down)
                AngleZ = (float)Math.Atan(-z / 20);
            if (fwrotate == KeyStates.Down)
                AngleX = (float)Math.Atan(x / 20);
            if (fwrotatebck == KeyStates.Down)
                AngleX = (float)Math.Atan(-x / 20);*/



        }

        public void Mouse_Capture(object sender, MouseEventArgs e)
        {
            
            /*SetCursorPos(0, 0);
            if (this.IsActive)
            {
                Cursor = Cursors.None;
                double newMouseX;
                double newMouseY;
                var pos = e.GetPosition(this);
                newMouseX = pos.X;
                newMouseY = pos.Y;
                mouseX -= newMouseX;
                mouseY -= newMouseY;
                AngleX = (float)Math.Atan(mouseX / mouseY);
                AngleZ = (float)Math.Acos(mouseY / Math.Sqrt(Math.Pow(mouseX, 2) + Math.Pow(mouseY, 2)));
            }
            else
                Cursor = Cursors.Arrow;*/
        }

        private void OpenGLControl_Resized(object sender, OpenGLRoutedEventArgs args)
        {
            heightCoeficient = Height / Width;
            widthCoeficient = Width / Height;
        }


        private void OpenGLControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

    }
}