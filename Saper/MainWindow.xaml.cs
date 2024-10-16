using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Threading;
using System.ComponentModel;

namespace Saper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static GameSettings gameSettings = new GameSettings();
        List<Cell> CellList = new List<Cell>();
        int CellWithoutBombs;
        int CellCountClicked;
        int time = -1;
        public static Coord[] CoordDifferenceArray = new Coord[] {
                new Coord(-1, -1),
                new Coord(0, -1),
                new Coord(1, -1),
                new Coord(-1, 0),
                new Coord(1, 0),
                new Coord(-1, 1),
                new Coord(0, 1),
                new Coord(1, 1)};
        public static Coord[] CoordDifferenceArray2 = new Coord[] {
                new Coord(0, 1),
                new Coord(0, -1),
                new Coord(1, 0),
                new Coord(-1, 0)};
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Thread TimeCounterThread = new Thread(TimeCounterFunc);
            TimeCounterThread.Start();
            Closing += new CancelEventHandler(Window_Closing);
            gameSettings.DefaultHeight = (int)MainGrid.Height;
            gameSettings.DefaultWidth = (int)MainGrid.Width;
        }

        private void SetSize10x10_Checked(object sender, RoutedEventArgs e)
        {
            gameSettings.FieldHeight = 10;
            gameSettings.FieldWidth = 10;
            gameSettings.ButtonHeight = 32;
            gameSettings.ButtonWidth = 32;
        }

        private void SetSize20x20_Checked(object sender, RoutedEventArgs e)
        {
            gameSettings.FieldHeight = 20;
            gameSettings.FieldWidth = 20;
            gameSettings.ButtonHeight = 26;
            gameSettings.ButtonWidth = 26;
        }

        private void SetSize30x30_Checked(object sender, RoutedEventArgs e)
        {
            gameSettings.FieldHeight = 30;
            gameSettings.FieldWidth = 30;
            gameSettings.ButtonHeight = 20;
            gameSettings.ButtonWidth = 20;
        }

        private void SetBomb10_Checked(object sender, RoutedEventArgs e)
        {
            gameSettings.CountBombs = 0.1f;
        }

        private void SetBomb20_Checked(object sender, RoutedEventArgs e)
        {
            gameSettings.CountBombs = 0.2f;
        }

        private void SetBomb30_Checked(object sender, RoutedEventArgs e)
        {
            gameSettings.CountBombs = 0.3f;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            MainGrid.Width = gameSettings.DefaultWidth;
            foreach (Cell item in CellList)
            {
                item.Destroy(MainGrid);
            }
            CellList.Clear();
            CellWithoutBombs = 0;
            CellCountClicked = 0;
            time = 0;
            for (int i = 0; i < gameSettings.FieldWidth; i++)
            {
                for (int j = 0; j < gameSettings.FieldHeight; j++)
                {
                    Cell temp = new Cell();
                    temp.Coord = new Coord(i, j);
                    CellList.Add(temp);
                }
            }
            foreach (var item in CellList)
            {
                item.CreateButton(MainGrid);
                item.ButtonCell.PreviewMouseLeftButtonUp += ButtonCell_PreviewMouseLeftButtonUp;
                item.ButtonCell.PreviewMouseRightButtonUp += ButtonCell_PreviewMouseRightButtonUp;
            }
            MainGrid.Height = gameSettings.DefaultHeight + (gameSettings.ButtonHeight * gameSettings.FieldHeight);
            if (gameSettings.ButtonWidth * gameSettings.FieldWidth > gameSettings.DefaultWidth)
            {
                MainGrid.Width = gameSettings.ButtonWidth * gameSettings.FieldWidth;
            }
            SetBombs();
        }

        private void ButtonCell_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Button temp = sender as Button;
            foreach (Cell item in CellList)
            {
                if (item.ButtonCell == temp)
                {
                    if (time != -1)
                    {
                        CheckCellOnCoord(item.Coord);
                    }
                    break;
                }
            }
        }
        private void ButtonCell_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Button temp = sender as Button;
            foreach (Cell item in CellList)
            {
                if (item.ButtonCell == temp)
                {
                    if (!item.isChecked)
                    {
                        if (item.isFlag)
                        {
                            item.isFlag = false;
                        }
                        else
                        {
                            if (gameSettings.CountFlags != 0)
                            {
                                item.isFlag = true;
                            }
                        }
                    }
                    FlagsCountLabel.Content = gameSettings.CountFlags;
                    break;
                }
            }
        }

        private void CheckCellOnCoord(Coord coord)
        {
            List<Coord> TempCheckCoord = new List<Coord>();
            if (!GetCellByCoord(coord).isBomb)
            {
                foreach (Coord item in CoordDifferenceArray)
                {
                    if (GetCellByCoord(coord + item) != null)
                    {
                        TempCheckCoord.Add(coord + item);
                    }
                }
                foreach (Coord item in TempCheckCoord)
                {
                    Cell temp = GetCellByCoord(coord);
                    if (!temp.isChecked)
                    {
                        temp.isChecked = true;
                        int CountBombs = 0;
                        foreach (Coord item2 in TempCheckCoord)
                        {
                            if (GetCellByCoord(item2).isBomb)
                            {
                                CountBombs++;
                            }
                        }
                        if (CountBombs == 0)
                        {
                            foreach (Coord item2 in CoordDifferenceArray2)
                            {
                                if (GetCellByCoord(coord + item2) != null)
                                {
                                    CheckCellOnCoord(coord + item2);
                                }
                            }
                        }
                        if (temp.isFlag)
                        {
                            temp.isFlag = false;
                        }
                        Image tempImage = new Image();
                        tempImage.Source = Cell.SetImage($"Pictures\\{CountBombs}.png");
                        temp.ButtonCell.Content = tempImage;
                        CellCountClicked++;
                    }
                }
                if (CellCountClicked == CellWithoutBombs)
                {
                    time = -1;
                    MessageBox.Show("Победа!", "Сапёр");
                }
            }
            else
            {
                Image tempImage = new Image();
                tempImage.Source = Cell.SetImage($"Pictures\\ClickedBomb.png");
                Cell tempCell = GetCellByCoord(coord);
                tempCell.ButtonCell.Content = tempImage;
                foreach (Cell item in CellList)
                {
                    if (item.Coord != tempCell.Coord)
                    {
                        if (item.isBomb)
                        {
                            if (item.isFlag)
                            {
                                Image tempImage2 = new Image();
                                tempImage2.Source = Cell.SetImage($"Pictures\\FlagBomb.png");
                                item.ButtonCell.Content = tempImage2;
                            }
                            else
                            {
                                Image tempImage3 = new Image();
                                tempImage3.Source = Cell.SetImage($"Pictures\\Bomb.png");
                                item.ButtonCell.Content = tempImage3;
                            }
                        }
                    }
                    item.isChecked = true;
                }
                time = -1;
                //MessageBox.Show("Lose", "Сапёр");
            }
            FlagsCountLabel.Content = gameSettings.CountFlags;
        }
        private Cell GetCellByCoord(Coord coord)
        {
            Cell tempCell = null;
            foreach (Cell item in CellList)
            {
                if (item.Coord.X == coord.X && item.Coord.Y == coord.Y)
                {
                    tempCell = item;
                }
            }
            return tempCell;
        }
        private void SetBombs()
        {
            Random rnd = new Random();
            float temp = ((float)(gameSettings.FieldHeight * gameSettings.FieldWidth) * gameSettings.CountBombs);
            gameSettings.CountFlags = (int)temp;
            FlagsCountLabel.Content = gameSettings.CountFlags.ToString();
            int SpawnedBombs = 0;
            while (SpawnedBombs != gameSettings.CountFlags)
            {
                int templist = rnd.Next(CellList.Count);
                if (!CellList[templist].isBomb)
                {
                    CellList[templist].isBomb = true;
                    SpawnedBombs++;
                }
            }
            foreach (Cell item in CellList)
            {
                if (!item.isBomb)
                {
                    CellWithoutBombs++;
                }
            }
        }
        private void TimeCounterFunc()
        {
            while (true)
            {
                Thread.Sleep(1000);
                if (time == -2)
                {
                    break;
                }
                if (time != -1)
                {
                    time++;
                    try
                    {
                        Dispatcher.Invoke(() => TimeCountLabel.Content = time);
                    }
                    catch 
                    {
                        break;
                    }
                }
            }
        }
        void Window_Closing(object sender, CancelEventArgs e)
        {
            time = -2;
        }
    }
    public class GameSettings
    {
        public int DefaultHeight { get; set; }
        public int DefaultWidth { get; set; }
        public int ResolutionScale { get; set; }
        public double FieldHeight { get; set; }
        public double FieldWidth { get; set; }
        public double ButtonHeight { get; set; }
        public double ButtonWidth { get; set; }
        public float CountBombs { get; set; }
        public int CountFlags { get; set; }
        public GameSettings()
        {
            ResolutionScale = 1;
            ButtonHeight = 20;
            ButtonWidth = 20;
        }
    }
    class Cell
    {
        public Coord? Coord { get; set; }
        public Button? ButtonCell { get; set; }
        public bool isBomb { get; set; }
        private bool _isFlag = false;
        public bool isFlag
        {
            get
            {
                return _isFlag;
            }
            set
            {
                _isFlag = value;
                if (value)
                {
                    MainWindow.gameSettings.CountFlags--;
                    Image tempImage = new Image();
                    tempImage.Source = SetImage("Pictures\\Flag.png");
                    if (ButtonCell != null)
                    {
                        ButtonCell.Content = tempImage;
                    }
                }
                else
                {
                    MainWindow.gameSettings.CountFlags++;
                    Image tempImage = new Image();
                    tempImage.Source = SetImage("Pictures\\DefaultButton.png");
                    if (ButtonCell != null)
                    {
                        ButtonCell.Content = tempImage;
                    }
                }
            }
        }
        public bool isChecked { get; set; }

        public void CreateButton(Grid MainGrid)
        {
            Button temp = new Button();
            temp.Height = MainWindow.gameSettings.ButtonHeight;
            temp.Width = MainWindow.gameSettings.ButtonWidth;
            temp.HorizontalAlignment = HorizontalAlignment.Left;
            temp.VerticalAlignment = VerticalAlignment.Top;
            temp.Margin = new Thickness(Coord.X * MainWindow.gameSettings.ButtonWidth,
                MainWindow.gameSettings.DefaultHeight + (Coord.Y * MainWindow.gameSettings.ButtonWidth),
                0, 0);
            Image tempImage = new Image();
            tempImage.Source = SetImage("Pictures\\DefaultButton.png");
            temp.Content = tempImage;
            ButtonCell = temp;
            MainGrid.Children.Add(temp);
        }
        public void Destroy(Grid MainGrid)
        {
            MainGrid.Children.Remove(ButtonCell);
        }

        public Cell()
        {
            Coord = null;
            ButtonCell = null;
            isBomb = false;
            isFlag = false;
            isChecked = false;
        }

        public static BitmapImage SetImage(string filepath)
        {
            var bi = new BitmapImage();

            if (File.Exists(filepath))
            {
                using (var fs = new FileStream(filepath, FileMode.Open))
                {
                    bi.BeginInit();
                    bi.StreamSource = fs;
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.EndInit();
                }

                bi.Freeze();
            }
            else
            {
                bi = null;
            }

            return bi;
        }
    }
    public class Coord
    {
        public int X { get; set; }
        public int Y { get; set; }
        public static Coord operator+(Coord coord1, Coord coord2)
        {
            return new Coord(coord1.X + coord2.X, coord1.Y + coord2.Y);
        }
        public static bool operator==(Coord coord1, Coord coord2)
        {
            if (coord1.X == coord2.X && coord1.Y == coord2.Y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool operator!=(Coord coord1, Coord coord2)
        {
            return !(coord1 == coord2);
        }
        public Coord (int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
