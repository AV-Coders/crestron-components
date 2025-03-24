using AVCoders.Core;
using Crestron.SimplSharpPro.DeviceSupport;
using Renci.SshNet;
using Directory = Crestron.SimplSharp.CrestronIO.Directory;
using SshClient = Renci.SshNet.SshClient;

namespace AVCoders.Crestron.TouchPanel;

/// <summary>
/// Uploads the vtz to the TouchPanel file once per instantiation.
/// Add the VTZ files to your project in the same way you would add the smart graphics SGD files. 
/// </summary>
public class DisplayListLoader : LogBase
{
    private const string RemoteTpPath = "/display/tp.vtz";
    private readonly TsxCcsUcCodec100EthernetReservedSigs _ethernetExtender;
    private readonly string _username;
    private readonly string _password;
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
        : base(name)
    {
        _ethernetExtender = ethernetExtender;
        _username = username;
        _password = password;
        _filePath = $"{Directory.GetApplicationDirectory()}/{fileName}";

        _ethernetExtender.DeviceExtenderSigChange += EthernetExtenderSigChange;
        Debug("Ready to load");
    }

    private void EthernetExtenderSigChange(DeviceExtender currentDeviceExtender, SigEventArgs args)
    {
        if (args.Event != eSigEvent.StringChange)
            return;
        if (args.Sig != _ethernetExtender.IpAddressFeedback)
            return;
        
        Debug($"Uploading - Local path: {_filePath}, TP Path: {RemoteTpPath}, TP IP: {_ethernetExtender.IpAddressFeedback.StringValue}");
        var uploadSuccess = Ssh.UploadFile(
            new SftpClient(_ethernetExtender.IpAddressFeedback.StringValue, 22, _username, _password),
            _filePath, RemoteTpPath, Name);

        if (!uploadSuccess)
        {
            Debug("DisplayList Upload failure");
            return;
        }
        Debug("Issuing project load");
        Ssh.RunCommand(
            new SshClient(_ethernetExtender.IpAddressFeedback.StringValue, 22, _username, _password), 
            "PROJECTLOAD", Name);
        Debug("Done!");
        _ethernetExtender.DeviceExtenderSigChange -= EthernetExtenderSigChange;
    }
}