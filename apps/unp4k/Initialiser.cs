﻿using unlib;

namespace unp4k;
internal static class Initialiser
{
    private static DirectoryInfo? DefaultOutputDirectory { get; }  = new(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "unp4k"));
    private static FileInfo? Defaultp4kFile { get; } = OS.IsWindows ? new(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Roberts Space Industries", "StarCitizen", "LIVE", "Data.p4k")) :
        new(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Desktop", "unp4k", "Data.p4k"));
    private static DirectoryInfo? DefaultExtractionDirectory { get; } = new(Path.Join(DefaultOutputDirectory.FullName, "output"));

    internal static async Task PreInit(string[] args)
    {
        Logger.ClearBuffer();
        Console.Title = $"unp4k: Pre-Initializing...";

        if (args.Length is 0)
        {
            Globals.P4kFile = Defaultp4kFile;
            Globals.OutDirectory = DefaultExtractionDirectory;
            Globals.Filters.Add("*.*");
            Logger.ClearBuffer();
            // Basically show the user the manual if there are no arguments.
            Logger.LogInfo('\n' +
                "################################################################################" + '\n' + '\n' +
                "                              unp4k <> Star Citizen                             " + '\n' + '\n' +
                "Extracts Star Citizen's Data.p4k into a directory of choice and even convert them into xml files!" + '\n' + '\n' +
               @"\" + '\n' +
               @" | Windows PowerShell: .\unp4ck -d -i " + '"' + "[InFilePath]" + '"' + " -o " + '"' + "[OutDirectoryPath]" + '"' + '\n' +
               @" | Windows Command Prompt: unp4ck -d -i " + '"' + "[InFilePath]" + '"' + " -o " + '"' + "[OutDirectoryPath]" + '"' + '\n' +
               @" | Linux Terminal: ./unp4ck -d -i " + '"' + "[InFilePath]" + '"' + " -o " + '"' + "[OutDirectoryPath]" + '"' + '\n' +
                " | " + '\n' +
               @" | Windows Example: unp4ck -i " + '"' + @"C:\Program Files\Roberts Space Industries\StarCitizen\LIVE\Data.p4k" + '"' + " -o " + '"' + @"C:\Windows\SC" + '"' + " -f " + '"' + "*.*" + '"' + " -d" + '\n' +
               @" | Ubuntu Example: unp4ck -i " + '"' + @"/home/USERNAME/unp4k/Data.p4k" + '"' + " -o " + '"' + @"/home/USERNAME/unp4k/output" + '"' + " -f " + '"' + "*.*" + '"' + " -d" + '\n' +
                " | " + '\n' +
               @" |\" + '\n' +
                " | - Mandatory arguments:" + '\n' +
                " | | -i: Delcares the input file path." + '\n' +
                " | | -o: Declared the output directory path." + '\n' +
                " | |" + '\n' +
                " | - Optional arguments:" + '\n' +
                " | | -f: Allows you to filter in the files you want." + '\n' +
                " | | -e: Enables error and exception printing to console." + '\n' +
                " | | -d: Enabled detailed logging." + '\n' +
                " | | -c: Makes extraction and smelting run at the same time (requires a lot of RAM)." + '\n' +
                " | | -w: Forces all files to be re-extraced and/or re-smelted." + '\n' +
                " | | -forge: Enables unforge to forge extracted files." + '\n' +
                " |/" + '\n' +
                " | " + '\n' +
               @" |\" + '\n' +
                " | - Format Examples:" + '\n' +
                " | | File Type Selection: .dcb" + '\n' +
                " | | Multi-File Type Selection: .dcb,.png,.gif" + '\n' +
                " | | Specific File Selection: Game.dcb" + '\n' +
                " | | Multi-Specific File Selection: Game.dcb,smiley_face.png,its_working.gif" + '\n' +
                " |/" + '\n' +
                "/" + '\n' +
                "################################################################################" + '\n' + '\n' +
               $"NO INPUT Data.p4k PATH HAS BEEN DECLARED. USING DEFAULT PATH " + '"' + $"{Defaultp4kFile.FullName}" + '"' + '\n' +
                "NO OUTPUT DIRECTORY PATH HAS BEEN DECLARED. ALL EXTRACTS WILL GO INTO " + '"' + $"{DefaultExtractionDirectory.FullName}" + '"' + '\n' + '\n' +
                "Press any key to continue!" + '\n');
            Console.ReadKey();
        }

        // Parse the arguments and do what they represent
        try
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLowerInvariant() is "-i") Globals.P4kFile = new(args[i + 1]);
                else if (args[i].ToLowerInvariant() is "-o") Globals.OutDirectory = new(args[i + 1]);
                else if (args[i].ToLowerInvariant() is "-f") Globals.Filters = args[i + 1].Split(',').ToList();
                else if (args[i].ToLowerInvariant() is "-e") Globals.PrintErrors = true;
                else if (args[i].ToLowerInvariant() is "-d") Globals.DetailedLogs = true;
                else if (args[i].ToLowerInvariant() is "-c") Globals.CombinePasses = true;
                else if (args[i].ToLowerInvariant() is "-w") Globals.ForceOverwrite = true;
                else if (args[i].ToLowerInvariant() is "-forge") Globals.ShouldSmelt = true;
            }
        }
        catch (IndexOutOfRangeException e)
        {
            if (Globals.PrintErrors) Logger.LogException(e);
            else Logger.LogError("An error has occured with the argument parser. Please ensure you have provided the relevant arguments!");
            Console.ReadKey();
            Logger.ClearBuffer();
            Environment.Exit(0);
        }
    }

    internal static async Task Init()
    {
        Console.Title = $"unp4k: Initializing...";

        // Default any of the null argument declared variables.
        if (Globals.P4kFile is null) Globals.P4kFile = Defaultp4kFile;
        if (Globals.OutDirectory is null) Globals.OutDirectory = DefaultExtractionDirectory;
        if (Globals.SmelterOutDirectory is null) Globals.SmelterOutDirectory = new(Path.Join(Globals.OutDirectory.FullName, "Smelted"));
        if (Globals.Filters.Count is 0) Globals.Filters.Add("*.*");

        if (!Globals.P4kFile.Exists)
        {
            Logger.LogError($"Input path '{Globals.P4kFile.FullName}' does not exist!");
            Logger.LogError($"Make sure you have the path pointing to a Star Citizen Data.p4k file!");
            Console.ReadKey();
            Logger.ClearBuffer();
            Environment.Exit(0);
        }

        if (!Globals.OutDirectory.Exists) Globals.OutDirectory.Create();
        if (!Globals.SmelterOutDirectory.Exists) Globals.SmelterOutDirectory.Create();
    }

    internal static async Task PostInit()
    {
        Console.Title = $"unp4k: Post-Initializing...";

        // Show the user any warning if anything worrisome is detected.
        char? proceed = null;
        bool shouldCheckProceed = false;
        while (proceed is null)
        {
            if (OS.IsLinux && Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Contains("/root/"))
            {
                shouldCheckProceed = true;
                Logger.NewLine();
                Logger.LogWarn("LINUX ROOT WARNING:");
                Logger.LogWarn("unp4k has been run as root via the sudo command!");
                Logger.LogWarn("This may cause issues because it will make the app target the /root/ path!");
            }
            if (Globals.Filters.Contains("*.*") || Globals.Filters.Any(x => x.Contains(".dcb")))
            {
                if (shouldCheckProceed) Logger.NewLine();
                else shouldCheckProceed = true;
                Logger.LogWarn("ENORMOUS JOB WARNING:");
                Logger.LogWarn("unp4k has been run with filters which include Star Citizen's Game.dcb file!");
                Logger.LogWarn("Due to what the Game.dcb contains, unp4k will need to run for far longer and will requires possibly hundreds of gigabytes of free space!");
            }
            if (Globals.ForceOverwrite)
            {
                if (shouldCheckProceed) Logger.NewLine();
                else shouldCheckProceed = true;
                Logger.LogWarn("OVERWRITE ENABLED:");
                Logger.LogWarn("unp4k has been run with the overwrite option!");
                Logger.LogWarn("Overwriting files could take very long depending on your other options!");
            }
            if (shouldCheckProceed)
            {
                Logger.NewLine();
                Logger.LogInfo("Are you sure you want to proceed? y/n: ");
                proceed = Console.ReadKey().KeyChar;
                if (proceed is null || proceed != 'y' && proceed != 'n')
                {
                    Logger.LogError("Please input y for yes or n for no!");
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    Logger.ClearBuffer();
                    proceed = null;
                }
                else if (proceed is 'n')
                {
                    Logger.ClearBuffer();
                    Environment.Exit(0);
                }
            }
            else break;
        }
        Logger.NewLine(2);
    }
}
