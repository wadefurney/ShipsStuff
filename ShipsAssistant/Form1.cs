using ShipsApi.DAO;
using ShipsApi.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace ShipsAssistant
{
    public partial class Form1 : Form
    {
        private class Constants
        {
            public static String Tier = "Tier";
            public static String Column = "Column";
            public static String Type = "Type";
            public static String Nation = "Nation";
            public static String Name = "Name";
            public static String Filters = "FILTERS";
            public static String Speed = "Speed";
            public static String TorpRange = "Torpedo Range";
        }

        private object _shipsLock = new object();

        private ListViewItemComparer _lvwItemComparer;

        //filters appear in order of list
        //columns should always be first because it filters different items than the other filters.
        private List<String> _filterCategories = new List<string> { Constants.Column, Constants.Type, Constants.Nation, Constants.Tier };
        private List<String> _columnNames = new List<string> { Constants.Name, Constants.Type, Constants.Nation, Constants.Tier, Constants.Speed, Constants.TorpRange };

        private Dictionary<String, TreeNode> _filterNodes;

        private System.Windows.Forms.ColumnClickEventHandler _sortHandler;

        private ShipsDAO _ships = null;
        private ShipsDAO Ships
        {
            get
            {
                lock (_shipsLock)
                {
                    //looks like this might be able to use a lock
                    if (_ships == null)
                    {
                        _ships = new ShipsDAO();
                    }
                    return _ships;
                }
            }
            set
            {

            }
        }

        public Form1()
        {
            InitializeComponent();
            SetupFilters();
            SetupShipDeails();

            _sortHandler = new System.Windows.Forms.ColumnClickEventHandler(this.lvShipDetails_ColumnClick);
            lvShipDetails.ColumnClick += _sortHandler;

            //this.lvShipDetails.ColumnClick +=
            //new System.Windows.Forms.ColumnClickEventHandler(this.lvShipDetails_ColumnClick);

            this.tvFilters.AfterCheck += TvFilters_AfterCheck;
        }

        private void SelectAllChildren(TreeNodeCollection nodes, bool isSelected)
        {
            foreach(TreeNode node in nodes)
            {
                if(node.Nodes.Count >  0)
                {
                    SelectAllChildren(node.Nodes, isSelected); 
                }
                node.Checked = isSelected; 
            }
        }

        private void TvFilters_AfterCheck(object sender, TreeViewEventArgs e)
        {
            TreeNode _node = e.Node;
            if (e.Node.Parent != null &&
               _filterCategories.Contains(e.Node.Parent.Name))
            {
                SetupShipDeails();
            }
            else
            {
                SelectAllChildren(e.Node.Nodes, e.Node.Checked);
            }
            

            /*
            if (e.Node.Checked == true)
            {
                MessageBox.Show("checked");
            }
            else
            {
                MessageBox.Show("Unchecked");
            }
            */
        }

        private void SetupFilters()
        {
            _filterNodes = new Dictionary<string, TreeNode>();
            TreeNode nodRoot = tvFilters.Nodes.Add(Constants.Filters);
            tvFilters.CheckBoxes = true;

            Dictionary<String, List<String>> filterNames = new Dictionary<string, List<string>>();
            filterNames[Constants.Column] = _columnNames;
            filterNames[Constants.Type] = Ships.AllTypes;
            filterNames[Constants.Nation] = Ships.AllNations;
            filterNames[Constants.Tier] = Ships.AllTiers;

            TreeNode filterCategory;
            foreach (string categoryName in _filterCategories)
            {
                filterCategory = nodRoot.Nodes.Add(categoryName);
                filterCategory.Name = categoryName;

                _filterNodes[categoryName] = filterCategory;
                foreach (string name in filterNames[categoryName])
                {
                    TreeNode filter = filterCategory.Nodes.Add(name);
                    filter.Name = name;
                    filter.Checked = true;

                }

            }

            nodRoot.ExpandAll();
        }

        private void SetupShipDeails()
        {
            lvShipDetails.BeginUpdate();
            tvFilters.BeginUpdate();
            lvShipDetails.Columns.Clear();
            //lvShipDetails is the list view
            lvShipDetails.View = View.Details;
            lvShipDetails.LabelEdit = false;
            lvShipDetails.AllowColumnReorder = true;
            lvShipDetails.GridLines = true;
            lvShipDetails.Sorting = SortOrder.Ascending;
            // Attach Subitems to the ListView


            HashSet<String> toFilter = new HashSet<String>(GetItemsToFilter(Constants.Column));            
            foreach (string label in _columnNames)
            {
                var c = lvShipDetails.Columns.Add(label, -1, HorizontalAlignment.Left);
                c.Name = label;
                if (toFilter.Contains(label))
                {
                    c.Width = 0;                     
                }                
            }
            LoadList();
          
            _lvwItemComparer = new ListViewItemComparer();
            this.lvShipDetails.ListViewItemSorter = _lvwItemComparer;
            //lvShipDetails.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            foreach(ColumnHeader ch in lvShipDetails.Columns)
            {
                if(!toFilter.Contains(ch.Name))
                {
                    ch.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize); 
                }
            }

            tvFilters.EndUpdate(); 
            lvShipDetails.EndUpdate();

            

        }

        ///////make it so i don't have to hard code this shit here!!!!!!!!!!!!!!!!!!!!!!!!
        ///////make it so i don't have to hard code this shit here!!!!!!!!!!!!!!!!!!!!!!!!///////make it so i don't have to hard code this shit here!!!!!!!!!!!!!!!!!!!!!!!!
        ///////make it so i don't have to hard code this shit here!!!!!!!!!!!!!!!!!!!!!!!!
        ///////make it so i don't have to hard code this shit here!!!!!!!!!!!!!!!!!!!!!!!!
        private void LoadList()
        {
            // Clear the ListView control
            lvShipDetails.Items.Clear();

            //have to filter out nonslected columns for sorting purposes 
            Dictionary<String, String> itemDetails;

            HashSet<String> toFilter = new HashSet<String>(GetItemsToFilter(Constants.Column));
            foreach (Ship ship in FilterShips(Ships.AllShips))
            {
                itemDetails = new Dictionary<string, string>();
                itemDetails[Constants.Type] = ship.Type;
                itemDetails[Constants.Nation] = ship.Nation;
                itemDetails[Constants.Tier] = ship.Tier.ToString();
                itemDetails[Constants.Name] = ship.Name;
                itemDetails[Constants.Speed] = ship.SpeedMax.ToString();
                itemDetails[Constants.TorpRange] = ship.TorpRangeMax.ToString(); 
                ListViewItem lvi = null;
                                
                lvi = new ListViewItem(itemDetails[_columnNames[0]]);        
                foreach(string name in _columnNames.Skip(1))
                {
                    lvi.SubItems.Add(itemDetails[name]);                    
                }        
                
                lvShipDetails.Items.Add(lvi);
                /*
                //_lvwItemComparer.SortColumn -= toFilter.Count;
                foreach (string column in _columnNames)
                {
                    string txt = itemDetails[column];
                    if (!toFilter.Contains(column))
                    {   
                        if(lvi == null)
                        {
                            lvi = new ListViewItem(txt); 
                        }
                        else
                        {
                            lvi.SubItems.Add(txt);
                        }                        
                    }  
                    else
                    {
                        Console.WriteLine("why"); 
                    }                  
                }                
                lvShipDetails.Items.Add(lvi);
                lvi = null; 
                */
            }
            FilterListColumns(); 
        }

        private void FilterListColumns()
        {
            return; 
            HashSet<String> toFilter = new HashSet<String>(GetItemsToFilter(Constants.Column));

            foreach (ListViewItem lvi in lvShipDetails.Items)
            {
                List<string> items = new List<String>(); 
                for(int i = 0; i < lvi.SubItems.Count; i++)
                {
                    if(!toFilter.Contains(_columnNames[i]))
                    {
                        items.Add(lvi.SubItems[i].Text);
                    }
                }
                lvi.SubItems.Clear();
                lvi.SubItems[0].Text = items.First(); 
                foreach (string txt in items.Skip(1))
                {
                    lvi.SubItems.Add(txt); 
                }
                Console.WriteLine(); 
            }
        }
        
        //removes ships based off of checked filters        
        private List<Ship> FilterShips(List<Ship> ships)
        {
            int start = ships.Count; 


            List<Ship> results = new List<Ship>();
            //don't mess with original list 
            List<Ship> filteredShips = new List<Ship>(ships);

            HashSet<String> toFilter;
            //always skip columns in this 
            foreach (string filterCateogry in _filterCategories.Skip(1))
            {
                toFilter = new HashSet<string>(GetItemsToFilter(filterCateogry));
                //make a category object that filters itself? don't like having to define each filter csource.  
                //switch statement might at least look nicer                
                if (filterCateogry.Equals(Constants.Tier))
                {
                    results = filteredShips.Where(x => !toFilter.Contains(
                        x.Tier.ToString()
                        )).ToList();
                }
                else if (filterCateogry.Equals(Constants.Nation))
                {
                    results = filteredShips.Where(x => !toFilter.Contains(
                        x.Nation
                    )).ToList();
                }
                else if (filterCateogry.Equals(Constants.Type))
                {
                    results = filteredShips.Where(x => !toFilter.Contains(
                        x.Type
                    )).ToList();

                }

                int resultCount = results.Count;
                if (results.Count > 0)
                    filteredShips = results.ToList(); 


            }
            int finish = results.Count; 
            return results;
        }

        private List<String> GetItemsToFilter(string filterCategoryName)
        {
            List<String> results = new List<string>();
            foreach (TreeNode filter in _filterNodes[filterCategoryName].Nodes)
            {
                if (filter.Checked == false)
                {
                    results.Add(filter.Name);
                }
            }
            return results;
        }

        // Perform Sorting on Column Headers
        private void lvShipDetails_ColumnClick(
            object sender,
            System.Windows.Forms.ColumnClickEventArgs e)
        {

            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == _lvwItemComparer.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (_lvwItemComparer.Order == SortOrder.Ascending)
                {
                    _lvwItemComparer.Order = SortOrder.Descending;
                }
                else
                {
                    _lvwItemComparer.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                _lvwItemComparer.SortColumn = e.Column;
                _lvwItemComparer.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.lvShipDetails.Sort();
        }



        // This class is an implementation of the 'IComparer' interface.
        public class ListViewItemComparer : IComparer
        {
            // Specifies the column to be sorted
            private int ColumnToSort;

            // Specifies the order in which to sort (i.e. 'Ascending').
            private SortOrder OrderOfSort;

            // Case insensitive comparer object
            private CaseInsensitiveComparer ObjectCompare;
            
            // Class constructor, initializes various elements
            public ListViewItemComparer()
            {
                // Initialize the column to '0'
                ColumnToSort = 0;

                // Initialize the sort order to 'none'
                OrderOfSort = SortOrder.None;

                // Initialize the CaseInsensitiveComparer object
                ObjectCompare = new CaseInsensitiveComparer();
            }

            // This method is inherited from the IComparer interface.
            // It compares the two objects passed using a case
            // insensitive comparison.
            //
            // x: First object to be compared
            // y: Second object to be compared
            //
            // The result of the comparison. "0" if equal,
            // negative if 'x' is less than 'y' and
            // positive if 'x' is greater than 'y'
            public int Compare(object x, object y)
            {
                int compareResult;
                ListViewItem listviewX, listviewY;

                // Cast the objects to be compared to ListViewItem objects
                listviewX = (ListViewItem)x;
                listviewY = (ListViewItem)y;

                string xTxt = listviewX.SubItems[ColumnToSort].Text;
                string yTxt = listviewY.SubItems[ColumnToSort].Text;


                decimal xDecimal, yDecimal; 


                if(decimal.TryParse(xTxt, out xDecimal) &&
                   decimal.TryParse(yTxt, out yDecimal))
                {
                    //number compare, should work for decimal and ints
                    compareResult = xDecimal.CompareTo(yDecimal); 
                }
                else
                {
                    // Case insensitive Compare
                    compareResult = ObjectCompare.Compare(
                        xTxt,
                        yTxt
                    );
                }

                

                // Calculate correct return value based on object comparison
                if (OrderOfSort == SortOrder.Ascending)
                {
                    // Ascending sort is selected, return normal result of compare operation
                    return compareResult;
                }
                else if (OrderOfSort == SortOrder.Descending)
                {
                    // Descending sort is selected, return negative result of compare operation
                    return (-compareResult);
                }
                else
                {
                    // Return '0' to indicate they are equal
                    return 0;
                }
            }

            // Gets or sets the number of the column to which to
            // apply the sorting operation (Defaults to '0').
            public int SortColumn
            {
                set
                {
                    ColumnToSort = value;
                }
                get
                {
                    return ColumnToSort;
                }
            }

            // Gets or sets the order of sorting to apply
            // (for example, 'Ascending' or 'Descending').
            public SortOrder Order
            {
                set
                {
                    OrderOfSort = value;
                }
                get
                {
                    return OrderOfSort;
                }
            }
        }

    }
}
