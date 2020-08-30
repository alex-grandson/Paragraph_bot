using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using Paragraph_Core;
using System.Text.RegularExpressions;

namespace revcom_bot
{
    public partial class Form1 : Form
    {
        SqlConnection sqlConnection;
        BackgroundWorker bw;
        bool botFree = true;
        string botBusyMessage = "Я пока немного занят. Заходи чуть позже";
        public Form1()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();

            //
            // TODO: Add constructor code after the InitializeComponent() call.
            //

            this.bw = new BackgroundWorker();
            this.bw.DoWork += bw_DoWork;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Admin\source\repos\revcom_bot\revcom_bot\Data.mdf;Integrated Security=True";
            sqlConnection = new SqlConnection(connectionString);
            await sqlConnection.OpenAsync();
            SqlDataReader sqlReader = null;
            SqlCommand command = new SqlCommand("SELECT * FROM [Data]", sqlConnection);
            try
            {
                sqlReader = await command.ExecuteReaderAsync();
                while (await sqlReader.ReadAsync())
                {
                    listBox1.Items.Add(Convert.ToString(sqlReader["Id"]) + "\t" + Convert.ToString(sqlReader["ChatId"]) + "\t" + EmailFormatter(Convert.ToString(sqlReader["Login"])) + "\t" + NameFormatter(Convert.ToString(sqlReader["Name"])) + "\t" + Convert.ToString(sqlReader["Views"]));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), ex.Source.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (sqlReader != null)
                    sqlReader.Close();
            }
        }

        private async void refreshBtn_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            SqlDataReader sqlReader = null;
            SqlCommand command = new SqlCommand("SELECT * FROM [Data]", sqlConnection);
            try
            {
                sqlReader = await command.ExecuteReaderAsync();
                while (await sqlReader.ReadAsync())
                {
                    listBox1.Items.Add(Convert.ToString(sqlReader["Id"]) + "\t" + Convert.ToString(sqlReader["ChatId"]) + "\t" + EmailFormatter(Convert.ToString(sqlReader["Login"])) + "\t" + NameFormatter(Convert.ToString(sqlReader["Name"])) + "\t" + Convert.ToString(sqlReader["Views"]));
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message.ToString(), ex.Source.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (sqlReader != null)
                    sqlReader.Close();
            }
        }

        private void closeDB_Click(object sender, EventArgs e)
        {
            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                sqlConnection.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                sqlConnection.Close();
        }

