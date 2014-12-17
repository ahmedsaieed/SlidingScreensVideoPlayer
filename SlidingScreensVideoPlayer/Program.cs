using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlidingScreensVideoPlayer
{
    class Program
    {
        

        static void Main(string[] args)
        {
            string[] playlist = new string[50];

            // Initialize VLC Object
            System.Console.WriteLine("Initializing Vidoe Player");
            AXVLC.VLCPlugin2Class player = new AXVLC.VLCPlugin2Class();
            
            // Find playlist.xml file
            string[] playlists = Directory.GetFiles(@"", "*.xml");

            // Check last modification date
            DateTime lastModified = System.IO.File.GetLastWriteTime("playlist.xml");

            // If modification date of playlist.xml.bak != modification date of playlist.xml
            if (System.IO.File.GetLastWriteTime("playlist.xml") != System.IO.File.GetLastWriteTime("playlist.xml.bak"))
                // Run homing (and wait until it's over)
                GoHome();
                // Copy playlist.xml to playlist.xml.bak
                File.Copy("playlist.xml", "playlist.xml.bak", true);    
                // Parse playlist.xml.bak
                playlist = ReadPlaylist();
            // Loop over playlist items
                for (int i = 0; i < playlist.Length; i++)
                {
                    // Play video

                    // Run corrosponding motion executable
                    RunMotion(playlist[i]);
                    // Wait until end of video is reached (either with endofplaylistevent or by waiting the video length)
                }
        }

        private static void RunMotion(string p)
        {
            Process.Start("motionExecutables\runmotion" + p.Substring(0,2) + ".exe");
            throw new NotImplementedException();
        }
        
        private static string[] ReadPlaylist()
        {
            throw new NotImplementedException();
        }

        private static void GoHome()
        {
            throw new NotImplementedException();
        }

        public static void playVideo(AXVLC.VLCPlugin2Class p) //start playing playlist
        {
            p.playlist.play();
            p.video.fullscreen = true;
        }


    }
}
