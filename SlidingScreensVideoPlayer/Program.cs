using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using NLog;

namespace SlidingScreensVideoPlayer
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // Initialize VLC Object
        public static AXVLC.VLCPlugin2Class player = new AXVLC.VLCPlugin2Class();


        static void Main(string[] args)
        {
            logger.Trace("Initializing Main()");
            // Run homing (and wait until it's over)

            logger.Info("Taking system to home position.");
            goHome();


            // Initialize playlist list<>
            logger.Trace("Initializing List<string> playlist.");
            List<string> playlist = new List<string>();

            // loop starts here
            while (true)
            {
                logger.Trace("Start of while loop iteration.");
                // Find playlist.xml file
                try
                {
                    logger.Info("Copying playlist.xml to playlist.xml.bak");
                    File.Copy("playlist.xml", "playlist.xml.bak", true);
                }
                catch (FileNotFoundException)
                {
                    logger.Error("playlist.xml not found. Please make sure that file exists and named correctly.");
                }
                catch (FileLoadException)
                {
                    logger.Error("Cannot load playlist.xml. Please make sure that file exists and named correctly.");
                }
                catch (Exception)
                {
                    logger.Error("Error while copying playlist.xml");
                }

                // Parse playlist.xml.bak
                logger.Info("Attempting to read playlist XML file.");

                playlist = readPlaylist("playlist.xml.bak");


                // Loop over playlist items
                logger.Trace("looping over playlist items");
                for (int i = 0; i < playlist.Count; i++)
                {
                    // Run corrosponding motion executable
                    logger.Info("Running motion file for " + playlist[i]);
                    runMotion(playlist[i]);
                    // Play video
                    logger.Info("Playing video file " + playlist[i]);
                    playVideo(playlist[i]);
                    // Wait until end of video is reached (either with endofplaylistevent or by waiting the video length)

                }
            }



            // string[] playlists = Directory.GetFiles(@"", "playlist.xml");


        }



        private static void runMotion(string p)
        {
            try
            {
                logger.Trace(@"Running motionExes\runmotion" + p.Substring(0, 2) + ".exe");
                Process.Start(@"motionExes\runmotion" + p.Substring(0, 2) + ".exe");
            }
            catch (Exception)
            {
                logger.Error("Problem running motion file: runmotion" + p.Substring(0, 2) + ".exe");
            }
        }

        private static List<String> readPlaylist(string playlistFile)
        {
            logger.Trace("Reading playlist.xml.bak");
            logger.Trace("Initializing XmlDocument");
            XmlDocument doc = new XmlDocument();
            logger.Trace("Initializing XmlReaderSettings");
            XmlReaderSettings settings = new XmlReaderSettings { NameTable = new NameTable() };
            logger.Trace("Initializing XmlNamespaceManager");
            XmlNamespaceManager xmlns = new XmlNamespaceManager(settings.NameTable);
            xmlns.AddNamespace("ns2", "http://www.w3.org/XML/2008/xsdl-exx/ns1");
            logger.Trace("Initializing XmlParserContext");
            XmlParserContext context = new XmlParserContext(null, xmlns, "", XmlSpace.Default);
            logger.Trace("Initializing XmlReader");
            XmlReader reader = XmlReader.Create(playlistFile, settings, context);
            logger.Info("XML reader created.");
            doc.Load(reader);
            logger.Info("XML namespace reader loaded successfully.");

            // Setup default namespace manager for searching through nodes
            logger.Trace("Setting up default namespace manager.");
            XmlNamespaceManager manager = new XmlNamespaceManager(doc.NameTable);
            logger.Trace("Setting up default namespace manager.");
            string defaultns = doc.DocumentElement.GetNamespaceOfPrefix("ns2");
            //string defaultns3 = doc.DocumentElement.GetNamespaceOfPrefix("ns3");
            //string defaultnssoap = doc.DocumentElement.GetNamespaceOfPrefix("soapenv");
            manager.AddNamespace("ns2", defaultns);
            //manager.AddNamespace("ns3", defaultns3);
            //manager.AddNamespace("soapenv", defaultnssoap);
            logger.Info("Default namespace set.");

            
            logger.Trace("Initializing ns2:frame/ns2:loops handle");
            XmlNodeList loops = doc.DocumentElement.SelectNodes("ns2:frame/ns2:loops", manager);

            //logger.Trace("Initializing soapenv:body/ns3:getPlaylistResponse/playlist/ns2:frame/ns2:loops handle");
            //XmlNodeList loops = doc.DocumentElement.SelectNodes("//soapenv:Envelope/soapenv:Body/ns3:getPlaylistResponse/playlist/ns2:frame/ns2:loops", manager);

            logger.Trace("Initializing ns2:playlistScope handle");
            XmlNodeList nodes = doc.DocumentElement.SelectNodes("ns2:playlistScope", manager);

            logger.Info("Reading system date.");
            DateTime today = DateTime.Today;

            logger.Info("Creating playlist out of XML entries.");
            List<string> pl = new List<string>();

            logger.Trace("Clearing playlist of any old entries, if any.");
            pl.Clear();

            foreach (XmlNode loop in loops)
            {

                logger.Trace("loop " + loop.ToString() + "of " + loops.ToString());
                logger.Trace("Checking date");
                if (today.ToString("yyyy-MM-dd") == loop.Attributes["day"].Value)
                {
                    logger.Trace("It's today!");
                    XmlNodeList slots = loop.SelectNodes("ns2:slot", manager);
                    foreach (XmlNode slot in slots)
                    {
                        logger.Trace("slot " + slot.ToString() + "of " + slots.ToString());
                        XmlNode fn = slot.SelectSingleNode("ns2:content/ns2:adCopy", manager);
                        pl.Add(fn.Attributes["originalFileName"].Value);
                        logger.Info("Adding filename: " + fn.Attributes["originalFileName"].Value + " to playlist.");
                        logger.Info("Current playlist: " + pl.ToString());
                    }
                    logger.Trace("Skipping rest of loops");
                    break;
                }
                else
                {
                    logger.Trace("Moving to next loop.");
                    continue;
                }
            }

            reader.Close();
            logger.Trace("Closing XML reader.");
            reader.Dispose();
            logger.Trace("Disposing XML reader memory.");
            logger.Info("Final playlist: " + pl.ToString());
            return pl;
        }

        private static void goHome()
        {
            try
            {
                logger.Trace("Calling the homing EXE");
                Process goHome = Process.Start(@"motionExes\goHome.exe");
                logger.Trace("Waiting for homing to finish");
                goHome.WaitForExit();
                logger.Info("Homing done successfully!");
            }
            catch (Exception)
            {
                logger.Fatal("Error running the homing process. System will now exit! DO NOT ATTEMPT TO RUN ANY MOTION UNTIL HOMING IS DONE SUCCESSFULLY.");
                //Environment.Exit(1000);
            }

        }

        public static void playVideo(string v) //start playing playlist
        {
            logger.Trace("player.input.state = " + player.input.state);
            logger.Trace("Forcing fullscreen");
            player.video.fullscreen = true;
            logger.Trace("Clearing VLC playlist");
            player.playlistClear();
            logger.Trace("VLC about to play: "+ v);
            player.addTarget(@"file:///" + v, null, AXVLC.VLCPlaylistMode.VLCPlayListReplaceAndGo, -666);
            logger.Trace("player.input.state = " + player.input.state);
            logger.Trace("Play NOW!");
            player.play();
            logger.Trace("player.input.state = " + player.input.state);
            do
            {
                Thread.Sleep(100);
                logger.Trace("player.input.state = " + player.input.state);
            }
            while (player.input.state < 4 || player.input.state > 6);
            
            logger.Trace("player.input.state = " + player.input.state);
        }


    }
}