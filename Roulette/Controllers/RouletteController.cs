using Roulette.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;
using System.Windows.Threading;

namespace Roulette.Controllers
{
    /// <summary>
    /// Gets data from port=4948 in json format={"Qualifier":"showWinningNumber","Data":{“WinningNumber":22}}
    /// Operates data and posts it to GUI(Graphic User Interface) Layer to MainWindow.xaml
    /// </summary>
    public class RouletteController
    {
        private const string QUALIFIER = "showWinningNumber";                   // Expected qualifier in json message
        private const int POST_DELAY = 11;                                      // Delay for posting new InputData from the previous one in seconds
        private const int INVALID_QUALIFIER = -99;                              // Error number -99. Sends if Qualifier is invalid
        private const int ERROR_INPUT_NOT_IN_RANGE = 99;                        // Error number 99. Sends if input data is not in range
        private const int PORT = 4948;                                          // Port, which is being listened

        private readonly IPAddress IP_ADDRESS = IPAddress.Parse("127.0.0.1");   // Ip Address, which i've been using during testing
        public event Action<InputData> dataGotten;                              // Event for waiting and showing data in GUI layer

        private Queue<InputData> _inputQueue;                                   // Queue with InputData which comes to TCP port
        private InputData _inputData;                                           // Last InputData in queue, which is sending to GUI
        private TcpListener _tcpListener;                                       // Port Listener (server)
        private DispatcherTimer _postingDataTimer;                              // Timer for sending data from BLL to GUI wlayer
        public string InputDataError { get; private set; }
        public RouletteController()
        {
            _tcpListener = new TcpListener(IP_ADDRESS, PORT);
            _tcpListener.Start();

            _inputQueue = new Queue<InputData>();

            _postingDataTimer = new DispatcherTimer();
            _postingDataTimer.Interval = TimeSpan.FromSeconds(POST_DELAY);
            _postingDataTimer.Tick += delegate{
                InputData inputData = PostInputData();

                if (inputData != null)
                    dataGotten.Invoke(inputData); 
            };
        }
        
        /// <summary>
        /// Asynchronously listens the port = 4948, if data comes, parsing it from bytes to json, from json, to InputData. Also check the qualifier, if it's invalid, Input Datra is null.
        /// </summary>
        public async void ListenPort()
        {
            _postingDataTimer.Start();
            await Task.Run(() =>
            {
                while (true)
                {
                    TcpClient client = _tcpListener.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();

                    byte[] data = new byte[256];
                    int bytes = stream.Read(data, 0, data.Length);
                    string jsonResponse = Encoding.UTF8.GetString(data, 0, bytes);

                    int winningNumber = ReturnWinningNumberFromJson(jsonResponse);
                    if (winningNumber != INVALID_QUALIFIER)
                    {
                        _inputData = new InputData(winningNumber);
                        if (IsDataInRange(_inputData?.InputValue ?? ERROR_INPUT_NOT_IN_RANGE))
                            _inputQueue.Enqueue(_inputData);
                    }
                    else
                    {
                        _inputData = null;
                    }

                    client.Close();
                }
            });
        }

        /// <summary>
        /// Method for returning InputData from queue to GUI layer. Checks if data is in range 0 to 36. If not, returns null.
        /// </summary>
        public InputData PostInputData()
        {
            if (_inputQueue.Count > 0)
            {
                InputData inputData = _inputQueue?.Dequeue();
                return IsDataInRange(inputData?.InputValue ?? ERROR_INPUT_NOT_IN_RANGE) ? inputData : null;
            }
            return null;
        }

        /// <summary>
        /// Checks if InputData is in range: 0 to 36
        /// </summary>
        private bool IsDataInRange(int inputData) => inputData <= 36 && inputData >= 0;

        /// <summary>
        /// Deserializes json message and returns a winning number from it
        /// </summary>
        private int ReturnWinningNumberFromJson(string json)
        {
            var definition = new { Qualifier = "", Data = new { WinningNumber = 0 } };              // Definition of Message
            var input = JsonConvert.DeserializeAnonymousType(json, definition);
            return CheckQualifier(input.Qualifier) ? input.Data.WinningNumber : INVALID_QUALIFIER;
        }

        /// <summary>
        /// Checks Qualifier in Json Message 
        /// </summary>
        private bool CheckQualifier(string qualifier) => QUALIFIER == qualifier;
    }
}
