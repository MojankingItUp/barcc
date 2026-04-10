using System.Diagnostics;
using barcc;
using static System.Console;

WriteLine("Disclamer!!! Only works on Linux. And maybe MAC.");

if (args.Length == 0 || args[0] == "-h" || args[0] == "")
{
    WriteLine("Options:");
    WriteLine("-h                == help");
    WriteLine("1. argument       == amount of GPUs");
    WriteLine("2. argument       == end frame for the animation");
    WriteLine("3. argument       == device backend (HIP, CUDA, etc...)");
    WriteLine("4. argument       == enable debug printing");
    WriteLine("5. argument       == the path to the project that we're going to render");
    WriteLine("6. argument       == output path for the images to be rendered");
    WriteLine("7. argument       == path to Blender's executable file");
    WriteLine();
    WriteLine("Supported device backends:");
    WriteLine("1. CUDA");
    WriteLine("2. OPTIX");
    WriteLine("3. HIP");
    WriteLine("4. METAL");
    WriteLine("5. ONEAPI");
    return;
}

int amountOfGpus = int.Parse(args[0]);
int endFrame = int.Parse(args[1]);
string deviceBackend = args[2]; // HIPRT or CUDA or whatever
bool debug = bool.Parse(args[3]);
string projectPath = Path.GetFullPath(args[4]);
string outputPath = Path.GetFullPath(args[5]);
string blenderPath = Path.GetFullPath(args[6]);

Process[] processes = new Process[amountOfGpus];
string[] blenderCommands = new string[amountOfGpus];

WriteLine("Commands to run:");
for (int i = 0; i < amountOfGpus; i++)
{
    blenderCommands[i] = CommandBuilder.BuildCommand(i, i, amountOfGpus, endFrame, "CYCLES", deviceBackend, outputPath, blenderPath, projectPath);
    WriteLine();
    WriteLine(blenderCommands[i]);
    WriteLine();
}

WriteLine($"Starting Blender on: {amountOfGpus} GPUs");

using FileSystemWatcher watcher = new(outputPath);
watcher.NotifyFilter = NotifyFilters.Attributes
                       | NotifyFilters.CreationTime
                       | NotifyFilters.DirectoryName
                       | NotifyFilters.FileName
                       | NotifyFilters.LastAccess
                       | NotifyFilters.LastWrite
                       | NotifyFilters.Security
                       | NotifyFilters.Size;

//watcher.Changed += OnWatcherOnChanged;
watcher.Created += OnWatcherOnChanged;

watcher.Filter = "*.avif";
watcher.EnableRaisingEvents = true;

for (int i = 0; i < amountOfGpus; i++)
{
    ProcessStartInfo proc = new();
    proc.FileName = "/bin/bash";
    proc.ArgumentList.Add("-c");
    proc.ArgumentList.Add(blenderCommands[i]);
    proc.UseShellExecute = false;
    proc.RedirectStandardOutput = !debug;
    proc.RedirectStandardError = !debug;
    
    processes[i] = Process.Start(proc)!;

    if (!debug)
    {
        processes[i].BeginOutputReadLine();
        processes[i].BeginErrorReadLine();
    }
}

foreach (Process p in processes)
{
    p.WaitForExit();
}

watcher.Created -= OnWatcherOnChanged;
return;

void OnWatcherOnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
{
    int rendered = Directory.GetFiles(outputPath, "*.avif").Length;
    WriteLine($"Rendered {rendered} out of {endFrame}");
}