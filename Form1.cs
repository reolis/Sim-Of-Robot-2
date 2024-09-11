using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Krasnyanskaya221327_Lab02_Sem5_Ver1.Form1;

namespace Krasnyanskaya221327_Lab02_Sem5_Ver1
{
    public partial class Form1 : Form
    {
        static int count = 0;
        static int oldCount = 0;
        private int savedCount = 0;
        private int N = 0;
        static int countOfTargets = 0;

        public static UDPServer server;

        private DateTime moveBackStartTime;
        private static bool isMovingBack = false;
        private static bool isMovingForward = false;
        private TimeSpan moveBackDuration = TimeSpan.FromSeconds(4);
        public TimeSpan moving = TimeSpan.FromSeconds(2);
        private int bumpCount = 0;

        public class UDPServer
        {
            public IPAddress IpAddress { get; set; }
            public int LocalPort { get; set; }
            public int RemotePort { get; set; }
            public UdpClient UdpClient { get; set; }
            public IPEndPoint IpEndPoint { get; set; }
            public byte[] Data { get; set; }
            public static Dictionary<string, int> DecodeText;

            public static string DecodeData { get; set; }
            public static int n, s, c, le, re, az, b, d0, d1, d2, d3, d4, d5, d6, d7, l0, l1, l2, l3, l4;

            public UDPServer(IPAddress ip, int localPort, int remotePort)
            {
                IpAddress = ip;
                LocalPort = localPort;
                RemotePort = remotePort;
                UdpClient = new UdpClient(LocalPort);
                IpEndPoint = new IPEndPoint(IpAddress, LocalPort);
            }

            public async Task ReceiveDataAsync()
            {
                while (true)
                {
                    var receivedResult = await UdpClient.ReceiveAsync();
                    Data = receivedResult.Buffer;
                    DecodingData(Data);
                }
            }

            public async Task SendDataAsync(byte[] data)
            {
                if (data != null)
                {
                    IPEndPoint pointServer = new IPEndPoint(IpAddress, RemotePort);
                    await UdpClient.SendAsync(data, data.Length, pointServer);
                }
            }

            public async Task SendRobotDataAsync()
            {
                string robotData = Robot.GetCommandsAsJson();
                byte[] dataToSend = Encoding.ASCII.GetBytes(robotData + "\n");
                await SendDataAsync(dataToSend);
            }

            private void DecodingData(byte[] data)
            {
                var message = Encoding.ASCII.GetString(data);
                DecodeText = JsonConvert.DeserializeObject<Dictionary<string, int>>(message);
                var lines = DecodeText.Select(kv => kv.Key + ": " + kv.Value.ToString());
                DecodeData = "IoT: " + string.Join(Environment.NewLine, lines);

                AnalyzeData(DecodeText);
            }

            private void AnalyzeData(Dictionary<string, int> pairs)
            {
                if (pairs.ContainsKey("n"))
                {
                    n = pairs["n"];
                    s = pairs["s"];
                    c = pairs["c"];
                    le = pairs["le"];
                    re = pairs["re"];
                    az = pairs["az"];
                    b = pairs["b"];
                    d0 = pairs["d0"];
                    d1 = pairs["d1"];
                    d2 = pairs["d2"];
                    d3 = pairs["d3"];
                    d4 = pairs["d4"];
                    d5 = pairs["d5"];
                    d6 = pairs["d6"];
                    d7 = pairs["d7"];
                    l0 = pairs["l0"];
                    l1 = pairs["l1"];
                    l2 = pairs["l2"];
                    l3 = pairs["l3"];
                    l4 = pairs["l4"];
                }
                else
                {
                    MessageBox.Show("No data");
                }
            }

        }

        public static class Robot
        {
            public static Dictionary<string, int> Commands = new Dictionary<string, int>
            {
                { "N", 0 },
                { "M", 0 },
                { "F", 0 },
                { "B", 0 },
                { "T", 0 },
            };

            public static bool isInStartZone = false;
            public static bool isInWaitingZone = false;
            public static bool isPalletGet = false;
            public static bool isReadyToPick = false;
            public static bool PickedOrder = false;
            public static bool ReturnedToFinal = false;
            public static bool FinalStateGot = false;
            public static int countOfOrders = 0;
            public static int n, s, c, le, re, az, b, d0, d1, d2, d3, d4, d5, d6, d7, l0, l1, l2, l3, l4;
            public static bool isFirstRotateDone = false, isFirstWayDone = false;
            public static Random random = new Random();
            public static int targetAzimuth1 = 5;
            public static int targetAzimuth2 = 62;
            public static int targetAzimuth3 = 82;