        async void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var key = e.Argument as String; // получаем ключ из аргументов
            try
            {
                var Bot = new Telegram.Bot.TelegramBotClient(key); // инициализируем API
                await Bot.SetWebhookAsync(""); // Обязательно! убираем старую привязку к вебхуку для бота

                // Callback'и от кнопок
                Bot.OnCallbackQuery += async (object sc, Telegram.Bot.Args.CallbackQueryEventArgs ev) =>
                {
                    var message = ev.CallbackQuery.Message;
                    if (ev.CallbackQuery.Data == "instruction")
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id,
                            "Инструкция по эксплуатации:" +
                            "\n\n1.	Будьте терпеливы.\nУбедительная просьба не тыкать его палкой по крайней мере пока он работает. Он очень обидчивый." +
                            "\n\n2.	Убедитесь в том, что введенные данные правильны ( это можно сделать с помощью команды /check )" +
                            "\n\n3.	Воспользуйтесь командой /work для получения оценок. Терпеливо ждите окончания процесса (это не так много времени занимает)");
                        await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id); // отсылаем пустое, чтобы убрать "часики" на кнопке
                    }
                    else
                    if (ev.CallbackQuery.Data == "faq")
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id,
                            "FAQ:" +
                            "\n\n1.	Почему он так долго работает?\n\nОн добросовестно относится к своей работе и очень старается помочь Вам." +
                            "\n\n2.	Что делать, если не выводит оценки?\n\nДождаться завершения работы. По завершению работы бот уведомит об её успешном или не очень результате. При каждом обращении к нему, у бота есть 3 попытки получить доступ к данным электронного дневника, но получается это не всегда в силу непредсказуемых обстоятельств (например: Ростелеком)." +
                            "\n\n3.	Как поддержать разработчика?\n\nЭто не очень популярный вопрос, но всё же я на него отвечу :)" +
                            "\nПоддержать разработчика можно монетой, узнав реквизиты тыкнув на соответствующую кнопку. Сумма пожертвования никак не повлияет на работу бота, но разработчику будет очень приятно и может быть он прислушается к просьбам пользователей.");

                        await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id); // отсылаем пустое, чтобы убрать "часики" на кнопке
                    }
                    else
                    if (ev.CallbackQuery.Data == "support")
                    {
                        if (message.Chat.Id != 402256119)
                        {
                            // Log info
                            await Bot.SendTextMessageAsync(402256119, $"{DateTime.Now}:\nПользователь {message.Chat.FirstName} {message.Chat.LastName} нажал на кнопку поддержки");
                        }

                        await Bot.SendTextMessageAsync(message.Chat.Id,
                            "Поддержка:" +
                            "\nP.S. Автор ни в коем случае ни к чему не призывает и не обязывает." +
                            "\nМне правда будет очень приятно :з" +
                            "\n\nПоддержать \"программиста\" можно следующими способами:");

                        await Bot.SendTextMessageAsync(message.Chat.Id, "1.	Карта Сбербанка:");
                        await Bot.SendTextMessageAsync(message.Chat.Id, "5469550037901797");

                        await Bot.SendTextMessageAsync(message.Chat.Id, "2.	Яндекс кошелёк:");
                        await Bot.SendTextMessageAsync(message.Chat.Id, "410013502974226");

                        await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id); // отсылаем пустое, чтобы убрать "часики" на кнопке
                    }
                };

                Bot.OnUpdate += async (object su, Telegram.Bot.Args.UpdateEventArgs evu) =>
                {
                    if (evu.Update.CallbackQuery != null || evu.Update.InlineQuery != null) return; // в этом блоке нам келлбэки и инлайны не нужны
                    var update = evu.Update;
                    var message = update.Message;
                    if (message == null) return;
                    if (message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
                    {
                        if (message.Text == "/start")
                        {
                            bool newUser = true;
                            string[] data = new string[2];
                            SqlDataReader sqlReader = null;
                            SqlCommand command = new SqlCommand("SELECT ChatId, Login, Password FROM [Data]", sqlConnection);
                            try
                            {
                                sqlReader = await command.ExecuteReaderAsync();
                                while (await sqlReader.ReadAsync())
                                {
                                    if (Convert.ToString(sqlReader["ChatId"]) == Convert.ToString(message.Chat.Id))
                                    {
                                        newUser = false;
                                        data[0] = Convert.ToString(sqlReader["Login"]);
                                        data[1] = Convert.ToString(sqlReader["Password"]);
                                        break;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                await Bot.SendTextMessageAsync(402256119, $"{DateTime.Now}: Начальник, случилась оказия!\n\n{ex.Message.ToString()}\n\n{ex.Source.ToString()}");
                            }
                            finally
                            {
                                if (sqlReader != null)
                                    sqlReader.Close();
                            }
                            if (newUser)
                            {
                                await Bot.SendTextMessageAsync(message.Chat.Id, "Привет! Я бот Параграф. Давай дружить.\nМне нужно, чтобы ты сказал мне логин и пароль. Тогда я думаю смогу тебе помочь");
                                await Bot.SendTextMessageAsync(message.Chat.Id, "Введи их пожалуйста в одном сообщении через пробел");
                                SqlCommand commandInsert = new SqlCommand("INSERT INTO [Data] (ChatId)VALUES(@ChatId)", sqlConnection);
                                commandInsert.Parameters.AddWithValue("ChatId", Convert.ToString(message.Chat.Id));
                                await commandInsert.ExecuteNonQueryAsync();

                                // Log info
                                await Bot.SendTextMessageAsync(402256119, $"{DateTime.Now}: Новый пользователь {message.Chat.FirstName} {message.Chat.LastName}");
                            }
                            else
                            {
                                await Bot.SendTextMessageAsync(message.Chat.Id, $"Я тебя помню. Кажется ты уже пользовался моими услугами.\nПроверь пожалуйста свои данные:\n{data[0]}\n{data[1]}");
                            }
                        }
                        else
                        if (message.Text[0] != '/' && message.Text.Contains(" ") && message.Text.Split().Count() == 2)
                        {
                            if (botFree)
                            {
                                string[] data = message.Text.Split();

                                // Cheching correct format of e-mail & password
                                string emailPattern = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                                                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$";
                                Regex regexLogin = new Regex(emailPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                                string passwordPattern = @"^(?=.*[a-z])(?=.*[0-9])";
                                Regex regexPass = new Regex(passwordPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

                                if (!regexLogin.IsMatch(data[0]))
                                {
                                    await Bot.SendTextMessageAsync(message.Chat.Id, "Неправильный формат почты.\nПопробуй еще раз.");
                                }
                                else if (!regexPass.IsMatch(data[1]))
                                {
                                    await Bot.SendTextMessageAsync(message.Chat.Id, "Неправильный формат пароля.\nПопробуй еще раз.");
                                }
                                else
                                {
                                    SqlCommand command = new SqlCommand("UPDATE [Data] SET [Login]=@Login, [Password]=@Password, [Name]=@Name WHERE [ChatId]=@ChatId", sqlConnection);
                                    command.Parameters.AddWithValue("Login", data[0]);
                                    command.Parameters.AddWithValue("Password", data[1]);
                                    command.Parameters.AddWithValue("Name", message.Chat.FirstName + " " + message.Chat.LastName);
                                    command.Parameters.AddWithValue("ChatId", Convert.ToString(message.Chat.Id));
                                    await command.ExecuteNonQueryAsync();
                                    await Bot.SendTextMessageAsync(message.Chat.Id, "Хорошо, я запомнил.\nЕсли ты ввёл почту и пароль правильно, то всё будет хорошо");
                                    await Bot.SendTextMessageAsync(message.Chat.Id, "Теперь введи команду /work, чтобы посмотреть свои оценки");
                                }
                            }
                            else
                            {
                                await Bot.SendTextMessageAsync(message.Chat.Id, botBusyMessage);
                            }

                        }
                        else
                        if (message.Text == "/work")
                        {
                            if (botFree)
                            {
                                botFree = false;
                                int countOfViews = 0;
                                await Bot.SendTextMessageAsync(message.Chat.Id, "Работаю...");
                                string currentUserLogin = "";
                                string currentUserPassword = "";
                                SqlDataReader sqlReader = null;
                                SqlCommand command = new SqlCommand("SELECT Login, Password, Views FROM [Data] WHERE [ChatId]=@ChatId", sqlConnection);
                                command.Parameters.AddWithValue("ChatId", Convert.ToString(message.Chat.Id));
                                try
                                {
                                    sqlReader = await command.ExecuteReaderAsync();
                                    while (await sqlReader.ReadAsync())
                                    {
                                        currentUserLogin = Convert.ToString(sqlReader["Login"]);
                                        currentUserPassword = Convert.ToString(sqlReader["Password"]);
                                        countOfViews = Convert.ToInt32(Convert.ToString(sqlReader["Views"]));
                                    }
                                    await Bot.SendTextMessageAsync(message.Chat.Id, "1-ая ступень...");
                                }
                                catch (Exception ex)
                                {
                                    await Bot.SendTextMessageAsync(402256119, $"{DateTime.Now}: Начальник, случилась оказия!\n\n{ex.Message.ToString()}\n\n{ex.Source.ToString()}");
                                }
                                finally
                                {
                                    if (sqlReader != null)
                                        sqlReader.Close();
                                }
                                User user = new User(currentUserLogin, currentUserPassword);
                                await Bot.SendTextMessageAsync(message.Chat.Id, "2-ая ступень...");
                                List<string> data = await user.GetInfoAsync();
                                int t = 3;
                                while (data.Count < 1 && t <= 5)
                                {
                                    await Bot.SendTextMessageAsync(message.Chat.Id, $"{t}-ая ступень...");
                                    data = await user.GetInfoAsync();
                                    t++;
                                }
                                if (t <= 5)
                                {
                                    if (user.Names.Count == 1)
                                        if (user.Grades[0].Contains("Электив"))
                                            await Bot.SendTextMessageAsync(message.Chat.Id, user.Names[0] + "\n\n" + user.Grades[0].Substring(0, user.Grades[0].IndexOf("Электив")));
                                        else
                                            await Bot.SendTextMessageAsync(message.Chat.Id, user.Names[0] + "\n\n" + user.Grades[0]);
                                    else
                                    {
                                        for (int i = 0; i < user.Names.Count; i++)
                                        {
                                            if (user.Grades[i].Contains("Электив"))
                                                await Bot.SendTextMessageAsync(message.Chat.Id, user.Names[i] + "\n\n" + user.Grades[i].Substring(0, user.Grades[i].IndexOf("Электив")));
                                            else
                                                await Bot.SendTextMessageAsync(message.Chat.Id, user.Names[i] + "\n\n" + user.Grades[i]);
                                        }
                                    }
                                    // Update count of views
                                    try
                                    {
                                        SqlCommand commandLogInfo = new SqlCommand("UPDATE [Data] SET [Views]=@Views WHERE [ChatId]=@ChatId", sqlConnection);
                                        commandLogInfo.Parameters.AddWithValue("Views", Convert.ToString(countOfViews + 1));
                                        commandLogInfo.Parameters.AddWithValue("ChatId", Convert.ToString(message.Chat.Id));
                                        await commandLogInfo.ExecuteNonQueryAsync();
                                    }
                                    catch (Exception ex)
                                    {
                                        await Bot.SendTextMessageAsync(402256119, $"{DateTime.Now}: Начальник, случилась оказия!\n\n{ex.Message.ToString()}\n\n{ex.Source.ToString()}");
                                    }
                                    finally
                                    {
                                        // Log info
                                        if (message.Chat.Id != 402256119)
                                            await Bot.SendTextMessageAsync(402256119, $"{DateTime.Now}: Вход\nПользователь {message.Chat.FirstName} {message.Chat.LastName} ({countOfViews + 1})\n{currentUserLogin}");
                                    }
                                }
                                else await Bot.SendTextMessageAsync(message.Chat.Id, "Не получилось :(\nПопробуй ещё раз");
                                botFree = true;
                            }
                            else
                            {
                                await Bot.SendTextMessageAsync(message.Chat.Id, botBusyMessage);
                            }
                        }
                        else
                        if (message.Text == "/check")
                        {
                            if (botFree)
                            {
                                string[] data = new string[2];
                                SqlDataReader sqlReader = null;
                                SqlCommand command = new SqlCommand("SELECT Login, Password FROM [Data] WHERE [ChatId]=@ChatId", sqlConnection);
                                command.Parameters.AddWithValue("ChatId", Convert.ToString(message.Chat.Id));
                                try
                                {
                                    sqlReader = await command.ExecuteReaderAsync();
                                    while (await sqlReader.ReadAsync())
                                    {
                                        data[0] = Convert.ToString(sqlReader["Login"]);
                                        data[1] = Convert.ToString(sqlReader["Password"]);
                                    }
                                    await Bot.SendTextMessageAsync(message.Chat.Id, "Ты хотел проверить информацию.\nВот твои данные:");
                                    await Bot.SendTextMessageAsync(message.Chat.Id, data[0] + "\n" + data[1]);
                                    await Bot.SendTextMessageAsync(message.Chat.Id, "Если хочешь изменить информацию о себе, просто отправь мне логин и пароль через пробел.\nЯ пойму ;)");
                                }
                                catch (Exception ex)
                                {
                                    await Bot.SendTextMessageAsync(402256119, $"{DateTime.Now}: Начальник, случилась оказия!\n\n{ex.Message.ToString()}\n\n{ex.Source.ToString()}");
                                }
                                finally
                                {
                                    if (sqlReader != null)
                                        sqlReader.Close();
                                }
                            }
                            else
                            {
                                await Bot.SendTextMessageAsync(message.Chat.Id, botBusyMessage);
                            }
                        }
                        else
                        if (message.Text == "/help")
                        {
                            var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(
                                                  new Telegram.Bot.Types.InlineKeyboardButton[][]
                                                  {
                                                            // First row
                                                            new [] {
                                                                new Telegram.Bot.Types.InlineKeyboardButton("FAQ","faq"),

                                                            },
                                                            // Second row
                                                            new [] {
                                                                new Telegram.Bot.Types.InlineKeyboardButton("Инструкция","instruction"),
                                                            },
                                                            // Third row
                                                            new [] {
                                                                new Telegram.Bot.Types.InlineKeyboardButton("Поддержать разработчика","support"),
                                                            },
                                                  }
                                              );

                            await Bot.SendTextMessageAsync(message.Chat.Id, "Справка:", false, false, 0, keyboard, Telegram.Bot.Types.Enums.ParseMode.Default);
                        }
                        else
                        if (message.Text == "/revolution")
                        {
                            await Bot.SendPhotoAsync(message.Chat.Id, "http://aftamat4ik.ru/wp-content/uploads/2017/03/photo_2016-12-13_23-21-07.jpg", "Revolution!");
                        }
                        else
                        {
                            bool madeOfDigits = true;
                            string msg = message.Text.Trim();
                            int sum = 0;
                            for (int i=0; i<msg.Length; i++)
                            {
                                if (msg[i] >= '0' && msg[i] <= '9')
                                    sum += (msg[i] - '0');
                                else
                                {
                                    madeOfDigits = false;
                                    break;
                                }
                            }
                            if (madeOfDigits)
                            {
                                await Bot.SendTextMessageAsync(message.Chat.Id, $"Средний балл: {(float)sum/msg.Length}");
                            }
                        }
                    }
                };
                // запускаем прием обновлений
                Bot.StartReceiving();
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                Console.WriteLine(ex.Message); // если ключ не подошел - пишем об этом в консоль отладки
            }

        }

        private void BtnRun_Click(object sender, EventArgs e)
        {
            var text = @txtKey.Text; // получаем содержимое текстового поля txtKey в переменную text
            if (text != "" && this.bw.IsBusy != true)
            {
                this.bw.RunWorkerAsync(text); // передаем эту переменную в виде аргумента методу bw_DoWork
                BtnRun.Text = "Бот запущен...";
            }
        }
        // Sending message to all Paragraph users
        private async void button1_Click(object sender, EventArgs e)
        {
            if (messageTextBox.Text != "")
            {
                var Bot = new Telegram.Bot.TelegramBotClient(txtKey.Text);
                SqlDataReader sqlReader = null;
                SqlCommand command = new SqlCommand("SELECT * FROM [Data]", sqlConnection);
                try
                {
                    sqlReader = await command.ExecuteReaderAsync();
                    while (await sqlReader.ReadAsync())
                    {
                        try
                        {
                            if (sqlReader["ChatId"] != null && Convert.ToInt64(sqlReader["ChatId"]) != -1)
                            {
                                await Bot.SendTextMessageAsync(Convert.ToInt64(sqlReader["ChatId"]), messageTextBox.Text);
                            }
                        }
                        catch
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString(), ex.Source.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    if (sqlReader != null)
                        sqlReader.Close();
                }
                messageTextBox.Text = "";
            }
        }

        public string EmailFormatter(string e)
        {
            if (e.Length > 22)
            {
                e = e.Substring(0, 22) + "...";
            }
            if (e.Length < 23)
            {
                e += "\t";
            }
            return e;
        }
        public string NameFormatter(string e)
        {
            if (e.Length > 17)
            {
                e = e.Substring(0, 17) + "...";
            }
            if (e.Length < 18)
            {
                e += "\t";
            }
            return e;
        }
    }
}