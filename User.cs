using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
    [Serializable]
    public class User
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public UserPrefs UserData { get; set; }

        public User(string login, string password)
        {
            Login = login;
            Password = password;
            UserData = new UserPrefs();
        }
    }

    [Serializable]
    public class UserPrefs
    {
        public Color BackColor { get; set; }
        public Font MyFont { get; set; }
        public Color FontColor { get; set; }
        public Image BackgroundImage { get; set; }

        public UserPrefs()
        {
            MyFont = new Font("Segoe UI", 12f, FontStyle.Bold);
            FontColor = Color.Black;
            BackColor = Color.White;
        }

        public UserPrefs(Font font, Color fontColor, Color backColor, Image image)
        {
            MyFont = font;
            FontColor = fontColor;
            BackColor = backColor;
            BackgroundImage = image;
        }
    }
}
