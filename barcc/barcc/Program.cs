using System.Diagnostics;
using barcc;

if (args[0] == "-h")
{
    Console.WriteLine("Options:");
    Console.WriteLine("-h                == help");
    Console.WriteLine("0. argument       == amount of GPUs");
    Console.WriteLine("1. argument       == device backend (HIPRT, CUDA, etc...)");
    Console.WriteLine("2. argument       == output path for the images to be rendered");
    Console.WriteLine("3. argument       == path to Blender's executable file");
    Console.WriteLine("4. argument       == end frame for the animation");
    Console.WriteLine("5. argument       == the path to the project that we're going to render");
}

int amountOfGpus = int.Parse(args[0]);
int endFrame = int.Parse(args[4]);
string deviceBackend = args[1]; // HIPRT or CUDA or whatever
string outputPath = Path.GetFullPath(args[2]);
string blenderPath = Path.GetFullPath(args[3]);
string projectPath = Path.GetFullPath(args[5]);
bool debug = bool.Parse(args[6]);

Process[] processes = new Process[amountOfGpus];
string[] blenderCommands = new string[amountOfGpus];

for (int i = 0; i < amountOfGpus; i++)
{
    blenderCommands[i] = CommandBuilder.BuildCommand(i, i, amountOfGpus, endFrame, "CYCLES", deviceBackend, outputPath, blenderPath, projectPath);
    Console.WriteLine(blenderCommands[i] + "\n");
}

Console.WriteLine($"Starting Blender on: {amountOfGpus} GPUs");

for (int i = 0; i < amountOfGpus; i++)
{
    ProcessStartInfo proc = new();
    proc.FileName = "/bin/bash";
    proc.ArgumentList.Add("-c");
    proc.ArgumentList.Add(blenderCommands[i]);
    proc.UseShellExecute = false;
    proc.RedirectStandardOutput = debug ^= true;
    proc.RedirectStandardError = debug ^= true;
    
    processes[i] = Process.Start(proc)!;
}

foreach (Process p in processes)
{
    p.WaitForExit();
}