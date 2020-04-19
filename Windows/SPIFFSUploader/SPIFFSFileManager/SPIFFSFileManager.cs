using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
//using System.Text.Json;
//using System.Text.Json.Serialization;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Threading;
using System.Diagnostics;

namespace SPIFFSUploader
{
  public enum COMMAND_CODE
  { 
    NONE = 0,
    OK = 1,
    FAIL,
    PUT_FILE,
    GET_FILE,
    DELETE_FILE,
    GET_FS,
    FORMAT,
    CREATE_FOLDER,
    DELETE_FOLDER
  }
  public class Commands : Dictionary<COMMAND_CODE, string>
  {
    public Commands()
    {
      this.Add(COMMAND_CODE.NONE,           "");
      this.Add(COMMAND_CODE.OK,             "ok");
      this.Add(COMMAND_CODE.FAIL,           "fail");
      this.Add(COMMAND_CODE.PUT_FILE,       "put_file");
      this.Add(COMMAND_CODE.GET_FILE,       "get_file");
      this.Add(COMMAND_CODE.DELETE_FILE,    "delete_file");
      this.Add(COMMAND_CODE.GET_FS,         "get_fs");
      this.Add(COMMAND_CODE.FORMAT,         "format");
      this.Add(COMMAND_CODE.CREATE_FOLDER, "create_folder"); // still not available
      this.Add(COMMAND_CODE.DELETE_FOLDER, "delete_folder"); // still not available
    }
  }
  public class ESPFFSJsonBasicContainer
  {
    public string appCode { get; private set; }
    public string version { get; private set; }
    public string author  { get; private set; }
    public string command { get; set; }
    public ESPFFSJsonBasicContainer()
    {
      appCode = "MCESPFFS";
      version = "1.0.0.0";
      author = "MC";
    }
  }
  public class ESPFFSJsonFile : ESPFFSJsonBasicContainer
  {
    public string filename { get; set; }
    public long size { get; set; }
    public ESPFFSJsonFile(string pathFilename, string remotePathFilename, COMMAND_CODE command = COMMAND_CODE.PUT_FILE) : base()
    {
      Commands cmd = new Commands();
      this.command = cmd[command];
      this.filename = remotePathFilename;
      if (File.Exists(pathFilename))
      {
        FileInfo fi = new FileInfo(pathFilename);
        size = fi.Length;
      }
    }
  }
  public class ESPFFSJsonFileDelete : ESPFFSJsonBasicContainer
  {
    public string filename { get; set; }
    public ESPFFSJsonFileDelete(string remotePathFilename) : base()
    {
      Commands cmd = new Commands();
      this.command = cmd[COMMAND_CODE.DELETE_FILE];
      this.filename = remotePathFilename;
    }
  }
  public class ESPFFSJsonCreateFolder : ESPFFSJsonBasicContainer
  {
    public string foldername { get; set; }
    public ESPFFSJsonCreateFolder(string remotefolderPath) : base()
    {
      Commands cmd = new Commands();
      this.command = cmd[COMMAND_CODE.CREATE_FOLDER];
      this.foldername = remotefolderPath;
    }
  }
  public class ESPFFSJsonDeleteFolder : ESPFFSJsonBasicContainer
  {
    public string foldername { get; set; }
    public ESPFFSJsonDeleteFolder(string remotefolderPath) : base()
    {
      Commands cmd = new Commands();
      this.command = cmd[COMMAND_CODE.DELETE_FOLDER];
      this.foldername = remotefolderPath;
    }
  }
  public class ESPFFSJson_FileSystem : ESPFFSJsonBasicContainer
  {
    public string filelist{ get; set; }
    public ESPFFSJson_FileSystem() : base()
    {
      Commands cmd = new Commands();
      this.command = cmd[COMMAND_CODE.GET_FS];
    }
  }
  public class SPIFFSFileManager
  {
    protected String Host = "";
    private int Port = 2020;
    private TcpClient client = new TcpClient();
    public delegate void ExecutingEvent(string Operation, double Percentage);
    public event ExecutingEvent Executing;

