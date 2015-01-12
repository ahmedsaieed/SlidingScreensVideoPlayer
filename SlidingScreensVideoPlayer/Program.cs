using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;


namespace SlidingScreensVideoPlayer
{
    class Program
    {
        public static AXVLC.VLCPlugin2Class player = new AXVLC.VLCPlugin2Class();
        

        static void Main(string[] args)
        {

            // Run homing (and wait until it's over)
            goHome();

            // Initialize VLC Object
            List<string> playlist = new List<string>();


            // loop starts here
            while (true)
            {
                // Find playlist.xml file
                try
                {
                    File.Copy("playlist.xml", "playlist.xml.bak", true);
                    Console.WriteLine("Copying playlist.xml");
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("playlist.xml not found. Please make sure that file exists and named correctly.");
                }

                // Parse playlist.xml.bak
                playlist = readPlaylist("playlist.xml.bak");


                    // Loop over playlist items
                    for (int i = 0; i < playlist.Count; i++)
                    {
                        
                        // Run corrosponding motion executable
                        runMotion(playlist[i]);
                        // Play video
                        playVideo(playlist[i]);
                        // Wait until end of video is reached (either with endofplaylistevent or by waiting the video length)
                        
                    }
                }
            


            // string[] playlists = Directory.GetFiles(@"", "playlist.xml");


        }

        

        private static void runMotion(string p)
        {
            try {
            Process.Start(@"motionExes\runmotion" + p.Substring(0, 2) + ".exe");
            }
            catch(Exception)
            {
                Console.WriteLine("Problem running motion file: runmotion" + p.Substring(0, 2) + ".exe");
            }
        }

        private static List <String> readPlaylist(string playlistFile)
        {
            Console.WriteLine("Parsing playlist.");
            XmlDocument doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings { NameTable = new NameTable() };
            XmlNamespaceManager xmlns = new XmlNamespaceManager(settings.NameTable);
            xmlns.AddNamespace("ns2", "http://www.w3.org/XML/2008/xsdl-exx/ns1");
            XmlParserContext context = new XmlParserContext(null, xmlns, "", XmlSpace.Default);
            //XmlReader reader = XmlReader.Create(@"c:\users\asaieed\downloads\test.xml", settings, context);
            XmlReader reader = XmlReader.Create(playlistFile, settings, context);
            Console.WriteLine("XML reader created.");
            doc.Load(reader);
            Console.WriteLine("Loading XML reader.");

            // Setup default namespace manager for searching through nodes
            XmlNamespaceManager manager = new XmlNamespaceManager(doc.NameTable);
            string defaultns = doc.DocumentElement.GetNamespaceOfPrefix("ns2");
            manager.AddNamespace("ns2", defaultns);
            Console.WriteLine("Default namespace set.");

            // XmlNodeList nodes = doc.DocumentElement.SelectNodes("/ns2:catalog/ns2:book", manager);
            XmlNodeList loops = doc.DocumentElement.SelectNodes("ns2:frame/ns2:loops", manager);
            XmlNodeList nodes = doc.DocumentElement.SelectNodes("ns2:playlistScope", manager);
            

            //System.Console.WriteLine(nodes[0].Attributes["endDateTime"].Value);

            //DateTime endDateTime = DateTime.Parse(nodes[0].Attributes["endDateTime"].Value);
            //DateTime startDateTime = DateTime.Parse(nodes[0].Attributes["startDateTime"].Value);
            //DateTime now = DateTime.Now;
            DateTime today = DateTime.Today;

            Console.WriteLine("Creating playlist.");
            List<string> pl = new List<string>();

            Console.WriteLine("Clearing playlist.");
            pl.Clear();

            foreach (XmlNode loop in loops)
            {
                Console.WriteLine("loop " + loop.ToString() +"of " + loops.ToString());
                Console.WriteLine("Checking date");
                if (today.ToString("yyyy-MM-dd") == loop.Attributes["day"].Value)
                {
                    XmlNodeList slots = loop.SelectNodes("ns2:slot", manager);
                    foreach (XmlNode slot in slots)
                    {
                        Console.WriteLine("slot " + slot.ToString() + "of " + slots.ToString());
                        XmlNode fn = slot.SelectSingleNode("ns2:content/ns2:adCopy", manager);
                        pl.Add(fn.Attributes["originalFileName"].Value);
                        Console.WriteLine("filename: " + fn.Attributes["originalFileName"].Value);
                        Console.WriteLine("Current playlist: " + pl.ToString());
                    }
                    Console.WriteLine("Skipping rest of loops");
                    break;
                }
                else
                {
                    Console.WriteLine("Moving to next loop.");
                    continue;
                }
            }

            reader.Close();
            Console.WriteLine("Closing XML reader.");
            reader.Dispose();
            return pl;

        }

        private static void goHome()
        {
            try
            {
                Process goHome = Process.Start(@"motionExes\goHome.exe");
                goHome.WaitForExit();
            }
            catch(Exception)
            {
                Console.WriteLine("Problem running goHome.exe");
            }

        }

        public static void playVideo(string v) //start playing playlist
        {
            player.video.fullscreen = true;
            player.playlistClear();
            player.addTarget(@"file:///" + v, null, AXVLC.VLCPlaylistMode.VLCPlayListReplaceAndGo, -666);
            Console.WriteLine("Playing "+ v);
            player.play();
            do
            {
                Thread.Sleep(100);
                Console.WriteLine("player.input.state = "+player.input.state);
            } 
            while (player.input.state < 4 || player.input.state > 6 );
        }


    }
}