            public static double xRobot = 0, yRobot = 0, xInterest = 100, yInterest = 100;
            public static double[] distances;  // Данные с дальномеров
            public static double[] angles = { 0, 45, 90, 135, 180, 225, 270, 315 };  // Углы дальномеров в градусах
            public static bool isFirstTargetDone = false, isSecondTargetDone = false, isThirdTargetDone = false;
            public static int Distance = 800;

            public static void UpdateData(Dictionary<string, int> pairs)
            {
                n = pairs["n"];
                s = pairs["s"];
                c = pairs["c"];
                le = pairs["le"];
                re = pairs["re"];
                az = pairs["az"];
                b = pairs["b"];
                d0 = pairs["d0"];
                d1 = pairs["d1"];
                d2 = pairs["d2"];
                d3 = pairs["d3"];
                d4 = pairs["d4"];
                d5 = pairs["d5"];
                d6 = pairs["d6"];
                d7 = pairs["d7"];
                l0 = pairs["l0"];
                l1 = pairs["l1"];
                l2 = pairs["l2"];
                l3 = pairs["l3"];
                l4 = pairs["l4"];
            }



            public static string GetCommandsAsJson()
            {
                return JsonConvert.SerializeObject(Commands);
            }

            public static void LoadCommandsFromJson(string json)
            {
                var newCommands = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
                if (newCommands != null)
                {
                    Commands = newCommands;
                }
            }

            public static void SetCommand(string key, int value)
            {
                if (Commands.ContainsKey(key))
                {
                    Commands[key] = value;
                }
                else
                {
                    throw new ArgumentException("Команда с таким ключом не существует.");
                }
            }

            public static void UpdateDecodeText()
            {
                UDPServer.DecodeText["n"] = Commands["N"];
            }

            public static void SendOldCommands()
            {
                string oldCommands = JsonConvert.SerializeObject(UDPServer.DecodeText, Formatting.None);

                byte[] data = Encoding.ASCII.GetBytes(oldCommands + "\n");

                UdpClient udpCommands = new UdpClient();
                IPEndPoint pointServer = new IPEndPoint(server.IpAddress, server.RemotePort);
                udpCommands.Send(data, data.Length, pointServer);

                string jsonString = JsonConvert.SerializeObject(Commands, Formatting.None);
                byte[] dataForRobot = Encoding.ASCII.GetBytes(jsonString + "\n");

                udpCommands.Send(dataForRobot, dataForRobot.Length, pointServer);
            }

            public static void RotateRight()
            {
                SetCommand("B", 25);
                SetCommand("F", 0);
            }

            public static void RotateLeft()
            {
                SetCommand("B", -25);
                SetCommand("F", 0);
            }

            public static void MoveStraight()
            {
                SetCommand("F", 100);
            }

            public static void MoveBack()
            {
                SetCommand("F", -100);
                SetCommand("B", 0);
            }

            public static void Stop()
            {
                SetCommand("B", 0);
                SetCommand("F", 0);
            }

            public static void MoveBackWhenBump()
            {
                SetCommand("F", -100);
            }

            public static int GetRandomAngle(bool moveToInterestPoint)
            {
                // Вероятность выбора угла к точке интереса
                int interestPointProbability = 20;

                // Если выбираем направление к точке интереса и вероятность срабатывает
                if (moveToInterestPoint && random.Next(0, 100) < interestPointProbability)
                {
                    var robotCoordinates = CalculateRobotCoors();
                    double robotX = robotCoordinates.Item1;
                    double robotY = robotCoordinates.Item2;

                    double targetAzimuth = 0;
                    if (!isFirstTargetDone)
                    {
                        targetAzimuth = targetAzimuth1;
                    }
                    else if (isFirstTargetDone && !isSecondTargetDone)
                    {
                        targetAzimuth = targetAzimuth2;
                    }
                    else if (isFirstTargetDone && isSecondTargetDone && !isThirdTargetDone)
                    {
                        targetAzimuth = targetAzimuth3;
                    }
                    else
                    {
                        // Если все цели достигнуты, выбираем случайное направление
                        return random.Next(-180, 181);
                    }

                    // Рассчитываем угол поворота к целевой точке
                    return CalculateTurnAngle((int)targetAzimuth);
                }
                else
                {
                    // В остальных случаях выбираем случайное направление
                    int randomAngle = random.Next(-180, 181);
                    return randomAngle;
                }
            }

            public static int GetAngleTowardsInterestPoint()
            {
                double deltaX = xInterest - CalculateRobotCoors().Item1;
                double deltaY = yInterest - CalculateRobotCoors().Item2;

                double angleRadians = Math.Atan2(deltaY, deltaX);
                double angleDegrees = angleRadians * (180 / Math.PI);

                return (int)angleDegrees;
            }

