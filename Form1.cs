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
using System.Net.Http;
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
        }

        private void InitEvents()
        {
            Load += Form1_Load;
            listBox1.DoubleClick += ListBox1_DoubleClick;

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

            listBox1.Font = font;

            textBox1.Font = font;

            textBox1.ForeColor = fontColor;

            listBox1.ForeColor = fontColor;

            button1.ForeColor = fontColor;
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

            textBox1.BackColor = color;

            button1.BackColor = color;
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

        private void ListBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.FocusedItem != null)
            {

            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            InitUserPrefs(user.UserData);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string langName = textBox1.Text;

        }

    }
}
