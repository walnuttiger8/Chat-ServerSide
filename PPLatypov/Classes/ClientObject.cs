﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace PPLatypov.Classes
{
     public class ClientObject
        {
            public static Form1 Form1;
            protected internal string Id { get; set; }
            protected internal NetworkStream Stream { get; set; }
            string userName;
            TcpClient client;
            ServerObject server;
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
                    Stream = client.GetStream();// получаем имя пользователя
                    string message = GetMessage();
                    userName = message;
                    message = userName + " вошел в чат";// посылаем сообщение о входе в чат всем подключенным пользователям
                    server.BroadcastMessage(message, this.Id);
                    Form1.richTextBoxChat.Invoke(new Action(() => Form1.richTextBoxChat.Text += message + '\n'));// в бесконечном цикле получаем сообщения от клиента
                    while (true)
                    {
                        try
                        {
                            message = GetMessage();
                            message = String.Format("{0}: {1}", userName, message);
                            Form1.richTextBoxChat.Invoke(new Action(() => Form1.richTextBoxChat.Text += message + '\n'));
                            server.BroadcastMessage(message, this.Id);
                        }
                        catch
                        {
                            message = String.Format("{0}: покинул чат", userName);
                            Form1.richTextBoxChat.Invoke(new Action(() => Form1.richTextBoxChat.Text += message + '\n'));
                            server.BroadcastMessage(message, this.Id);
                            break;
                        }
                    }

                }
                catch (Exception e)
                {
                    Form1.richTextBoxChat.Invoke(new Action(() => Form1.richTextBoxChat.Text += e.Message + '\n'));
                }
                finally
                {
                    //в случае выхода из цикла закрываем ресурсы
                    server.RemoveConnection(this.Id);
                    Close();
                }
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

        }
}
