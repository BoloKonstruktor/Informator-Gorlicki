using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Security.Permissions;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;


namespace InformatorGorlicki
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string ImagePath = "res/img/";
        private string mainTitle;
        private string MainURL { get; set; }

        private double TextWidth { get; set; } //Szerokość tekstu
        private int ScrollPos { get; set; } //Pozycja przesunięcia tekstu
        private string NewsURL { get; set; }
        private int Div { get; set; }


        public DispatcherTimer Timer0 = new DispatcherTimer();

        public News news = new News();

        public IniFile Ini = new IniFile("Konfiguracja.ini");

        public Brush LinkColor = (Brush)new BrushConverter().ConvertFrom("#0055b9");

        private string GetMainTitle()
        {
            return mainTitle;
        }

        private void SetMainTitle(string value)
        {
            mainTitle = value;
        }

        public MainWindow()
        {
            InitializeComponent();
            Timer0.Tick += new EventHandler(TextScroller);
            Timer0.Interval = new TimeSpan(0, 0, 0, 0, 10);

            int WAWidth = (int)SystemParameters.WorkArea.Width;
            int WAHeight = (int)SystemParameters.WorkArea.Height;

            Top = WAHeight - (int)Height;
            Left = WAWidth - (int)Width;

            SetMainTitle(Title);
            BtnImgSrc("start.png");

            string url = Ini.Read("MainURL");

            if (url == null || url == "")
            {
                url = "https://schron.tk/news/json/";
                Ini.Write("MainURL", url);
            }

            MainURL = url;
        }

        public void BringToFront()
        {
            ShowActivated = false;

            if (Visibility != Visibility.Visible)
            {
                Visibility = Visibility.Visible;
            }

            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            _ = Activate();
            Topmost = true;

        }

        public bool PostRequest(string url, string data, ref string output, ref string errors, int timeout = 3000)
        {
            if (url is null || data is null || errors is null)
            {
                return false;
            }

            WebRequest request;
            Stream dataStream;

            try
            {
                // Create a request using a URL that can receive a post.
                request = WebRequest.Create(url);
                request.Timeout = timeout;

                // Set the Method property of the request to POST.
                request.Method = "POST";

                // Create POST data and convert it to a byte array.
                string postData = data;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                // Set the ContentType property of the WebRequest.
                request.ContentType = "application/x-www-form-urlencoded";
                // Set the ContentLength property of the WebRequest.
                request.ContentLength = byteArray.Length;
                // Get the request stream.
                dataStream = request.GetRequestStream();
                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.
                dataStream.Close();
            }
            catch (Exception err)
            {
                errors = err.Message;
                return false;
            }

            try
            {
                // Get the response.
                WebResponse response = request.GetResponse();

                // Get the stream containing content returned by the server.
                // The using block ensures the stream is automatically closed.
                using (dataStream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.
                    string responseFromServer = reader.ReadToEnd();

                    output = responseFromServer;
                }

                // Close the response.
                response.Close();
            }
            catch (Exception err)
            {

                errors = err.Message;
                return false;
            }

            return true;
        }

        public static void GoToSite(string url)
        {
            try
            {
                // Process.Start("C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe \"" + url + "\"");

                _ = Process.Start("cmd", "/C start " + url);
            }
            catch (Exception E)
            {
                _ = MessageBox.Show(E.Message);
            }

        }

        private void GetNews()
        {
            string str = string.Empty;
            news.Get(ref str);

            if (news.Link != "")
            {
                CkLbNews.Cursor = Cursors.Hand;
            }
            else
            {
                CkLbNews.Cursor = Cursors.Arrow;
            }

            CkLbNews.Foreground = LinkColor;


            if (news.NID == 0 && news.IsNew())
            {
                PlaySound(Properties.Resources.d1);
                ShowNewMessages();
                BringToFront();
            }

            //Title = "LastDate[" + news.GID + "]=" + news.LastDate[news.GID] + " CountNewMessages()=" + news.CountNewMessages().ToString();

            CkLbNews.Margin = new Thickness(0, 0, 0, 0);
            CkLbNews.Content = str;
            CkLbNews.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size s = CkLbNews.DesiredSize;
            TextWidth = s.Width;
        }

        private void PlaySound(UnmanagedMemoryStream sound)
        {
            
            SoundPlayer player = new SoundPlayer(sound);
            player.Play();
        }

        private void ShowNewMessages()
        {
            int cnt = news.CountNewMessages();

            if (cnt > 0)
            {

                Title = "Nowa wiadomość! (" + cnt.ToString() + ") " + GetMainTitle();
            
            } else
            {
                Title = GetMainTitle();
            }
        }

        public void BtnImgSrc(string filename)
        {
            string path = ImagePath + filename;

            if (File.Exists(path) && BtnImage != null)
            {
                BtnImage.Source = new ImageSourceConverter().ConvertFromString(path) as ImageSource;
                // _ = BtnImage.FindResource(Properties.Resources.stop);// = Properties.Resources.start;
                //object O = Properties.Resources.ResourceManager.GetObject("start"); //Return an object from the image chan1.png in the project
                //BtnImage = (Image)O;
            }
        }


        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            if (Timer0.IsEnabled)
            {
                Timer0.Stop();
                BtnImgSrc("start.png");
                return;
            }

            var Result = string.Empty;
            var Error = string.Empty;

            Title = "Pobieranie danych z " + CbSource.Text + " ...";


            if (PostRequest(NewsURL, "", ref Result, ref Error, 5000))
            {
                ScrollPos = 0;
                news.Set(Result);
                news.LastDate.Clear();
                GetNews();
                Timer0.Start();
                BtnImgSrc("stop.png");
            }
            else
            {
                _ = MessageBox.Show(Error, GetMainTitle() + " - Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            Title = GetMainTitle();

        }

        private void TextScroller(object sender, EventArgs e)
        {
            //DispatcherTimer T = sender as DispatcherTimer;
            int TW = (int)ActualWidth - (int)CbSource.ActualWidth - (int)BtnTest.ActualWidth;

            if (ScrollPos < TextWidth + TW)
            {
                ScrollPos += 1;
            }
            else
            {
                ScrollPos = TW * -1;


                if (news.NID < news.CountMessages - 1)
                {
                    news.NID++;
                }
                else
                {
                    news.NID = 0;
                    news.GID++;

                    if (news.GID >= news.CountGroups)
                    {
                        news.GID = 0;
                        var Result = string.Empty;
                        var Error = string.Empty;

                        if (PostRequest(NewsURL, "", ref Result, ref Error, 5000))
                        {
                            news.Set(Result);
                        }
                        else
                        {
                            _ = MessageBox.Show(Error, GetMainTitle() + " - Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }

                //Title = "news.CountGroups="+news.CountGroups+ " news.GID=" + news.GID + " news.CountMessages="+news.CountMessages+" news.NID=" + news.NID;
                GetNews();
            }

            CkLbNews.Margin = new Thickness((ScrollPos * -1), 0, 0, 0);

            if (Div < 50)
            {
                Div++;
            }
            else
            {
                if (news.NID == 0 && news.IsNew())
                {
                    CkLbNews.Foreground = CkLbNews.Foreground == LinkColor ? Brushes.Green : LinkColor;
                }
                else
                {
                    CkLbNews.Foreground = LinkColor;
                }

                Div = 0;
            }
        }

        private void CkLbNews_Click(object sender, RoutedEventArgs e)
        {
            if (news.Link != "")
            {
                if (news.NID == 0 && news.IsNew())
                {
                    news.SetAsRead();
                    ShowNewMessages();
                }

                GoToSite(news.Link);
            }
        }

        private void CbSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox CB = (ComboBox)sender;
            Ini.Write("SelectedIndex", CB.SelectedIndex.ToString());
            ComboBoxItem typeItem = (ComboBoxItem)CB.SelectedItem;
            string value = typeItem.Content.ToString();
            NewsURL = MainURL + value + "/";
            Timer0.Stop();
            BtnImgSrc("start.png");
        }

        private void CkLbNews_MouseMove(object sender, MouseEventArgs e)
        {
            Timer0.Stop();
        }

        private void CkLbNews_MouseLeave(object sender, MouseEventArgs e)
        {
            Timer0.Start();
        }

        private void BtnTest_Loaded(object sender, RoutedEventArgs e)
        {
            BtnTest_Click(sender, e);
        }

        private void CbSource_Loaded(object sender, RoutedEventArgs e)
        {
            string Str = string.Empty;
            string Err = string.Empty;
            
            if (PostRequest(MainURL, "", ref Str, ref Err))
            {
                ComboBox cb = sender as ComboBox;
                
                news.LoadHosts(Str, cb);

                string idx = Ini.Read("SelectedIndex");
                cb.SelectedIndex = idx != null && idx != "" ? short.Parse(idx) : 0;
            }
        }
    }

    public class ClickableLabel : Label
    {

        public static readonly RoutedEvent ClickEvent;

        static ClickableLabel()
        {

            ClickEvent = Button.ClickEvent.AddOwner(typeof(ClickableLabel));

        }

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }

            remove { RemoveHandler(ClickEvent, value); }

        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            CaptureMouse();

        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (IsMouseCaptured)
            {

                ReleaseMouseCapture();

                if (IsMouseOver)

                    RaiseEvent(new RoutedEventArgs(ClickEvent, this));
            }

        }
    }
}
