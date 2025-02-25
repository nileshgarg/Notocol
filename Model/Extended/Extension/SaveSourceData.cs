﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Extended.Extension
{
    public class SaveSourceData
    {
        public SourceDataForExtension sourceData { get; set; }

        public FolderTreeRecursive folderTree { get; set; }

        public List<FolderTreeRecursive> addedFolders { get; set; }
        public Dictionary<string, long> addedFolderIDs { get; set; }
        
    }
}
