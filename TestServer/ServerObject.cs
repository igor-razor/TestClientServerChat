using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace TestChatServer
{
    public class ServerObject
    {
        private const int PORT = 8888;
        static TcpListener tcpListener; // сервер для прослушивания
        List<ClientObject> clients = new List<ClientObject>(); // все подключения
        List<string> clientsNames = new List<string>();// имена подключенных

        //struct Tclients { ClientObject client; string clientName;}
        //List<Tclients> Lclients = new List<Tclients>();

        protected internal void AddConnection(ClientObject clientObject) { clients.Add(clientObject); }

        protected internal void RemoveConnection(string id)
        {
            // получаем по id закрытое подключение
            ClientObject client = clients.FirstOrDefault(c => c.Id == id);
            // и удаляем его из списка подключений
            if (client != null)
                clients.Remove(client);
        }

        // прослушивание входящих подключений
        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, PORT);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        // РЕтрансляция сообщения ВСЕМ подключенным клиентам (кроме Id)
        protected internal void BroadcastMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);

            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id != id) // если id клиента не равно id отправляющего
                {
                    clients[i].Stream.Write(data, 0, data.Length); //передача данных
                }
            }
        }

        // трансляция сообщения ОДНОМУ клиенту (Id)
        protected internal void PersonalMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            clients[clients.FindIndex(x => x.Id == id)].Stream.Write(data, 0, data.Length);
        }

        // отключение всех клиентов
        protected internal void Disconnect()
        {
            tcpListener.Stop(); //остановка сервера

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); //отключение клиента
            }
            Environment.Exit(0); //завершение процесса
        }

        protected internal void AddClientName(string nameUser) { clientsNames.Add(nameUser); }

        protected internal void RemoveClientName(string nameUser) { clientsNames.Remove(nameUser); }

        protected internal string GetClientNames()
        {
            string str = "Список клиентов:\n";

            foreach (string s in clientsNames)
            { str += s + "\n"; }

            return str;
        }

        protected internal string GetAllQuestions()
        {
            string str = "Список запросов серверу:\n" + sGetQuestions + "\n" + sGetClients + "\n" + sGetTime + "\n" + sExitMessage + "\n";

            foreach (string s in ServerQ)
            { str += s + "\n"; }

            return str;
        }

        protected internal string GetTime()
        {
            string sHour = Convert.ToString(DateTime.Now.Hour);
            if (sHour.Length == 1) { sHour = "0" + sHour; }
            string sMin = Convert.ToString(DateTime.Now.Minute);
            if (sMin.Length == 1) { sMin = "0" + sMin; }

            return sHour + ":" + sMin;
        }


        protected internal string serverName = "Сервер";
        protected internal string EmptyAnswer = "";
        protected internal string sGetTime = "время";
        protected internal string sExitMessage = "пока";
        protected internal string sExitAnswer = "давайдосвиданиянеподберешьтакуюстроку";
        protected internal string sGetClients = "клиенты";
        protected internal string sGetQuestions = "вопросы";
        protected internal string[] ServerQ = { "как дела?", "ты кто?", "кто здесь?", "как пройти в библиотеку?", "высота Эвереста" };
        protected internal string[] ServerA = { "норм", "чат-сервер", "я, конечно", "прямо, направо, а потом налево", "8848 метров" };

    }
}