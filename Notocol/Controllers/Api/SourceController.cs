﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Model;
using Repository;
using Model.Extended.Extension;
using Model.Extended;
using Business;
using Notocol.Models;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Web;
using System.Text;

namespace Notocol.Controllers.Api
{
    public class SourceController : ApiController
    {
        SourceHelper sourceHelper = new SourceHelper();

        private string GetMixpanelIDFromCookie()
        {
            CookieHeaderValue idCookie = null;
            idCookie = Request.Headers.GetCookies
                                ("distinctId").SingleOrDefault();

            string data = (idCookie != null) ? idCookie["distinctId"].Value : null;

            return data;
        }

        private bool CheckUser()
        {
            if (Utility.GetCurrentUserID() <= 0)
            {
                CookieHeaderValue tokenInfoCookie = null;
                tokenInfoCookie = Request.Headers.GetCookies
                                    ("TOKEN-INFO").SingleOrDefault();

                string data = (tokenInfoCookie != null) ? tokenInfoCookie["TOKEN-INFO"].Value : null;
                //Check if tokenInfo cookie exists, if found set session with values found in that

                if (data != null)
                {
                    long userID = 0;
                    string userName = null;
                    if (Utility.GetUserInfoFromCookieData(data, out userID, out userName))
                    {
                        Utility.SetUserSession(userID, userName);
                        return true;
                    }
                }

                return false;
            }
            return true;
        }

        
        [HttpPost]
        public JObject SaveSource([FromBody]SaveSourceData saveSourceData)
        {
            bool firstSave = saveSourceData.sourceData.sourceUserID > 0 ? false : true;

            saveSourceData.sourceData.uri = HttpUtility.UrlDecode(saveSourceData.sourceData.uri);
            saveSourceData.sourceData.url = HttpUtility.UrlDecode(saveSourceData.sourceData.url);

            Uri url = new Uri(saveSourceData.sourceData.url);

            if (!CheckUser())
                return JObject.FromObject(new
                {
                    status = "failure",
                });


            if (saveSourceData.addedFolders != null)
            {
                FolderHelper folderHelper = new FolderHelper();
                saveSourceData.addedFolderIDs = folderHelper.HandleNewAddedFolders(saveSourceData.addedFolders, Utility.GetCurrentUserID());

                if (saveSourceData.addedFolders.Count != saveSourceData.addedFolderIDs.Count)
                {
                    return JObject.FromObject(new
                    {
                        status = "failure",
                    });
                }

                string newRequiredFolderID = saveSourceData.sourceData.folderData == null ? null : (saveSourceData.sourceData.folderData.selectedFolder == null) ? null : (saveSourceData.sourceData.folderData.selectedFolder.folderID.Contains("a") ? saveSourceData.sourceData.folderData.selectedFolder.folderID : null);
                
                if (newRequiredFolderID != null)
                {
                    long newID = 0;
                    if (saveSourceData.addedFolderIDs.TryGetValue(newRequiredFolderID, out newID))
                    {
                        saveSourceData.sourceData.folderData.selectedFolder.folderID = newID.ToString();
                    }
                    else
                    {
                        return JObject.FromObject(new
                        {
                            status = "failure",
                        });
                    }
                }

            }
            saveSourceData.sourceData = sourceHelper.SaveSource(saveSourceData.sourceData, Utility.GetCurrentUserID(), Utility.GetCurrentUserName());


            sourceHelper.SendEventNewPageAdded(saveSourceData, Utility.GetCurrentUserName(), firstSave);
            if (saveSourceData.sourceData != null && saveSourceData.sourceData.sourceUserID != 0)
            {
                return JObject.FromObject(new{
                    status = "success",
                    saveSourceData = saveSourceData
                });
            }else{
                return JObject.FromObject(new
                {
                    status = "failure",
                });
            }
        }

        [HttpDelete]
        public bool DeleteSource(long sourceUserID)
        {
            
            if (!CheckUser())
            {
                return false;

            }
            return sourceHelper.DeleteSourceUser(sourceUserID, Utility.GetCurrentUserID(), Utility.GetCurrentUserName());
        }

        [HttpGet]
        public JObject GetSourceData(string URI, string Link)
        {

            URI = HttpUtility.UrlDecode(URI);
            Link = HttpUtility.UrlDecode(Link);
            //var e = Encoding.GetEncoding(URI);
            Uri url = new Uri(Link);
            if (!CheckUser())
            {
                return JObject.FromObject(new
                {
                    status = "failure",
                });
            }
            SourceDataForExtension sourceData = sourceHelper.GetSourceDataForExtension(URI, Link, Utility.GetCurrentUserID());
            return JObject.FromObject(new
            {
                sourceData = sourceData
            });
        }
    }
}
