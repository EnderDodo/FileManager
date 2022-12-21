using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileManager
{
    public partial class AuthForm : Form
    {
        const string dataFileName = "users.dat";

        public AuthForm()
        {
            InitializeComponent();
        }

        private void signUpButton_Click(object sender, EventArgs e)
        {
            string login = loginTextBox.Text;
            string password = passwordTextBox.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                return;

            User userData = new User(BCrypt.Net.BCrypt.HashPassword(login), BCrypt.Net.BCrypt.HashPassword(password));
            SerializeData(userData);
        }

        public static void SerializeData(User userData)
        {
            BinaryFormatter binFormat = new BinaryFormatter();
            string usersFilePath = Path.Combine(Path.GetTempPath(), dataFileName);

            FileStream fStream = new FileStream(usersFilePath, FileMode.Create, FileAccess.Write, FileShare.None);

            binFormat.Serialize(fStream, userData);
            fStream.Close();
        }

        private void signInButton_Click(object sender, EventArgs e)
        {
            string login = loginTextBox.Text;
            string password = passwordTextBox.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                return;

            BinaryFormatter binFormat = new BinaryFormatter();
            string usersFilePath = Path.Combine(Path.GetTempPath(), dataFileName);
            FileStream fStream = File.OpenRead(usersFilePath);

            User user = (User)binFormat.Deserialize(fStream);

            fStream.Close();

            if (BCrypt.Net.BCrypt.Verify(login, user.Login) && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                Form1 form1 = new Form1(user);
                form1.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("i dunno who u r...... u cant pass...");
            }
        }
    }
}
