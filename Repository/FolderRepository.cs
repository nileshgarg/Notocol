﻿using Model;
using Repository.Search;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class FolderRepository : BaseRepository
    {
        public FolderRepository()
        {
            CreateDataContext();
        }
        public Folder AddFolder(Folder folder)
        {
            try
            {
                using (GetDataContext())
                {
                    folder.created = folder.updated = DateTime.Now;
                    context.Entry(folder).State = EntityState.Added;
                    context.SaveChanges();

                }
            }
            catch
            {
                throw;
            }
            finally
            {
                DisposeContext();
            }
            ElasticSearchTest es = new ElasticSearchTest();
            es.AddFolder(folder);
            
            return folder;
        }

        public bool DeleteFolder(Folder folder)
        {
            if (folder.ID <= 0) return false;
            try
            {
                using (GetDataContext())
                {
                    context.Entry(folder).State = EntityState.Deleted;
                    context.SaveChanges();

                }
            }
            catch
            {
                throw;
            }
            finally
            {
                DisposeContext();
            }
            //}
            return true;
        }

        public IList<Folder> GetFolder(long userID, string name)
        {
            Folder folder = null;
            IList<Folder> folders = null;
            try
            {
                using (GetDataContext())
                {
                   folders = (from folderObjects in context.Folders
                                             where folderObjects.userID == userID && folderObjects.name.Equals(name)
                                    select folder).ToList();

                    
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                DisposeContext();
            }
            //}
            return folders;
        }

        public Folder GetFolder(long folderID)
        {
            Folder folder = null;
            try
            {
                using (GetDataContext())
                {
                    IList<Folder> folders = (from folderObjects in context.Folders
                                             where folderObjects.ID == folderID
                                             select folderObjects).ToList();

                    if (folders.Count > 0) folder = folders[0];
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                DisposeContext();
            }
            //}
            return folder;
        }

        public IList<Folder> GetFoldersForUser(long userID)
        {
            IList<Folder> folders;
            try
            {
                using (GetDataContext())
                {
                    folders = (from folderObjects in context.Folders
                                             where folderObjects.userID == userID
                               select folderObjects).ToList();

                    
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                DisposeContext();
            }
            //}
            return folders;

        }

        public Folder UpdateFolder(Folder folder)
        {
            try
            {
                using (GetDataContext())
                {
                    folder.updated = DateTime.Now;
                    context.Entry(folder).State = EntityState.Modified;
                    context.SaveChanges();

                }
            }
            catch
            {
                throw;
            }
            finally
            {
                DisposeContext();
            }
            //}
            return folder;
        }


        public Folder GetFolderUnderParent(long userID, string folderName, long parentID)
        {
            Folder folder = null;
            using (GetDataContext())
            {
                try
                {
                    IList<Folder> checkFolders = (from folders in context.Folders
                                                   where folders.parentID == parentID &&
                                                             folders.name == folderName

                                                   select folders).ToList();

                    if (checkFolders.Count > 0) folder = checkFolders[0];
                   
                }

                catch
                {
                    throw;
                }
            }

            return folder;
        }

        public IList<string> GetFolderNames(string folderQuery, long userID)
        {
            IList<string> folderNames = new List<string>();
            ElasticSearchTest es = new ElasticSearchTest();
            folderNames = es.GetFolderSuggestions(folderQuery, userID);

            return folderNames;
        }
    }
}
