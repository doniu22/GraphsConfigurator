using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Konfigurator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<String> nazwy_tla = new List<String>();
        private List<String> bacground_paths = new List<String>();
        private List<Punkt> punkty = new List<Punkt>();          // lista przechowująca punkty
        private List<Polaczenie> polaczenia = new List<Polaczenie>();    // lista przechowująca połaczenia
        private Line linia;                                              //linia do polaczen
        private Ellipse circle;                                          //ellipsa do punktów
        private TextBlock nazwa;                                         //nazwa do punktów

        private Punkt sp,ep;
        
        private ImageBrush brush;
        private SolidColorBrush mySolidColorBrush = new SolidColorBrush();
        private SolidColorBrush mySolidColorBrush2 = new SolidColorBrush();
        private int points_iterator = 0;
        private int iterator2 = 0;
        private int actual_floor = 0;
        private int floor_counter = 0;
        private string directory_path;

        public MainWindow()
        {
            InitializeComponent();
            mySolidColorBrush.Color = Colors.Gray;
            mySolidColorBrush2.Color = Colors.Red;

        }

        private void load_Data(object sender, RoutedEventArgs e)
        {
            
            nazwy_tla.Clear();
            bacground_paths.Clear();
            punkty.Clear();
            polaczenia.Clear();
            background_map.Children.Clear();
            pointsListBox.Items.Clear();
            edgesListBox.Items.Clear();
            sourcePoint.Items.Clear();
            destinationPoint.Items.Clear();

            MessageBox.Show("Get data to edit! Background should be in the same folder!!");

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.csv)|*.csv";

            if (openFileDialog.ShowDialog() == true)
            {
                actual_floor = 1;
                textBox2.Text = actual_floor.ToString();

                directory_path = System.IO.Path.GetDirectoryName(openFileDialog.FileName) + "\\";


                using (StreamReader sr = new StreamReader(openFileDialog.FileName))
                {

                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        // załadowanie krawędzi
                        if (line.StartsWith("E:"))
                        {
                            string[] dataString = line.Substring(2).Split(';');

                            // dodanie połączenia do listy połączeń
                            var p1 = punkty.Find(new Predicate<Punkt>((Punkt p) => p.PointName.Equals(dataString[0])));
                            var p2 = punkty.Find(new Predicate<Punkt>((Punkt p) => p.PointName.Equals(dataString[1])));

                            Polaczenie newEdge = new Polaczenie(p1, p2, Double.Parse(dataString[2]));
                            polaczenia.Add(newEdge);
                        }

                        //załadowanie punktów
                        else if (line.StartsWith("P:"))
                        {
                            string[] dataString = line.Substring(2).Split(';');

                            // dodanie punktu do listy punktów
                            Punkt newPoint = new Punkt(dataString[0], Double.Parse(dataString[1]), Double.Parse(dataString[2]), Double.Parse(dataString[3]), dataString[4]);
                            punkty.Add(newPoint);

                        }

                        // załadowanie tła mapy
                        else if (line.StartsWith("G:"))
                        {
                            string[] dataString = line.Substring(2).Split(';');


                            for (int i = 0; i < dataString.Count(); i++)
                            {
                                nazwy_tla.Add(dataString[i]);
                                bacground_paths.Add(directory_path + dataString[i]);
                            }

                            floor_counter = dataString.Count();

                            brush = new ImageBrush();
                            brush.ImageSource = new BitmapImage(new Uri(bacground_paths[actual_floor - 1]));
                            background_map.Background = brush;




                        }
                    }
                }


                if (floor_counter == 1)
                {
                    back_floor.IsEnabled = false;
                    next_floor.IsEnabled = false;
                }
                else
                    next_floor.IsEnabled = true;

                points_iterator = Int32.Parse(punkty[punkty.Count-1].PointName.Substring(1)); // pobranie id ostatniego punktu
                draw_Points();
                draw_Edges();
                drawing.IsEnabled = true;
            }
        }
       
        private void onMouseDown(object sender, MouseButtonEventArgs e)
        {

            if (draw_points.IsChecked == true)
            {
                points_iterator++;
                add_New_Point(e);
                background_map.Children.Clear();
                pointsListBox.Items.Clear();
                edgesListBox.Items.Clear();
                sourcePoint.Items.Clear();
                destinationPoint.Items.Clear();
                draw_Points();
                draw_Edges();
            }
            else if (draw_edges.IsChecked == true)
            {
                
                for (int i = 0; i < punkty.Count; i++)
                {
                    if (punkty[i].PointZ == actual_floor)
                    {
                        if ((punkty[i].PointXY.X >= e.GetPosition(background_map).X - 4 && punkty[i].PointXY.X <= e.GetPosition(background_map).X + 4)
                            && (punkty[i].PointXY.Y >= e.GetPosition(background_map).Y - 4 && punkty[i].PointXY.Y <= e.GetPosition(background_map).Y + 4))
                        {
                            iterator2++;

                            if (iterator2 == 1)
                            {
                                sp = punkty[i];
                            }
                            else if (iterator2 == 2)
                            {


                                ep = punkty[i];
                                iterator2 = 0;
                                if (sp.Equals(ep))
                                {
                                    MessageBox.Show("You haven't add an edge between the same points!");
                                }
                                else
                                {
                                    if (!edge_Exsist(sp, ep))
                                    {
                                        add_New_Edge(sp, ep);
                                        background_map.Children.Clear();
                                        pointsListBox.Items.Clear();
                                        edgesListBox.Items.Clear();
                                        sourcePoint.Items.Clear();
                                        destinationPoint.Items.Clear();
                                        draw_Points();
                                        draw_Edges();
                                    }
                                    else
                                    {
                                        MessageBox.Show("Edge exsist!!!");
                                    }
                                }

                            }

                        }
                    }
                }

            }
        }

        private void add_New_Point(MouseButtonEventArgs e)
        {
            //ustawienie nazwy tworzonego punktu
            string pointName = textBox.Text + points_iterator;  

            // dodanie punktu do listy punktów 
            punkty.Add(new Punkt(pointName, Math.Round(e.GetPosition(background_map).X,0), Math.Round(e.GetPosition(background_map).Y,0), Double.Parse(textBox2.Text), ""));

        }

        private void add_New_Edge(Punkt startPoint, Punkt endPoint)
        {
            Double X2, X1, Y2, Y1, koszt;

            X2 = endPoint.PointXY.X;
            X1 = startPoint.PointXY.X;
            Y2 = endPoint.PointXY.Y;
            Y1 = startPoint.PointXY.Y;

            if (set_koszt.Text == "")
                koszt = Math.Sqrt((Math.Pow((X2 - X1), 2.0) + Math.Pow((Y2 - Y1), 2.0)));
            else
                koszt = double.Parse(set_koszt.Text);

            //dodanie nowego połączenia do listy połączeń
            polaczenia.Add(new Polaczenie(startPoint, endPoint, Math.Round(koszt, 0)));
           
        }

        private void draw_Points()
        {
            
            double left, top;

            for (int i = 0; i < punkty.Count; i++)
            {

                
                    //dodanie punktu do combo boxów
                    sourcePoint.Items.Add(punkty[i].PointName);
                    destinationPoint.Items.Add(punkty[i].PointName);
                    // dodanie punktu do aktywnej listy punktów
                    pointsListBox.Items.Add(punkty[i].PointName + ';' + punkty[i].PointXY.X + ';' + punkty[i].PointXY.Y + ';' + punkty[i].PointZ);
                
                if (punkty[i].PointZ == actual_floor)
                {
                    circle = new Ellipse();  //punkt wyświetlany na canvas
                    circle.Height = 8;
                    circle.Width = 8;
                    circle.Stroke = mySolidColorBrush;
                    circle.Fill = mySolidColorBrush;

                    nazwa = new TextBlock(); //nazwa punktu wyświetlanego na canvas
                    nazwa.Text = punkty[i].PointName;
                    nazwa.Foreground = mySolidColorBrush2;
                    nazwa.FontSize = 10;

                    left = punkty[i].PointXY.X - circle.Width / 2;
                    top = punkty[i].PointXY.Y - circle.Width / 2;

                    nazwa.Margin = new Thickness(left + 4, top - 15, 0, 0);
                    circle.Margin = new Thickness(left, top, 0, 0);



                    background_map.Children.Add(circle); //dodanie do kanwy punktu i jego nazwy
                    background_map.Children.Add(nazwa);

                    
                }
            }

            
        }

        private void draw_Edges()
        {

            for (int i = 0; i < polaczenia.Count; i++)
            {
                //dodanie połączenia do listy połaczeń
                edgesListBox.Items.Add(polaczenia[i].A1.PointName + "->" + polaczenia[i].B1.PointName);

                if (polaczenia[i].A1.PointZ == actual_floor && polaczenia[i].B1.PointZ == actual_floor)
                {
                    linia = new Line();
                    linia.Fill = mySolidColorBrush;
                    linia.Stroke = mySolidColorBrush;
                    linia.StrokeThickness = 2;
                    linia.X1 = polaczenia[i].A1.PointXY.X;
                    linia.Y1 = polaczenia[i].A1.PointXY.Y;
                    linia.X2 = polaczenia[i].B1.PointXY.X;
                    linia.Y2 = polaczenia[i].B1.PointXY.Y;

                    // dodanie nowego połączenia do kanwy
                    background_map.Children.Insert(0, linia);
  
                }
            }
        }

        private void choose_Backgrounds(object sender, RoutedEventArgs e)
        {
            //czyszczenie
            nazwy_tla.Clear();
            bacground_paths.Clear();
            punkty.Clear();
            polaczenia.Clear();
            background_map.Children.Clear();
            pointsListBox.Items.Clear();
            edgesListBox.Items.Clear();
            sourcePoint.Items.Clear();
            destinationPoint.Items.Clear();

            points_iterator = 0;

            MessageBox.Show("Get all bitmaps of organisation for 1 floor!");

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png)|*.png";
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
            {
                actual_floor = 1;
                textBox2.Text = actual_floor.ToString();

                directory_path = System.IO.Path.GetDirectoryName(openFileDialog.FileName) + "\\";

                foreach (string filename in openFileDialog.FileNames)
                {
                    bacground_paths.Add(filename);
                    nazwy_tla.Add(System.IO.Path.GetFileName(filename));
                }

                floor_counter = bacground_paths.Count;
                                
                brush = new ImageBrush();
                brush.ImageSource = new BitmapImage(new Uri(bacground_paths[actual_floor-1]));
                background_map.Background = brush;

                drawing.IsEnabled = true;

                if (floor_counter == 1)
                {
                    back_floor.IsEnabled = false;
                    next_floor.IsEnabled = false;
                }
                else
                    next_floor.IsEnabled = true;

            }
        }

        private void save_Data(object sender, RoutedEventArgs e)
        {
            StreamWriter save = new StreamWriter(directory_path + "\\data.csv", false);
            String line = "G:";

            for (int k = 0; k < nazwy_tla.Count(); k++)
            {
                if (k == nazwy_tla.Count() - 1)
                {
                    line = line + nazwy_tla[k];
                }
                else
                {
                    line = line + nazwy_tla[k] + ';';
                }
            }
            save.WriteLine(line);


            for (int i = 0; i < punkty.Count(); i++)
            {
                line = "P:" + punkty[i].PointName + ';' + punkty[i].PointXY.X + ';' + punkty[i].PointXY.Y + ';' + punkty[i].PointZ + ';' + punkty[i].PointText;
                save.WriteLine(line);
            }

            for (int j = 0; j < polaczenia.Count(); j++)
            {
                line = "E:" + polaczenia[j].A1.PointName + ';' + polaczenia[j].B1.PointName + ';' + polaczenia[j].Koszt.ToString(CultureInfo.GetCultureInfo("en-GB"));
                save.WriteLine(line);
            }
            save.Close();

        }

        private void add_Edge_Click(object sender, RoutedEventArgs e)
        {
            if (sourcePoint.SelectedIndex == -1 || destinationPoint.SelectedIndex == -1)
                MessageBox.Show("You have to choose start and end points!");
            else if (sourcePoint.SelectedIndex == destinationPoint.SelectedIndex)
                MessageBox.Show("You haven't add an edge between the same points!!");
            else if (set_koszt.Text == "")
                MessageBox.Show("You have to set expense of this edge!!!");
            else
            {
                add_New_Edge(punkty[sourcePoint.SelectedIndex], punkty[destinationPoint.SelectedIndex]);
                background_map.Children.Clear();
                pointsListBox.Items.Clear();
                edgesListBox.Items.Clear();
                sourcePoint.Items.Clear();
                destinationPoint.Items.Clear();
                set_koszt.Text = "";
                draw_Points();
                draw_Edges();
            }
        }

        private void deletePointBtn_Click(object sender, RoutedEventArgs e)
        {
            if (pointsListBox.SelectedIndex == -1)
            {
                MessageBox.Show("You have to choose point to delete it!");
            }
            else
            {
                
                for (int i = 0; i < polaczenia.Count; i++)
                {
                    if (polaczenia[i].A1.Equals(punkty[pointsListBox.SelectedIndex]) || polaczenia[i].B1.Equals(punkty[pointsListBox.SelectedIndex]))
                    {
                        polaczenia.RemoveAt(i);
                        i--;                  
                    }
                }
                punkty.RemoveAt(pointsListBox.SelectedIndex);

                background_map.Children.Clear();
                pointsListBox.Items.Clear();
                edgesListBox.Items.Clear();
                sourcePoint.Items.Clear();
                destinationPoint.Items.Clear();

                draw_Points();
                draw_Edges();

            }
        }

        private void deleteEdgeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (edgesListBox.SelectedIndex == -1)
            {
                MessageBox.Show("You have to choose an edge to delete it!");
            }
            else
            {
                polaczenia.RemoveAt(edgesListBox.SelectedIndex);

                background_map.Children.Clear();
                pointsListBox.Items.Clear();
                edgesListBox.Items.Clear();
                sourcePoint.Items.Clear();
                destinationPoint.Items.Clear();

                draw_Points();
                draw_Edges();

            }
        }

        private void next_floor_Click(object sender, RoutedEventArgs e)
        {
            actual_floor++;
            textBox2.Text = actual_floor.ToString();
            back_floor.IsEnabled = true;
            if (actual_floor == floor_counter)
            {
                next_floor.IsEnabled = false;
            }

            brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri(bacground_paths[actual_floor - 1]));
            background_map.Background = brush;

            background_map.Children.Clear();
            pointsListBox.Items.Clear();
            edgesListBox.Items.Clear();
            sourcePoint.Items.Clear();
            destinationPoint.Items.Clear();
            draw_Points();
            draw_Edges();
        }

        private void back_floor_Click(object sender, RoutedEventArgs e)
        {
            actual_floor--;
            textBox2.Text = actual_floor.ToString();
            next_floor.IsEnabled = true;
            if (actual_floor == 1)
            {
                back_floor.IsEnabled = false;
            }

            brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri(bacground_paths[actual_floor - 1]));
            background_map.Background = brush;

            background_map.Children.Clear();
            pointsListBox.Items.Clear();
            edgesListBox.Items.Clear();
            sourcePoint.Items.Clear();
            destinationPoint.Items.Clear();
            draw_Points();
            draw_Edges();
        }

        private bool edge_Exsist(Punkt p1, Punkt p2 )
        {

            for (int i = 0; i < polaczenia.Count; i++)
            {

                if ((polaczenia[i].A1.Equals(p1) && polaczenia[i].B1.Equals(p2)) || (polaczenia[i].A1.Equals(p2) && polaczenia[i].B1.Equals(p1)))
                {
                    return true;
                }

            }


            return false;

        }
        

    }
}
