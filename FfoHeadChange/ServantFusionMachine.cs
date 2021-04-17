using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FfoHeadChange
{
    public class ServantIconData
    {
        public int ServantID;
        public int Direction;
        public float Scale;
        public int HeadX;
        public int HeadY;
        public int BodyX;
        public int BodyY;
        public int HeadX2;
        public int HeadY2;
    }

    public class ServantInfo 
    {
        public int ServantID;
        public string Name;
        public string FirstName;
        public string MiddleName;
        public string LastName;
        public string Flavor;
        public string FlavorFirst;
        public string FlavorMiddle;
        public string FlavorLast;
    }

    public class ServantFusionMachine
    {
        private static Dictionary<int, ServantIconData> servantIconDb = null;
        private static Dictionary<int, ServantInfo> servantInfoDb = null;
        private static volatile int sessionID = 0;
        private static Mutex globalMutex = new Mutex();
        // not thread safe method
        public static void GenerateFusionServant(Window window, Canvas canvas, int headID, int bodyID, int backgroundID)
        {
            int currentSession;
            using (globalMutex)
            {
                if (servantIconDb == null)
                {
                    MessageMgr.SendMessage(null, "Initializing servant icon database");
                    servantIconDb = new Dictionary<int, ServantIconData>();
                    var csvTable = SimpleCsvParser.ParseTable(StartupConfig.SERVANT_DB_PART_FILE);
                    int size = csvTable.Values.First().Count;
                    for (int i = 0; i < size; i++)
                    {
                        int servantID = int.Parse(csvTable["servant_id"][i]);
                        servantIconDb.Add(servantID, new ServantIconData()
                        {
                            ServantID = servantID,
                            Direction = int.Parse(csvTable["direction"][i]),
                            Scale = float.Parse(csvTable["scale"][i]),
                            HeadX = int.Parse(csvTable["head_x"][i]),
                            HeadY = int.Parse(csvTable["head_y"][i]),
                            BodyX = int.Parse(csvTable["body_x"][i]),
                            BodyY = int.Parse(csvTable["body_y"][i]),
                            HeadX2 = int.Parse(csvTable["head_x2"][i]),
                            HeadY2 = int.Parse(csvTable["head_y2"][i])
                        });

                    }
                    MessageMgr.SendMessage(null, "Finished servant icon database initialization");
                }
                
                sessionID++;
                currentSession = sessionID;
            }
            servantIconDb.TryGetValue(headID, out var headData);
            servantIconDb.TryGetValue(bodyID, out var bodyData);

            if (bodyData == null) bodyData = headData;

            // perform file IO
            byte[] bgBack = ReadAllBytes(StartupConfig.BACKGROUND_DIRECTORY + "/bg_" + backgroundID.ToString("000") + ".png");
            byte[] bodyBack = ReadAllBytes(StartupConfig.BODY_DIRECTORY + "/sv" + bodyID.ToString("000") + "_body_back.png");
            byte[] headBack = ReadAllBytes(StartupConfig.HEAD_DIRECTORY + "/sv" + headID.ToString("000") + "_head_back.png");
            byte[] bodyMiddle = ReadAllBytes(StartupConfig.BODY_DIRECTORY + "/sv" + bodyID.ToString("000") + "_body_middle.png");
            byte[] headFront = ReadAllBytes(StartupConfig.HEAD_DIRECTORY + "/sv" + headID.ToString("000") + "_head_front.png");
            byte[] bodyFront = ReadAllBytes(StartupConfig.BODY_DIRECTORY + "/sv" + bodyID.ToString("000") + "_body_front.png");
            byte[] bgFront = ReadAllBytes(StartupConfig.BACKGROUND_DIRECTORY + "/bg_" + backgroundID.ToString("000") + "_front.png");

            using (globalMutex)
            {
                if (sessionID != currentSession)
                    return; // session changed: this might not be the newest session, and then return
                window.Dispatcher.Invoke(new ThreadStart(delegate
                {
                    canvas.Children.Clear();
                    if (bgBack != null)
                    {
                        canvas.Children.Add(CreateImageFromBytes(bgBack));
                    }
                    if (bodyBack != null)
                    {
                        var e = CreateImageFromBytes(bodyBack);
                        canvas.Children.Add(e);
                        Canvas.SetLeft(e, -256);
                        Canvas.SetTop(e, -152);
                    }
                    if (headBack != null)
                    {
                        var e = CreateImageFromBytes(headBack);
                        canvas.Children.Add(e);
                        int headX = (bodyData.HeadX2 == 0 || bodyID == headID) ? bodyData.HeadX : bodyData.HeadX2;
                        int headY = (bodyData.HeadX2 == 0 || bodyID == headID) ? bodyData.HeadY : bodyData.HeadY2;

                        if (headData.Scale != bodyData.Scale || (bodyData.Direction != 1 && bodyData.Direction != headData.Direction))
                        {
                            float sign = (bodyData.Direction != 1 && bodyData.Direction != headData.Direction) ? -1 : 1;
                            float scale = bodyData.Scale / headData.Scale;
                            var tf = new ScaleTransform(scale * sign, scale);
                            e.RenderTransform = tf;
                            Canvas.SetLeft(e, 256 - sign * scale * 512 + (headX - 512));
                            Canvas.SetTop(e, (720 - 1024 * scale) / 2 + (headY - 512));
                        }
                        else
                        {
                            Canvas.SetLeft(e, -256 + (headX - 512));
                            Canvas.SetTop(e, -152 + (headY - 512));
                        }
                    }
                    if (bodyMiddle != null)
                    {
                        var e = CreateImageFromBytes(bodyMiddle);
                        canvas.Children.Add(e);
                        Canvas.SetLeft(e, -256);
                        Canvas.SetTop(e, -152);
                    }
                    if (headFront != null)
                    {
                        var e = CreateImageFromBytes(headFront);
                        canvas.Children.Add(e);

                        int headX = (bodyData.HeadX2 == 0 || bodyID == headID) ? bodyData.HeadX : bodyData.HeadX2;
                        int headY = (bodyData.HeadX2 == 0 || bodyID == headID) ? bodyData.HeadY : bodyData.HeadY2;

                        if (headData.Scale != bodyData.Scale || (bodyData.Direction != 1 && bodyData.Direction != headData.Direction))
                        {
                            float sign = (bodyData.Direction != 1 && bodyData.Direction != headData.Direction) ? -1 : 1;
                            float scale = bodyData.Scale / headData.Scale;
                            var tf = new ScaleTransform(scale * sign, scale);
                            e.RenderTransform = tf;
                            Canvas.SetLeft(e, 256 - sign * scale * 512 + (headX - 512));
                            Canvas.SetTop(e, (720 - 1024 * scale) / 2 + (headY - 512));
                        }
                        else
                        {
                            Canvas.SetLeft(e, -256 + (headX - 512));
                            Canvas.SetTop(e, -152 + (headY - 512));
                        }
                    }
                    if (bodyFront != null)
                    {
                        var e = CreateImageFromBytes(bodyFront);
                        canvas.Children.Add(e);
                        Canvas.SetLeft(e, -256);
                        Canvas.SetTop(e, -152);
                    }
                    if (bgFront != null)
                    {
                        canvas.Children.Add(CreateImageFromBytes(bgFront));
                    }
                }));

                MessageMgr.SendMessage(null, "Done");
            }
        }

        private static Image CreateImageFromBytes(byte[] blob)
        {
            var img = new Image();
            var bmpsrc = new BitmapImage();
            bmpsrc.BeginInit();
            bmpsrc.StreamSource = new MemoryStream(blob);
            bmpsrc.EndInit();
            img.Source = bmpsrc;
            img.RenderTransformOrigin = new Point(0, 0);
            return img;
        }

        private static byte[] ReadAllBytes(string path)
        {
            if (File.Exists(path))
            {
                MessageMgr.SendMessage(null, "Reading file: " + path);
                var fs = new FileStream(path, FileMode.Open);
                var data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                fs.Close();
                return data;
            }
            return null;
        }

        public static void GetFusionServantInfo(int headID, int bodyID, int backgroundID, out string servantName, out string servantInfo)
        {
            servantName = "";
            servantInfo = "";
            using (globalMutex)
            {
                if (servantInfoDb == null)
                {
                    MessageMgr.SendMessage(null, "Initializing servant localization database");
                    servantInfoDb = new Dictionary<int, ServantInfo>();
                    var csvTable = SimpleCsvParser.ParseTable(StartupConfig.SERVANT_DB_LOCALIZE_FILE);
                    int size = csvTable.Values.First().Count;
                    for (int i = 0; i < size; i++)
                    {
                        int servantID = int.Parse(csvTable["servant_id"][i]);
                        servantInfoDb.Add(servantID, new ServantInfo()
                        {
                            ServantID = servantID,
                            FirstName = csvTable["first_ja_JP"][i],
                            MiddleName = csvTable["middle_ja_JP"][i],
                            LastName = csvTable["last_ja_JP"][i],
                            Name = csvTable["name_ja_JP"][i],
                            Flavor = csvTable["flavor_ja_JP"][i],
                            FlavorFirst = csvTable["flavor_first_ja_JP"][i],
                            FlavorMiddle = csvTable["flavor_middle_ja_JP"][i],
                            FlavorLast = csvTable["flavor_last_ja_JP"][i]
                        });

                    }
                    MessageMgr.SendMessage(null, "Finished servant localization database initialization");
                }
            }
            if (headID == bodyID && headID == backgroundID)
            {
                // same id
                if (servantInfoDb.TryGetValue(headID, out var info))
                {
                    servantName = info.Name;
                    servantInfo = info.Flavor;
                }
            }
            else
            {
                if (servantInfoDb.TryGetValue(headID, out var headInfo) && servantInfoDb.TryGetValue(bodyID, out var bodyInfo) && servantInfoDb.TryGetValue(backgroundID, out var backgroundInfo))
                {
                    servantName = headInfo.FirstName + bodyInfo.MiddleName + backgroundInfo.LastName;
                    servantInfo = headInfo.FlavorFirst + bodyInfo.FlavorMiddle + backgroundInfo.FlavorLast;
                }
            }
        }
    }
}
