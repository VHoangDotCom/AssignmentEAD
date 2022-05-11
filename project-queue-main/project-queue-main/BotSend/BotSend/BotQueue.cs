using HtmlAgilityPack;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSend
{
   public class BotQueue
    {
        public void SendQueue()
        {
            List<string> ListArticleLink = new List<string>();
            String sql = "SELECT * FROM Sources";
            String sqlar = "SELECT UrlSource FROM [Articles]";
            List<Sourse> sources = new List<Sourse>();

            using (SqlConnection cnn = ConnectDB.GetConnectSql())
            {
                try
                {
                    cnn.Open();
                    Console.WriteLine("connect success");
                    using (SqlCommand command = new SqlCommand(sql, cnn))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Sourse sourse = new Sourse()

                                {
                                    Id = reader.GetInt32(0),
                                    Url = reader.GetString(1),
                                    Tag = reader.GetString(2),
                                    SelectorSubUrl = reader.GetString(3),
                                    SubUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
                                    SelectorTitle = reader.GetString(5),
                                    SelectorImage = reader.GetString(6),
                                    SelectorDescription = reader.IsDBNull(7) ? null : reader.GetString(7),
                                    SelectorContent = reader.GetString(8),
                                };
                                sources.Add(sourse);
                            }
                        }
                        using (SqlCommand commandline = new SqlCommand(sqlar, cnn))
                        {
                            using (SqlDataReader reader = commandline.ExecuteReader())
                            {

                                while (reader.Read())
                                {
                                    string articlelink = reader.GetString(0);
                                    ListArticleLink.Add(articlelink);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
          
            foreach (var sourse in sources)
            {

                HashSet<string> ListString = new HashSet<string>();
                HashSet<Sourse> ListSubSource = new HashSet<Sourse>();
                var url = sourse.Url;
                
                var web = new HtmlWeb();
                HtmlDocument doc = web.Load(url);
                var nodeList = doc.QuerySelectorAll(sourse.SelectorSubUrl);

                foreach (var node in nodeList)
                {
                    if (node.GetAttributeValue("href", null) == "/")
                    {

                    }
                    else if (node.GetAttributeValue("href", null) == null)
                    {

                    }
                    else
                    {
                        ListString.Add(node.GetAttributeValue("href", null));
                        
                    }
                }
                var result = ListString.Except(ListArticleLink).ToArray();
                for (int i = 0; i < result.Length; i++)
                {
                    var re = result[i];

                    if (i > 0)
                    {

                        var rebe = result[i - 1];
                        if (!re.Contains(rebe))
                        {
                            Sourse subsourse = new Sourse()
                            {
                                Id = sourse.Id,
                                SelectorSubUrl = sourse.SelectorSubUrl,
                                SubUrl = re,
                                SelectorDescription = sourse.SelectorDescription,
                                SelectorTitle = sourse.SelectorTitle,
                                SelectorImage = sourse.SelectorImage,
                                SelectorContent = sourse.SelectorContent
                            };
                            Console.WriteLine(re);
                            var factory = new ConnectionFactory() { HostName = "localhost" };
                            using (var connection = factory.CreateConnection())
                            using (var channel = connection.CreateModel())
                            {
                                channel.QueueDeclare(queue: "SubSource",
                                                    durable: false,
                                                    exclusive: false,
                                                    autoDelete: false,
                                                    arguments: null);
                                var yourObject = JsonConvert.SerializeObject(subsourse);
                                var body = Encoding.UTF8.GetBytes(yourObject);
                                channel.BasicPublish(exchange: "",
                                                    routingKey: "SubSource",
                                                    basicProperties: null,
                                                    body: body);
                            }
                        }
                    }
                    else
                    {
                        Sourse subsourse = new Sourse()
                        {
                            Id = sourse.Id,
                            SelectorSubUrl = sourse.SelectorSubUrl,
                            SubUrl = re,
                            SelectorDescription = sourse.SelectorDescription,
                            SelectorTitle = sourse.SelectorTitle,
                            SelectorImage = sourse.SelectorImage,
                            SelectorContent = sourse.SelectorContent
                        };
                        Console.WriteLine(re);
                        var factory = new ConnectionFactory() { HostName = "localhost" };
                        using (var connection = factory.CreateConnection())
                        using (var channel = connection.CreateModel())
                        {
                            channel.QueueDeclare(queue: "SubSource",
                                                durable: false,
                                                exclusive: false,
                                                autoDelete: false,
                                                arguments: null);
                            var yourObject = JsonConvert.SerializeObject(subsourse);
                            var body = Encoding.UTF8.GetBytes(yourObject);
                            channel.BasicPublish(exchange: "",
                                                routingKey: "SubSource",
                                                basicProperties: null,
                                                body: body);
                        }
                    }
                }
            }
            Console.WriteLine(" Press [enter] to exit.");

        }
    }
}
