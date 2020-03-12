using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SwissTransport;
namespace Fahrplan_Applikation_GUI {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        Transport transport;
        NavigationEngine navigationEngine;

        public MainWindow() {
            InitializeComponent();
            transport = new Transport();

            List<Grid> menuEntries = new List<Grid>() { fahrplanGrid, abfahrtstafelGrid, stationensucheGrid };
            navigationEngine = new NavigationEngine(menuEntries, fahrplan, titleLabel);

            for (int i = 0; i < 24; i++) {
                fahrplanTimePicker.Items.Add(i.ToString() + ":00");
            }
        }

        private void onFahrplanSearchClick(object sender, RoutedEventArgs e) {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            if (vonFahrplanComboBox.Text.Length > 0 && bisFahrplanComboBox.Text.Length > 0) {

                if (fahrplanDatePicker.Text == "") {
                    fahrplanDatePicker.SelectedDate = DateTime.Now;
                }
                if (fahrplanTimePicker.Text == "") {
                    fahrplanTimePicker.Text = DateTime.Now.ToString("HH:mm");
                }

                DateTime selectedDate = new DateTime();
                if (fahrplanDatePicker.SelectedDate != null) {
                    selectedDate = (DateTime)fahrplanDatePicker.SelectedDate;
                }

                selectedDate = validateDate(selectedDate);
                fahrplanDatePicker.Text = selectedDate.ToString("dd/MM/yyyy");
                fahrplanTimePicker.Text = validateTime(fahrplanTimePicker.Text);

                try {
                    Connections connections = transport.GetConnections(
                        vonFahrplanComboBox.Text,
                        bisFahrplanComboBox.Text,
                        selectedDate.ToString("MM/dd/yyyy"),
                        fahrplanTimePicker.Text
                        );
                    fahrplanListBox.ItemsSource = parseFahrplanRows(connections);
                } catch (System.Net.WebException) {
                    showError("Es konnte keine Verbindung hergestellt werden.");
                }
            } else {
                showError("Bitte geben Sie eine Abfharts- und Ankunftsstation an.");
            }
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
        }

        private string validateTime(string time) {
            if (!Regex.IsMatch(time, "^([0-1]?[0-9]|[2][0-3]):([0-5][0-9])$") && time != "") {
                showError("Ungültiges Format der Zeit.");
                time = DateTime.Now.ToString("HH:mm");
            }
            return time;
        }

        private DateTime validateDate(DateTime? date) {
            if (date.HasValue) {
                DateTime d = (DateTime)date;
                if (Regex.IsMatch(d.ToString("dd/MM/yyyy"), "^[0-9]{2}/[0-9]{2}/[0-9]{4}$")) {
                    return d;
                } else {
                    showError("Ungültiges Format des Datums.");
                    return DateTime.Now;
                }
            } else {
                return DateTime.Now;
            }

        }

        private List<string> parseFahrplanRows(Connections connections) {
            List<string> connectionsStringList = new List<string>();
            if (connections.ConnectionList.Count > 0) {
                for (int i = 0; i < 5; i++) {
                    if (i < connections.ConnectionList.Count) {
                        string outStr;

                        Connection el = connections.ConnectionList.ElementAt(i);
                        outStr = el.From.Station.Name + " (" + DateTime.Parse(el.From.Departure).ToString("dd.MM HH:mm") + ")";
                        outStr += " -> ";
                        outStr += el.To.Station.Name + " (" + DateTime.Parse(el.To.Arrival).ToShortTimeString() + ")";
                        outStr += " - (" + el.Duration + ")";
                        connectionsStringList.Add(outStr);
                    }
                }
            } else {
                showError("Es wurden keine Resultate für die Eingaben gefunden.");
            }
            return connectionsStringList;

        }

        private List<string> getStationStr(String str) {
            List<Station> stationList = transport.GetStations(str).StationList;
            List<string> stationNameList = new List<string>();
            foreach (Station s in stationList) {
                if (s.Id != null) {
                    stationNameList.Add(s.Name);
                }
            }
            return stationNameList;
        }

        private void onNavButtonClick(object sender, RoutedEventArgs e) {
            if (sender is Button) {
                Button b = (Button)sender;
                navigationEngine.setActiveGrid(b);
            } else {
                throw new ArgumentOutOfRangeException();
            }
        }

        private void onStationensucheSucheClick(object sender, RoutedEventArgs e) {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            try {
                stationensucheListBox.ItemsSource = searchStations(stationensucheSuchenComboBox.Text);
            } catch (System.Net.WebException) {
                showError("Es konnte keine Verbindung hergestellt werden.");
            }
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
        }

        private void showError(string message) {
            MessageBox.Show(message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void onAbfahrtstafelSucheClick(object sender, RoutedEventArgs e) {
            try {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                if (abfahrtstafelSuchenComboBox.Text.Length > 0) {
                    StationBoardRoot stationBoardRoot = transport.GetStationBoard(abfahrtstafelSuchenComboBox.Text, "");
                    if (stationBoardRoot.Entries.Count > 0) {
                        List<string> abfahrtstafelStringList = new List<string>();

                        foreach (StationBoard s in stationBoardRoot.Entries) {
                            string outStr;
                            outStr = Math.Round((s.Stop.Departure - DateTime.Now).TotalMinutes).ToString();
                            outStr += "' - " + s.Number + " " + s.To;

                            abfahrtstafelStringList.Add(outStr);
                        }
                        abfahrtstafelListBox.ItemsSource = abfahrtstafelStringList;
                    } else {
                        showError("Es wurde keine Station für die gewünschte Abfrage gefunden.");
                    }


                } else {
                    showError("Bitte geben Sie eine Station an.");
                }
            } catch (System.Net.WebException) {
                showError("Es konnte keine Verbindung hergestellt werden.");
            } finally {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private List<string> searchStations(string searchStr) {
            if (searchStr.Length != 0) {
                List<string> stations = getStationStr(searchStr);
                if (stations.Count > 0) {
                    return stations;
                } else {
                    showError("Die gewünschte Abfrage hat keine Resultate geliefert.");
                    return null;
                }
            } else {
                showError("Bitte geben Sie eine Suche ein.");
                return null;
            }
        }

        private void searchStationsAutoComplete(object sender, KeyEventArgs e) {
            try {
                var comboBox = sender as ComboBox;
                comboBox.IsDropDownOpen = true;
                List<string> stationList = getStationStr(comboBox.Text);
                comboBox.ItemsSource = stationList;
            } catch (System.Net.WebException) {
                //Nichts, da das Textfeld auch abgefüllt werden kann falls keine Autocompletion exisitert.
            }

        }

        private void onFahrplanSwitchClick(object sender, RoutedEventArgs e) {
            string temp = vonFahrplanComboBox.Text;
            vonFahrplanComboBox.Text = bisFahrplanComboBox.Text;
            bisFahrplanComboBox.Text = temp;
        }

        private void onEmailFahrplanClick(object sender, RoutedEventArgs e) {
            if (!fahrplanListBox.Items.IsEmpty) {
                string outStr = "";
                foreach (string s in fahrplanListBox.Items) {
                    outStr += s;
                }
                System.Diagnostics.Process.Start("mailto:?subject=Fahrplan&body=" + outStr);
            } else {
                showError("Es müssen Verbindungen angezeigt werden um diese verschiken zu können.");
            }
        }
    }
}