            public static int CalculateTurnAngle(int targetAzimuth)
            {
                // Разница между целевым и текущим азимутом
                int deltaAngle = targetAzimuth - az;

                // Приводим к диапазону [-180, 180]
                if (deltaAngle > 180)
                    deltaAngle += 360;
                else if (deltaAngle < -180)
                    deltaAngle -= 360;

                return deltaAngle;
            }

            public static void PerformRandomWalk()
            {
                // Определяем пороговое значение для минимального расстояния
                const double minDistance = 85;

                // Получаем данные о расстояниях до стен
                double[] distances = { d0, d1, d2, d3, d4, d5, d6, d7 };
                double[] anglesInRadians = angles.Select(angle => angle * Math.PI / 180).ToArray();

                // Находим направление с наибольшим расстоянием до стены
                int bestDirection = -1;
                double maxDistance = 0;

                for (int i = 0; i < distances.Length; i++)
                {
                    if (distances[i] > maxDistance && distances[i] > minDistance)
                    {
                        maxDistance = distances[i];
                        bestDirection = i;
                    }
                }

                // Если подходящее направление найдено
                if (bestDirection != -1)
                {
                    // Определяем угол для поворота
                    double angleToTurn = anglesInRadians[bestDirection] * (180 / Math.PI) - az;

                    // Приводим угол к диапазону [-180, 180]
                    if (angleToTurn > 180) angleToTurn -= 360;
                    if (angleToTurn < -180) angleToTurn += 360;

                    // Рассчитываем скорость поворота
                    int turnSpeed = Math.Abs((int)angleToTurn) * 100 / 180;
                    turnSpeed = Math.Max(turnSpeed, 10);

                    // Устанавливаем команду поворота и движение вперед
                    if (angleToTurn > 0)
                    {
                        SetCommand("B", turnSpeed);  // Вправо
                        MoveStraight();
                    }
                    else
                    {
                        SetCommand("B", -turnSpeed); // Влево
                        MoveStraight();
                    }
                }
                else
                {
                    // Если не удалось найти безопасное направление, выбираем случайное направление
                    int randomAngle = GetRandomAngle(moveToInterestPoint: false);
                    int turnSpeed = Math.Abs(randomAngle) * 100 / 180;
                    turnSpeed = Math.Max(turnSpeed, 10);

                    // Устанавливаем команду поворота и движение вперед
                    if (randomAngle > 0)
                    {
                        SetCommand("B", turnSpeed);  // Вправо
                        MoveStraight();
                    }
                    else
                    {
                        SetCommand("B", -turnSpeed); // Влево
                        MoveStraight();
                    }
                }
            }


            public static void HandleCollision()
            {
                if (!isMovingBack)
                {
                    // При столкновении определяем свободное направление
                    distances = new double[] { d0, d1, d2, d3, d4, d5, d6, d7 };
                    int closestDirection = Array.IndexOf(distances, distances.Min());
                    int turnDirection = (closestDirection < 30) ? 1 : -1;

                    // Двигаемся назад и поворачиваем в свободное направление
                    MoveBack();
                    SetCommand("B", -100 * turnDirection);
                    isMovingBack = true;
                }
                else
                {
                    // Если уже двигались назад, продолжаем случайное движение
                    PerformRandomWalk();
                    isMovingBack = false;
                }
            }

            public static (double, double) CalculateRobotCoors()
            {
                distances = new double[8] { d0, d1, d2, d3, d4, d5, d6, d7 };  // Данные с дальномеров
                double[] anglesInRadians = angles.Select(angle => angle * Math.PI / 180).ToArray();

                for (int i = 0; i < distances.Length; i++)
                {
                    double xWall = distances[i] * Math.Cos(anglesInRadians[i]);
                    double yWall = distances[i] * Math.Sin(anglesInRadians[i]);

                    xRobot += xWall;
                    yRobot += yWall;
                }

                // Нормализуем координаты
                xRobot /= distances.Length;
                yRobot /= distances.Length;

                return (xRobot, yRobot);
            }

            public static (double, double) CalculateTargetCoordinates(double robotX, double robotY, double distance, double azimuth)
            {
                double interestX = robotX + distance * Math.Cos(azimuth);
                double interestY = robotY + distance * Math.Sin(azimuth);

                return (interestX, interestY);
            }

