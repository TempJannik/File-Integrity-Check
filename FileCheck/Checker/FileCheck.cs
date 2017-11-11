using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Checker
{
    class FileCheck
    {
        private Dictionary<string, string> LocalFileMD = new Dictionary<string, string>();
        private Dictionary<string, string> ServerFileMD = new Dictionary<string, string>();
        private string[] ExceptionFiles = { "mods/bfheroes/mugshot.png" };
        private string GameLocation;

        public FileCheck(string Location)
        {
            GameLocation = Location;
        }

        public void GetLocalMD5()
        {
            string[] localfilelist = Directory.GetFiles(GameLocation); //Reads all files' paths from the path specified in the constructor
            foreach (var file in localfilelist)
            {
                LocalFileMD.Add(file, CalculateMD5(file)); //Add the Files path (with name) and its MD5 to Dictionary
            }
        }

        public void GetServerMD5() //Needs to be adjusted to whatever method is chosen to get the MD5s from server files.
        {
            string[] localfilelist = File.ReadAllLines("manifest.txt"); //Change to downloaded file
            foreach (var file in localfilelist)
            {
                string[] Data = file.Split('='); //Assuming the format is path=filename=hash the indexes are 0=1=2
                ServerFileMD.Add(Data[0], Data[2]); //Add Path and hash as key and value like in the localmd5
            }
        }

        public bool IsExceptionFile(string FileName)
        {
            for (int i = 0; i < ExceptionFiles.Length; i++)
            {
                if (FileName.Contains(ExceptionFiles[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public void CompareMD5()
        {
            GetLocalMD5();
            GetServerMD5();
            ArrayList MissingFiles = new ArrayList();

            foreach (KeyValuePair<string, string> file in ServerFileMD) //Key = Path; Value = MD5
            {
                if (!LocalFileMD.TryGetValue(GameLocation + file.Key, out string currentFile)) continue; //If hash for the path in the manifest.txt isnt found skip the item

                if (currentFile != file.Value && !IsExceptionFile(file.Key)) //Check if the LocalMD5s Dictioniary has the same hash as the one in ServerFileMD && Check if the file thats missing/faulty is a file that is suppose to change
                {
                    MissingFiles.Add(file.Key); //If they are different add to Arraylist
                }
            }

            if (MissingFiles.Count == 0)
            {
                Console.WriteLine("No invalid files.");
            }
            else
            {
                Console.WriteLine(MissingFiles.Count + " file(s) invalid or missing.");
                DownloadMissingFiles(MissingFiles);
            }
        }

        public void DownloadMissingFiles(ArrayList FilePaths)
        {
            foreach (var MissingFile in FilePaths)
            {
                DownloadFile(MissingFile.ToString()); //DownloadFile method might have to be edited for this
            }
        }

        public string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
