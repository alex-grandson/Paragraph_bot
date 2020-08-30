using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Paragraph_Core
{
    class Parser
    {
        // Html parser fields
        public static HtmlAgilityPack.HtmlDocument cabinetPage = new HtmlAgilityPack.HtmlDocument();
        public static HtmlAgilityPack.HtmlDocument marksPage = new HtmlAgilityPack.HtmlDocument();
        public static HtmlAgilityPack.HtmlNodeCollection students;
        public static HtmlAgilityPack.HtmlNode nameNode;
        public static HtmlAgilityPack.HtmlNodeCollection teachersListCollection;
        public static HtmlAgilityPack.HtmlNodeCollection marksLinks;
        public static HtmlAgilityPack.HtmlNodeCollection lessonNamesCollection;
        public static HtmlAgilityPack.HtmlNodeCollection finalGradesCollection;
        public static HtmlAgilityPack.HtmlNodeCollection gradesCollection;
        public static HtmlAgilityPack.HtmlNodeCollection gapesCollection;
        public static HtmlAgilityPack.HtmlNodeCollection lessonsList;
        public static HtmlAgilityPack.HtmlNodeCollection teachersList;
        public static HtmlAgilityPack.HtmlNodeCollection teachersListHeaders;

        public static int lessonCounter = 0;
        private static string name = "Имя ученика";
        private static string fullData = "";
        private static string teachers = "";

        static public void Parse(User user, string login, string password, bool showGapes, bool showTeachers)
        {
            string data = "Login=" + login + "&Password=" + password + "&doLogin=1";
            using (WebBrowser webBrowser1 = new WebBrowser())
            {
                webBrowser1.ScriptErrorsSuppressed = true;
                // Отключаем загрузку картнок
                Microsoft.Win32.RegistryKey myRegKey = Microsoft.Win32.Registry.CurrentUser;
                myRegKey = myRegKey.OpenSubKey(@"Software\Microsoft\Internet Explorer\Main", true);
                myRegKey.SetValue("Display Inline Images", "no");
            webBrowser1.Navigate("https://petersburgedu.ru/user/auth/login", "_self", System.Text.ASCIIEncoding.ASCII.GetBytes(data), "Content-Type: application/x-www-form-urlencoded; charset=UTF-8");
                while (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
                {
                    Application.DoEvents();
                }
                webBrowser1.Navigate("https://petersburgedu.ru/dnevnik/cabinet");
                while (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
                {
                    Application.DoEvents();
                }
                cabinetPage.LoadHtml(webBrowser1.DocumentText);
                if (cabinetPage.ToString().Contains("Ресурс временно недоступен"))
                {
                    user.serverAccessesability = false;
                    //Console.WriteLine("Ресурс временно недоступен, приносим свои извинения");
                } else
                {
                    user.serverAccessesability = true;
                }
                // If there only one student, program will do that it must do
                students = cabinetPage.DocumentNode.SelectNodes("//div[@class=\"user-box\"]/div[@class=\"heading\"]/div[@class=\"fio\"]/a");
                teachersListCollection = cabinetPage.DocumentNode.SelectNodes("//div[@class=\"popupContent\"]/div/table[@class=\"teachers-mail-list\"]");
                // Parsing Marks Page Adress for every student
                marksLinks = cabinetPage.DocumentNode.SelectNodes("//a[@class=\"marks\"]");

                if (students != null && marksLinks != null)
                {
                    List<string> userNames = new List<string>();
                    List<string> userGrades = new List<string>();
                    List<string> userTeachers = new List<string>();
                    for (int studentNum = 0; studentNum < students.Count; studentNum++)
                    {
                        webBrowser1.Navigate("https://petersburgedu.ru" + marksLinks[studentNum].Attributes["href"].Value);
                        while (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
                        {
                            Application.DoEvents();
                        }

                        // NAME ---------------------------------------------------------------------------------------
                        marksPage.LoadHtml(webBrowser1.DocumentText);
                        if (students.Count > 1)
                        {
                            nameNode = marksPage.DocumentNode.SelectSingleNode("//div[@class=\"title\"]");
                            user.multipleStudent = true; // Несколько учащихся в аккаунте
                        }
                        else
                        {
                            nameNode = marksPage.DocumentNode.SelectSingleNode("//h2[@class=\"student-title\"]");
                            user.multipleStudent = false;
                        }
                        if (nameNode != null)
                        {
                            string t = nameNode.InnerText.ToString().Trim();
                            if (user.multipleStudent)
                                name = t;
                            else
                                name = t.Substring(0, t.LastIndexOf(',')) + " " + t.Split(',')[2];
                        }
                        // GRADES ---------------------------------------------------------------------------------------
                        lessonNamesCollection = marksPage.DocumentNode.SelectNodes("//div[@class=\"lessons-list\"]/div[@class=\"cell\"]");
                        finalGradesCollection = marksPage.DocumentNode.SelectNodes("//table[@class=\"marks-summary\"]/tbody/tr/td[1]");
                        gapesCollection = marksPage.DocumentNode.SelectNodes("//table[@class=\"marks-summary\"]/tbody/tr/td[2]");
                        if (lessonNamesCollection != null)
                        {
                            fullData = "";
                            for (int i = 0; i < lessonNamesCollection.Count; i++)
                            {
                                gradesCollection = marksPage.DocumentNode.SelectNodes("//table[@class=\"marks-table\"]/tbody/tr/td/div[" + (i + 1).ToString() + "]/span[@class=\"grade\"]");
                                // Lesson name and Final Grade output
                                float average = 0f, sum = 0f;
                                string gradesList = "";
                                if (gradesCollection != null)
                                {
                                    for (int j = 0; j < gradesCollection.Count; j++)
                                    {
                                        // Normal grades parsing
                                        if (gradesCollection[j].InnerText.Length < 2)
                                        {
                                            sum += Byte.Parse(gradesCollection[j].InnerText);
                                            gradesList += gradesCollection[j].InnerText.ToString().Trim();
                                        }
                                    }
                                    average = sum / gradesCollection.Count;
                                }
                                string gapeStr = (showGapes) ? " [" + gapesCollection[i].InnerText.ToString().Trim() + "]\n" : "\n";
                                string averageStr = (gradesCollection != null) ? "   [Средняя: " + average + "]\n" : "   Оценок нет";
                                string finalStr = (finalGradesCollection[i] != null && finalGradesCollection[i].InnerText.ToString().Trim() != "") ? "     [Итоговая: " + finalGradesCollection[i].InnerText.ToString().Trim() + "]\n" : "";
                                fullData += lessonNamesCollection[i].InnerText.ToString().Trim() + gapeStr + averageStr + finalStr + "   " + gradesList + "\n";
                            }
                        }


                        // TEACHERS ---------------------------------------------------------------------------------------
                        if (showTeachers)
                        {
                            teachersListHeaders = teachersListCollection[studentNum].SelectNodes("//div/div/div/div/h2");
                            lessonsList = teachersListCollection[studentNum].SelectNodes("//tr/td[@class=\"lesson\"]/span");
                            teachersList = teachersListCollection[studentNum].SelectNodes("//tr/td[2]/span[@class=\"container\"]/span");
                            if (lessonsList != null && teachersList != null && teachersListHeaders != null)
                            {
                                teachers = "";
                                do
                                {
                                    if (lessonsList[lessonCounter].InnerText.ToString() != "&nbsp" && lessonsList[lessonCounter].InnerText.ToString() != " ")
                                        teachers += lessonsList[lessonCounter].InnerText.ToString().Trim() + " — " + teachersList[lessonCounter].InnerText.ToString().Trim() + "\n";

                                    if (lessonCounter < Math.Min(lessonsList.Count, teachersList.Count) - 1)
                                        lessonCounter++;

                                    if (lessonsList[lessonCounter].InnerText.ToString().Trim() == "Классный руководитель")
                                        break;

                                } while (lessonCounter < Math.Min(lessonsList.Count, teachersList.Count) - 1);
                            }
                        } else
                        {
                            teachers = "";
                        }
                        if (nameNode != null)
                        {
                            userNames.Add(name);        // Name
                            userGrades.Add(fullData);   // Grades
                            userTeachers.Add(teachers); // Teachers
                        }
                    }
                    user.Names  = userNames;
                    user.Grades = userGrades;
                    user.Teachers = userTeachers;
                }
                else
                {
                    if (!user.serverAccessesability)
                    {
                        // на ремонте
                    } else
                    {
                        // хз почему
                    }
                    // Обработка ошибки, когда не удается получить данные учащихся на этапе загрузки кабинета
                }
                // Возвращаем стандартное значение
                myRegKey.SetValue("Display Inline Images", "yes");
            }
        }
    }
}
