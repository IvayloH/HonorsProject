using System.IO;

/// <summary>
/// Simple class for writing to a file. 
/// Used to debug messages to a file to help debugging and testing.
/// </summary>
public class FileLogger
{
    private string errorFileName;
    private string debugfileName;
    private bool outputToErrorLogOnly;
    private bool loggingErrorForFirstTime;

    public FileLogger(bool outputToErrorLogOnly=false)
    {
        debugfileName = "debugLog.txt";
        errorFileName = "errorLog.txt";
        loggingErrorForFirstTime = true;
        this.outputToErrorLogOnly = outputToErrorLogOnly;

        if (!outputToErrorLogOnly)
            ClearFile(debugfileName);
    }

    /// <summary>
    /// Writes the passed in string message to a text file - 'errorLog.txt', overwritting the old contents.
    /// </summary>
    /// <param name="log">The message to be saved to the file.</param>
    public void LogError(string log)
    {
        if (loggingErrorForFirstTime)
        {
            ClearFile(errorFileName);
            loggingErrorForFirstTime = false;
        }

        StreamWriter writer = new StreamWriter(errorFileName, true);
        writer.WriteLine(log);
        writer.Close();
    }
    /// <summary>
    /// Writes the passed in string message to a text file - 'debugLog.txt', without overwritting the old contents.
    /// </summary>
    /// <param name="log">The message to be saved to the file.</param>
    public void Log(string log)
    {
        if (outputToErrorLogOnly)
            return;
        StreamWriter writer = new StreamWriter(debugfileName, true);
        writer.WriteLine(log);
        writer.Close();
    }

    /// <summary>
    /// Overwrites the contents of the specified file with an empty string.
    /// </summary>
    /// <param name="path">The file to be cleared.</param>
    private void ClearFile(string path)
    {
        StreamWriter writer = new StreamWriter(path, false);
        writer.Write("");
        writer.Close();
    }
}