            public static async Task MoveBackForDurationAsync(TimeSpan duration)
            {
                // Начало движения назад
                Robot.MoveBack();

                // Ожидание в течение заданного времени
                await Task.Delay(duration);
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string solutionDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            string filePath = Path.Combine(solutionDirectory, "textbox_data.json");

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var data = JsonConvert.DeserializeObject<dynamic>(json);

                textBox1.Text = data.TextBox1;
                textBox2.Text = data.TextBox2;
                textBox3.Text = data.TextBox3;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var data = new
            {
                TextBox1 = textBox1.Text,
                TextBox2 = textBox2.Text,
                TextBox3 = textBox3.Text
            };

            string solutionDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            string filePath = Path.Combine(solutionDirectory, "textbox_data.json");

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public void SplitDataToTextBoxs()
        {
            var message = Encoding.ASCII.GetString(server.Data);
            var text = JsonConvert.DeserializeObject<Dictionary<string, int>>(message);

            foreach (var chr in text)
            {
                if (chr.Key == "d0")
                {
                    textBox4.Text = chr.Value.ToString();
                }
                if (chr.Key == "d1")
                {
                    textBox5.Text = chr.Value.ToString();
                }
                if (chr.Key == "d2")
                {
                    textBox6.Text = chr.Value.ToString();
                }
                if (chr.Key == "d3")
                {
                    textBox7.Text = chr.Value.ToString();
                }
                if (chr.Key == "d4")
                {
                    textBox8.Text = chr.Value.ToString();
                }
                if (chr.Key == "d5")
                {
                    textBox9.Text = chr.Value.ToString();
                }
                if (chr.Key == "d6")
                {
                    textBox10.Text = chr.Value.ToString();
                }
                if (chr.Key == "d7")
                {
                    textBox11.Text = chr.Value.ToString();
                }

                if (chr.Key == "n")
                {
                    textBox12.Text = chr.Value.ToString();
                }
                if (chr.Key == "s")
                {
                    textBox13.Text = chr.Value.ToString();
                }
                if (chr.Key == "c")
                {
                    textBox14.Text = chr.Value.ToString();
                }
                if (chr.Key == "re")
                {
                    textBox15.Text = chr.Value.ToString();
                }
                if (chr.Key == "le")
                {
                    textBox16.Text = chr.Value.ToString();
                }
                if (chr.Key == "az")
                {
                    textBox17.Text = chr.Value.ToString();
                }
                if (chr.Key == "b")
                {
                    textBox18.Text = chr.Value.ToString();
                }
                if (chr.Key == "l0")
                {
                    textBox24.Text = chr.Value.ToString();
                }
                if (chr.Key == "l1")
                {
                    textBox23.Text = chr.Value.ToString();
                }
                if (chr.Key == "l2")
                {
                    textBox22.Text = chr.Value.ToString();
                }
                if (chr.Key == "l3")
                {
                    textBox21.Text = chr.Value.ToString();
                }
                if (chr.Key == "l4")
                {
                    textBox20.Text = chr.Value.ToString();
                }
            }

            

        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            if (UDPServer.DecodeData != null)
            {
                SplitDataToTextBoxs();

                richTextBox1.Text = "\r\n" + "Here is data";
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();

                count++;
                Robot.Distance--;

                // Проверяем, произошло ли столкновение
                if (UDPServer.b == 1) // Столкновение
                {
                    Robot.HandleCollision();
                }
                else
                {
                    // Запуск метода для выполнения случайного блуждания в течение двух секунд
                    Robot.PerformRandomWalk();

                    if ((UDPServer.l0 == 0 || UDPServer.l2 == 0 || UDPServer.l4 == 0) && (UDPServer.c == 1 || UDPServer.c == 2
                        || UDPServer.c == 3))
                    {
                        richTextBox1.Text = "Here is target zone";
                        countOfTargets++;

                        if (countOfTargets > 0 && UDPServer.c == 1)
                        {
                            Robot.isFirstTargetDone = true;
                            Robot.Distance = 800;
                        }
                        else if (countOfTargets > 0 && UDPServer.c == 2)
                        {
                            Robot.isSecondTargetDone = true;
                            Robot.Distance = 800;
                        }
                        else if (countOfTargets > 0 && UDPServer.c == 3)
                        {
                            Robot.isThirdTargetDone = true;
                            Robot.Distance = 800;
                        }
                    }
                    count++;
                }

                Robot.SetCommand("N", count);

                // Обновляем данные робота
                Robot.UpdateDecodeText();
                Robot.SendOldCommands();
                await server.SendRobotDataAsync();

                textBox19.Text = count.ToString();
            }
        }



        private async Task PerformRandomWalkForDurationAsync(TimeSpan duration)
        {
            // Начало выполнения RandomWalk
            Robot.PerformRandomWalk();

            // Ожидание в течение заданного времени
            await Task.Delay(duration);
        }


        private async void button2_Click(object sender, EventArgs e)
        {
            server = new UDPServer(IPAddress.Parse(textBox3.Text), Int32.Parse(textBox2.Text), Int32.Parse(textBox1.Text));
            await server.ReceiveDataAsync();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer1.Start();
        }
    }
}
