using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Fahrplan_Applikation_GUI {
    class NavigationEngine {
        private List<Grid> menuEntries = new List<Grid>();
        public Grid activeGrid;
        private Label currentPageLabel;

        public NavigationEngine(List<Grid> menuEntries, Button startButton, Label currentPageLabel) {
            this.menuEntries = menuEntries;
            this.currentPageLabel = currentPageLabel;
            setActiveGrid(startButton);
        }

        public void setActiveGrid(Button b) {
            Grid gridToSelect = getGridFromString(b.Name);
            foreach(Grid g in menuEntries) {
                g.Visibility = System.Windows.Visibility.Hidden;
            }
            gridToSelect.Visibility = System.Windows.Visibility.Visible;
            activeGrid = gridToSelect;
            currentPageLabel.Content = b.Content;
        }

        private Grid getGridFromString(string name) {
            Grid grid = null;
            foreach(Grid iterativeGrid in menuEntries) {
                if(iterativeGrid.Name == (name + "Grid")) {
                    grid = iterativeGrid;
                }
            }
            if(grid != null) {
                return grid;
            } else {
                throw new ArgumentOutOfRangeException();
            }
            
        }
    }
}