    public SPIFFSFileManager(String Host = null)
    {
      Executing += new ExecutingEvent(ExecutingDoNothingEvent);
      this.Host = Host;
    }
    private void ExecutingDoNothingEvent(string Operation, double Percentage)
    { }
    public void begin(string Host = null)
    {
      if (Host != null)
        this.Host = Host;
    }
    private bool connect(bool raiseEvent = true)
    {
      client = new TcpClient();
      var tsk = client.ConnectAsync(Host, Port);
      double counter = 0;
      while (!client.Connected && !tsk.IsCompleted)
      {
        if (raiseEvent) 
          Executing("Connecting...", counter);
        counter += 0.00001;
      }
      if (raiseEvent) 
        Executing("", 0);
      //IAsyncResult result = client.BeginConnect(Host, Port, null, null);
      return client.Connected; //result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(10));

    }
    private long getSize(string path)
    {
      FileInfo fi = new FileInfo(path);
      return fi.Length;
    }
    private bool isBinary(string path)
    {
      long length = getSize(path);
      if (length == 0) return false;

      using (StreamReader stream = new StreamReader(path))
      {
        int ch;
        while ((ch = stream.Read()) != -1)
        {
          if (isControlChar(ch))
          {
            return true;
          }
        }
      }
      return false;
    }
    private bool isBinary(byte[] byteArray)
    {
      for (int i = 0; i < byteArray.Length; i++)
        if (isControlChar(byteArray[i]))
          return true;
      return false;
    }
    private bool isControlChar(int ch)
    {
      return (ch > Chars.NUL && ch < Chars.BS)
          || (ch > Chars.CR && ch < Chars.SUB);
    }
    public static class Chars
    {
      public static char NUL = (char)0; // Null char
      public static char BS = (char)8; // Back Space
      public static char CR = (char)13; // Carriage Return
      public static char SUB = (char)26; // Substitute
    }
    private void sendHeaderMessage<T>(T classMessage)
    {
      NetworkStream stream = client.GetStream();
      JsonSerializer JS = new JsonSerializer();
      string jsonMsg = JsonConvert.SerializeObject(classMessage);
      byte[] bytes = Encoding.ASCII.GetBytes(jsonMsg);
      stream.Write(bytes, 0, bytes.Length);
    }
    private bool waitReceive(int timeoutSeconds = 5)
    {
      TimeSpan timeout = TimeSpan.FromSeconds(timeoutSeconds);
      Stopwatch s = new Stopwatch();
      s.Start();
      NetworkStream stream = client.GetStream();
      while (!stream.DataAvailable && s.Elapsed < timeout) ;
      return stream.DataAvailable;
    }
    private string getJsonResponseMessage(int timeoutSeconds = 5)
    {
      NetworkStream stream = client.GetStream();
      List<byte> responseList = new List<byte>();
      if (waitReceive(timeoutSeconds))
      {
        int BracketCounter = 0;
        while (stream.DataAvailable)
        {
          byte val = (byte)stream.ReadByte();
          responseList.Add(val);
          if (val == '{') BracketCounter++;
          if (val == '}') BracketCounter--;
          if (BracketCounter == 0) break;
          //Thread.Sleep(10);
        }
      }
      string response = Encoding.ASCII.GetString(responseList.ToArray());
      return response;
    }
    public class FileData
    {
      public string filename { get; set; }
      public int filesize { get; set; }
      public FileData(string Filename, int FileSize)
      {
        this.filename = Filename;
        this.filesize = FileSize;
      }
    }
    public class SPIFFS_FileSystem : ESPFFSJsonBasicContainer
    {
      public List<FileData> fs { get; set; }
      public int total_size { get; set; }
      public int used_size { get;  set; }
      public FileData Find(string path)
      {
        return this.fs.Find(X => X.filename == path);
      }
      public SPIFFS_FileSystem()
      {
        fs = new List<FileData>();
      }
      public int Size(string pathFilename)
      {
        int returnData = 0;
        FileData item = Find(pathFilename);
        if (item != null) returnData += item.filesize;
        return returnData;
      }
      public int Size(List<string> pathfiles)
      {
        int returnData = 0;
        for (int i = 0; i < pathfiles.Count; i++)
        {
          FileData item = Find(pathfiles[i]);
          if (item != null)
            returnData += item.filesize;
          else
            throw new Exception("Path Not found [" + pathfiles[i] + " !");
        }
        return returnData;
      }
    }
    public class FileToCopy
    {
      public FileToCopy(string pathFilename, string remotePathFilename)
      {
        this.pathFilename       = pathFilename;
        this.remotePathFileName = remotePathFilename;
      }
      public string pathFilename { get; set; }
      public string remotePathFileName { get; set; }

    }
    public SPIFFS_FileSystem GetFileSystem()
    {
      SPIFFS_FileSystem returnData = null;
      bool success = connect(false);
      if (success)
      {
        ESPFFSJson_FileSystem fileSystem = new ESPFFSJson_FileSystem();
        sendHeaderMessage(fileSystem);
        NetworkStream stream = client.GetStream();
        string response = getJsonResponseMessage();
        client.Close();
        try
        {
          SPIFFS_FileSystem data = JsonConvert.DeserializeObject<SPIFFS_FileSystem>(response);
          if (data != null && data.command == "ok")
          {
            returnData = data;
          }
        }catch
        {
          returnData = new SPIFFS_FileSystem();
        }

      }
      return returnData;
    }
    public bool Put(List<FileToCopy> files)
    {
      bool returnData = true;
      for (int i = 0; i < files.Count; i++)
      {
        returnData &= Put(files[i].pathFilename, files[i].remotePathFileName);
      }
      return returnData;
    }
    public bool Put(string pathFilename, string remotePathFilename)
    {
      bool returnData = false;
      bool success = connect(false);
      if (success)
      {
        string filename = Path.GetFileName(pathFilename);
        FileInfo fi = new FileInfo(pathFilename);
        ESPFFSJsonFile Header = new ESPFFSJsonFile(pathFilename, remotePathFilename);
        sendHeaderMessage(Header);
        NetworkStream stream = client.GetStream();
        client.NoDelay = true;

        string response = getJsonResponseMessage();
        if (response.Contains("\"command\":\"ok\""))
        {
          returnData = true;
          //if (isBinary(pathFilename))
          //{
          BinaryReader br = new BinaryReader(File.Open(pathFilename, FileMode.Open));

          //for (int i = 0; i < fi.Length; i++)
          //{
          //  stream.WriteByte(br.ReadByte());
          //  
          //  if ((i % (fi.Length / (fi.Length/ 100))) == 0)
          //    Executing("Put" + pathFilename, ((double)(i + 1)) / fi.Length);
          //}
          int bufferSize = 128;
          byte[] bytes = new byte[bufferSize];
          for (int i = 0; i < fi.Length; i+= bufferSize)
          {
            int bytesCount = br.Read(bytes, 0, bufferSize);
            try
            {
              stream.Write(bytes, 0, bytesCount);
            } catch
            {
              br.Close();
              return false;
            }
            if ((i % (fi.Length / (fi.Length/ 100))) == 0)
              Executing("Put " + pathFilename, ((double)(i + 1)) / fi.Length);
          }
          br.Close();
/*          
          }
          else
          {
            string filecontent = File.ReadAllText(pathFilename);
            byte[] ascii = Encoding.ASCII.GetBytes(filecontent);
            Executing("Put " + pathFilename, 1.0);
            stream.Write(ascii, 0, ascii.Length);
          }
*/
        }
        client.Close();
        return returnData;
      }
      else
        return returnData;

      //client.Available

    }
    public bool Get(string pathFilename, string remotePathFilename)
    {
      bool returnData = false;
      bool success = connect(false);
      if (success)
      {
        string filename = Path.GetFileName(pathFilename);
        FileInfo fi = new FileInfo(pathFilename);
        ESPFFSJsonFile Header = new ESPFFSJsonFile(pathFilename, remotePathFilename, COMMAND_CODE.GET_FILE);
        sendHeaderMessage(Header);
        NetworkStream stream = client.GetStream();

        string response = getJsonResponseMessage();
        ESPFFSJsonFile responseFile = JsonConvert.DeserializeObject<ESPFFSJsonFile>(response);
        if (responseFile!=null && responseFile.command == "ok")
        {
          List<byte> bytesList = new List<byte>();
          String CommandDescr = new Commands()[COMMAND_CODE.GET_FILE];
          for (int i = 0; i < responseFile.size; i++)
          {
            if ((i % (responseFile.size/(responseFile.size/100))) == 0)
              Executing ( "Get " + pathFilename, ((double)(i + 1)) / responseFile.size);
            if (waitReceive())
            {
              bytesList.Add((byte)stream.ReadByte());
            }
          }
          Executing("", 1);
          byte[] bytes = bytesList.ToArray();

          //if (isBinary(bytes))
          //{
          BinaryWriter bw = new BinaryWriter(File.Open(pathFilename, FileMode.Create));
          bw.Write(bytes);
          bw.Close();
          //} else
          //{
          //  StreamWriter sw = new StreamWriter(pathFilename, false, Encoding.ASCII);
          //
          //}
          returnData = true;
        }
        client.Close();
        return returnData;
      }
      else
        return returnData;

      //client.Available

    }
    public bool Delete(string remotePathFilename)
    {
      
      bool success = connect(false);
      if (success)
      {
        ESPFFSJsonFileDelete Header = new ESPFFSJsonFileDelete(remotePathFilename);
        sendHeaderMessage(Header);
        NetworkStream stream = client.GetStream();

        string response = getJsonResponseMessage();
        client.Close();
        if (response.Contains("\"command\":\"ok\""))
        {
        return true;
        }
      }
      return false;
    }
    public bool CreateFolder(string remoteFolderPath)
    {

      bool success = connect(false);
      if (success)
      {
        ESPFFSJsonCreateFolder Header = new ESPFFSJsonCreateFolder(remoteFolderPath);
        sendHeaderMessage(Header);
        NetworkStream stream = client.GetStream();

        string response = getJsonResponseMessage();
        client.Close();
        if (response.Contains("\"command\":\"ok\""))
        {
          return true;
        }
      }
      return false;
    }
    public bool RemoveFolder(string remoteFolderPath)
    {

      bool success = connect(false);
      if (success)
      {
        ESPFFSJsonDeleteFolder Header = new ESPFFSJsonDeleteFolder(remoteFolderPath);
        sendHeaderMessage(Header);
        NetworkStream stream = client.GetStream();

        string response = getJsonResponseMessage();
        client.Close();
        if (response.Contains("\"command\":\"ok\""))
        {
          return true;
        }
      }
      return false;
    }
    public bool Format()
    {
      bool success = connect(false);
      if (success)
      {
        ESPFFSJsonBasicContainer Header = new ESPFFSJsonBasicContainer();
        Header.command = new Commands()[COMMAND_CODE.FORMAT];
        sendHeaderMessage(Header);
        NetworkStream stream = client.GetStream();

        string response = getJsonResponseMessage();
        client.Close();
        if (response.Contains("\"command\":\"ok\""))
        {
          return true;
        }
      }
      return false;
    }
  }
}
