using System;
using System.Net.Sockets;
using System.Text;

namespace TestChatServer
{
    public class ClientObject
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        string userName;
        TcpClient client;
        ServerObject server; // объект сервера

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        public void Process()
        {
            try
            {
                Stream = client.GetStream();
                // получаем имя пользователя
                string message = GetMessage();
                userName = message;
                server.AddClientName(userName);
                message = userName + " вошел в чат";
                // посылаем сообщение о входе в чат всем подключенным пользователям
                server.BroadcastMessage(message, this.Id);
                Console.WriteLine(message);

                // стартовая информация для нового клиента
                server.PersonalMessage(server.GetAllQuestions(), this.Id);

                // в бесконечном цикле получаем сообщения от клиента
                while (true)
                {
                    try
                    {
                        message = GetMessage();

                        if (!ServerAnswer(message))
                        {
                            break;
                        }

                    }
                    catch
                    {
                        message = String.Format("{0}: покинул чат", userName);
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                // в случае выхода из цикла закрываем ресурсы
                server.RemoveClientName(userName);
                server.RemoveConnection(this.Id);
                Close();
            }
        }

        // обработка запросов клиента
        private bool ServerAnswer(string message)
        {
            if (message == server.sExitMessage)
            {
                message = String.Format("{0}: покинул чат", userName);
                Console.WriteLine(message);
                server.BroadcastMessage(message, this.Id);
                server.PersonalMessage(server.sExitAnswer, this.Id);
                server.RemoveClientName(userName);
                server.RemoveConnection(this.Id);
                Close();
                return false;
                //break;
            }

            string answer = PersonalAnswer(message);

            if (answer == server.EmptyAnswer)
            {
                answer = String.Format("{0}: {1}", userName, message);
                server.BroadcastMessage(answer, this.Id); // ретрансляция сообщения остальным клиентам
            }
            else
            {
                answer = String.Format("{0}: {1}", server.serverName, answer);
                Console.WriteLine("{0}: {1}", userName, message);
                server.PersonalMessage(answer, this.Id); // трансляция персонального ответа
            }

            Console.WriteLine(answer);
            return true;
        }

        // чтение входящего сообщения и преобразование в строку
        private string GetMessage()
        {
            byte[] data = new byte[64]; // буфер для получаемых данных
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }

        // закрытие подключения
        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }


        private string PersonalAnswer(string question)
        {
            string answer = server.EmptyAnswer;

            try
            {
                answer = server.ServerA[Array.IndexOf(server.ServerQ, question)];
            }
            catch
            {
                if (question == server.sGetTime) { answer = server.GetTime(); }
                if (question == server.sGetClients) { answer = server.GetClientNames(); }
                if (question == server.sGetQuestions) { answer = server.GetAllQuestions(); }
            }

            return answer;
        }

    }
}