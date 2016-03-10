using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Utils;
using Newtonsoft.Json;
using GoogleContactTest.Class;
using Google.GData.Client;
using Google.Contacts;
using Google.GData.Contacts;

namespace GoogleContactTest
{
    /// <summary>
    /// OAuthV2 的摘要描述
    /// </summary>
    public class OAuthV2 : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            string code = context.Request.Params["code"];

            if (code != null)
            {
                string contact = GetContact(GetAccessToken(code));
                context.Response.ContentType = "text/plain";
                context.Response.Write(contact);
                return;
            }
            string _oauthUrl = string.Format("https://accounts.google.com/o/oauth2/auth?" +
                        "scope={0}&state={1}&redirect_uri={2}&response_type=code&client_id={3}&approval_prompt=force",
                         System.Web.HttpUtility.UrlEncode("https://www.google.com/m8/feeds/ https://www.googleapis.com/auth/contacts.readonly"),
                        "",
                         System.Web.HttpUtility.UrlEncode(APConfig.GetAppConfig("REDIRECT_URI")),
                         System.Web.HttpUtility.UrlEncode(APConfig.GetAppConfig("GOOGLE_CLIENT_ID")));
            HttpContext.Current.Response.Redirect(_oauthUrl);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }

        }

        #region 取得Token
        /// <summary>
        /// 取得Token
        /// </summary>
        /// <param name="code">code</param>
        /// <returns></returns>
        private GoogleAccessToken GetAccessToken(string code)
        {
            string responseFromServer = "";
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://accounts.google.com/o/oauth2/token");

            string queryStringFormat = @"code={0}&client_id={1}&client_secret={2}&redirect_uri={3}&grant_type=authorization_code";
            string contents = string.Format(queryStringFormat
                                               , code
                                               , System.Web.HttpUtility.UrlEncode(APConfig.GetAppConfig("GOOGLE_CLIENT_ID"))
                                               , System.Web.HttpUtility.UrlEncode(APConfig.GetAppConfig("GOOGLE_CLIENT_SECRET"))
                                               , System.Web.HttpUtility.UrlEncode(APConfig.GetAppConfig("REDIRECT_URI")));

            request.Method = "POST";
            byte[] postcontentsArray = Encoding.UTF8.GetBytes(contents);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postcontentsArray.Length;
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(postcontentsArray, 0, postcontentsArray.Length);
                requestStream.Close();
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    responseFromServer = reader.ReadToEnd();
                    reader.Close();
                    responseStream.Close();
                    response.Close();
                }
            }
            return JsonConvert.DeserializeObject<GoogleAccessToken>(responseFromServer);
        }
        #endregion

        #region 取得聯絡人
        /// <summary>
        /// 取得聯絡人
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns></returns>
        private string GetContact(GoogleAccessToken token)
        {
            string refreshToken = token.refresh_token;
            string accessToken = token.access_token;
            string scopes = "https://www.google.com/m8/feeds/contacts/default/full/";
            OAuth2Parameters oAuthparameters = new OAuth2Parameters()
            {
                ClientId = APConfig.GetAppConfig("GOOGLE_CLIENT_ID"),
                ClientSecret = APConfig.GetAppConfig("GOOGLE_CLIENT_SECRET"),
                RedirectUri = APConfig.GetAppConfig("REDIRECT_URI"),
                Scope = scopes,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };


            RequestSettings settings = new RequestSettings("<var>YOUR_APPLICATION_NAME</var>", oAuthparameters);
            ContactsRequest cr = new ContactsRequest(settings);
            ContactsQuery query = new ContactsQuery(ContactsQuery.CreateContactsUri("default"));
            query.NumberToRetrieve = 5000;
            Feed<Contact> feed = cr.Get<Contact>(query);

            StringBuilder sb = new StringBuilder();
            int i = 1;
            foreach (Contact contact in feed.Entries)
            {
                sb.Append(i + " ").AppendLine(contact.Name.FullName);
                i++;
                //foreach (EMail email in entry.Emails)
                //{
                //    sb.Append(i + " ").Append(email.Address).AppendLine();
                //    i++;
                //}
            }
            return sb.ToString();
        }
        #endregion

    }
}