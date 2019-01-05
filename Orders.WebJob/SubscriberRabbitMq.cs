using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Orders.WebJob.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Orders.WebJob
{
public class SubscriberRabbitMq
{
        private readonly string _rabbitMqUri;

        public SubscriberRabbitMq(string rabbitMqUri)
        {
            _rabbitMqUri = rabbitMqUri;
        }

        public void SubscribeAndProcessOrdersFromRabbitMq()
        {
            System.Console.WriteLine("SubscribeAndProcessOrdersFromRabbitMq..");
            System.Console.WriteLine(Environment.GetEnvironmentVariable("RabbitMqService"));
            
            var factory = new ConnectionFactory
            {
                UserName = "guest",
                Password = "guest",
                HostName = Environment.GetEnvironmentVariable("RabbitMqService"),
                VirtualHost = "/",
                //Uri = new Uri(_rabbitMqUri)
            };

            System.Console.WriteLine(factory.Uri);

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            
            channel.QueueDeclare(queue: "orders",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine("Message received {0}", message);

                var order = JsonConvert.DeserializeObject<Order>(message);
            };
            
            channel.BasicConsume(queue: "orders",
                                 autoAck: true,
                                 consumer: consumer);
        }
    }
}