using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NDesk.Options;

namespace Log_Archiver
{
    class Archiver
    {
        //Display a hardcoded error message on the command prompt screen
        static void Display_Help()
        {
            string help =
@"Log Archiver
Version 1.0
This application provides a means of archiving logs that are saved on the
system. This application uses the file information for each file in the 
directory and compares that information against the current day.  It then
subtracts those two values to determine if the files should be archived.
After finishing the archive, the program checks checks the archive directory
to determine which archive files should be deleted permanently.

    Usage:
    archiver.exe -d 2 -a 14 [set of directories]

    -d -delete_after [integer] (Default 14)
        Delete files within the archives and its subdirectories that are older
        that the specified number of days.

    -a -archive_after [integer] (Default 1)
        Archive files within the specified folders that are older than the
        specified number of days.

    -s -save [string] (Default C:\log_archive\)
        Save files to be archived to this directory.

    -v -verbose
        Print detailed operation messages to screen

    -h -? -help
        Print help message to screen.

    [set of directories]
        Parse all the files in these directories for files needed to be 
        archived.

";
            Console.WriteLine(help);
        }

        //Archive a file to the specified director renaming it to include
        //it's last write date
        static void Archive_File(string file, string archive_dir, bool verbose)
        {
            File.Move(file, Path.GetDirectoryName(archive_dir) + "\\" + Path.GetFileNameWithoutExtension(file) + "." + File.GetLastWriteTime(file).Month.ToString() + "_"
                + File.GetLastWriteTime(file).Day.ToString() + "_" + File.GetLastWriteTime(file).Year.ToString() + ".txt");
        }

        //Loop through a given list of log directories and its subfolders for files to archive
        static void Check_Logs(string[] dirs, string archive_dir, int archive_after, bool verbose)
        {
            foreach (string directory in dirs)
            {
                if (verbose) Console.WriteLine("Checking directory => " + directory + "...");
                string[] directories = Directory.GetDirectories(directory);
                Check_Logs(directories, archive_dir, archive_after, verbose);

                string[] files = Directory.GetFiles(directory);
                foreach (string file in files)
                {
                    if (verbose) Console.WriteLine("   Checking file => " + file + "...");
                    DateTime curr_time = DateTime.Now;
                    DateTime file_time = File.GetLastWriteTime(file);
                    int time = (curr_time - file_time).Days;
                    if (time >= archive_after)
                    {
                        try
                        {
                            Archive_File(file, archive_dir, verbose);
                            if (verbose) Console.WriteLine("   File Archived");
                        }
                        catch (Exception ex)
                        {
                            if (verbose) Console.WriteLine("   File Unable to be Archived");
                        }

                    }
                }
            }
        }

        //Loop through the archive folder and its subfolders for files to 
        //delete
        static void Check_Archives(string[] archive_dir, int delete_after, bool verbose)
        {
            foreach (string directory in archive_dir)
            {
                if (verbose) Console.WriteLine("Checking archive => " + directory + "...");
                string[] dirs = Directory.GetDirectories(directory);
                Check_Archives(dirs, delete_after, verbose);

                string[] files = Directory.GetFiles(directory);
                foreach (string file in files)
                {
                    if (verbose) Console.WriteLine("   Checking file => " + file + "...");
                    DateTime curr_time = DateTime.Now;
                    DateTime file_time = File.GetLastWriteTime(file);
                    int time = (curr_time - file_time).Days;
                    if (time >= delete_after)
                    {
                        if (verbose) Console.WriteLine("   File Deleted");
                        File.Delete(file);
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            //Declare program defaults
            int delete_after = 14, archive_after = 1;
            string[] savedir = new string[] { "C:\\log_archive\\" };
            bool help = false;
            bool verbose = false;

            //Create list of command line options to specify
            //which variables should be set
            var set = new OptionSet()
            {
                { "d|delete_after=", (int v) => delete_after = v },
                { "a|archive_after=", (int v) => archive_after = v },
                { "h|help|?", v => help = true }, 
                { "s|save", v => savedir[0] = v },
                { "v|verbose", v => verbose = true }, 
            };

            List<string> extra = new List<string>();
            try
            {
                //Attempt to parse out options
                try
                {
                    extra = set.Parse(args);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Display_Help();
                    return;
                }
                if (verbose) Console.Write("Initializing.");
                if (verbose) Console.Write(".");

                if (verbose) Console.Write(".");
                //if user asked for help or no directories to check were specified
                if (help || extra.Count < 1)
                {
                    Console.WriteLine("Usage Error Detected");
                    Display_Help();
                    return;
                }

                //If save directory does not exist, create it
                if (!Directory.Exists(savedir[0]))
                {
                    Directory.CreateDirectory(savedir[0]);
                }

                if (verbose) Console.WriteLine("Complete");
                if (verbose) Console.WriteLine("Archiving files not writtin to in " + archive_after.ToString() + " day(s)");
                if (verbose) Console.WriteLine("Deleting files not writtin to in " + delete_after.ToString() + " day(s)");
                if (verbose) Console.WriteLine("Starting Check...");

                //Check specified folders for files to archive
                Check_Logs(extra.ToArray(), savedir[0], archive_after, verbose);

                //Check archives for files to delete
                Check_Archives(savedir, delete_after, verbose);
                if (verbose) Console.Write("Check Complete...");
                if (verbose) Console.WriteLine("Exiting");
            }
            catch (Exception ex)
            {
                if (verbose) Console.WriteLine("Failed");
                //if an error occurs, do not close, print error to screen 
                //and ask user for confirmation
                Console.WriteLine("An error occurred:");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Press any key to acknowledge...");
                Console.ReadKey();
            }
        }
    }
}
