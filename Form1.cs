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
using System.Threading;
using System.Text.RegularExpressions;
using System.Net;

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

        #region Init
        private void Form1_Load(object sender, EventArgs e)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo item in allDrives)
            {
                listBox1.Items.Add(item.Name);
                listBox2.Items.Add(item.Name);
            }

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

        private void SetListBoxColor(Color color)
        {
            listBox1.BackColor = color;
            listBox2.BackColor = color;

            textBox1.BackColor = color;
            textBox2.BackColor = color;

            button1.BackColor = color;
            button2.BackColor = color;
        }

        #endregion

        #region Settings
        private void backgroundMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
                SetListBoxColor(colorDialog.Color);
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
        #endregion

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
            if (isSearching || isRenaming || isDonwloading)
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
                if (isSearching)
                {
                    Search();
                }
                if (isDonwloading)
                    Download();
            }

            if (e.KeyCode == Keys.Escape && isDonwloading)
            {
                tokenSource.Cancel();
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

        bool isSearching = false;
        private void search_Click(object sender, EventArgs e)
        {
            isSearching = true;
            textBox1.Focus();
        }

        private void Search()
        {
            string dir = Cleanse(listBox1.SelectedItem.ToString());
            string path = Path.Combine(currPath1, dir);
            listBox1.Items.Clear();

            string pattern = textBox1.Text;

            SearchByPatternAsync(pattern, path);
        }

        public async void SearchByPatternAsync(string pattern, string searchPath)
        {
            await Task.Run(() =>
            {
                if (File.Exists(searchPath))
                {
                    SearchIntoFile(pattern, searchPath);
                }
                else
                {
                    SearchIntoDirectory(pattern, searchPath);
                }
            });

            MessageBox.Show("Поиск завершён");
            isSearching = false;
        }

        private void SearchIntoDirectory(string pattern, string path)
        {
            Regex regex = new Regex(pattern);
            try
            {
                Parallel.ForEach(Directory.GetDirectories(path).Where(file => !new DirectoryInfo(file).Attributes.HasFlag(FileAttributes.Hidden | FileAttributes.System)), dirName =>
                {
                    if (regex.IsMatch(new DirectoryInfo(dirName).Name))
                    {
                        listBox1.BeginInvoke(new MethodInvoker(delegate
                        {
                            listBox1.Items.Add("[" + new DirectoryInfo(dirName).Name + "]");
                        }));
                    }
                    SearchIntoDirectory(pattern, dirName);
                });

                Parallel.ForEach(Directory.GetFiles(path).Where(file => !new DirectoryInfo(file).Attributes.HasFlag(FileAttributes.Hidden | FileAttributes.System)), fileName =>
                {
                    if (regex.IsMatch(new FileInfo(fileName).Name))
                    {
                        listBox1.BeginInvoke(new MethodInvoker(delegate
                        {
                            listBox1.Items.Add(new FileInfo(fileName).Name);
                        }));
                    }
                    else if (!IsFileLocked(new FileInfo(fileName)))
                    {
                        SearchIntoFile(pattern, fileName);
                    }
                });
            }
            catch { }
        }

        private static bool IsFileLocked(FileInfo file)
        {
            try
            {
                FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
                stream.Dispose();
            }
            catch
            {
                return true;
            }
            return false;
        }

        private void SearchIntoFile(string pattern, string path)
        {
            try
            {
                string fileData = File.ReadAllText(path);
                if (new Regex(pattern).IsMatch(fileData))
                {
                    listBox1.BeginInvoke(new MethodInvoker(delegate
                    {
                        listBox1.Items.Add(new FileInfo(path).Name);
                    }));

                }
            }
            catch { }
        }

        bool isDonwloading = false;
        private void downloadMenuItem_Click(object sender, EventArgs e)
        {
            isDonwloading = true;
            textBox1.Focus();
        }

        CancellationTokenSource tokenSource;
        private void Download()
        {
            if (tokenSource != null)
                tokenSource.Cancel();
            
            string remoteFilename = textBox1.Text;
            string localFileName = Path.Combine(currPath1, remoteFilename.Split('/').Last());

            tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            DownloadFileAsync(remoteFilename, localFileName, token);
            OpenDirectory(listBox1, string.Empty, currPath1);
            OpenDirectory(listBox2, string.Empty, currPath2);
        }

        public async void DownloadFileAsync(string remoteFilename, string localFilename, CancellationToken token)
        {
            // Function will return the number of bytes processed
            // to the caller. Initialize to 0 here.
            bool isError = false;
            await Task.Run(() =>
            {
                Stream remoteStream = null;
                Stream localStream = null;
                WebResponse response = null;

                // Use a try/catch/finally block as both the WebRequest and Stream
                // classes throw exceptions upon error
                try
                {
                    // Create a request for the specified remote file name
                    WebRequest request = WebRequest.Create(remoteFilename);
                    if (request != null)
                    {
                        // Send the request to the server and retrieve the
                        // WebResponse object 
                        response = request.GetResponse();
                        if (response != null)
                        {
                            // Once the WebResponse object has been retrieved,
                            // get the stream object associated with the response's data
                            remoteStream = response.GetResponseStream();

                            // Create the local file
                            localStream = File.Create(localFilename);

                            // Allocate a 1k buffer
                            byte[] buffer = new byte[1024];
                            int bytesRead;

                            // Simple do/while loop to read from stream until
                            // no bytes are returned
                            do
                            {
                                //cancel
                                if (token.IsCancellationRequested)
                                {
                                    localStream.Close();
                                    File.Delete(localFilename);
                                    MessageBox.Show("Отмена");
                                    break;
                                }
                                // Read data (up to 1k) from the stream
                                bytesRead = remoteStream.Read(buffer, 0, buffer.Length);

                                // Write the data to the local file
                                localStream.Write(buffer, 0, bytesRead);
                            } while (bytesRead > 0);
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    isError = true;
                }
                finally
                {
                    // Close the response and streams objects here
                    // to make sure they're closed even if an exception
                    // is thrown at some point
                    if (response != null) response.Close();
                    if (remoteStream != null) remoteStream.Close();
                    if (localStream != null) localStream.Close();
                }

                // Return total bytes processed to caller.
            }, token);

            if (!token.IsCancellationRequested && !isError)
                MessageBox.Show("Загрузка завершена");
        }

        private void properties_Click(object sender, EventArgs e)
        {
            string dir = Cleanse(listBox1.SelectedItem.ToString());
            string path = Path.Combine(currPath1, dir);
            string text = File.ReadAllText(path);

            var t1 = Task.Run(() =>
            {
                int count = Regex.Split(text, @"\W+").Where(x => !string.IsNullOrEmpty(x)).Count();
                MessageBox.Show($"Count of words: {count}\n");
            });

            var t2 = Task.Run(() =>
            {
                int count = text.Split('\n').Length;
                MessageBox.Show($"Count of lines: {count}\n");
            });

            var t3 = Task.Run(() =>
            {
                var dict = new Dictionary<string, int>();

                Parallel.ForEach(Regex.Split(text.ToLower(), @"\W+"), item =>
                {
                    if (string.IsNullOrEmpty(item) || item.Length <= 5)
                        return;

                    lock (dict)
                    {
                        if (dict.ContainsKey(item))
                            dict[item]++;
                        else
                            dict[item] = 1;
                    }
                });

                string message = "";

                dict.OrderByDescending(x => x.Value)
                    .Take(10)
                    .ToList()
                    .ForEach(x => { message += $"Word = {x.Key} | count = {x.Value}\n"; });

                MessageBox.Show(message);
            });
        }
    }
}
