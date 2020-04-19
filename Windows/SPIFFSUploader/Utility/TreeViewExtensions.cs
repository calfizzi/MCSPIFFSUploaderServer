using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.IO;


namespace MCExtensions
{
  public static class TreeViewExtensions
  {
    // Initialize the TreeView from a directory,
    // its subdirectories, and their files.
    public static void LoadFromDirectory(this TreeView trv, string directory, int folder_img, int file_img)
    {
      DirectoryInfo dir_info = new DirectoryInfo(directory);
      AddDirectoryNodes(trv, dir_info, null, folder_img, file_img);
    }
    // Add this directory's node and sub-nodes.
    public static void UpdateDirectoryNodes(this TreeView trv, DirectoryInfo dir_info, TreeNode parent, int folder_img, int file_img, int level = 2)
    {
      AddDirectoryNodes(trv, dir_info, parent.Parent, folder_img, file_img, level );
      //foreach (DirectoryInfo subdir in dir_info.GetDirectories())
      //  AddDirectoryNodes(trv, subdir, parent, folder_img, file_img, level - 1);
      //foreach (DirectoryInfo subdir in dir_info.GetDirectories())
      //  AddDirectoryNodes(trv, subdir, parent, folder_img, file_img, level - 1);
    }
    public static TreeNode FindContent(this TreeNodeCollection tnc, string text, bool searchAllChildren = false)
    {
      for (int i = 0; i < tnc.Count; i++)
      {
        if (tnc[i].Text == text)
          return tnc[i];
      }
      return null;
    }
    public static bool IsFolder (this TreeNode tn)
    {
      return tn.ImageIndex == 0;
    }
    public static bool Exists(this TreeNodeCollection tnc, string text, bool searchAllChildren = false)
    {
      return tnc.FindContent(text, searchAllChildren) != null;
    }
    public static void AddDirectoryNodes(this TreeView trv, DirectoryInfo dir_info, TreeNode parent, int folder_img, int file_img, int level = 2)
    {
      // Add the directory's node.
      TreeNode dir_node;
      TreeNodeCollection rootNodes = null;
      if (parent == null)
        rootNodes = trv.Nodes;
      else
        rootNodes = parent.Nodes;
      String nameToSearchFor = dir_info.Name.Last() == '\\' ? dir_info.Name.Substring(0, dir_info.Name.Length - 1) : dir_info.Name;
      if (rootNodes.Exists(nameToSearchFor))
        dir_node = rootNodes.FindContent(nameToSearchFor);
      else
        dir_node = rootNodes.Add(nameToSearchFor);
      /*
        if (parent == null)
        if (!trv.Nodes.Exists(dir_info.Name))
          dir_node = trv.Nodes.Add(dir_info.Name);
        else
          dir_node = trv.Nodes.FindContent(dir_info.Name);
      else if (!parent.Nodes.Exists(dir_info.Name))
        dir_node = parent.Nodes.Add(dir_info.Name);
      else
        dir_node = parent.Nodes.FindContent(dir_info.Name);
        */
      try { dir_info.GetDirectories(); }
      catch
      { 
        return; 
      }
      if (level == 0) return;
      // Add the folder image.
      if (folder_img >= 0)
      {
        dir_node.ImageIndex = folder_img;
        dir_node.SelectedImageIndex = folder_img;
      }

      // Add subdirectories.
      if (level == 1)
      {
        if (dir_info.GetDirectories().Length > 0)
          AddDirectoryNodes(trv, dir_info.GetDirectories()[0], dir_node, folder_img, file_img, level - 1);
      }
      else
      {
        foreach (DirectoryInfo subdir in dir_info.GetDirectories())
          AddDirectoryNodes(trv, subdir, dir_node, folder_img, file_img, level - 1);
      }
      if (level == 1)
      {
        if (dir_info.GetFiles().Length>0)
        {
          if (!dir_node.Nodes.Exists(dir_info.GetFiles()[0].Name))
          {
            TreeNode file_node = dir_node.Nodes.Add(dir_info.GetFiles()[0].Name);
            if (file_img >= 0)
            {
              file_node.ImageIndex = file_img;
              file_node.SelectedImageIndex = file_img;
            }
          }
        }
      }
      else
      {
        // Add file nodes.
        foreach (FileInfo file_info in dir_info.GetFiles())
        {
          if (!dir_node.Nodes.Exists(file_info.Name))
          {
            TreeNode file_node = dir_node.Nodes.Add(file_info.Name);
            if (file_img >= 0)
            {
              file_node.ImageIndex = file_img;
              file_node.SelectedImageIndex = file_img;
            }
          }
        }
      }
    }
    public static TreeNode FindTreeNodeByFullPath(this TreeNodeCollection collection, string fullPath, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
    {
      string pathTOSearchFor = (fullPath.Last() == '\\') ? fullPath.Substring(0, fullPath.Length - 1) : fullPath;
      var foundNode = collection.Cast<TreeNode>().FirstOrDefault(tn => string.Equals(tn.FullPath, pathTOSearchFor, comparison));
      if (null == foundNode)
      {
        foreach (var childNode in collection.Cast<TreeNode>())
        {
          var foundChildNode = FindTreeNodeByFullPath(childNode.Nodes, pathTOSearchFor, comparison);
          if (null != foundChildNode)
          {
            return foundChildNode;
          }
        }
      }
      return foundNode;
    }
    public static TreeNode FindTreeNodeByPartialPath(this TreeNodeCollection collection, string partialPath, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
    {
      TreeNode foundNode = null;
      for (int i = 0; i < collection.Count; i++)
      {
        if (collection[i].FullPath.Replace("\\\\","\\").IndexOf(partialPath, comparison) == 0)
        {
          foundNode = collection[i];
          break;
        }
      }
      //foundNode = collection.Cast<TreeNode>().FirstOrDefault(tn => string.Equals(tn.FullPath, fullPath, comparison));
      if (foundNode == null)
      {
        foreach (TreeNode childNode in collection.Cast<TreeNode>())
        {
          TreeNode foundChildNode = FindTreeNodeByPartialPath(childNode.Nodes, partialPath, comparison);
          if (foundChildNode != null )
          {
            return foundChildNode;
          }
        }
      }
      return foundNode;
    }
    public static List<TreeNode> GetAllNodes(this TreeView _self)
    {
      List<TreeNode> result = new List<TreeNode>();
      foreach (TreeNode child in _self.Nodes)
      {
        result.AddRange(child.GetAllNodes());
      }
      return result;
    }
    public static List<TreeNode> GetAllNodes(this TreeNode _self)
    {
      List<TreeNode> result = new List<TreeNode>();
      result.Add(_self);
      foreach (TreeNode child in _self.Nodes)
      {
        result.AddRange(child.GetAllNodes());
      }
      return result;
    }
    public static List<string> GetAllNodesText(this TreeNode _self)
    {
      List<string> result = new List<string>();
      result.Add(_self.Text);
      foreach (TreeNode child in _self.Nodes)
      {
        result.AddRange(child.GetAllNodesText());
      }
      return result;
    }
    public static List<string> GetAllNodesFullPath(this TreeNode _self, int imageIndexFilter = -1)
    {
      List<string> result = new List<string>();
      if (imageIndexFilter==-1 || _self.ImageIndex == imageIndexFilter)
        result.Add(_self.FullPath);
      foreach (TreeNode child in _self.Nodes)
      {
        result.AddRange(child.GetAllNodesFullPath(imageIndexFilter));
      }
      return result;
    }
  }
}
