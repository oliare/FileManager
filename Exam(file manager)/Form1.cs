using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Exam_file_manager_
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public void ShowFolders()
        {
            treeView1.BeginUpdate();
            string[] drives = Directory.GetLogicalDrives();
            foreach (var drive in drives)
            {
                var info = new DirectoryInfo(drive);
                TreeNode tn = new TreeNode()
                {
                    Text = info.Name,
                    Tag = info,
                };
                treeView1.Nodes.Add(tn);
                AddDirs(tn);
            }
            treeView1.EndUpdate();
        }
        public void ShowFiles()
        {
            if (treeView1.SelectedNode == null)
            {
                return;
            }
            DirectoryInfo di = new DirectoryInfo(treeView1.SelectedNode.FullPath);
            FileInfo[] files = { };
            ListViewItem item;

            listView1.Items.Clear();
            listView1.SmallImageList = imageList1;
            if (di.Exists)
                files = di.GetFiles();

            listView1.BeginUpdate();
            foreach (FileInfo file in files)
            {
                Icon iconf;
                item = new ListViewItem(file.Name);
                item.Tag = file;
                listView1.Items.Add(item);
                iconf = SystemIcons.WinLogo;

                if (!imageList1.Images.ContainsKey(file.Extension))
                {
                    iconf = Icon.ExtractAssociatedIcon(file.FullName);
                    imageList1.Images.Add(file.Extension, iconf);
                }
                item.ImageKey = file.Extension;
                item.SubItems.Add(FileSize((ulong)file.Length));
                item.SubItems.Add(file.LastWriteTime.ToShortDateString());
                item.SubItems.Add(file.Extension);
            }
            listView1.EndUpdate();
        }
        private string FileSize(ulong bytes)
        {
            string[] sizes = { "bytes", "MB", "KB", "GB", "TB" };
            int converted = 0;

            while (bytes >= 1024 && converted < sizes.Length - 1)
            {
                converted++;
                bytes = bytes / 1024;
            }
            return $"{bytes} {sizes[converted]}";
        }

        public void AddDirs(TreeNode tn)
        {
            string path = tn.FullPath;
            DirectoryInfo di = new DirectoryInfo(path);
            DirectoryInfo[] dirs = { };
            try
            {
                if (di.Exists)
                    dirs = di.GetDirectories();
            }
            catch (Exception)
            {
                //MessageBox.Show("Error: " + ex.Message); // access denied
            }

            foreach (DirectoryInfo dir in dirs)
            {
                TreeNode tnd = new TreeNode()
                {
                    Text = dir.Name,
                    Tag = dir,
                };
                tn.Nodes.Add(tnd);
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            ShowFolders();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            textBox1.Text = e.Node.FullPath;
            ShowFiles();
        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            treeView1.BeginUpdate();
            foreach (TreeNode tn in e.Node.Nodes)
            {
                AddDirs(tn);
            }
            treeView1.EndUpdate();
        }
        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(MousePosition, ToolStripDropDownDirection.Right);
            }
        }
        // OPEN FILE
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string path = Path.Combine(treeView1.SelectedNode.FullPath, listView1.SelectedItems[0].Text);
                OpenFile(path);
            }
            else
            {
                MessageBox.Show("No item selected to open");
            }
        }
        private void OpenFile(string fname)
        {
            try
            {
                Process.Start(fname);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening the file: {ex.Message}");
            }
        }

        // COPY
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0) // treeView1.SelectedNode != null
            {
                try
                {
                    var fileInfo = (FileInfo)listView1.SelectedItems[0].Tag;

                    Clipboard.SetText(fileInfo.FullName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error copying file: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("No item to copy or no destination folder selected");
            }
        }


        // PASTE FILE
        private void pasteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                var folder = textBox1.Text;
                var file = Clipboard.GetText();
                string dest = Path.Combine(folder, $"{Path.GetFileNameWithoutExtension(file)}_copy{Path.GetExtension(file)}");
                File.Copy(file, dest);
                listView1.Items.Clear();
                treeView1.Nodes.Clear();
                ShowFiles();
                ShowFolders();
                MessageBox.Show("File pasted successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error paste file: " + ex.Message);
            }
        }
        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNew("file");
        }

        private void folderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNew("folder");
        }
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0 && listView1.SelectedItems[0].Text == "New")
            {
                TreeNode node = treeView1.SelectedNode.Parent;

                if (node != null)
                {
                    string title = node.Text;

                    if (title == "File")
                    {
                        CreateNew("file");
                    }
                    else if (title == "Folder")
                    {
                        CreateNew("folder");
                    }
                }
            }
        }
        private void CreateNew(string type)
        {
            string create = Interaction.InputBox($"Enter the {type} name:", $">>> Creating {type}", "");

            if (!string.IsNullOrEmpty(create))
            {
                TreeNode node = treeView1.SelectedNode;
                string dest = Path.Combine(node.FullPath, create);
                try
                {
                    if (type == "file")
                    {
                        File.Create(dest).Close();
                    }
                    else if (type == "folder")
                    {
                        Directory.CreateDirectory(dest);
                    }
                    treeView1.Nodes.Clear();
                    ShowFolders();
                    ShowFiles();
                }
                catch
                {
                    MessageBox.Show($"{type} '{create}' created successfully");
                }
            }
            else
            {
                MessageBox.Show($"Item creation error !");
            }
        }

        private void moveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    DialogResult result = dialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        string dest = dialog.SelectedPath;
                        foreach (ListViewItem item in listView1.SelectedItems)
                        {
                            string path = Path.Combine(treeView1.SelectedNode.FullPath, item.Text);
                            MoveFile(path, dest);
                        }
                        MessageBox.Show("File moved successfully");
                    }
                    else
                        MessageBox.Show("File weren't moved");
                }
            }
            else
                MessageBox.Show("No item selected to move");
        }
        private void MoveFile(string source, string dest)
        {
            string path = Path.Combine(dest, Path.GetFileName(source));
            File.Move(source, path);
            ShowFiles();
        }

        // DELETE FILE    
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                string path = Path.Combine(treeView1.SelectedNode.FullPath, listView1.SelectedItems[0].Text);

                if (MessageBox.Show($"Are you sure want to delete the file '{path}'?", ">>> Deleting",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    DeleteFile(path);
                }
            }
            else
            {
                MessageBox.Show("No file selected to delete");
            }
        }
        private void DeleteFile(string file)
        {
            try
            {
                File.Delete(file);
                ShowFiles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting the file: {ex.Message}");
            }
        }

        // DELETE FOLDER    
        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                string path = treeView1.SelectedNode.FullPath;

                DialogResult result = MessageBox.Show($"Are you sure you want to delete the folder '{path}'?",
                    "Confirm deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    DeleteFolder(path);
                }
            }
            else
            {
                MessageBox.Show("No folder selected to delete");
            }
        }
        private void DeleteFolder(string path)
        {
            try
            {
                Directory.Delete(path);
                treeView1.Nodes.Clear();
                ShowFolders();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting folder: {ex.Message}");
            }
        }

        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "No folder selected to open";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFileName = openFileDialog.FileName;
                    Process.Start(selectedFileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening the file: {ex.Message}");
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Process.Start("https://uk.wikipedia.org/wiki/FAR_Manager");
        }


    }
}
