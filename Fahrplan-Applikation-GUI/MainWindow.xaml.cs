using System;
using System.Collections.Generic;
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
using SwissTransport;
namespace Fahrplan_Applikation_GUI {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        Transport transport;
        NavigationEngine navigationEngine;

        Connections fahrplanConnections { get { return fahrplanConnections; } set { fahrplanConnections = value; } }
        public MainWindow() {
            InitializeComponent();
            transport = new Transport();
            List<Grid> menuEntries = new List<Grid>() { fahrplanGrid, abfahrtstafelGrid, stationensucheGrid, stationenBeiMirGrid };
            navigationEngine = new NavigationEngine(menuEntries, "fahrplan", titleLabel);
        }

        private void onFahrplanSearchClick(object sender, RoutedEventArgs e) {
            if (vonFahrplanComboBox.Text.Length > 0 && bisFahrplanComboBox.Text.Length > 0) {
                Connections connections = transport.GetConnections(vonFahrplanComboBox.Text, bisFahrplanComboBox.Text);
                if (connections.ConnectionList.Count > 0) {

                    List<string> connectionsStringList = new List<string>();
                    for (int i = 0; i < 5; i++) {
                        if (i < connections.ConnectionList.Count) {
                            string outStr;
                            Connection el = connections.ConnectionList.ElementAt(i);
                            outStr = el.From.Station.Name + " (" + DateTime.Parse(el.From.Departure).ToShortTimeString() + ")";
                            outStr += " -> ";
                            outStr += el.To.Station.Name + " (" + DateTime.Parse(el.To.Arrival).ToShortTimeString() + ")";
                            outStr += " - (" + el.Duration + ")";
                            connectionsStringList.Add(outStr);
                        }
                    }
                    fahrplanListBox.ItemsSource = connectionsStringList;
                } else {
                    showError("Es wurden keine Resultate gefunden.");
                }
            } else {
                showError("Bitte geben Sie eine Abfharts- und Ankunftsstation an.");
            }
        }
        private List<string> searchStationName(String str) {
            List<Station> stationList = transport.GetStations(str).StationList;
            List<string> stationNameList = new List<string>();
            foreach (Station s in stationList) {
                stationNameList.Add(s.Name);
            }
            return stationNameList;
        }

        private void onNavButtonClick(object sender, RoutedEventArgs e) {
            if (sender is Button) {
                Button b = (Button)sender;
                navigationEngine.setActiveGrid(b.Name);
            } else {
                throw new ArgumentOutOfRangeException();
            }
        }

        private void onStationensucheSucheClick(object sender, RoutedEventArgs e) {
            if (stationensucheTextBox.Text.Length != 0) {
                List<string> stations = searchStationName(stationensucheTextBox.Text);
                if (stations.Count > 0) {
                    stationensucheListBox.ItemsSource = stations;
                } else {
                    stationensucheListBox.Items.Clear();
                    showError("Die gewünschte Abfrage hat keine Resultate geliefert.");
                }
            } else {
                showError("Bitte geben Sie eine Suche ein.");
            }
        }

        private void showError(string message) {
            MessageBox.Show(message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void stationensucheTextBoxKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter)
                onStationensucheSucheClick(sender, null);
        }
    }
}
