using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Paragraph_Core
{
    public class User
    {
        public int index = 0;
        private string login = "";
        private string password = "";
        public static List<string> names;
        public static List<string> grades;
        public static List<string> teachers;
        // Поле Login
        public string Login
        {
            get
            {
                return login;
            }
            set
            {
                login = value;
            }
        }

        // Поле Password
        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
            }
        }

        // Поле Данных
        public List<string> Names
        {
            get
            {
                return names;
            }
            set
            {
                if (value != null)
                    names = value;
            }
        }
        public List<string> Grades
        {
            get
            {
                return grades;
            }
            set
            {
                if (value != null)
                    grades = value;
            }
        }
        public List<string> Teachers
        {
            get
            {
                return teachers;
            }
            set
            {
                if (value != null)
                    teachers = value;
            }
        }

        public bool showGapes = false;
        public bool showTeachers = false;
        public bool multipleStudent = false;
        public bool serverAccessesability = false;
        public bool busy = true;
        // Class Constructor
        public User(string _login, string _password)
        {
            Login = _login;
            Password = _password;
        }
        public User(string _login, string _password, bool _showGapes, bool _showTeachers)
        {
            Login = _login;
            Password = _password;
            showGapes = _showGapes;
            showTeachers = _showTeachers;
        }
        // Methods 
        async public Task<List<string>> GetInfoAsync()
        {
            busy = true;
            Thread thread = new Thread(() => Parser.Parse(this, this.Login, Password, this.showGapes, this.showTeachers));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Priority = ThreadPriority.Highest;
            thread.Start();
            List<string> info = new List<string>();
            while (!(thread.ThreadState == ThreadState.Stopped))
            {
                if (thread.ThreadState == ThreadState.Stopped && this.Names != null)
                {
                    for (int i = 0; i < this.Names.Count; i++)
                    {
                        info.Add(this.Names[i]);
                        if (this.Grades != null)
                            info.Add(this.Grades[i]);
                        if (this.Teachers != null)
                            info.Add(this.Teachers[i]);
                    }
                    break;
                }
            }
            busy = false;
            return info;
        }
    }
}
