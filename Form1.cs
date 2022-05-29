using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace FileManager
{
    public partial class Form1 : Form
    {
        readonly User user;

        public Form1(User user)
        {
            this.user = user;
            InitializeComponent();

            InitEvents();

            InitUserPrefs(user.UserData);
        }

        private void InitEvents()
        {
            Load += Form1_Load;
            listBox1.DoubleClick += ListBox1_DoubleClick;
            listBox2.DoubleClick += ListBox2_DoubleClick;
            listBox1.MouseUp += ListBox1_MouseUp;

            textBox1.GotFocus += TextBox1_GotFocus;
            textBox1.LostFocus += TextBox1_LostFocus;
            textBox1.KeyDown += TextBox1_KeyDown;

            this.FormClosed += Form1_FormClosed;
        }

        private void InitUserPrefs(UserPrefs userPrefs)
        {
            SetFont(userPrefs.MyFont, userPrefs.FontColor);
            SetImage(userPrefs.BackgroundImage);
            SetListBoxColor(userPrefs.BackColor);
        }

        private void fontMenuItem_Click(object sender, EventArgs e)
        {
            FontDialog fontDialog = new FontDialog();
            fontDialog.Font = button1.Font;
            fontDialog.Color = button1.ForeColor;
            fontDialog.MaxSize = 36;
            fontDialog.ShowColor = true;

            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                SetFont(fontDialog.Font, fontDialog.Color);
            }
        }

        private void SetFont(Font font, Color fontColor)
        {
            button1.Font = font;
            button2.Font = font;

            listBox1.Font = font;
            listBox2.Font = font;

            textBox1.Font = font;
            textBox2.Font = font;

            textBox1.ForeColor = fontColor;
            textBox2.ForeColor = fontColor;

            listBox1.ForeColor = fontColor;
            listBox2.ForeColor = fontColor;

            button1.ForeColor = fontColor;
            button2.ForeColor = fontColor;
        }

        private void backgroundMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
                SetListBoxColor(colorDialog.Color);
        }

        private void SetListBoxColor(Color color)
        {
            listBox1.BackColor = color;
            listBox2.BackColor = color;

            textBox1.BackColor = color;
            textBox2.BackColor = color;

            button1.BackColor = color;
            button2.BackColor = color;
        }

        private void pictureMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                openFileDialog.Filter = "Image Files | *.jpg; *.jpeg; *.png";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string imagePath = openFileDialog.FileName;
                    Image image = Image.FromFile(imagePath);

                    SetImage(image);
                }
            }
        }

        private void SetImage(Image image)
        {
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.BackgroundImage = image;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            UserPrefs newUserPrefs = new UserPrefs(listBox1.Font, listBox1.ForeColor, listBox1.BackColor, this.BackgroundImage);
            user.UserData = newUserPrefs;

            AuthForm.SerializeData(user);

            Application.Exit();
        }

        string currPath1 = string.Empty;
        string currPath2 = string.Empty;


        private void OpenDirectory(ListBox listBox, string dirName, string currPath)
        {
            if (string.IsNullOrEmpty(dirName) && string.IsNullOrEmpty(currPath))
                return;
            if (dirName != null)
            {
                listBox.Items.Clear();
                string path = Path.Combine(currPath, dirName);
                DirectoryInfo currDir = new DirectoryInfo(path);
                foreach (DirectoryInfo item in currDir.GetDirectories().Where(dir => !dir.Attributes.HasFlag(FileAttributes.Hidden | FileAttributes.System)))
                {
                    listBox.Items.Add('[' + item.ToString() + ']');
                }
                foreach (FileInfo item in currDir.GetFiles().Where(dir => !dir.Attributes.HasFlag(FileAttributes.Hidden | FileAttributes.System)))
                {
                    listBox.Items.Add(item);
                }
            }
        }
        private string Cleanse(string dir)
        {
            if (dir.StartsWith("["))
            {
                dir = dir.Remove(dir.Length - 1);
                dir = dir.Remove(0, 1);
            }
            return dir;
        }
        private void ListBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {

                string dir = Cleanse(listBox1.SelectedItem.ToString());
                string path = Path.Combine(currPath1, dir);
                if (Directory.Exists(path))
                {
                    OpenDirectory(listBox1, dir, currPath1);
                    currPath1 = path;
                    textBox1.Text = currPath1;
                }
                else if (File.Exists(path))
                {
                    Process.Start(path);
                }
            }
        }
        private void ListBox2_DoubleClick(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem != null)
            {
                string dir = Cleanse(listBox2.SelectedItem.ToString());
                string path = Path.Combine(currPath2, dir);
                if (Directory.Exists(path))
                {
                    OpenDirectory(listBox2, dir, currPath2);
                    currPath2 = path;
                    textBox2.Text = currPath2;
                }
                else if (File.Exists(path))
                {
                    Process.Start(path);
                }
            }
        }
        private void ListBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (listBox1.SelectedItem != null)
                {
                    contextMenuStrip1.Show(Cursor.Position);

                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo item in allDrives)
            {
                listBox1.Items.Add(item.Name);
                listBox2.Items.Add(item.Name);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currPath1))
                if (Directory.GetParent(currPath1) != null)
                {
                    currPath1 = Directory.GetParent(currPath1).FullName;
                    OpenDirectory(listBox1, string.Empty, currPath1);
                    textBox1.Text = currPath1;
                }
                else
                {
                    listBox1.Items.Clear();
                    textBox1.Text = "";
                    DriveInfo[] allDrives = DriveInfo.GetDrives();
                    foreach (DriveInfo item in allDrives)
                    {
                        listBox1.Items.Add(item.Name);
                    }
                }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currPath2))
                if (Directory.GetParent(currPath2) != null)
                {
                    currPath2 = Directory.GetParent(currPath2).FullName;
                    OpenDirectory(listBox2, string.Empty, currPath2);
                    textBox2.Text = currPath2;
                }
                else
                {
                    listBox2.Items.Clear();
                    textBox2.Text = "";
                    DriveInfo[] allDrives = DriveInfo.GetDrives();
                    foreach (DriveInfo item in allDrives)
                    {
                        listBox2.Items.Add(item.Name);
                    }
                }

        }

        private void copy_Click(object sender, EventArgs e)
        {
            Copy();
        }
        private void Copy()
        {
            string dir = Cleanse(listBox1.SelectedItem.ToString());
            string path = Path.Combine(currPath1, dir);
            try
            {
                if (!string.IsNullOrEmpty(currPath2))
                {
                    if (File.Exists(path))
                    {
                        File.Copy(path, currPath2);
                    }

                    if (Directory.Exists(path))
                    {
                        string distPath = Directory.CreateDirectory(Path.Combine(currPath2, dir)).FullName;

                        CopyDirectory(path, distPath);
                    }
                }
                OpenDirectory(listBox2, string.Empty, currPath2);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            var dir = new DirectoryInfo(sourceDir);
            DirectoryInfo[] dirs = dir.GetDirectories();

            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir);
            }
        }
        private void Delete()
        {
            string dir = Cleanse(listBox1.SelectedItem.ToString());
            string path = Path.Combine(currPath1, dir);
            try
            {

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }


                OpenDirectory(listBox1, string.Empty, currPath1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void move_Click(object sender, EventArgs e)
        {
            Copy();
            Delete();
        }

        private void delete_Click(object sender, EventArgs e)
        {
            Delete();
        }

        private void archive_Click(object sender, EventArgs e)
        {
            string dir = Cleanse(listBox1.SelectedItem.ToString());
            string path = Path.Combine(currPath1, dir);
            try
            {
                if (!string.IsNullOrEmpty(currPath2))
                {
                    if (File.Exists(path))
                    {
                        string zipName = Path.GetFileNameWithoutExtension(path) + ".zip";
                        ArchiveFile(path, Path.Combine(currPath2, zipName));
                    }

                    if (Directory.Exists(path))
                    {
                        string zipName = Path.Combine(currPath2, dir + "ArchivedByZip.zip");

                        File.Create(zipName).Close();
                        ArchiveDirectory(path, zipName);
                    }
                }
                OpenDirectory(listBox2, string.Empty, currPath2);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ArchiveFile(string filePath, string zipPath)
        {
            using (ZipArchive zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
            }
        }

        private void ArchiveDirectory(string path, string zipPath)
        {
            using (ZipArchive zip = ZipFile.Open(zipPath, ZipArchiveMode.Update))
            {
                ZipArchiveEntry entry;
                DirectoryInfo directory = new DirectoryInfo(path);
                FileInfo[] files = directory.GetFiles("*");

                foreach (FileInfo file in files)
                {
                    entry = zip.CreateEntryFromFile(Path.Combine(path, file.Name), directory.Name + "/" + file.Name);
                }
            }
        }

        private void rename_Click(object sender, EventArgs e)
        {
            isRenaming = true;
            textBox1.Focus();
        }

        bool isRenaming;
        private void TextBox1_GotFocus(object sender, EventArgs e)
        {
            textBox1.Text = string.Empty;
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (isRenaming)
                {
                    Rename();
                }
            }
        }

        private void Rename()
        {
            string dir = Cleanse(listBox1.SelectedItem.ToString());
            string path = Path.Combine(currPath1, dir);
            try
            {
                if (!string.IsNullOrEmpty(currPath1) && !string.IsNullOrEmpty(textBox1.Text))
                {
                    if (File.Exists(path))
                    {
                        FileInfo file = new FileInfo(path);
                        string newPath = Path.Combine(Path.GetDirectoryName(path), textBox1.Text);

                        file.MoveTo(newPath);
                    }

                    if (Directory.Exists(path))
                    {
                        DirectoryInfo directory = new DirectoryInfo(path);

                        string newPath = Path.Combine(directory.Parent.FullName, textBox1.Text);
                        Directory.Move(directory.FullName, newPath);
                    }
                }
                OpenDirectory(listBox1, string.Empty, currPath1);
                OpenDirectory(listBox2, string.Empty, currPath2);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            listBox1.Focus();
        }

        private void TextBox1_LostFocus(object sender, EventArgs e)
        {
            textBox1.Text = currPath1;
        }
    }
}
