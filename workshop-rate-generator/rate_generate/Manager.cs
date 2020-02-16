using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace rate_generate
{
    /// <summary>
    /// Manager run the program main logic.
    /// Contain the Random for all the rates classes.
    /// </summary>
    class Manager
    {
        private Dictionary<string, Rates> rateCollection;
        private int RateInterval;
        private static System.Timers.Timer timer;
        private readonly IConfiguration _config;
        private ILogger<Manager> _logger;
        private string QueueName;
        private string ClientID;
        private string CMDChannel;
        private bool AutoStart;
        private KubeMQ.SDK.csharp.Queue.Queue queue;
        private string KubemqAddress;
        internal static Random rnd;
        private CancellationTokenSource source;
        private CancellationToken token;

        public Manager(IConfiguration configuration, ILogger<Manager> logger)
        {
            _logger = logger;
            _config = configuration;
            rnd = new Random();
            source = new CancellationTokenSource();
            token = source.Token;
            QueueName = GetKubemqQueue();
            RateInterval = GetRateInterval();
            ClientID = GetKubemqClient();
            KubemqAddress = GetKubemqAddress();
            CMDChannel = GetCMDChannelName();
            AutoStart = GetAutoStart();


            Task.Run(() =>
            {
                SubscribeToRequests();
            });
            try
            {
                queue = new KubeMQ.SDK.csharp.Queue.Queue(QueueName, ClientID, KubemqAddress);
                KubeMQ.Grpc.PingResult pingResult=  queue.Ping();
                queue.AckAllQueueMessagesResponse();
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"failed to connect to kubemq on err ${ex.Message}");
                source.Cancel(false);
                System.Environment.Exit(1);

            }
            startSendingRates();
            _logger.LogInformation($"initialized Manger sending rates to channel: {QueueName}");


        }
        /// <summary>
        /// Start sending rate to MSMQ under selected path
        /// </summary>
        private void startSendingRates()
        {
            rateCollection = new Dictionary<string, Rates>
            {
                {"EURUSD", new Rates("EURUSD",1,AutoStart) },
                {"AUDUSD",new Rates("AUDUSD",2,AutoStart) },
                {"USDJPY", new Rates("USDJPY",3,AutoStart) },
                {"CHFJPY", new Rates("CHFJPY",4,AutoStart) },
                {"NZDUSD", new Rates("NZDUSD",5,AutoStart) },
                {"GBPJPY", new Rates("GBPJPY",6,AutoStart) },
                {"USDILS",new Rates("USDILS",7,AutoStart) }
            };
            _logger.LogInformation($"Starting to generate rates");
            SetRateTimer();
        }


        #region configuration load-up

        private string GetKubemqQueue()
        {

            // get interval from appsettings.json
            string channel_name = Convert.ToString(_config["KubemqQueue"]);


            if (string.IsNullOrEmpty(channel_name))
            {

                _logger.LogError("failed to set 'KubemqQueue'");
                throw new Exception("failed to set 'KubemqQueue'");
            }
            _logger.LogDebug("'KubemqQueue' was set to {0}", channel_name);

            return channel_name;
        }
        private string GetKubemqAddress()
        {

            // get interval from appsettings.json
            string kubemq_address = Convert.ToString(_config["KubemqAddress"]);


            if (string.IsNullOrEmpty(kubemq_address))
            {

                _logger.LogError("failed to set 'KubemqAddress'");
                throw new Exception("failed to set 'KubemqAddress'");
            }
            _logger.LogDebug("'KubemqAddress' was set to {0}", kubemq_address);

            return kubemq_address;
        }

        private string GetKubemqClient()
        {

            // get interval from appsettings.json
            string client_name = Convert.ToString(_config["KubemqClient"]);


            if (string.IsNullOrEmpty(client_name))
            {

                _logger.LogError("failed to set 'KubemqClient'");
                throw new Exception("failed to set 'KubemqClient'");
            }
            _logger.LogDebug("'KubemqClient' was set to {0}", client_name);

            return client_name;
        }

        private string GetCMDChannelName()
        {

            // get interval from appsettings.json
            string cmd_name = Convert.ToString(_config["CMDChannel"]);


            if (string.IsNullOrEmpty(cmd_name))
            {

                _logger.LogError("failed to set 'CMDChannel'");
                throw new Exception("failed to set 'CMDChannel'");
            }
            _logger.LogDebug("'CMDChannel' was set to {0}", cmd_name);

            return cmd_name;
        }

        private bool GetAutoStart()
        {

            // get interval from appsettings.json
            string AutoStart = Convert.ToString(_config["AutoStart"]);
            bool start;

            if (!bool.TryParse(AutoStart, out start))
            {

                _logger.LogError("failed to set 'AutoStart setting auto start to true'");
            }

            _logger.LogDebug("'AutoStart' was set to {0}", AutoStart);

            return start;
        }

        private int GetRateInterval()
        {

            // get interval from appsettings.json
            string rateQu = Convert.ToString(_config["RateInterval"]);
            int.TryParse(rateQu, out int parsedInterval);

            if (parsedInterval < 0)
            {
                _logger.LogError("failed to set 'RateInterval' to value of {0}, setting to defualt of 3000", rateQu.ToString());
                parsedInterval = 3000;
            }
            _logger.LogDebug("'RateInterval' was set to{0}", rateQu.ToString());


            return parsedInterval;
        }
        #endregion



        /// <summary>
        /// Set the rate timer.
        /// </summary>
        private void SetRateTimer()
        {

            timer = new System.Timers.Timer(RateInterval);

            timer.Elapsed += OnRateSend;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        /// <summary>
        /// Create and send the active rate report.
        /// </summary>
        private void OnRateSend(Object source, ElapsedEventArgs e)
        {
            List<SenderMessageBody> ratesList = new List<SenderMessageBody>();
            foreach (var rate in rateCollection)
            {
                SenderMessageBody message = new SenderMessageBody()
                {
                    ID = rate.Value.id,
                    Name = rate.Value.rateName,
                };
                if (rate.Value.isActive)
                {
                    message.Ask = rate.Value.buy.ToString();
                    message.Bid = rate.Value.sell.ToString();
                }
                else
                {
                    message.Ask = null;
                    message.Bid = null;
                }
                ratesList.Add(message);
            }
            try
            {
                var res = queue.SendQueueMessage(new KubeMQ.SDK.csharp.Queue.Message
                {
                    Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ratesList)),
                    Metadata = "Rate message json encoded in UTF8"
                });
                if (res.IsError)
                {
                    _logger.LogInformation($"message enqueue error, error:{res.Error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        private void SubscribeToRequests()
        {
            /// Init a new CommandQuery subscriber on the KubeMQ to receive commands
            KubeMQ.SDK.csharp.CommandQuery.Responder responder = new KubeMQ.SDK.csharp.CommandQuery.Responder(KubemqAddress);

            Console.WriteLine($"init KubeMQ CommandQuery subscriber :{CMDChannel}");

            responder.SubscribeToRequests(new KubeMQ.SDK.csharp.Subscription.SubscribeRequest()
            {
                SubscribeType = KubeMQ.SDK.csharp.Subscription.SubscribeType.Commands,
                Channel = CMDChannel,
                ClientID = ClientID

            }, (KubeMQ.SDK.csharp.CommandQuery.RequestReceive request) =>
            {
                Console.WriteLine($"CommandQuery RequestReceive :{request}");
                KubeMQ.SDK.csharp.CommandQuery.Response response = null;

                string strMsg = string.Empty;
                object body = System.Text.Encoding.Default.GetString(request.Body);
                RateRequest req = new RateRequest();
                if (request.Metadata== "some_metadata2")
                {
                    try
                    {

                        req = JsonConvert.DeserializeObject<RateRequest>(Encoding.UTF8.GetString(request.Body));
                        rateCollection[req.Name].isActive = req.Active;
                        queue.AckAllQueueMessagesResponse();
                        response = new KubeMQ.SDK.csharp.CommandQuery.Response(request)
                        {
                            Body = Encoding.UTF8.GetBytes("o.k"),
                            Error = "None",
                            ClientID = ClientID,
                            Executed = true,
                            Metadata = "OK",
                            Timestamp = DateTime.UtcNow,
                        };
                        _logger.LogInformation($"Returned response about instrument ${req.Name} to ${req.Active}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to parse request on err :{ex.Message}");
                        response = new KubeMQ.SDK.csharp.CommandQuery.Response(request)
                        {
                            Body = Encoding.UTF8.GetBytes("o.k"),
                            Error = ex.Message,
                            ClientID = ClientID,
                            Executed = true,
                            Metadata = "Failed",
                            Timestamp = DateTime.UtcNow,
                        };
                    }
                }
                else
                {
                    foreach (var rate in this.rateCollection)
                    {
                        rate.Value.isActive = true;
                        _logger.LogInformation($"Started rate on ${rate.Key}");
                    }
                    response = new KubeMQ.SDK.csharp.CommandQuery.Response(request)
                    {
                        Body = Encoding.UTF8.GetBytes("started"),
                        Error = "None",
                        ClientID = ClientID,
                        Executed = true,
                        Metadata = "OK",
                        Timestamp = DateTime.UtcNow,
                    };
                }
               
                return response;
            },null,token);

            return;
        }

    }
    /// <summary>
    /// Rate sending to the Client
    /// </summary>
    [Serializable]
    class SenderMessageBody
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Ask { get; set; }
        public string Bid { get; set; }
    }

    /// <summary>
    /// Request Receiving from the Client
    /// </summary>
    [Serializable]
    public class RateRequest
    {
        public string Name { get; set; }
        public bool Active { get; set; }
    }

}
