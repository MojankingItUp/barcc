using System.Diagnostics;
using barcc;
using static System.Console;

WriteLine("Disclamer!!! Only works on Linux. And maybe MAC.");

if (args.Length == 0 || args[0] == "-h" || args[0] == "")
{
    WriteLine("Options:");
    WriteLine("-h                == help");
    WriteLine("1. argument       == amount of GPUs");
    WriteLine("2. argument       == start frame for the animation");
    WriteLine("3. argument       == end frame for the animation");
    WriteLine("4. argument       == device backend (HIP, CUDA, etc...)");
    WriteLine("5. argument       == enable debug printing");
    WriteLine("6. argument       == the path to the project that we're going to render");
    WriteLine("7. argument       == output path for the images to be rendered");
    WriteLine("8. argument       == path to Blender's executable file");
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
int startFrame = int.Parse(args[1]);
int endFrame = int.Parse(args[2]);
string deviceBackend = args[3]; // HIPRT or CUDA or whatever
bool debug = bool.Parse(args[4]);
string projectPath = Path.GetFullPath(args[5]);
string outputPath = Path.GetFullPath(args[6]);
string blenderPath = Path.GetFullPath(args[7]);

if (debug)
{
    WriteLine("amountOfGpus: " + amountOfGpus);
    WriteLine("endFrame: " + endFrame);
    WriteLine("deviceBackend: " + deviceBackend);
    WriteLine("debug: " + debug);
    WriteLine("projectPath: " + projectPath);
    WriteLine("outputPath: " + outputPath);
    WriteLine("blenderPath: " + blenderPath);
    WriteLine("startFrame: " + startFrame);
}

Process[] processes = new Process[amountOfGpus];

using FileSystemWatcher watcher = new(outputPath);
watcher.NotifyFilter = NotifyFilters.Attributes
                       | NotifyFilters.CreationTime
                       | NotifyFilters.DirectoryName
                       | NotifyFilters.FileName
                       | NotifyFilters.LastAccess
                       | NotifyFilters.LastWrite
                       | NotifyFilters.Security
                       | NotifyFilters.Size;

watcher.Created += OnWatcherOnChanged;

watcher.Filter = "*.avif";
watcher.EnableRaisingEvents = true;

WriteLine($"Starting Blender on: {amountOfGpus} GPUs");
WriteLine("Commands to run:");
for (int i = 0; i < amountOfGpus; i++)
{
    string blenderCommand = CommandBuilder.BuildCommand(i, startFrame + i, amountOfGpus, endFrame, "CYCLES", deviceBackend, outputPath, blenderPath, projectPath);
    WriteLine();
    WriteLine(blenderCommand);
    WriteLine();
    
    ProcessStartInfo proc = new();
    proc.FileName = "/bin/bash";
    proc.ArgumentList.Add("-c");
    proc.ArgumentList.Add(blenderCommand);
    proc.UseShellExecute = false;
    // We want to redirect standard out/err. It's redirected to /dev/null.
    // So when we set debug to "false", what we're doing is just
    // setting redirect to /dev/null true.
    proc.RedirectStandardOutput = !debug;
    proc.RedirectStandardError = !debug;
    
    processes[i] = Process.Start(proc)!;

    if (!debug)
    {
        // If debug is false, then we drain the standard out/err
        // as is this program's stack will overflow.
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
    WriteLine($"Rendered {startFrame + rendered} out of {endFrame}");
}