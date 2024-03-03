using AVCoders.Crestron.Core;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Renci.SshNet;
using Directory = Crestron.SimplSharp.CrestronIO.Directory;

namespace AVCoders.Crestron.TouchPanel;

/// <summary>
/// Uploads the vtz to the TouchPanel file once per instantiation.
/// Add the VTZ files to your project in the same way you would add the smart graphics SGD files. 
/// </summary>
public class DisplayListLoader
{
    private const string RemoteTpPath = "/display/tp.vtz";
    private readonly TsxCcsUcCodec100EthernetReservedSigs _ethernetExtender;
    private readonly string _username;
    private readonly string _password;
    private readonly string _name;
    private readonly string _filePath;

    /// <summary>
    /// Creates an instance of the file uploader that's automatically loads the file
    /// </summary>
    /// <param name="name">The name used for logging purposes</param>
    /// <param name="ethernetExtender">
    /// The touchpanel's ExtenderEthernetReservedSigs.
    /// Remember to call _panelInstance.ExtenderEthernetReservedSigs.Use() before registering the panel
    /// </param>
    /// <param name="fileName">The .vtz filename (include .vtz)</param>
    /// <param name="username">Touchpanel's username for SSH and SFTP access</param>
    /// <param name="password">Touchpanel's password for SSH and SFTP access</param>
    public DisplayListLoader(string name, TsxCcsUcCodec100EthernetReservedSigs ethernetExtender, string fileName, string username, string password)
    {
        _name = name;
        _ethernetExtender = ethernetExtender;
        _username = username;
        _password = password;
        _filePath = $"{Directory.GetApplicationDirectory()}/{fileName}";

        _ethernetExtender.DeviceExtenderSigChange += EthernetExtenderSigChange;
        Log("Ready to load");
    }

    private void EthernetExtenderSigChange(DeviceExtender currentDeviceExtender, SigEventArgs args)
    {
        if (args.Event != eSigEvent.StringChange)
            return;
        if (args.Sig != _ethernetExtender.IpAddressFeedback)
            return;
        
        Log($"Uploading - Local path: {_filePath}, TP Path: {RemoteTpPath}, TP IP: {_ethernetExtender.IpAddressFeedback.StringValue}");
        var uploadSuccess = SSH.UploadFile(
            new SftpClient(_ethernetExtender.IpAddressFeedback.StringValue, 22, _username, _password),
            _filePath, RemoteTpPath, _name);

        if (!uploadSuccess)
        {
            Log("DisplayList Upload failure");
            return;
        }
        Log("Issuing project load");
        SSH.RunCommand(
            new SshClient(_ethernetExtender.IpAddressFeedback.StringValue, 22, _username, _password), 
            "PROJECTLOAD", _name);
        Log("Done!");
        _ethernetExtender.DeviceExtenderSigChange -= EthernetExtenderSigChange;
    }

    private void Log(string message) => CrestronConsole.PrintLine($"{DateTime.Now} - {_name} - DisplayListLoader - {message}");
}