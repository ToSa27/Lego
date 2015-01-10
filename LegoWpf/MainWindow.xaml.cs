using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Xml;

namespace LegoWpf
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private System.Timers.Timer t;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                tbStatus.Text = "Loading Data...";
                Lego.Connection.Connect();
                ThreadPool.QueueUserWorkItem(Lego.Connection.LoadAsync);
                t = new System.Timers.Timer();
                t.AutoReset = true;
                t.Interval = 1000;
                t.Elapsed += t_Elapsed;
                t.Start();
            }
            catch (Exception ex)
            {
            }
        }

        void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!Lego.Connection.IsReady)
            {
                Dispatcher.Invoke((Action)delegate { 
                    tbStatus.Text = "Loading Data...\n" + Lego.Connection.Status; 
                });
            }
            else
            {
                t.Stop();
                Dispatcher.Invoke((Action)delegate { 
                    tbStatus.Text = "Loading Data Completed.\n" + Lego.Connection.Status;
                    lvSets.ItemsSource = Lego.Connection.ds.Set.Rows;
                    lvBuilds.ItemsSource = Lego.Connection.ds.Build.Rows;
                    lvParts.ItemsSource = Lego.Connection.ds.Part.Rows;
                    lvElements.ItemsSource = Lego.Connection.ds.Element.Rows;
                    //cbAdditional.ItemsSource = Lego.Connection.ds.Element.Rows;
                    cbContainer.ItemsSource = Lego.Connection.ds.Container.Rows;
                    cbBin.ItemsSource = Lego.Connection.ds.Bin.Rows;
                    tcMain.SelectedItem = tiSets;
                });
                Lego.Connection.Save();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Lego.Connection.Disconnect();
        }

        private void lvSets_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lvSets.SelectedItem != null)
            {
                Lego.LegoDS.SetRow sr = lvSets.SelectedItem as Lego.LegoDS.SetRow;
                Dispatcher.Invoke((Action)delegate
                {
                    tiSet.DataContext = sr;
//                    lvSetContent.ItemsSource = sr.SetContent;
                    lvSetContent.ItemsSource = sr.GetSetContentRows();
                    tcMain.SelectedItem = tiSet;
                });
            }
        }

        private void bnBuild_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Lego.LegoDS.SetRow sr = tiSet.DataContext as Lego.LegoDS.SetRow;
                Lego.LegoDS.BuildRow br = sr.AddBuild();
            }
            catch
            {
            }
        }

        private void lvBuilds_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lvBuilds.SelectedItem != null)
            {
                Lego.LegoDS.BuildRow br = lvBuilds.SelectedItem as Lego.LegoDS.BuildRow;
                Dispatcher.Invoke((Action)delegate
                {
                    tiBuild.DataContext = br;
//                    lvSetContent.ItemsSource = sr.SetContent;
                    cbMissing.ItemsSource = br.SetRow.GetSetContentRows();
                    lvBuild.ItemsSource = br.GetBuildDiffRows();
                    tcMain.SelectedItem = tiBuild;
                });
            }
        }

        private void bnMissing_Click(object sender, RoutedEventArgs e)
        {
            if (cbMissing.SelectedItem != null)
            {
                Lego.LegoDS ds = Lego.Connection.ds;
                Lego.LegoDS.BuildRow br = tiBuild.DataContext as Lego.LegoDS.BuildRow;
                Lego.LegoDS.SetContentRow scr = cbMissing.SelectedItem as Lego.LegoDS.SetContentRow;
                Lego.LegoDS.BuildDiffRow bdr = ds.BuildDiff.NewBuildDiffRow();
                bdr.BuildRow = br;
                bdr.PartRow = scr.PartRow;
                bdr.ElementRow = scr.ElementRow;
                bdr.ColorRow = scr.ColorRow;
                bdr.CountDiff = -Math.Abs(int.Parse(tbMissing.Text));
                ds.BuildDiff.AddBuildDiffRow(bdr);
            }
        }

        private void bnAdditional_Click(object sender, RoutedEventArgs e)
        {
            Lego.LegoDS ds = Lego.Connection.ds;
            Lego.LegoDS.ElementRow er = ds.Element.GetById(tbAdditionalId.Text);
            if (er != null)
            {
                Lego.LegoDS.BuildRow br = tiBuild.DataContext as Lego.LegoDS.BuildRow;
                Lego.LegoDS.BuildDiffRow bdr = ds.BuildDiff.NewBuildDiffRow();
                bdr.BuildRow = br;
                bdr.PartRow = er.PartRow;
                bdr.ElementRow = er;
                bdr.ColorRow = er.ColorRow;
                bdr.CountDiff = Math.Abs(int.Parse(tbAdditional.Text));
                ds.BuildDiff.AddBuildDiffRow(bdr);
            }
        }

        private void bnUnBuild_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Lego.LegoDS.BuildRow br = tiBuild.DataContext as Lego.LegoDS.BuildRow;
                br.Unbuild();
            }
            catch
            {
            }
        }

        private ObservableCollection<Lego.LegoDS.InventoryRow> TempInventoryRows;

        private void bnInventory_Click(object sender, RoutedEventArgs e)
        {
            Lego.LegoDS ds = Lego.Connection.ds;
            Button b = sender as Button;
            string pid = b.Tag as string;
            Lego.LegoDS.PartRow pr = ds.Part.GetById(pid);
            TempInventoryRows = new ObservableCollection<Lego.LegoDS.InventoryRow>();
            foreach (Lego.LegoDS.ElementRow er in pr.GetElementRows())
            {
                Lego.LegoDS.InventoryRow ir = ds.Inventory.NewInventoryRow();
                ir.PartRow = pr;
                ir.ElementRow = er;
                ir.ColorRow = er.ColorRow;
                ir.Date = DateTime.Now;
                ir.CountBin = er.CountAvailable;
                TempInventoryRows.Add(ir);
            }
            tiInventory.DataContext = pr;
            lvInventory.ItemsSource = TempInventoryRows;
            tcMain.SelectedItem = tiInventory;
        }

        private void bnInventorySave_Click(object sender, RoutedEventArgs e)
        {
            Lego.LegoDS ds = Lego.Connection.ds;
            // ToDo : delete old inventory
            Lego.LegoDS.PartRow pr = tiInventory.DataContext as Lego.LegoDS.PartRow;
            pr.BinRow = cbBin.SelectedItem as Lego.LegoDS.BinRow;
            DateTime it = DateTime.Now;
            foreach (Lego.LegoDS.InventoryRow ir in TempInventoryRows)
                if (ir.Count > 0)
                {
                    ir.Date = it;
                    ds.Inventory.AddInventoryRow(ir);
                }
        }

        private void bnInventoryMinus_Click(object sender, RoutedEventArgs e)
        {
            Lego.LegoDS ds = Lego.Connection.ds;
            Button b = sender as Button;
            string eid = b.Tag as string;
            foreach (Lego.LegoDS.InventoryRow ir in TempInventoryRows)
                if (ir.ElementRow.Number == eid)
                    ir.CountBin = ir.CountBin - 1;
        }

        private void bnInventoryPlus_Click(object sender, RoutedEventArgs e)
        {
            Lego.LegoDS ds = Lego.Connection.ds;
            Button b = sender as Button;
            string eid = b.Tag as string;
            foreach (Lego.LegoDS.InventoryRow ir in TempInventoryRows)
                if (ir.ElementRow.Number == eid)
                    ir.CountBin = ir.CountBin + 1;
        }

        private void bnInventoryDone_Click(object sender, RoutedEventArgs e)
        {
            Lego.LegoDS ds = Lego.Connection.ds;
            Button b = sender as Button;
            string eid = b.Tag as string;
            foreach (Lego.LegoDS.InventoryRow ir in TempInventoryRows)
                if (ir.ElementRow.Number == eid)
                    ir.SetCount(ir.CountBin + ir.ElementRow.CountBuilt);
        }

        private void bnContainerNew_Click(object sender, RoutedEventArgs e)
        {
            Lego.LegoDS ds = Lego.Connection.ds;
            Lego.LegoDS.ContainerRow cr = ds.Container.NewContainerRow();
            cr.Name = tbNewName.Text;
            ds.Container.AddContainerRow(cr);
            cbContainer.ItemsSource = ds.Container.Rows;
            cbContainer.SelectedItem = cr;
        }

        private void bnBinNew_Click(object sender, RoutedEventArgs e)
        {
            Lego.LegoDS ds = Lego.Connection.ds;
            Lego.LegoDS.BinRow br = ds.Bin.NewBinRow();
            br.ContainerRow = cbContainer.SelectedItem as Lego.LegoDS.ContainerRow;
            br.Name = tbNewName.Text;
            ds.Bin.AddBinRow(br);
            UpdateBinOptions();
            cbBin.SelectedItem = br;
        }

        private void cbContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateBinOptions();
        }

        private void UpdateBinOptions()
        {
            try
            {
                Lego.LegoDS.ContainerRow cr = cbContainer.SelectedItem as Lego.LegoDS.ContainerRow;
                cbBin.ItemsSource = cr.GetBinRows();
            }
            catch
            {
                cbBin.ItemsSource = null;
            }
        }

        private void bnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Lego.Connection.Update();
        }

        private void bnBuildDiffDel_Click(object sender, RoutedEventArgs e)
        {
            Lego.LegoDS ds = Lego.Connection.ds;
            Lego.LegoDS.BuildRow br = tiBuild.DataContext as Lego.LegoDS.BuildRow;
            Button b = sender as Button;
            string eid = b.Tag as string;
            List<Lego.LegoDS.BuildDiffRow> tbd = new List<Lego.LegoDS.BuildDiffRow>();
            foreach (Lego.LegoDS.BuildDiffRow dr in br.GetBuildDiffRows())
                if (dr.ElementRow.Number == eid)
                    tbd.Add(dr);
            while (tbd.Count > 0)
            {
                Lego.LegoDS.BuildDiffRow dr = tbd[0];
                tbd.Remove(dr);
                ds.BuildDiff.RemoveBuildDiffRow(dr);
            }
        }
    }
}
