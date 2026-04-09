namespace barcc;

public static class CommandBuilder
{
    public static string BuildCommand(int gpuIndex, int offset, int frameJump, int endFrame, string renderMethod, string deviceBackend, string outputPath, string blenderPath, string projectPath)
    {
        return $"HIP_VISIBLE_DEVICES={gpuIndex} '{blenderPath}' -b '{projectPath}' -o '{outputPath}/frame_####' -E {renderMethod} -s {offset} -e {endFrame} --frame-jump {frameJump} -a -- --cycles-device {deviceBackend}";
    }
}