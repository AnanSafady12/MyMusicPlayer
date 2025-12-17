using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Player_Events
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
   
    
    public partial class MainWindow : Window
    {

        public event EventHandler<Color> ColorChanged;

        public MainWindow()
        {
            InitializeComponent();
            GetColors();
            this.Title += Environment.ProcessId.ToString() + " - ";
            this.Title += Environment.MachineName.ToString() + " - ";
            this.Title += Environment.OSVersion.ToString() + " - ";
            this.Title += Guid.NewGuid().ToString() + " - ";
        }
        public void GetColors()
        {
            PropertyInfo[] colorsProps = typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static);
            //PropertyInfo firstColor = colorsProps[0];
            //object obj = firstColor.GetValue(null);

            //if(obj is Color color)
            //{
            //    //use color
            //}

            var colors = colorsProps.Select(p => new
            {
                Name = p.Name,
                Brush = new SolidColorBrush((Color)p.GetValue(null))
            })
                .OrderBy(c => c.Name)
                .ToList();


            this.lstcolors.ItemsSource = colors;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">the object invoker</param>
        /// <param name="e">data object related to the event</param>
        private void lstcolors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox lb)
            {
                if (lb.SelectedItems is not null)
                {
                    dynamic selectedColor = lb.SelectedItem;

                    if (selectedColor.Brush is SolidColorBrush brushColor)
                    {
                        Color colorSelected = brushColor.Color;
                        if (ColorChanged != null)
                        {
                            ColorChanged.Invoke(this, colorSelected);
                        }
                    }
                }
            }
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SubWindow subWindow = new SubWindow();
            this.ColorChanged += subWindow.MainWindow_ColorChanged;

            subWindow.Show();
        }


    }
}