using HtmlAgilityPack;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BloatRemove
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ObservableCollection<PackageApp> packageApps;
        public MainWindow()
        {
            InitializeComponent();
        }

        public void UninstallApp(object sender, RoutedEventArgs e)
        {
            string pckToUninstall = ((PackageApp)((Button)sender).DataContext).PackageFullName;
            //string lastPart = pckToUninstall.Split('.').Last();

            //bool result = Interaction.InputBox() == DialogResult.OK;
            //MessageBox.Show("shell uninstall -k --user 0 " + pckToUninstall);
            MessageBox.Show("Are you sur ? " + pckToUninstall + " will be deleted.");

            DeletePackage((PackageApp)((Button)sender).DataContext);
        }


        private void Check_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists("adb/adb.exe"))
            {
                MessageBox.Show("Files missing ! Exit.");
                return;
            }

            UpdatePackageList();
            lblCount.Content = "Nb Pkg found : " + packageApps.Count();




        }



        private void DeletePackage(PackageApp pckToUninstall)
        {
            string result = "";
            DateTime dt = DateTime.Now;

            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "adb/adb.exe",
                    Arguments = "shell pm uninstall -k --user 0 " + pckToUninstall.PackageFullName,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                result += proc.StandardOutput.ReadLine() + Environment.NewLine;
            }
            MessageBox.Show(result);
            Directory.CreateDirectory("log");
            File.AppendAllText("log/DeletedApp_" + dt.Year + "_" + dt.Month + "_" + dt.Day + ".txt", result);

            packageApps.Remove(pckToUninstall);
            //UpdatePackageList();


        }


        private void UpdatePackageList()
        {
            packageApps = new ObservableCollection<PackageApp>();
            PackageApp packageApp;
            string stdOutput;
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "adb/adb.exe",
                    Arguments = "shell pm list packages",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                stdOutput = proc.StandardOutput.ReadLine();
                if (stdOutput.Substring(0, 8) == "package:")
                {
                    packageApp = new PackageApp
                    {
                        PackageFullName = stdOutput.Remove(0, 8).Trim(' '),
                        GooglePlayLink = "https://play.google.com/store/apps/details?id=" + stdOutput.Remove(0, 8).Trim(' ')
                    };
                    if (packageApp.PackageFullName.Trim(' ') != "") packageApps.Add(packageApp);
                }
            }
            packageAppDataGrid.ItemsSource = packageApps;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            System.Windows.Data.CollectionViewSource packageAppViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("packageAppViewSource")));
            packageAppViewSource.Source = packageApps;

        }

      
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < packageApps.Count; i++)
            {
                packageApps[i] = UpdateDataOfPackage(packageApps.ElementAt(i));
                //lblCountUpdate.Content = " Updated Data : " + (i + 1) + " / " + packageApps.Count;

            }


        }

        private PackageApp UpdateDataOfPackage(PackageApp pkg)
        {

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(pkg.GooglePlayLink);

            try
            {
                HtmlNode node = doc.DocumentNode.SelectNodes("/html/body/div[1]/div[4]/c-wiz/div/div[2]/div/div/main/c-wiz[1]/c-wiz[1]/div/div[2]/div/div[1]/c-wiz[1]/h1/span").First();
               pkg.Title = node.InnerText;

                //node = doc.DocumentNode.SelectNodes("/html/body/div[1]/div[4]/c-wiz[3]/div/div[2]/div/div/main/c-wiz[1]/c-wiz[1]/div/div[1]/div/img").First();
                //pkg.Ico = node.GetAttributeValue("src", "");
            }
            catch (Exception e)
            {
                //throw;
            }

            return pkg;
        }
    }
}
