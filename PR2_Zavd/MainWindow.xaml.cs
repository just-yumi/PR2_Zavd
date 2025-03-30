using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace SpaceObjectsSimulator
{
    public partial class MainWindow : Window
    {
        private readonly List<SpaceObject> _objects = new List<SpaceObject>();
        private readonly Random _random = new Random();
        private readonly DispatcherTimer _animationTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            InitializeAnimationTimer();
            AddInitialObjects();
        }

        private void InitializeAnimationTimer()
        {
            _animationTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
            _animationTimer.Tick += (s, e) => AnimateObjects();
            _animationTimer.Start();
        }

        private void AddInitialObjects()
        {
            for (int i = 0; i < 3; i++)
            {
                AddCube();
                AddPyramid();
                AddFace();
            }
        }

        private void AddCube_Click(object sender, RoutedEventArgs e) => AddCube();
        private void AddPyramid_Click(object sender, RoutedEventArgs e) => AddPyramid();
        private void AddFace_Click(object sender, RoutedEventArgs e) => AddFace();

        private void AddCube()
        {
            var cube = new SpaceObject(CreateCubeModel(), MainViewport);
            _objects.Add(cube);
        }

        private void AddPyramid()
        {
            var pyramid = new SpaceObject(CreatePyramidModel(), MainViewport);
            _objects.Add(pyramid);
        }

        private void AddFace()
        {
            var face = new SpaceObject(CreateFaceModel(), MainViewport);
            _objects.Add(face);
        }

        private Model3D CreateCubeModel()
        {
            var mesh = new MeshGeometry3D
            {
                Positions = new Point3DCollection
                {
                    // Front face
                    new Point3D(-0.5, -0.5, 0.5), new Point3D(0.5, -0.5, 0.5),
                    new Point3D(0.5, 0.5, 0.5), new Point3D(-0.5, 0.5, 0.5),
                    // Back face
                    new Point3D(-0.5, -0.5, -0.5), new Point3D(0.5, -0.5, -0.5),
                    new Point3D(0.5, 0.5, -0.5), new Point3D(-0.5, 0.5, -0.5)
                },
                TriangleIndices = new Int32Collection
                {
                    // Front face
                    0,1,2, 0,2,3,
                    // Back face
                    4,6,5, 4,7,6,
                    // Left face
                    4,0,3, 4,3,7,
                    // Right face
                    1,5,6, 1,6,2,
                    // Top face
                    3,2,6, 3,6,7,
                    // Bottom face
                    4,5,1, 4,1,0
                }
            };

            var material = new DiffuseMaterial(new SolidColorBrush(
                Color.FromRgb((byte)_random.Next(256), (byte)_random.Next(256), (byte)_random.Next(256))));

            return new GeometryModel3D(mesh, material);
        }

        private Model3D CreatePyramidModel()
        {
            var mesh = new MeshGeometry3D
            {
                Positions = new Point3DCollection
                {
                    // Base
                    new Point3D(-0.5, -0.5, -0.5), new Point3D(0.5, -0.5, -0.5),
                    new Point3D(0.5, -0.5, 0.5), new Point3D(-0.5, -0.5, 0.5),
                    // Apex
                    new Point3D(0, 0.5, 0)
                },
                TriangleIndices = new Int32Collection
                {
                    // Base
                    0,1,2, 0,2,3,
                    // Sides
                    0,4,1, 1,4,2,
                    2,4,3, 3,4,0
                }
            };

            var material = new DiffuseMaterial(new SolidColorBrush(
                Color.FromRgb((byte)_random.Next(256), (byte)_random.Next(256), (byte)_random.Next(256))));

            return new GeometryModel3D(mesh, material);
        }

        private Model3D CreateFaceModel()
        {
            var mesh = new MeshGeometry3D
            {
                Positions = new Point3DCollection
                {
                    new Point3D(-0.5, -0.5, 0), new Point3D(0.5, -0.5, 0),
                    new Point3D(0.5, 0.5, 0), new Point3D(-0.5, 0.5, 0)
                },
                TriangleIndices = new Int32Collection { 0, 1, 2, 0, 2, 3 }
            };

            var material = new DiffuseMaterial(new SolidColorBrush(
                Color.FromRgb((byte)_random.Next(256), (byte)_random.Next(256), (byte)_random.Next(256))));

            return new GeometryModel3D(mesh, material);
        }

        private void AnimateObjects()
        {
            foreach (var obj in _objects)
            {
                // Random rotation
                obj.Rotate(new Vector3D(
                    _random.NextDouble() * 0.5,
                    _random.NextDouble() * 0.5,
                    _random.NextDouble() * 0.5));

                // Random movement in zero-g
                if (GravityCheckBox.IsChecked == true)
                {
                    obj.Move(new Vector3D(
                        (_random.NextDouble() - 0.5) * 0.02,
                        (_random.NextDouble() - 0.5) * 0.02,
                        (_random.NextDouble() - 0.5) * 0.02));
                }

                // Bounce off walls
                if (obj.Position.X < -3 || obj.Position.X > 3)
                    obj.Velocity = new Vector3D(-obj.Velocity.X, obj.Velocity.Y, obj.Velocity.Z);
                if (obj.Position.Y < -3 || obj.Position.Y > 3)
                    obj.Velocity = new Vector3D(obj.Velocity.X, -obj.Velocity.Y, obj.Velocity.Z);
                if (obj.Position.Z < -5 || obj.Position.Z > 5)
                    obj.Velocity = new Vector3D(obj.Velocity.X, obj.Velocity.Y, -obj.Velocity.Z);
            }
        }

        private void ClearScene_Click(object sender, RoutedEventArgs e)
        {
            foreach (var obj in _objects)
            {
                obj.RemoveFromViewport(MainViewport);
            }
            _objects.Clear();
        }

        protected override void OnClosed(EventArgs e)
        {
            _animationTimer.Stop();
            base.OnClosed(e);
        }
    }

    public class SpaceObject
    {
        public ModelVisual3D Visual { get; }
        public Transform3DGroup Transform { get; }
        public TranslateTransform3D Translation { get; }
        public RotateTransform3D RotationX { get; }
        public RotateTransform3D RotationY { get; }
        public RotateTransform3D RotationZ { get; }
        public Point3D Position => new Point3D(Translation.OffsetX, Translation.OffsetY, Translation.OffsetZ);
        public Vector3D Velocity { get; set; }

        public SpaceObject(Model3D model, Viewport3D viewport)
        {
            // Initialize transforms
            Translation = new TranslateTransform3D();
            RotationX = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 0));
            RotationY = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0));
            RotationZ = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 0));

            Transform = new Transform3DGroup();
            Transform.Children.Add(RotationX);
            Transform.Children.Add(RotationY);
            Transform.Children.Add(RotationZ);
            Transform.Children.Add(Translation);

            // Create visual
            Visual = new ModelVisual3D
            {
                Content = new Model3DGroup
                {
                    Children = { model }
                },
                Transform = Transform
            };

            // Add to viewport
            viewport.Children.Add(Visual);

            // Set random initial position and velocity
            var random = new Random();
            Translation.OffsetX = (random.NextDouble() - 0.5) * 4;
            Translation.OffsetY = (random.NextDouble() - 0.5) * 4;
            Translation.OffsetZ = (random.NextDouble() - 0.5) * 4;
            Velocity = new Vector3D(
                (random.NextDouble() - 0.5) * 0.1,
                (random.NextDouble() - 0.5) * 0.1,
                (random.NextDouble() - 0.5) * 0.1);
        }

        public void Rotate(Vector3D axisAngle)
        {
            ((AxisAngleRotation3D)RotationX.Rotation).Angle += axisAngle.X;
            ((AxisAngleRotation3D)RotationY.Rotation).Angle += axisAngle.Y;
            ((AxisAngleRotation3D)RotationZ.Rotation).Angle += axisAngle.Z;
        }

        public void Move(Vector3D delta)
        {
            Translation.OffsetX += delta.X;
            Translation.OffsetY += delta.Y;
            Translation.OffsetZ += delta.Z;
            Velocity = delta;
        }

        public void RemoveFromViewport(Viewport3D viewport)
        {
            viewport.Children.Remove(Visual);
        }
    }
}