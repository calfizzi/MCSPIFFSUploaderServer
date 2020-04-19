using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MCExtensions
{

  //[Designer("System.Windows.Forms.Design.LabelDesigner")]
  //[ToolboxItem("System.Windows.Forms.Design.AutoSizeToolboxItem")]
  class DriveCombo : System.Windows.Forms.ComboBox
  {

    [DllImport("kernel32.dll")]
    private static extern long GetDriveType(string driveLetter);
    [DllImport("kernel32.dll")]
    private static extern long GetVolumeInformation(
    string PathName,
    StringBuilder VolumeNameBuffer,
    UInt32 VolumeNameSize,
    ref UInt32 VolumeSerialNumber,
    ref UInt32 MaximumComponentLength,
    ref UInt32 FileSystemFlags,
    StringBuilder FileSystemNameBuffer,
    UInt32 FileSystemNameSize);

    struct DriveInfo
    {
      private string letter;  //drive letter : "C:\","A:\"
      private int type;       //below
      private string name;    //Disc Label

      public int Type
      {
        get
        {
          return type;
        }
      }

      public SHSTOCKICONID Icon
      {
        get
        {
          if (name == "Computer") return SHSTOCKICONID.SIID_DESKTOPPC;
          if (type == 5) return SHSTOCKICONID.SIID_DRIVECD;//cd
          if (type == 3) return SHSTOCKICONID.SIID_DRIVEFIXED;//fixed
          if (type == 2) return SHSTOCKICONID.SIID_DRIVEREMOVE;//removable
          if (type == 4) return SHSTOCKICONID.SIID_DRIVENET;//remote disk
          if (type == 6) return SHSTOCKICONID.SIID_DRIVERAM;//ram disk
          return SHSTOCKICONID.SIID_APPLICATION;//unknown
        }
      }

      public string Letter
      {
        get
        {
          if (letter != "") return " (" + letter.Substring(0, letter.Length-1) + ")";
          else return "";
        }
      }
      public string Name
      {
        get
        {
          if (name != "") return name;
          else
          {
            switch (this.Type)
            {
              case 3:
                if (letter == System.IO.Directory.GetDirectoryRoot(
                                      System.Environment.SystemDirectory))
                  return "System";
                else
                  return "Local Disc";
              case 5: return "CD Rom";
              case 6: return "RAM Disc";
              case 4: return "Network Drive";
              case 2:
                if (letter == "A:\\") return "3.5 Floppy";
                else return "Removable Disc";
              default: return "";
            }
          }
        }
      }

      //TYPE:
      //5-A CD-ROM drive. 
      //3-A hard drive. 
      //6-A RAM disk. 
      //4-A network drive or a drive located on a network server. 
      //2-A floppy drive or some other removable-disk drive. 
      public DriveInfo(string strLetter, int intType, string strName)
      {
        letter = strLetter;
        name = strName;
        type = intType;
      }
    }

    private ArrayList availableDrives = new ArrayList();

    protected override void OnDrawItem(System.Windows.Forms.DrawItemEventArgs e)
    {
      OnComboDrawItem(this, e);
      base.OnDrawItem(e);    //Without this line, the event won't be fired
                          //...
    }
    private void AutoSet()
    {
      this.ItemHeight = 22;
      this.Height = 22;
      this.DrawMode = DrawMode.OwnerDrawFixed;
      this.DropDownStyle = ComboBoxStyle.DropDownList;

    }
    protected override void OnDropDown(EventArgs e)
    {
      this.AutoSet();
      findLogicalDrives();
      this.Items.Clear();
      foreach (DriveInfo tempDrvInfo in availableDrives)
      {
        this.Items.Add(tempDrvInfo.Letter);
      }
      this.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

      base.OnDropDown(e);
    }
    protected override void OnDisplayMemberChanged(EventArgs e)
    {
      base.OnDisplayMemberChanged(e);
    }
    //protected override void OnInvalidated(System.Windows.Forms.InvalidateEventArgs e)
    //{
    //  this.AutoSet();
    //  this.Items.Clear();
    //  foreach (DriveInfo tempDrvInfo in availableDrives)
    //  {
    //    this.Items.Add(tempDrvInfo.Letter);
    //  }
    //  this.SelectedIndex = 0;
    //  base.OnInvalidated(e);
    //}
    //protected override void OnPaint(PaintEventArgs e)
    //{
    //  this.AutoSet();
    //  base.OnPaint(e);
    //}
    //protected override void OnResize(EventArgs e)
    //{
    //  this.AutoSet();
    //  base.OnResize(e);
    //}
    private Size _getAutoSize(Size proposedSize)
    {
      Size size = new Size(proposedSize.Width, 20);
      return size;

    }
    private Size _getAutoSize()
    {
      Size size = new Size(this.Width, 20);
      return size;
    }
    
    public override Size GetPreferredSize(Size proposedSize)
    {
      base.GetPreferredSize(proposedSize);
      return _getAutoSize(proposedSize);
    }

    protected override void SetBoundsCore(int x, int y, int width, int height,
            BoundsSpecified specified)
    {
      //  Only when the size is affected...
      //if (this.AutoSize && (specified & BoundsSpecified.Size) != 0)
      //{
      Size size = _getAutoSize(new Size(width, height));

      width = size.Width;
      height = size.Height;
      //}
      //this.Height = size.Height;
      base.SetBoundsCore(x, y, width, height, specified);
    }
    
    public DriveCombo() : base()
    {
      this.AutoSet();
      findLogicalDrives();
      this.Items.Clear();
      foreach (DriveInfo tempDrvInfo in availableDrives)
      {
        this.Items.Add(tempDrvInfo.Letter);
      }
      this.SelectedIndex = 0;
      this.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
    }


    private void OnComboDrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
    {

      Bitmap tempBitmap = new Bitmap(e.Bounds.Width, e.Bounds.Height, e.Graphics);
      Graphics tempGraphics = Graphics.FromImage(tempBitmap);

      //all items with offset, but MyComputer
      int offset = 0; if (e.Index == 0) offset = 0; else offset = 10;

      //item in comboBoxEdit no space in front
      if ((e.State & DrawItemState.ComboBoxEdit) != 0) offset = 0;

      if (e.Index == -1) return;// ???????????

      string tempLetter = ((DriveInfo)availableDrives[e.Index]).Letter;
      string tempName = ((DriveInfo)availableDrives[e.Index]).Name;
      string tempString = tempName + tempLetter;
      SHSTOCKICONID tempIcon = ((DriveInfo)availableDrives[e.Index]).Icon;

      tempGraphics.FillRectangle(new SolidBrush(e.BackColor),
                         new Rectangle(0, 0, tempBitmap.Width, tempBitmap.Height));
      tempGraphics.DrawString(tempString, e.Font, new SolidBrush(e.ForeColor),
                              new Point(28 + offset, (tempBitmap.Height - e.Font.Height) / 2));
      //tempGraphics.DrawImage(imageList1.Images[tempIcon],
      //                         new Rectangle(new Point(6 + offset, 0), new Size(16, 16)));

      Icon icon = GetStockIcon(tempIcon, SHGSI.SHGSI_ICON | SHGSI.SHGSI_SMALLICON);
      tempGraphics.DrawImage(icon.ToBitmap(),
                              new Rectangle(new Point(6 + offset, 0), new Size(16, 16)));

      e.Graphics.DrawImage(tempBitmap, e.Bounds.X, e.Bounds.Y);
    }



    private void findLogicalDrives()
    {
      availableDrives.Clear();
      string[] tempString = Directory.GetLogicalDrives();

      DriveInfo tempInfo = new DriveInfo("", 0, "Computer");
      availableDrives.Add(tempInfo);

      foreach (string tempDrive in tempString)
      {
        int tempType = getDriveType(tempDrive);
        string tempName = GetDriveName(tempDrive);
        tempInfo = new DriveInfo(tempDrive, tempType, tempName);
        availableDrives.Add(tempInfo);
      }

    }




    public int getDriveType(string drive)
    {
      if ((GetDriveType(drive) & 5) == 5) return 5;//cd
      if ((GetDriveType(drive) & 3) == 3) return 3;//fixed
      if ((GetDriveType(drive) & 2) == 2) return 2;//removable
      if ((GetDriveType(drive) & 4) == 4) return 4;//remote disk
      if ((GetDriveType(drive) & 6) == 6) return 6;//ram disk
      return 0;
    }


    public string GetDriveName(string drive)
    {
      //receives volume name of drive
      StringBuilder volname = new StringBuilder(256);
      //receives serial number of drive,not in case of network drive(win95/98)
      uint sn = 0;
      uint maxcomplen = 0;//receives maximum component length
      UInt32 sysflags = new UInt32();
      StringBuilder sysname = new StringBuilder(256);//receives the file system name
      long retval = new long();//return value

      retval = GetVolumeInformation(drive, volname, 256, ref sn, ref maxcomplen,
                                    ref sysflags, sysname, 256);

      if (retval != 0) return volname.ToString();
      else return "";
    }



    public void initDriveCombo()
    {
    }





    [DllImport("shell32.dll")]
    public static extern int SHGetStockIconInfo(SHSTOCKICONID siid, uint uFlags, ref SHSTOCKICONINFO psii);

    [DllImport("user32.dll")]
    public static extern bool DestroyIcon(IntPtr handle);

    private static Icon GetStockIcon(SHSTOCKICONID type, SHGSI size)
    {
      var info = new SHSTOCKICONINFO();
      info.cbSize = (uint)Marshal.SizeOf(info);

      SHGetStockIconInfo(type, (uint)size, ref info);

      var icon = (Icon)Icon.FromHandle(info.hIcon).Clone(); // Get a copy that doesn't use the original handle
      DestroyIcon(info.hIcon); // Clean up native icon to prevent resource leak

      return icon;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct SHSTOCKICONINFO
    {
      public uint cbSize;
      public IntPtr hIcon;
      public int iSysIconIndex;
      public int iIcon;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
      public string szPath;
    }


    private const uint SHSIID_FOLDER = 0x3;
    private const uint SHGSI_ICON = 0x100;
    private const uint SHGSI_LARGEICON = 0x0;
    private const uint SHGSI_SMALLICON = 0x1;

    [Flags()]
    public enum SHGSI : UInt32
    {

      /// <summary>
      /// The szPath and iIcon members of the SHSTOCKICONINFO structure receive the path and icon index of the requested icon, in a format suitable for passing to the ExtractIcon function. The numerical value of this flag is zero, so you always get the icon location regardless of other flags.
      /// </summary>
      SHGSI_ICONLOCATION = 0,

      /// <summary>
      /// The hIcon member of the SHSTOCKICONINFO structure receives a handle to the specified icon.
      /// </summary>
      SHGSI_ICON = 0x100,

      /// <summary>
      /// The iSysImageImage member of the SHSTOCKICONINFO structure receives the index of the specified icon in the system imagelist.
      /// </summary>
      SHGSI_SYSICONINDEX = 0x4000,

      /// <summary>
      /// Modifies the SHGSI_ICON value by causing the function to add the link overlay to the file's icon.
      /// </summary>
      SHGSI_LINKOVERLAY = 0x8000,

      /// <summary>
      /// Modifies the SHGSI_ICON value by causing the function to blend the icon with the system highlight color.
      /// </summary>
      SHGSI_SELECTED = 0x10000,

      /// <summary>
      /// Modifies the SHGSI_ICON value by causing the function to retrieve the large version of the icon, as specified by the SM_CXICON and SM_CYICON system metrics.
      /// </summary>
      SHGSI_LARGEICON = 0x0,

      /// <summary>
      /// Modifies the SHGSI_ICON value by causing the function to retrieve the small version of the icon, as specified by the SM_CXSMICON and SM_CYSMICON system metrics.
      /// </summary>
      SHGSI_SMALLICON = 0x1,

      /// <summary>
      /// Modifies the SHGSI_LARGEICON or SHGSI_SMALLICON values by causing the function to retrieve the Shell-sized icons rather than the sizes specified by the system metrics.
      /// </summary>
      SHGSI_SHELLICONSIZE = 0x4

    }
    public enum SHSTOCKICONID : UInt32
    {
      /// <summary>
      /// Document of a type with no associated application.
      /// </summary>
      SIID_DOCNOASSOC = 0,
      /// <summary>
      /// Document of a type with an associated application.
      /// </summary>
      SIID_DOCASSOC = 1,
      /// <summary>
      /// Generic application with no custom icon.
      /// </summary>
      SIID_APPLICATION = 2,
      /// <summary>
      /// Folder (generic, unspecified state).
      /// </summary>
      SIID_FOLDER = 3,
      /// <summary>
      /// Folder (open).
      /// </summary>
      SIID_FOLDEROPEN = 4,
      /// <summary>
      /// 5.25-inch disk drive.
      /// </summary>
      SIID_DRIVE525 = 5,
      /// <summary>
      /// 3.5-inch disk drive.
      /// </summary>
      SIID_DRIVE35 = 6,
      /// <summary>
      /// Removable drive.
      /// </summary>
      SIID_DRIVEREMOVE = 7,
      /// <summary>
      /// Fixed drive (hard disk).
      /// </summary>
      SIID_DRIVEFIXED = 8,
      /// <summary>
      /// Network drive (connected).
      /// </summary>
      SIID_DRIVENET = 9,
      /// <summary>
      /// Network drive (disconnected).
      /// </summary>
      SIID_DRIVENETDISABLED = 10,
      /// <summary>
      /// CD drive.
      /// </summary>
      SIID_DRIVECD = 11,
      /// <summary>
      /// RAM disk drive.
      /// </summary>
      SIID_DRIVERAM = 12,
      /// <summary>
      /// The entire network.
      /// </summary>
      SIID_WORLD = 13,
      /// <summary>
      /// A computer on the network.
      /// </summary>
      SIID_SERVER = 15,
      /// <summary>
      /// A local printer or print destination.
      /// </summary>
      SIID_PRINTER = 16,
      /// <summary>
      /// The Network virtual folder (FOLDERID_NetworkFolder/CSIDL_NETWORK).
      /// </summary>
      SIID_MYNETWORK = 17,
      /// <summary>
      /// The Search feature.
      /// </summary>
      SIID_FIND = 22,
      /// <summary>
      /// The Help and Support feature.
      /// </summary>
      SIID_HELP = 23,

      // OVERLAYS...

      /// <summary>
      /// Overlay for a shared item.
      /// </summary>
      SIID_SHARE = 28,
      /// <summary>
      /// Overlay for a shortcut.
      /// </summary>
      SIID_LINK = 29,
      /// <summary>
      /// Overlay for items that are expected to be slow to access.
      /// </summary>
      SIID_SLOWFILE = 30,

      // MORE ICONS...

      /// <summary>
      /// The Recycle Bin (empty).
      /// </summary>
      SIID_RECYCLER = 31,
      /// <summary>
      /// The Recycle Bin (not empty).
      /// </summary>
      SIID_RECYCLERFULL = 32,
      /// <summary>
      /// Audio CD media.
      /// </summary>
      SIID_MEDIACDAUDIO = 40,
      /// <summary>
      /// Security lock.
      /// </summary>
      SIID_LOCK = 47,
      /// <summary>
      /// A virtual folder that contains the results of a search.
      /// </summary>
      SIID_AUTOLIST = 49,
      /// <summary>
      /// A network printer.
      /// </summary>
      SIID_PRINTERNET = 50,
      /// <summary>
      /// A server shared on a network.
      /// </summary>
      SIID_SERVERSHARE = 51,
      /// <summary>
      /// A local fax printer.
      /// </summary>
      SIID_PRINTERFAX = 52,
      /// <summary>
      /// A network fax printer.
      /// </summary>
      SIID_PRINTERFAXNET = 53,
      /// <summary>
      /// A file that receives the output of a Print to file operation.
      /// </summary>
      SIID_PRINTERFILE = 54,
      /// <summary>
      /// A category that results from a Stack by command to organize the contents of a folder.
      /// </summary>
      SIID_STACK = 55,
      /// <summary>
      /// Super Video CD (SVCD) media.
      /// </summary>
      SIID_MEDIASVCD = 56,
      /// <summary>
      /// A folder that contains only subfolders as child items.
      /// </summary>
      SIID_STUFFEDFOLDER = 57,
      /// <summary>
      /// Unknown drive type.
      /// </summary>
      SIID_DRIVEUNKNOWN = 58,
      /// <summary>
      /// DVD drive.
      /// </summary>
      SIID_DRIVEDVD = 59,
      /// <summary>
      /// DVD media.
      /// </summary>
      SIID_MEDIADVD = 60,
      /// <summary>
      /// DVD-RAM media.
      /// </summary>
      SIID_MEDIADVDRAM = 61,
      /// <summary>
      /// DVD-RW media.
      /// </summary>
      SIID_MEDIADVDRW = 62,
      /// <summary>
      /// DVD-R media.
      /// </summary>
      SIID_MEDIADVDR = 63,
      /// <summary>
      /// DVD-ROM media.
      /// </summary>
      SIID_MEDIADVDROM = 64,
      /// <summary>
      /// CD+ (enhanced audio CD) media.
      /// </summary>
      SIID_MEDIACDAUDIOPLUS = 65,
      /// <summary>
      /// CD-RW media.
      /// </summary>
      SIID_MEDIACDRW = 66,
      /// <summary>
      /// CD-R media.
      /// </summary>
      SIID_MEDIACDR = 67,
      /// <summary>
      /// A writeable CD in the process of being burned.
      /// </summary>
      SIID_MEDIACDBURN = 68,
      /// <summary>
      /// Blank writable CD media.
      /// </summary>
      SIID_MEDIABLANKCD = 69,
      /// <summary>
      /// CD-ROM media.
      /// </summary>
      SIID_MEDIACDROM = 70,
      /// <summary>
      /// An audio file.
      /// </summary>
      SIID_AUDIOFILES = 71,
      /// <summary>
      /// An image file.
      /// </summary>
      SIID_IMAGEFILES = 72,
      /// <summary>
      /// A video file.
      /// </summary>
      SIID_VIDEOFILES = 73,
      /// <summary>
      /// A mixed (media) file.
      /// </summary>
      SIID_MIXEDFILES = 74,


      /// <summary>
      /// Folder back. Represents the background Fold of a Folder.
      /// </summary>
      SIID_FOLDERBACK = 75,
      /// <summary>
      /// Folder front. Represents the foreground Fold of a Folder.
      /// </summary>
      SIID_FOLDERFRONT = 76,
      /// <summary>
      /// Security shield.
      /// </summary>
      /// <remarks>Use for UAC prompts only. This Icon doesn't work on all purposes.</remarks>
      SIID_SHIELD = 77,
      /// <summary>
      /// Warning (Exclamation mark).
      /// </summary>
      SIID_WARNING = 78,
      /// <summary>
      /// Informational (Info).
      /// </summary>
      SIID_INFO = 79,
      /// <summary>
      /// Error (X).
      /// </summary>
      SIID_ERROR = 80,
      /// <summary>
      /// Key.
      /// </summary>
      SIID_KEY = 81,
      /// <summary>
      /// Software.
      /// </summary>
      SIID_SOFTWARE = 82,
      /// <summary>
      /// A UI item, such as a button, that issues a rename command.
      /// </summary>
      SIID_RENAME = 83,
      /// <summary>
      /// A UI item, such as a button, that issues a delete command.
      /// </summary>
      SIID_DELETE = 84,
      /// <summary>
      /// Audio DVD media.
      /// </summary>
      SIID_MEDIAAUDIODVD = 85,
      /// <summary>
      /// Movie DVD media.
      /// </summary>
      SIID_MEDIAMOVIEDVD = 86,
      /// <summary>
      /// Enhanced CD media.
      /// </summary>
      SIID_MEDIAENHANCEDCD = 87,
      /// <summary>
      /// Enhanced DVD media.
      /// </summary>
      SIID_MEDIAENHANCEDDVD = 88,
      /// <summary>
      /// Enhanced DVD media.
      /// </summary>
      SIID_MEDIAHDDVD = 89,
      /// <summary>
      /// High definition DVD media in the Blu-ray Disc™ format.
      /// </summary>
      SIID_MEDIABLURAY = 90,
      /// <summary>
      /// Video CD (VCD) media.
      /// </summary>
      SIID_MEDIAVCD = 91,
      /// <summary>
      /// DVD+R media.
      /// </summary>
      SIID_MEDIADVDPLUSR = 92,
      /// <summary>
      /// DVD+RW media.
      /// </summary>
      SIID_MEDIADVDPLUSRW = 93,
      /// <summary>
      /// A desktop computer.
      /// </summary>
      SIID_DESKTOPPC = 94,
      /// <summary>
      /// A mobile computer (laptop).
      /// </summary>
      SIID_MOBILEPC = 95,
      /// <summary>
      /// The User Accounts Control Panel item.
      /// </summary>
      SIID_USERS = 96,
      /// <summary>
      /// Smart media.
      /// </summary>
      SIID_MEDIASMARTMEDIA = 97,
      /// <summary>
      /// CompactFlash media.
      /// </summary>
      SIID_MEDIACOMPACTFLASH = 98,
      /// <summary>
      /// A cell phone.
      /// </summary>
      SIID_DEVICECELLPHONE = 99,
      /// <summary>
      /// A digital camera.
      /// </summary>
      SIID_DEVICECAMERA = 100,
      /// <summary>
      /// A digital video camera.
      /// </summary>
      SIID_DEVICEVIDEOCAMERA = 101,
      /// <summary>
      /// An audio player.
      /// </summary>
      SIID_DEVICEAUDIOPLAYER = 102,
      /// <summary>
      /// Connect to network.
      /// </summary>
      SIID_NETWORKCONNECT = 103,
      /// <summary>
      /// The Network and Internet Control Panel item.
      /// </summary>
      SIID_INTERNET = 104,
      /// <summary>
      /// A compressed file with a .zip file name extension.
      /// </summary>
      SIID_ZIPFILE = 105,
      /// <summary>
      /// The Additional Options Control Panel item.
      /// </summary>
      SIID_SETTINGS = 106,
      /// <summary>
      /// Windows Vista with Service Pack 1 (SP1) and later. High definition DVD drive (any type - HD DVD-ROM, HD DVD-R, HD-DVD-RAM) that uses the HD DVD format.
      /// </summary>
      SIID_DRIVEHDDVD = 132,
      /// <summary>
      /// Windows Vista with SP1 and later. High definition DVD drive (any type - BD-ROM, BD-R, BD-RE) that uses the Blu-ray Disc format.
      /// </summary>
      SIID_DRIVEBD = 133,
      /// <summary>
      /// Windows Vista with SP1 and later. High definition DVD-ROM media in the HD DVD-ROM format.
      /// </summary>
      SIID_MEDIAHDDVDROM = 134,
      /// <summary>
      /// Windows Vista with SP1 and later. High definition DVD-R media in the HD DVD-R format.
      /// </summary>
      SIID_MEDIAHDDVDR = 135,
      /// <summary>
      /// Windows Vista with SP1 and later. High definition DVD-RAM media in the HD DVD-RAM format.
      /// </summary>
      SIID_MEDIAHDDVDRAM = 136,
      /// <summary>
      /// Windows Vista with SP1 and later. High definition DVD-ROM media in the Blu-ray Disc BD-ROM format.
      /// </summary>
      SIID_MEDIABDROM = 137,
      /// <summary>
      /// Windows Vista with SP1 and later. High definition write-once media in the Blu-ray Disc BD-R format.
      /// </summary>
      SIID_MEDIABDR = 138,
      /// <summary>
      /// Windows Vista with SP1 and later. High definition read/write media in the Blu-ray Disc BD-RE format.
      /// </summary>
      SIID_MEDIABDRE = 139,
      /// <summary>
      /// Windows Vista with SP1 and later. A cluster disk array.
      /// </summary>
      SIID_CLUSTEREDDRIVE = 140,

      /// <summary>
      /// The highest valid value in the enumeration. Values over 160 are Windows 7-only icons.
      /// </summary>
      SIID_MAX_ICONS = 175
    }
  }
}


