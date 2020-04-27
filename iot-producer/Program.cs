using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Amazon.IotData;
using Amazon.IotData.Model;
using Newtonsoft.Json;

namespace iot_producer
{
    class Program
    {
        static void Main(string[] args)
        {
            var iotEndpoint = Environment.GetEnvironmentVariable("IOT_ENDPOINT") ?? "a2cc07h411h71j.iot.us-west-2.amazonaws.com";
            var topic = Environment.GetEnvironmentVariable("IOT_TOPIC") ?? "topic/AWSMeetupGroup_DataServicesDemo-sensor-ingest-topic";
            // var brokerPort = 443;//8883;
            
            
            var msg = new SensorMessage { Type = "HMM", Value = 12.34, Timestamp = System.DateTime.UtcNow};
            var msgJson =  JsonConvert.SerializeObject(msg);
            Console.WriteLine(msgJson);

            var caCert = new X509Certificate("./secure/amzn.crt");
            var clientCert = new X509Certificate2("./secure/db6db19cdc.pfx", "MyPassword1");
           
            Action<PublishResponse> a = delegate(PublishResponse r) {
                Console.WriteLine(r);
            };
           
            using(var client = new Amazon.IotData.AmazonIotDataClient($"https://{iotEndpoint}")) {
                var req = new PublishRequest{
                    Topic = "topic/AWSMeetupGroup_DataServicesDemo-sensor-ingest-topic",
                    Payload = new MemoryStream(Encoding.UTF8.GetBytes(msgJson)),
                    
                    
                };
                var response = client.PublishAsync(req).Result;
                Console.WriteLine(response.HttpStatusCode);
                
            }
            
            // var client = new AmazonIoTClient(iotEndpoint, brokerPort, true, caCert, clientCert, MqttSslProtocols.TLSv1_2);
            // client.ProtocolVersion = MqttProtocolVersion.Version_3_1_1;

            // var clientId = $"AWSMeetupGroup_SensorProducer_{Guid.NewGuid().ToString()}";

            // client.Connect(clientId);
            
            // Console.WriteLine($"Connected to AWS IOT with client ID ${clientId}");

            // client.Publish(topic, Encoding.UTF8.GetBytes(msgJson));

            Console.WriteLine($"Wrote ${msgJson} to ${topic}");
        }


      
    }

    class SensorMessage {
       public string Type {get; set;}
        public double Value {get;set;}
        public DateTime Timestamp {get;set;}

    }
}