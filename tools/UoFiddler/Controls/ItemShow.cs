/***************************************************************************
 *
 * $Author: Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Ultima;

namespace Controls
{
    
    public partial class ItemShow : UserControl
    {
        public ItemShow()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            refMarker = this;
            if (!Files.CacheData)
                PreloadItems.Visible = false;
            ProgressBar.Visible = false;
        }

        private static ItemShow refMarker = null;
        private bool Loaded = false;

        private void MakeHashFile()
        {
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FileName = Path.Combine(path, "UOFiddlerArt.hash");
            using (FileStream fs = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                BinaryWriter bin = new BinaryWriter(fs);
                byte[] md5 = Files.GetMD5(Files.GetFilePath("Art.mul"));
                if (md5 == null)
                    return;
                int length = md5.Length;
                bin.Write(length);
                bin.Write(md5);
                foreach (ListViewItem item in listView1.Items)
                {
                    bin.Write((int)item.Tag);
                }
            }
        }

        /// <summary>
        /// Searches Objtype and Select
        /// </summary>
        /// <param name="graphic"></param>
        /// <returns></returns>
        public static bool SearchGraphic(int graphic)
        {
            int index = 0;
            for (int i = index; i < refMarker.listView1.Items.Count; i++)
            {
                ListViewItem item = refMarker.listView1.Items[i];
                if ((int)item.Tag == graphic)
                {
                    if (refMarker.listView1.SelectedItems.Count == 1)
                        refMarker.listView1.SelectedItems[0].Selected = false;
                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Searches for name and selects
        /// </summary>
        /// <param name="name"></param>
        /// <param name="next">starting from current selected</param>
        /// <returns></returns>
        public static bool SearchName(string name,bool next)
        {
            int index=0;
            if (next)
            {
                if (refMarker.listView1.SelectedIndices.Count == 1)
                    index = refMarker.listView1.SelectedIndices[0] + 1;
                if (index >= refMarker.listView1.Items.Count)
                    index = 0;
            }

            for (int i = index; i < refMarker.listView1.Items.Count; i++)
            {
                ListViewItem item = refMarker.listView1.Items[i];
                if (TileData.ItemTable[(int)item.Tag].Name.Contains(name))
                {
                    if (refMarker.listView1.SelectedItems.Count == 1)
                        refMarker.listView1.SelectedItems[0].Selected = false;
                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        public void Reload()
        {
            if (Loaded)
                OnLoad(this, EventArgs.Empty);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            this.Cursor = Cursors.AppStarting;
            Loaded = true;
            listView1.BeginUpdate();
            listView1.Clear();

            if ((Files.UseHashFile) && (Files.CompareHashFile("Art")))
            {
                string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                string FileName = Path.Combine(path, "UOFiddlerArt.hash");
                if (File.Exists(FileName))
                {
                    using (BinaryReader bin = new BinaryReader(new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
                    {
                        int length = bin.ReadInt32();
                        bin.BaseStream.Seek(length, SeekOrigin.Current);
                        while (bin.BaseStream.Length != bin.BaseStream.Position)
                        {
                            int i = bin.ReadInt32();
                            ListViewItem item = new ListViewItem(i.ToString(), 0);
                            item.Tag = i;
                            listView1.Items.Add(item);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < 0x4000; i++)
                {
                    if (Art.IsValidStatic(i))
                    {
                        ListViewItem item = new ListViewItem(i.ToString(), 0);
                        item.Tag = i;
                        listView1.Items.Add(item);
                    }
                }
                if (Files.UseHashFile)
                    MakeHashFile();
            }

            listView1.TileSize = new Size(Options.ArtItemSizeWidth, Options.ArtItemSizeHeight);

            listView1.EndUpdate();
            this.Cursor = Cursors.Default;
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                int i = (int)listView1.SelectedItems[0].Tag;
                namelabel.Text = String.Format("Name: {0}", TileData.ItemTable[i].Name);
                graphiclabel.Text = String.Format("Graphic: 0x{0:X4} ({0})", i);
                UpdateDetail(i);
            }
        }

        private void drawitem(object sender, DrawListViewItemEventArgs e)
        {
            int i = (int)e.Item.Tag;

            Bitmap bmp = Art.GetStatic(i);

            if (bmp != null)
            {
                if (listView1.SelectedItems.Contains(e.Item))
                    e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds.X, e.Bounds.Y, e.Bounds.Width,e.Bounds.Height);
                else
                    e.Graphics.FillRectangle(Brushes.White, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);

                if (Options.ArtItemClip)
                {
                    e.Graphics.DrawImage(bmp, e.Bounds.X + 1, e.Bounds.Y + 1,
                                         new Rectangle(0, 0, e.Bounds.Width - 1, e.Bounds.Height - 1),
                                         GraphicsUnit.Pixel);
                }
                else
                {
                    int width=bmp.Width;
                    int height=bmp.Height;
                    if (width > e.Bounds.Width)
                    {
                        width = e.Bounds.Width;
                        height = e.Bounds.Height * bmp.Height/ bmp.Width;
                    }
                    if (height > e.Bounds.Height)
                    {
                        height = e.Bounds.Height;
                        width = e.Bounds.Width * bmp.Width / bmp.Height;
                    }
                    e.Graphics.DrawImage(bmp,
                                         new Rectangle(e.Bounds.X + 1, e.Bounds.Y + 1, width, height));
                }
                if (!listView1.SelectedItems.Contains(e.Item))
                    e.Graphics.DrawRectangle(Pens.Gray, e.Bounds.X, e.Bounds.Y, e.Bounds.Width,e.Bounds.Height);
            }
        }

        private void listView_DoubleClicked(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                ItemDetail f = new ItemDetail((int)listView1.SelectedItems[0].Tag);
                f.TopMost = true;
                f.Show();
            }
        }

        private ItemSearch showform=null;
        private void Search_Click(object sender, EventArgs e)
        {
            if ((showform == null) || (showform.IsDisposed))
            {
                showform = new ItemSearch();
                showform.TopMost = true;
                showform.Show();
            }
        }

        private void UpdateDetail(int id)
        {
            Ultima.ItemData item = Ultima.TileData.ItemTable[id];
            Bitmap bit = Ultima.Art.GetStatic(id);
            splitContainer2.SplitterDistance = bit.Size.Height + 10;
            Bitmap newbit = new Bitmap(DetailPictureBox.Size.Width, DetailPictureBox.Size.Height);
            Graphics newgraph = Graphics.FromImage(newbit);
            newgraph.Clear(Color.FromArgb(-1));
            newgraph.DrawImage(bit, (DetailPictureBox.Size.Width - bit.Width) / 2, 5);

            DetailPictureBox.Image = newbit;

            DetailTextBox.Clear();
            DetailTextBox.AppendText(String.Format("Name: {0}\n", item.Name));
            DetailTextBox.AppendText(String.Format("Graphic: 0x{0:X4}\n", id));
            DetailTextBox.AppendText(String.Format("Height/Capacity: {0}\n", item.Height));
            DetailTextBox.AppendText(String.Format("Weight: {0}\n", item.Weight));
            DetailTextBox.AppendText(String.Format("Animation: {0}\n", item.Animation));
            DetailTextBox.AppendText(String.Format("Quality/Layer/Light: {0}\n", item.Quality));
            DetailTextBox.AppendText(String.Format("Quantity: {0}\n", item.Quantity));
            DetailTextBox.AppendText(String.Format("Hue: {0}\n", item.Hue));
            DetailTextBox.AppendText(String.Format("StackingOffset/Unk4: {0}\n", item.StackingOffset));
            DetailTextBox.AppendText(String.Format("Flags: {0}\n", item.Flags));
            if ((item.Flags & TileFlag.Animation) != 0)
            {
                Animdata.Data info = Animdata.GetAnimData(id);
                if (info != null)
                    DetailTextBox.AppendText(String.Format("Animation FrameCount: {0} Interval: {1}\n", info.FrameCount, info.FrameInterval));
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            DialogResult result =
                        MessageBox.Show("Are you sure? Will take a while", 
                        "Save", 
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                this.Cursor = Cursors.WaitCursor;
                Art.Save(AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
                this.Cursor = Cursors.Default;
                MessageBox.Show(String.Format("Saved to {0}", AppDomain.CurrentDomain.SetupInformation.ApplicationBase), 
                    "Save", 
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClickReplace(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = false;
                dialog.Title = "Choose image file to replace";
                dialog.CheckFileExists = true;
                dialog.Filter = "image file (*.tiff)|*.tiff";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Bitmap bmp = new Bitmap(dialog.FileName);
                    Art.ReplaceStatic((int)listView1.SelectedItems[0].Tag, bmp);
                    listView1.Invalidate();
                    UpdateDetail((int)listView1.SelectedItems[0].Tag);
                }
            }
        }

        private void onTextChanged_Insert(object sender, EventArgs e)
        {
            int index;
            bool candone;
            if (InsertText.Text.Contains("0x"))
            {
                string convert = InsertText.Text.Replace("0x", "");
                candone = int.TryParse(convert, System.Globalization.NumberStyles.HexNumber, null, out index);
            }
            else
                candone = int.TryParse(InsertText.Text, System.Globalization.NumberStyles.Integer, null, out index);

            if (index > 0xBFFF)
                candone = false;
            if (candone)
            {
                if (Art.IsValidStatic(index))
                    InsertText.ForeColor = Color.Red;
                else
                    InsertText.ForeColor = Color.Black;
            }
            else
                InsertText.ForeColor = Color.Red;
        }

        private void onKeyDown_Insert(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int index;
                bool candone;
                if (InsertText.Text.Contains("0x"))
                {
                    string convert = InsertText.Text.Replace("0x", "");
                    candone = int.TryParse(convert, System.Globalization.NumberStyles.HexNumber, null, out index);
                }
                else
                    candone = int.TryParse(InsertText.Text, System.Globalization.NumberStyles.Integer, null, out index);
                if (index > 0x3FFF)
                    candone = false;
                if (candone)
                {
                    if (Art.IsValidStatic(index))
                        return;
                    contextMenuStrip1.Close();
                    OpenFileDialog dialog = new OpenFileDialog();
                    dialog.Multiselect = false;
                    dialog.Title = String.Format("Choose image file to insert at 0x{0:X}", index);
                    dialog.CheckFileExists = true;
                    dialog.Filter = "image file (*.tiff)|*.tiff";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Bitmap bmp = new Bitmap(dialog.FileName);
                        Art.ReplaceStatic(index, bmp);
                        //bug in listview always added to end so...
                        listView1.BeginUpdate();
                        listView1.Items.Clear();
                        for (int i = 0; i < 0x4000; i++)
                        {
                            if (Art.IsValidStatic(i))
                            {
                                ListViewItem item = new ListViewItem(i.ToString(), 0);
                                item.Tag = i;
                                listView1.Items.Add(item);
                            }
                        }
                        listView1.EndUpdate();
                        SearchGraphic(index);
                    }
                }
            }
        }

        private void onClickRemove(object sender, EventArgs e)
        {
            int i = (int)listView1.SelectedItems[0].Tag;
            DialogResult result =
                        MessageBox.Show(String.Format("Are you sure to remove 0x{0:X}", i), 
                        "Save", 
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                Art.RemoveStatic(i);
                i = listView1.SelectedItems[0].Index;
                listView1.SelectedItems[0].Selected = false;
                listView1.Items.RemoveAt(i);
                listView1.Invalidate();
            }
        }

        private void onClickFindFree(object sender, EventArgs e)
        {
            int id = (int)listView1.SelectedItems[0].Tag;
            id++;
            for (int i = listView1.SelectedItems[0].Index + 1; i < listView1.Items.Count; i++)
            {
                if (id < (int)listView1.Items[i].Tag)
                {
                    ListViewItem item = listView1.Items[i];
                    if (listView1.SelectedItems.Count == 1)
                        listView1.SelectedItems[0].Selected = false;
                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible();
                    break;
                }
                id++;
            }
        }

        #region Preloader
        private void OnClickPreload(object sender, EventArgs e)
        {
            if (PreLoader.IsBusy)
                return;
            ProgressBar.Minimum = 1;
            ProgressBar.Maximum = listView1.Items.Count;
            ProgressBar.Step = 1;
            ProgressBar.Value = 1;
            ProgressBar.Visible = true;
            PreLoader.RunWorkerAsync();
        }

        private void PreLoaderDoWork(object sender, DoWorkEventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                Art.GetStatic((int)item.Tag);
                PreLoader.ReportProgress(1);
            }
        }

        private void PreLoaderProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.PerformStep();
        }

        private void PreLoaderCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressBar.Visible = false;
        }
        #endregion
    }
}