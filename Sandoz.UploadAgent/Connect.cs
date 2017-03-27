using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tamir.SharpSsh;

namespace Sandoz.UploadAgent
{
    //public class Connect
    //{
    //    public static void RunExample()
    //    {
    //        try
    //        {
    //            SshConnectionInfo oConnect = new SshConnectionInfo();
    //            oConnect.Host = "";
    //            oConnect.User = "";
    //            oConnect.Pass = "";

    //            SshTransferProtocolBase oProtocol;

    //            oProtocol = new Sftp(oConnect.Host, oConnect.User);

    //            oProtocol.Password = oConnect.Pass;
    //            oProtocol.OnTransferStart += new FileTransferEvent(sshCp_OnTransferStart);
    //            oProtocol.OnTransferProgress += new FileTransferEvent(sshCp_OnTransferProgress);
    //            oProtocol.OnTransferEnd += new FileTransferEvent(sshCp_OnTransferEnd);

    //            oProtocol.Connect();

    //            string sRemoteFilePath = "";
    //            string sLocalFilePath = "";
    //            oProtocol.Get(sRemoteFilePath, sLocalFilePath);
                   

    //            oProtocol.Close();
    //        }
    //        catch (Exception e)
    //        {
    //            Console.WriteLine(e.Message);
    //        }
    //    }

    //    private static void sshCp_OnTransferStart(string src, string dst, int transferredBytes, int totalBytes, string message)
    //    {
            
    //    }

    //    private static void sshCp_OnTransferProgress(string src, string dst, int transferredBytes, int totalBytes, string message)
    //    {
            
    //    }

    //    private static void sshCp_OnTransferEnd(string src, string dst, int transferredBytes, int totalBytes, string message)
    //    {
            
    //    }
    //}

    //public struct SshConnectionInfo
    //{
    //    public string Host;
    //    public string User;
    //    public string Pass;
    //    public string IdentityFile;
    //}
}
