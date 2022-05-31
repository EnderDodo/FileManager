using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileManager
{
    public class ListViewColumnSorter : IComparer
    {

        private int ColumnToSort;

        private SortOrder OrderOfSort;

        private CaseInsensitiveComparer ObjectCompare;

        public ListViewColumnSorter()
        {
            ColumnToSort = 0;

            OrderOfSort = SortOrder.None;

            ObjectCompare = new CaseInsensitiveComparer();
        }

        public int Compare(object x, object y)
        {
            int compareResult;
            ListViewItem listviewX, listviewY;

            listviewX = (ListViewItem)x;
            listviewY = (ListViewItem)y;

            compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);

            if (ColumnToSort == 4)
            {
                if (!string.IsNullOrEmpty(listviewX.SubItems[ColumnToSort].Text) && !string.IsNullOrEmpty(listviewY.SubItems[ColumnToSort].Text))
                    {
                    float reviewX = float.Parse(listviewX.SubItems[ColumnToSort].Text.Replace("$", "").Replace(".", ","));
                    float reviewY = float.Parse(listviewY.SubItems[ColumnToSort].Text.Replace("$", "").Replace(".", ","));

                    compareResult = reviewX > reviewY ? 1 : -1;
                }
            }

            if (OrderOfSort == SortOrder.Ascending)
            {
                return compareResult;
            }
            else if (OrderOfSort == SortOrder.Descending)
            {
                return (-compareResult);
            }
            else
            {
                return 0;
            }
        }

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
