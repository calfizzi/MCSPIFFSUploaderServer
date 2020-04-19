using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using MCExtensions;
using Utility;


namespace SPIFFSUploader
{
  public partial class frmSPIFFSUploader : Form
  {
    private SPIFFSFileManager ESP_SPIFFS                    = new SPIFFSFileManager();
    private SPIFFSFileManager.SPIFFS_FileSystem ESP_fsInfo  = null;
    private object dragdata = null;

    private int _CopyFullSize;
    private int CopyFullSize { 
      get { return _CopyFullSize; } 
      set { _CopyFullSize = value; CurrentFileSize = 0; SumFileSize = 0; } 
    }
    private int CurrentFileSize = 0;
    private int SumFileSize = 0;

    public frmSPIFFSUploader()
    {
      InitializeComponent();
      ESP_SPIFFS.Executing += new SPIFFSFileManager.ExecutingEvent(ESP_SPIFFS_Executing);
    }
    private void frmSPIFFSUploader_Load(object sender, EventArgs e)
    {
      string dir = Path.Combine(Environment.CurrentDirectory, "..\\..");
      DirectoryInfo dir_info = new DirectoryInfo("d:\\");

      trvDirectory.LoadFromDirectory(dir_info.FullName, 0, 1);
      //trvDirectory.ExpandAll();
      trvDirectory.SelectedNode = trvDirectory.Nodes[0];
      setBitmapConnectionStatus(false);

      this.MinimumSize = new Size(600, 300);
      AdjustForSize();
      //this.MaximumSize = new Size(this.Width, Int32.MaxValue);
    }
    private void frmSPIFFSUploader_Resize(object sender, EventArgs e)
    {
      AdjustForSize();
    }
    private void setBitmapConnectionStatus(bool Connected)
    {
      Bitmap bmp = new Bitmap(picConnect.ClientSize.Width, picConnect.ClientSize.Height);
      Graphics g = Graphics.FromImage(bmp);
      Brush brush = Connected ? new System.Drawing.SolidBrush(Color.FromArgb(0, 224, 0)) : Brushes.Red;
      g.FillEllipse(brush, 1, 1, bmp.Width - 1, bmp.Height - 1);
      picConnect.Image = bmp;
    }
    private void ESP_SPIFFS_Executing(string Operation, double Percentage)
    {

      float progressivePercentage = CopyFullSize == 0 ? 0 : (float)SumFileSize / CopyFullSize;
      float loadPercentage        = CopyFullSize == 0 ? 0 : (float)CurrentFileSize / CopyFullSize;
      int value = 0;
      if (Operation == "Connecting...")
        value = (int)(Percentage * 1000);
      else
        value = (int)(progressivePercentage * 1000 + loadPercentage* Percentage * 1000);
      if (value >= 1) 
        pbPercentage.Value = value - 1;
      //pbPercentage.Refresh();
      //pbPercentage.Update();
      if (value == 1000)
        value = 0;
      pbPercentage.Value = value;
      lblStatus.Text = Operation;
      lblStatus.Update();
    }
    private  void AdjustForSize()
    {
      trvDirectory.Left = 10;
      trvDirectory.Width = this.ClientSize.Width / 2 - 10 - 10;
      trvDirectory.Height = this.ClientSize.Height - trvDirectory.Top - 45;
      SPIFFS.Left = trvDirectory.Left + trvDirectory.Width + 10;
      SPIFFS.Width = this.ClientSize.Width / 2 - 10 ;
      SPIFFS.Height = trvDirectory.Height;

      lblSPIFFS_Info.Left = SPIFFS.Left;
      txtPath.Width = trvDirectory.Left + trvDirectory.Width - txtPath.Left;

    }
    private void ClientSend()
    {
      //SPIFFSFileManager ESPFFS = new SPIFFSFileManager("192.168.0.25");
      ESP_SPIFFS.begin(txtHostName.Text);
      ESP_SPIFFS.Put(@"d:\Source\C#\Test_SimpleServer\Test.txt", "Test.txt");
      /*
      TcpClient client = new TcpClient();
      client.Connect("192.168.0.25", 2020);
      string msg = "ciao\n";
      client.GetStream().Write(Encoding.UTF8.GetBytes(msg), 0, msg.Length);
      client.Close();
      */
    }
    private TreeNode SPIFFS_GenerateFolderPath(TreeNode nodeparent, string path)
    {
      TreeNode node;
      if (path.Contains("\\"))
      {
        node = SPIFFS_GenerateFolderPath(nodeparent, Path.GetDirectoryName(path));
        node = node.Nodes.Add(Path.GetFileName(path));
        nodeparent.ImageIndex = 0;
        nodeparent.SelectedImageIndex = 0;
        node.ImageIndex = 1;
        node.SelectedImageIndex = 1;
      }
      else
      {
        if (SPIFFS.Nodes.FindTreeNodeByFullPath("SPIFFS\\" + path) == null)
        {
          node = nodeparent.Nodes.Add(path);
          nodeparent.ImageIndex = 0;
          nodeparent.SelectedImageIndex = 0;
          node.ImageIndex = 1;
          node.SelectedImageIndex = 1;
        }
        else
          node = SPIFFS.Nodes.FindTreeNodeByFullPath("SPIFFS\\" + path);
      }
      return node;
    }
    private void btnConnect_Click(object sender, EventArgs e)
    {
      Cursor = Cursors.WaitCursor;
      SPIFFS.Nodes.Clear();
      //SPIFFSFileManager ESPFFS = new SPIFFSFileManager(txtHostName.Text);
      ESP_SPIFFS.begin(txtHostName.Text);
      ESP_fsInfo = ESP_SPIFFS.GetFileSystem();
      if (ESP_fsInfo != null)
      {
        setBitmapConnectionStatus(true);
        lblSPIFFS_Info.Text = "Used memory: " + ESP_fsInfo.used_size + " Bytes on " + ESP_fsInfo.total_size + " Bytes";
        TreeNode file_node = null;
        //if (ESP_fsInfo.fs.Count > 0)
        file_node = SPIFFS.Nodes.Add("SPIFFS");
        file_node.ImageIndex = 0;
        file_node.SelectedImageIndex = 0;
        for (int i = 0; i < ESP_fsInfo.fs.Count; i++)
        {
          string pathfilename = ESP_fsInfo.fs[i].filename.Substring(1);
          pathfilename = pathfilename.Replace("/", "\\");
          string path = Path.GetDirectoryName(pathfilename);
          string filename = Path.GetFileName(pathfilename);
          TreeNode currentNode = file_node;
          if (path != "")
          {
            currentNode = SPIFFS_GenerateFolderPath(file_node, path);
            //if (SPIFFS.Nodes.FindTreeNodeByFullPath("SPIFFS\\" + path)==null)
            //  currentNode = file_node.Nodes.Add(path);
            //else
            //  currentNode = SPIFFS.Nodes.FindTreeNodeByFullPath("SPIFFS\\" + path);
          }
          TreeNode lastNode = currentNode.Nodes.Add(i.ToString(), filename + " ( " + ESP_fsInfo.fs[i].filesize.ToString() + " bytes )");
          lastNode.ImageIndex = 1;
          lastNode.SelectedImageIndex = 1;
          currentNode.ImageIndex = 0;
          currentNode.SelectedImageIndex = 0;
        }
        SPIFFS.ExpandAll();
      }
      Cursor = Cursors.Default;

    }
    private void trvDirectory_AfterExpand(object sender, TreeViewEventArgs e)
    {
      DirectoryInfo dir_info = new DirectoryInfo(e.Node.FullPath);
      trvDirectory.UpdateDirectoryNodes(dir_info, e.Node, 0, 1);

      //trvDirectory.
    }
    private void AddNodeToList (List<SPIFFSFileManager.FileToCopy> files, TreeNode node)
    {
      DirectoryInfo dir_info = new DirectoryInfo(node.FullPath);
      if (dir_info.Attributes != FileAttributes.Directory)
        files.Add(new SPIFFSFileManager.FileToCopy(node.FullPath, node.Text));
    }
    private void AddFolderToList(List<SPIFFSFileManager.FileToCopy> files, TreeNode node)
    {
      for (int i = 0; i < node.Nodes.Count; i++)
      {
        TreeNode currentNode = node.Nodes[i];
        DirectoryInfo dir_info = new DirectoryInfo(currentNode.FullPath);

        if (dir_info.Attributes == FileAttributes.Directory)
        {
          AddFolderToList(files, currentNode);
        }
        else
        {
          files.Add(new SPIFFSFileManager.FileToCopy(currentNode.FullPath, currentNode.Text));
        }
      }

    }
    private void btnSynchronizeSPIFFS_Click(object sender, EventArgs e)
    {
      //ESP_SPIFFS.begin(txtHostName.Text);
      List<SPIFFSFileManager.FileToCopy> files = new List<SPIFFSFileManager.FileToCopy>();
      TreeNode currentNode = trvDirectory.SelectedNode;
      if (currentNode.Nodes.Count == 0)
      {
        AddNodeToList(files, currentNode);
      }else
      {
        AddFolderToList(files, currentNode);
      }
      /*
      foreach (System.Collections.DictionaryEntry obj in trvDirectory.SelNodes)
      {
        TreeNode node = ((MWControlSuite.MWTreeNodeWrapper)obj.Value).Node as TreeNode;
        files.Add(new SPIFFSFileManager.FileToCopy(node.FullPath, node.Text));
      }*/

      //ESPFFS = new SPIFFSFileManager(txtHostName.Text);
      //ESPFFS.Put(files);

    }
    private void cmbdrive_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (cmbdrive.SelectedIndex > 0)
      {
        DirectoryInfo dir_info = new DirectoryInfo(cmbdrive.Text.Replace("(","").Replace(")","") + "\\");
        trvDirectory.Nodes.Clear();
        trvDirectory.LoadFromDirectory(dir_info.FullName, 0, 1);
        //trvDirectory.ExpandAll();
        trvDirectory.SelectedNode = trvDirectory.Nodes[0];
      }

    }
    private void trvDirectory_MouseDown(object sender, MouseEventArgs e)
    {
      dragdata = null;
    }
    private void trvDirectory_MouseMove(object sender, MouseEventArgs e)
    {
      if (e.Button ==MouseButtons.Left && dragdata==null)
      {
        Point ScreenPoint = new Point(e.X, e.Y);
        //Point ControlPoint = trvDirectory.PointToClient(ScreenPoint);
        TreeNode current = trvDirectory.GetNodeAt(ScreenPoint);
        trvDirectory.DoDragDrop(current, DragDropEffects.Copy | DragDropEffects.Move);

      }
    }
    private void SPIFFS_InsertTree(TreeNode sourceNodeParent, TreeNode destinationNode)
    {
      TreeNode node = destinationNode.Nodes.Add(sourceNodeParent.Text);
      string path = node.FullPath.Replace("SPIFFS\\", "/").Replace("\\", "/");
      if (sourceNodeParent.Nodes.Count == 0) // is as file
      {
        int FileSize = (int)new System.IO.FileInfo(sourceNodeParent.FullPath).Length;
        node.ImageIndex = 1;
        node.SelectedImageIndex = 1;

        FileInfo fi = new FileInfo(sourceNodeParent.FullPath);
        CurrentFileSize = (int)fi.Length;
        ESP_SPIFFS.Put(sourceNodeParent.FullPath, path);
        SumFileSize += CurrentFileSize;
        ESP_fsInfo.fs.Add(new SPIFFSFileManager.FileData(path, FileSize));
      }
      else
      {
        node.ImageIndex = 0;
        node.SelectedImageIndex = 0;
        ESP_SPIFFS.CreateFolder(path);
      }
      for (int i = 0; i < sourceNodeParent.Nodes.Count; i++)
      {
        SPIFFS_InsertTree(sourceNodeParent.Nodes[i], node);
      }

    }
    private bool SPIFFS_IsDroppable(DragEventArgs e)
    {
      bool returnData = false;
      if (e.Data.GetDataPresent(typeof(TreeNode)))
      {
        TreeNode source = e.Data.GetData(typeof(TreeNode)) as TreeNode;
        dragdata = source;
        if (SPIFFS.Nodes.Count > 0 && source.TreeView == trvDirectory)
          returnData = true;
      }
      if (e.Data.GetDataPresent(DataFormats.FileDrop))
        returnData = true;
      if (returnData)
        e.Effect = DragDropEffects.Copy;
      else
        e.Effect = DragDropEffects.None;
      return returnData;
    }
    private void SPIFFS_DragEnter(object sender, DragEventArgs e)
    {
      if (SPIFFS_IsDroppable(e))
      {
        TreeNode source = null;
        string[] files = null;
        if (e.Data.GetDataPresent(typeof(TreeNode)))
          source = e.Data.GetData(typeof(TreeNode)) as TreeNode;
        else if (e.Data.GetDataPresent(DataFormats.FileDrop))
          files = (string[])e.Data.GetData(DataFormats.FileDrop);
      }
    }
    private void SPIFFS_DragOver(object sender, DragEventArgs e)
    {
      Point ScreenPoint = new Point(e.X, e.Y);
      Point ControlPoint = SPIFFS.PointToClient(ScreenPoint);
      TreeNode current = SPIFFS.GetNodeAt(ControlPoint);
      if (SPIFFS_IsDroppable(e))
      {
        if (current != null && current.IsFolder())
        {
          SPIFFS.SelectedNode = current;
          this.Text = current.Text;
          e.Effect = DragDropEffects.Copy;
        }
        else e.Effect = DragDropEffects.None;
        SPIFFS.Focus();
      }
      else e.Effect = DragDropEffects.None;
      /*
      if (SPIFFS_IsDroppable(e))
      {
        if (current != null && current.IsFolder())
        {
          SPIFFS.SelectedNode = current;
        }
        else if (current != null && current.Parent != null)
          SPIFFS.SelectedNode = current.Parent;
        else if (SPIFFS.Nodes.Count > 0)
          SPIFFS.SelectedNode = SPIFFS.Nodes[0];
        else
          SPIFFS.SelectedNode = null;
        SPIFFS.Focus();
      }
      */
    }
    private void SPIFFS_DragDrop(object sender, DragEventArgs e)
    {
      Cursor = Cursors.WaitCursor;
      if (SPIFFS_IsDroppable(e))
      {
        if (e.Data.GetDataPresent(typeof(TreeNode)))
        {
          TreeNode source = e.Data.GetData(typeof(TreeNode)) as TreeNode;
          CopyFullSize = (int)trvDirectory_Size(source.GetAllNodesFullPath(1));
          SPIFFS_InsertTree(source, SPIFFS.SelectedNode);
          SPIFFS.SelectedNode.Expand();
        }
        else if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
          List<string> files = ((string[])e.Data.GetData(DataFormats.FileDrop)).ToList();
          for (int i = 0; i < files.Count; i++)
          {
            FileAttributes attr = File.GetAttributes(files[i]);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
              DirectoryInfo di = new DirectoryInfo(files[i]);
              files.InsertRange(i + 1, Directory.GetFiles(files[i]));
            }
          }
          CopyFullSize = (int)trvDirectory_Size(files);
          string originFolderPath = Path.GetDirectoryName(files[0]);
          for (int i = 0; i < files.Count; i++)
          {
            FileAttributes attr = File.GetAttributes(files[i]);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
              string folderName = Path.GetFileName(files[i]);
              if (!SPIFFS.SelectedNode.Nodes.Exists(folderName))
              {
                TreeNode node = SPIFFS.SelectedNode.Nodes.Add(folderName);
                node.ImageIndex = 0;
                node.SelectedImageIndex = 0;

              }
            } else
            {
              FileInfo fi = new FileInfo(files[i]);
              string fileName = Path.GetFileName(files[i]);
              CurrentFileSize = (int)fi.Length;
              string path = files[i].Substring(originFolderPath.Length);
              String folderPath = Path.GetDirectoryName(path);
              path = path.Replace("\\", "/");
              ESP_SPIFFS.Put(files[i], path);
              SumFileSize += CurrentFileSize;
              ESP_fsInfo.fs.Add(new SPIFFSFileManager.FileData(path, CurrentFileSize));

              TreeNode node = SPIFFS.Nodes.FindTreeNodeByFullPath("SPIFFS" + folderPath);
              TreeNode nodeToDelete = SPIFFS.Nodes.FindTreeNodeByPartialPath("SPIFFS" + folderPath + fileName);
              if (nodeToDelete!=null)
                node.Nodes.Remove(nodeToDelete);
              node = node.Nodes.Add(fileName);
              node.ImageIndex = 1;
              node.SelectedImageIndex = 1;
              SPIFFS.Update();
            }

          }
        }
      }
      ESP_SPIFFS_Executing("", 0);
      SPIFFS.SelectedNode.ExpandAll();
      Cursor = Cursors.Default;

    }
    private void SPIFFS_Delete_Click(object sender, EventArgs e)
    {
      TreeNode node = SPIFFS.SelectedNode;
      string message = "Do you want delete the ";
      if (node.IsFolder())
        message += "folder [";
      else
        message += "file [";
      message += node.Text + "] ? ";
      DialogResult res = MessageBox.Show(message, "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
      if (res == DialogResult.Yes)
      {
        string pathName = SPIFFS_GetFullPath(node);
        if (node.IsFolder())
        {
          List<string> files = SPIFFS_GetNodeFileList(node);
          SPIFFS.Nodes.Remove(node);
          for (int i = 0; i < files.Count; i++)
          {
            SPIFFSFileManager.FileData item = ESP_fsInfo.fs.Find(X => X.filename == files[i]);
            if (item != null) ESP_fsInfo.fs.Remove(item);
            if (ESP_SPIFFS.Delete(files[i]))
            {
              ESP_fsInfo.fs.Remove(item);
            }
            else
              MessageBox.Show("An error occured, File Not deleted!", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Error);
          }
        }
        else
        {
          SPIFFSFileManager.FileData item = ESP_fsInfo.fs.Find(X => X.filename == pathName);
          ////int index = Convert.ToInt32(node.Name);
          if (item != null) ESP_fsInfo.fs.Remove(item);
          SPIFFS.Nodes.Remove(node);
          if (ESP_SPIFFS.Delete(pathName))
          {
            ESP_fsInfo.fs.Remove(item);
            SPIFFS.Nodes.Remove(node);
          }
          else
            MessageBox.Show("An error occured, File Not deleted!", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
    }
    private void SPIFFS_CreateFolder_Click(object sender, EventArgs e)
    {
      TreeNode node = SPIFFS.SelectedNode;
      string FolderName = "NewFolder";
      if (InputBox.Show("New Folder", "Please enter folder name", ref FolderName) == DialogResult.OK)
      {
        TreeNode newFolder =  node.Nodes.Add(FolderName);
        newFolder.ImageIndex = 0;
        newFolder.SelectedImageIndex = 0;
        node.Expand();
      }
    }
    private void SPIFFS_MouseDown(object sender, MouseEventArgs e)
    {
      dragdata = null;
      if (e.Button == MouseButtons.Right)
      {
        Point ScreenPoint = new Point(e.X, e.Y);
        //Point ControlPoint = SPIFFS.PointToClient(ScreenPoint);
        TreeNode current = SPIFFS.GetNodeAt(ScreenPoint);
        SPIFFS.SelectedNode = current;
        ContextMenu cm = new ContextMenu();
        cm.MenuItems.Add("Delete", new EventHandler(SPIFFS_Delete_Click));
        if (current.IsFolder())
          cm.MenuItems.Add("Create Folder", new EventHandler(SPIFFS_CreateFolder_Click));
        SPIFFS.ContextMenu = cm;
      }
    }
    private void SPIFFS_MouseMove(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left && dragdata == null)
      {
        Point ScreenPoint = new Point(e.X, e.Y);
        //Point ControlPoint = trvDirectory.PointToClient(ScreenPoint);
        TreeNode current = SPIFFS.GetNodeAt(ScreenPoint);
        if (current!=null)
          SPIFFS.DoDragDrop(current, DragDropEffects.Copy | DragDropEffects.Move);

      }

    }
    private string SPIFFS_GetFullPath(TreeNode node)
    {
      string retrunData = node.FullPath.Replace("SPIFFS\\", "/").Replace("SPIFFS", "/").Replace("\\", "/");
      int index = retrunData.IndexOf(" ");
      if (index >= 0)
        retrunData = retrunData.Substring(0, index);
      return retrunData;
    }
    public List<string> SPIFFS_GetNodeFileList(TreeNode Node)
    {
      List<string> PathFiles = new List<string>();
      PathFiles = Node.GetAllNodesFullPath(1);
      for (int i = 0; i < PathFiles.Count; i++)
      {
        PathFiles[i] = PathFiles[i].Replace("SPIFFS", "").Replace("\\", "/");
        int indexSpace = PathFiles[i].IndexOf(" ");
        if (indexSpace >= 0)
          PathFiles[i] = PathFiles[i].Substring(0, indexSpace);
      }
      return PathFiles;
    }
    public List<string> SPISFS_GetNodeFolderList(TreeNode Node)
    {
      List<string> PathFiles = new List<string>();
      PathFiles = Node.GetAllNodesFullPath(0);
      for (int i = 0; i < PathFiles.Count; i++)
      {
        PathFiles[i] = PathFiles[i].Replace("SPIFFS\\", "");
        int indexSpace = PathFiles[i].IndexOf(" ");
        if (indexSpace >= 0)
          PathFiles[i] = PathFiles[i].Substring(0, indexSpace);
      }
      return PathFiles;
    }
    public List<string> trvDirectory_GetNodeFileList(TreeNode Node)
    {
      List<string> PathFiles = new List<string>();
      PathFiles = Node.GetAllNodesFullPath(1);
      //for (int i = 0; i < PathFiles.Count; i++)
      //{
      //  int indexSpace = PathFiles[i].IndexOf(" ");
      //  if (indexSpace >= 0)
      //    PathFiles[i] = PathFiles[i].Substring(0, indexSpace);
      //}
      return PathFiles;
    }
    public long trvDirectory_Size(List<string> pathfiles)
    {
      long returnData = 0;
      for (int i = 0; i < pathfiles.Count; i++)
      {
        FileAttributes attr = File.GetAttributes(pathfiles[i]);
        if ((attr & FileAttributes.Directory) != FileAttributes.Directory)
        {
          FileInfo fi = new FileInfo(pathfiles[i]);
          returnData += fi.Length;
        }

      }
      return returnData;
    }
    private void trvDirectory_InsertTree(TreeNode sourceNodeParent, TreeNode destinationNode)
    {
      string filename = sourceNodeParent.Text;
      int index = filename.IndexOf(" ");
      if (index >= 0)
        filename = filename.Substring(0, index);
      TreeNode node = null;
      if (destinationNode.Nodes.FindContent(filename) == null)
        node = destinationNode.Nodes.Add(filename);
      else
        node = destinationNode.Nodes.FindContent(filename);
      string path = node.FullPath ;
      if (sourceNodeParent.Nodes.Count == 0) // is as file
      {
        node.ImageIndex = 1;
        node.SelectedImageIndex = 1;
        CurrentFileSize = ESP_fsInfo.Size(SPIFFS_GetFullPath(sourceNodeParent));
        ESP_SPIFFS.Get(path, SPIFFS_GetFullPath(sourceNodeParent));
        SumFileSize += CurrentFileSize;
      }
      else
      {
        node.ImageIndex = 0;
        node.SelectedImageIndex = 0;
        Directory.CreateDirectory(path);
        //ESP_SPIFFS.CreateFolder(path);
      }

      for (int i = 0; i < sourceNodeParent.Nodes.Count; i++)
      {
        trvDirectory_InsertTree(sourceNodeParent.Nodes[i], node);
      }
      
    }
    private bool trvDirectoryIsDroppable(DragEventArgs e)
    {
      bool returnData = false;
      if (e.Data.GetDataPresent(typeof(TreeNode)))
      {
        TreeNode source = e.Data.GetData(typeof(TreeNode)) as TreeNode;
        dragdata = source;
        if (trvDirectory.Nodes.Count > 0 && source.TreeView == SPIFFS)
          returnData = true;
      }
      if (returnData)
        e.Effect = DragDropEffects.Copy;
      else
        e.Effect = DragDropEffects.None;
      return returnData;
    }
    private void trvDirectory_DragEnter(object sender, DragEventArgs e)
    {
      if (SPIFFS_IsDroppable(e))
      {
        TreeNode source = e.Data.GetData(typeof(TreeNode)) as TreeNode;
      }

    }
    private void trvDirectory_DragOver(object sender, DragEventArgs e)
    {
      Point ScreenPoint = new Point(e.X, e.Y);
      Point ControlPoint = trvDirectory.PointToClient(ScreenPoint);
      TreeNode current = trvDirectory.GetNodeAt(ControlPoint);
      //while (ControlPoint.X>=0 && current==null)
      //{
      //  current = trvDirectory.GetNodeAt(ControlPoint);
      //  ControlPoint.X--;
      //}
      if (trvDirectoryIsDroppable(e))
      {
        if (current != null && current.IsFolder())
        {
          trvDirectory.SelectedNode = current;
          e.Effect = DragDropEffects.Copy;
        }
        else e.Effect = DragDropEffects.None;
        /*
        else if (current != null && current.Parent != null)
          trvDirectory.SelectedNode = current.Parent;
        else if (trvDirectory.Nodes.Count > 0)
          trvDirectory.SelectedNode = trvDirectory.Nodes[0];
        else
          trvDirectory.SelectedNode = null;
        */
        trvDirectory.Focus();
      }
      else e.Effect = DragDropEffects.None;
    }
    private void trvDirectory_DragDrop(object sender, DragEventArgs e)
    {
      Cursor = Cursors.WaitCursor;
      Point ScreenPoint = new Point(e.X, e.Y);
      Point ControlPoint = trvDirectory.PointToClient(ScreenPoint);
      TreeNode current = trvDirectory.GetNodeAt(ControlPoint);
      if (trvDirectoryIsDroppable(e))
      {
        TreeNode source = e.Data.GetData(typeof(TreeNode)) as TreeNode;

        List<string> PathFiles = SPIFFS_GetNodeFileList(source);
        CopyFullSize = ESP_fsInfo.Size(PathFiles);

        trvDirectory_InsertTree(source, trvDirectory.SelectedNode);
        trvDirectory.SelectedNode.Expand();

      }
      ESP_SPIFFS_Executing("", 0);
      Cursor = Cursors.Default;

    }
    private void trvDirectory_AfterSelect(object sender, TreeViewEventArgs e)
    {
      txtPath.Text = trvDirectory.SelectedNode.FullPath.Replace("\\\\", "\\") ;

    }
    private void txtPath_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (Char.IsDigit(e.KeyChar))
      {
        txtPath.Tag = "working";
        return;
      }
      if (Char.IsControl(e.KeyChar))
      {
        txtPath.Tag = "working";
        return;
      }
      txtPath.Tag = "";
      //if (e.KeyChar >= 32 && e.KeyChar < 0xFFFF)
      //{
      //  int start = txtPath.SelectionStart + 1;
      //  txtPath.Text += e.KeyChar;
      //  TreeNode node = trvDirectory.Nodes.FindTreeNodeByPartialPath(txtPath.Text);
      //  if (node != null)
      //  {
      //    DirectoryInfo dir_info = new DirectoryInfo(node.FullPath);
      //    trvDirectory.UpdateDirectoryNodes(dir_info, node, 0, 1);
      //
      //    String Path = node.FullPath.Replace("\\\\", "\\");
      //    txtPath.Text = Path;
      //    txtPath.SelectionStart = start;
      //    txtPath.SelectionLength = Path.Length - start;
      //  }
      //  e.Handled = true;
      //}
    }
    private void txtPath_TextChanged(object sender, EventArgs e)
    {
      
      if (txtPath.Tag == null || (string)txtPath.Tag != "working")
      {
        txtPath.Tag = "working";

        int start = txtPath.SelectionStart;
        TreeNode node = trvDirectory.Nodes.FindTreeNodeByPartialPath(txtPath.Text);
        if (node != null)
        {
          if (txtPath.Text.Last() == '\\')
          {
            node = trvDirectory.Nodes.FindTreeNodeByFullPath(txtPath.Text);
            DirectoryInfo dir_info = new DirectoryInfo(node.FullPath);
            trvDirectory.UpdateDirectoryNodes(dir_info, node, 0, 1);
            node.Expand();
            if (node.Nodes.Count>0)
              node = node.Nodes[0];
          }
          String Path = node.FullPath.Replace("\\\\", "\\");
          txtPath.Text = Path;

          txtPath.SelectionStart = start;
          txtPath.SelectionLength = Path.Length - start;
          
          txtPath.Tag = "";
        }
      }
    }
    private void btnFormat_Click(object sender, EventArgs e)
    {
      ESP_SPIFFS.Format();
    }
    private void txtHostName_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
      {
        btnConnect_Click(this, new EventArgs());
        e.Handled = e.SuppressKeyPress = true;
      }
    }

  }
}
