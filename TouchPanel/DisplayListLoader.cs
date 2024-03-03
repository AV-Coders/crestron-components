using AVCoders.Crestron.Core;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Renci.SshNet;
using Directory = Crestron.SimplSharp.CrestronIO.Directory;

namespace AVCoders.Crestron.TouchPanel;

public class DisplayListLoader
{
    private const string RemoteTpPath = "/display/tp.vtz";
    private readonly TsxCcsUcCodec100EthernetReservedSigs _ethernetExtender;
    private readonly string _username;
    private readonly string _password;
    private readonly string _name;
    private readonly string _filePath;
    private bool _fileLoaded;

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
        if (_fileLoaded)
            return;
        if (args.Event != eSigEvent.StringChange)
            return;
        if (args.Sig != _ethernetExtender.IpAddressFeedback)
            return;
        
        Log($"Uploading file to TP at IP {_ethernetExtender.IpAddressFeedback.StringValue}");
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
        _fileLoaded = true;
    }

    private void Log(string message) => CrestronConsole.PrintLine($"{DateTime.Now} - {_name} - DisplayListLoader - {message}");
}