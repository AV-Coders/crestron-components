using Crestron.SimplSharp;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace AVCoders.Crestron.Core;

public class SSH
{
    public static string RunCommand(SshClient sshClient, string command, string name)
    {
        try
        {
            Log($"{name} - Connecting SSH");
            
            sshClient.Connect();
            
            Log($"{name} - Running SSH command: {command}");
            var result = sshClient.RunCommand(command).Result;
            
            Log($"{name} - Disconnecting");
            sshClient.Disconnect();
            return result;
        }
        catch (SshAuthenticationException e)
        {
            Log($"{name} - Authentication Error");
            Log(e.ToString());
            return e.ToString();
        }
        catch (SshException e)
        {
            Log($"{name} - Unhandled SSH Exception");
            Log(e.ToString());
            return e.ToString();
        }
        catch (SocketException e)
        {
            Log($"{name} - Socket Exception");
            Log(e.ToString());
            return e.ToString();
        }
    }

    public static bool UploadFile(SftpClient sftpClient, string localFile, string remotePath, string name)
    {
        try
        {
            Log($"{name} - Connecting SFTP");
            sftpClient.Connect();
            Log($"{name} - Sending {localFile} to {remotePath}");
            var fileStream = File.OpenRead(localFile);
            sftpClient.UploadFile(fileStream, remotePath);
            fileStream.Close();
            Log($"{name} - Disconnecting SFTP");
            sftpClient.Disconnect();
        }
        catch (Exception e)
        {
            Log($"{name} - Error uploading files: {e.GetType()}");
            Log(e.Message);
            Log(e.StackTrace ?? string.Empty);
            return false;
        }
        return true;
    }
    
    private static void Log(string message) => CrestronConsole.PrintLine($"{DateTime.Now} - SSH - {message}");
}