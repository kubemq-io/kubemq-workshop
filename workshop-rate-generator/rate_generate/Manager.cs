using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace rate_generate
{
    /// <summary>
    /// Manager run the program main logic.
    /// Contain the Random for all the rates classes.
    /// </summary>
    class Manager
    {
        public static Random rnd;
        private Dictionary<string, Rates> rateCollection;
        private int RateInterval;
        private static System.Timers.Timer timer;
        private readonly IConfiguration _config;
        private ILogger<Manager> _logger;
        private string QueueName;
        private string ClientID;
        private KubeMQ.SDK.csharp.Queue.Queue queue;
        private string KubemqAddress;

        public Manager(IConfiguration configuration, ILogger<Manager> logger)
        {
            _logger = logger;
            _config = configuration;
            rnd = new Random();

            QueueName = GetKubemqQueue();
            RateInterval = GetRateInterval();
            ClientID = GetKubemqClient();
            KubemqAddress = GetKubemqAddress();
            try
            {
                queue = new KubeMQ.SDK.csharp.Queue.Queue(QueueName, ClientID, KubemqAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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
                {"EURUSD", new Rates("EURUSD",1) },
                {"AUDUSD",new Rates("AUDUSD",2) },
                {"USDJPY", new Rates("USDJPY",3) },
                {"CHFJPY", new Rates("CHFJPY",4) },
                {"NZDUSD", new Rates("NZDUSD",5) },
                {"GBPJPY", new Rates("GBPJPY",6) },
                {"USDILS",new Rates("USDILS",7) }
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

            timer = new Timer(RateInterval);

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
                else
                {
                    _logger.LogInformation($"message sent at, {res.SentAt}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

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

}
