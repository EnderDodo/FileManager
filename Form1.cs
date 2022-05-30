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
using System.Text.RegularExpressions;
using System.Web;

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
            numericUpDown1.Font = font;
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

        private async void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            string langName = textBox1.Text;

            decimal countBooks = numericUpDown1.Value;

            int countPages = (int)Math.Ceiling(countBooks / 16);

            for (int i = 1; i <= countPages; i++)
            {
                string htmlString = await GetHtmlByPageIdAsync(langName, i);

                if (htmlString != null)
                {
                    Parse(htmlString, (int)countBooks);
                }
            }

            MessageBox.Show("Готово");
        }

        string url = "https://www.amazon.com/s?k={CurrentLangName}&i=stripbooks-intl-ship&page={CurrentPage}";
        public async Task<string> GetHtmlByPageIdAsync(string langName, int id)
        {
            var currentUrl = url.Replace("{CurrentLangName}", langName).Replace("{CurrentPage}", id.ToString());

            var clientHandler = new HttpClientHandler()
            { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };

            var client = new HttpClient(clientHandler);

            string source = null;
            client.DefaultRequestHeaders.Add("User-Agent", ".Net app");

            var response = await client.GetAsync(currentUrl);

            if (response != null && response.StatusCode == HttpStatusCode.OK)
            {
                source = await response.Content.ReadAsStringAsync();
            }

            return source;
        }

        private void Parse(string htmlPage, int countBooks)
        {
            Regex rgxItem = new Regex("<div class=\"a-section a-spacing-small a-spacing-top-small\">(.*?)<div class=\"sg-col sg-col-4-of-12 sg-col-4-of-16 sg-col-8-of-20\">");

            Regex rgxTitle = new Regex("<span class=\"a-size-medium a-color-base a-text-normal\">(.*?)</span>");

            Regex rgxAuthorWithRef = new Regex("<a class=\"a-size-base a-link-normal s-underline-text s-underline-link-text s-link-style\" href=\".*?\">(.*?)</a>");
            Regex rgxAuthor = new Regex("<span class=\"a-size-base\">(.*?)</span>");

            Regex rgxReview = new Regex("<span class=\"a-icon-alt\">(.*?) out of 5 stars</span>");

            Regex rgxDateRelease = new Regex("<span class=\"a-size-base a-color-secondary a-text-normal\">(.*?)</span>");

            Regex rgxPrice = new Regex("<span class=\"a-offscreen\">(.*?)</span>");

            Regex rgxRef = new Regex("<a class=\"a-link-normal s-underline-text s-underline-link-text s-link-style a-text-normal\" href=\"/(.*?)\">");

            foreach (Match match in rgxItem.Matches(htmlPage))
            {
                string htmlItem = match.Groups[1].Value;
                    
                string title = HttpUtility.HtmlDecode(rgxTitle.Match(htmlItem).Groups[1].Value);

                string authors = "";

                foreach (Match authorWithRef in rgxAuthorWithRef.Matches(htmlItem))
                {
                    authors += authorWithRef.Groups[1].Value + ", ";
                }

                foreach (Match author in rgxAuthor.Matches(htmlItem))
                {
                    string item = author.Groups[1].Value.Trim();
                    if (item != "by" && item != "and" && item != "," && item != ", et al.")
                    {
                        authors += item + ", ";
                    }

                }

                authors = HttpUtility.HtmlDecode(authors.Trim().TrimEnd(new char[] { ' ', ',' }));

                string review = rgxReview.Match(htmlItem).Groups[1].Value;
                if (string.IsNullOrEmpty(review))
                    review = "Null";

                string dateRelease = rgxDateRelease.Match(htmlItem).Groups[1].Value;
                if (string.IsNullOrEmpty(dateRelease))
                    dateRelease = "Null";

                string price = rgxPrice.Match(htmlItem).Groups[1].Value;

                string bookRef = "https://amazon.com/" + rgxRef.Match(htmlItem).Groups[1].Value.Replace(";", "&");

                ListViewItem listViewItem = new ListViewItem(title);
                listViewItem.SubItems.Add(authors);
                listViewItem.SubItems.Add(review);
                listViewItem.SubItems.Add(dateRelease);
                listViewItem.SubItems.Add(price);
                listViewItem.SubItems.Add(bookRef);
                listBox1.Items.Add(listViewItem);

                countBooks--;
                if (countBooks == 0)
                    return;
            }
        }

    }
}
