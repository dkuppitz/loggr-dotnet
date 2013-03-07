using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Web;

namespace Loggr
{
    /// <summary>
    /// Allows applications to post events and track users with Loggr
    /// </summary>
    public class LogClient
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of Loggr.LogClient class by using configuration file settings
        /// </summary>
        public LogClient()
            : this(Utility.Configuration.LogKey, Utility.Configuration.ApiKey, Utility.Configuration.Server, Utility.Configuration.Version, Utility.Configuration.Secure)
        {
        }

        /// <summary>
        /// Initializes a new instance of Loggr.LogClient class by using configuration file settings, but specifying SSL
        /// </summary>
        /// <param name="Secure">Use SSL for posting to Loggr</param>
        public LogClient(bool Secure)
            : this(Utility.Configuration.LogKey, Utility.Configuration.ApiKey, Utility.Configuration.Server, Utility.Configuration.Version, Secure)
        {
        }

        /// <summary>
        /// Initializes a new instance of Loggr.LogClient class by using the specified LogKey and ApiKey
        /// </summary>
        /// <param name="LogKey">Key used to identify a log on Loggr</param>
        /// <param name="ApiKey">Key used to provide access to API on Loggr</param>
        public LogClient(string LogKey, string ApiKey)
            : this(LogKey, ApiKey, Utility.Configuration.Server, Utility.Configuration.Version, Utility.Configuration.Secure)
        {
        }

        /// <summary>
        /// Initializes a new instance of Loggr.LogClient class by using the specified LogKey, ApiKey and SSL mode
        /// </summary>
        /// <param name="LogKey">Key used to identify a log on Loggr</param>
        /// <param name="ApiKey">Key used to provide access to API on Loggr</param>
        /// <param name="Secure">Use SSL for posting to Loggr</param>
        public LogClient(string LogKey, string ApiKey, bool Secure)
            : this(LogKey, ApiKey, Utility.Configuration.Server, Utility.Configuration.Version, Secure)
        {
        }

        /// <summary>
        /// Initializes a new instance of Loggr.LogClient class by using the specified LogKey, ApiKey, Server, Version and SSL mode
        /// </summary>
        /// <param name="LogKey">Key used to identify a log on Loggr</param>
        /// <param name="ApiKey">Key used to provide access to API on Loggr</param>
        /// <param name="Server">Hostname of server for posting to Loggr (typically post.loggr.net)</param>
        /// <param name="Version">Version of API for posting to Loggr (typically 1)</param>
        /// <param name="Secure">Use SSL for posting to Loggr</param>
        public LogClient(string LogKey, string ApiKey, string Server, string Version, bool Secure)
        {
            _logKey = LogKey;
            _apiKey = ApiKey;
            _server = Server;
            _version = Version;
            _secure = Secure;
        }

        #endregion

        #region Properties

        protected string _apiKey = "";
        protected string _logKey = "";
        protected string _version = "";
        protected string _server = "";
        protected bool _secure = false;

        public string ApiKey
        {
            get
            {
                return _apiKey;
            }
            set
            {
                _apiKey = value;
            }
        }

        public string LogKey
        {
            get
            {
                return _logKey;
            }
            set
            {
                _logKey = value;
            }
        }

        public string Version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
            }
        }

        public string Server
        {
            get
            {
                return _server;
            }
            set
            {
                _server = value;
            }
        }

        public bool Secure
        {
            get
            {
                return _secure;
            }
            set
            {
                _secure = value;
            }
        }

        #endregion

        #region Post

        private delegate void PostEventDelegate(Event eventObj);

        /// <summary>
        /// Posts the specified event to Loggr (posts asynchronously)
        /// </summary>
        /// <param name="eventObj">A Loggr.Event that contains the event to send</param>
        public void Post(Event eventObj)
        {
            this.Post(eventObj, true);
        }

        /// <summary>
        /// Posts the specified event to Loggr
        /// </summary>
        /// <param name="eventObj">A Loggr.Event that contains the event to send</param>
        /// <param name="async">A bool that specifies how the event should be posted. Typically an application will post asynchronously for best performance, but sometimes an event needs to be posted synchronously if the application needs to block until the event has completed posting</param>
        public virtual void Post(Event eventObj, bool async)
        {
            // make sure our event has at least a text field
            if (string.IsNullOrEmpty(eventObj.Text))
                throw new ApplicationException("Event cannot have an empty Text field");

            // modify event based on configuration
            MergeConfigurationWithEvent(eventObj);

            // post async or sync
            if (async)
            {
                PostEventDelegate del = new PostEventDelegate(PostEventBase);
                del.BeginInvoke(eventObj, null, null);
            }
            else PostEventBase(eventObj);
        }

        [DebuggerNonUserCode()]
        protected void PostEventBase(Event eventObj)
        {
            if (!string.IsNullOrEmpty(this.ApiKey) && !string.IsNullOrEmpty(this.LogKey))
            {
                string url = string.Format("{3}://{0}/{1}/logs/{2}/events", this.Server, this.Version, this.LogKey, this.Secure ? "https" : "http");
                string postStr = string.Format("{0}&apikey={1}", CreateEventQuerystring(eventObj), this.ApiKey);
                try
                {
                    HttpClient.PostData(url, postStr);
                }
                catch (Exception)
                {
                    // ignore ex from post
                }
            }
        }

        #endregion

        #region Track

        private delegate void TrackUserDelegate(string username, string email, string page);

        /// <summary>
        /// Tracks a user on Loggr
        /// </summary>
        /// <param name="username">Username of user to track</param>
        public void TrackUser(string username)
        {
            this.TrackUser(username, "", "", true);
        }

        /// <summary>
        /// Tracks a user on Loggr
        /// </summary>
        /// <param name="username">Username of user to track</param>
        /// <param name="email">Email address of user to track</param>
        public void TrackUser(string username, string email)
        {
            this.TrackUser(username, email, "", true);
        }

        /// <summary>
        /// Tracks a user on Loggr
        /// </summary>
        /// <param name="username">Username of user to track</param>
        /// <param name="email">Email address of user to track</param>
        /// <param name="page">Page being viewed by user</param>
        public void TrackUser(string username, string email, string page)
        {
            this.TrackUser(username, email, page, true);
        }

        /// <summary>
        /// Tracks a user on Loggr
        /// </summary>
        /// <param name="username">Username of user to track</param>
        /// <param name="email">Email address of user to track</param>
        /// <param name="page">Page being viewed by user</param>
        /// <param name="async">A bool that specifies how user tracking should be sent to Loggr. Typically an application will post asynchronously for best performance, but sometimes it needs to be posted synchronously if the application needs to block until the post has completed</param>
        public void TrackUser(string username, string email, string page, bool async)
        {
            // post async or sync
            if (async)
            {
                TrackUserDelegate del = new TrackUserDelegate(TrackUserBase);
                del.BeginInvoke(username, email, page, null, null);
            }
            else TrackUserBase(username, email, page);
        }

        [DebuggerNonUserCode()]
        protected void TrackUserBase(string username, string email, string page)
        {
            if (!string.IsNullOrEmpty(this.ApiKey) && !string.IsNullOrEmpty(this.LogKey))
            {
                string url = string.Format("{3}://{0}/{1}/logs/{2}/users", this.Server, this.Version, this.LogKey, this.Secure ? "https" : "http");
                string postStr = string.Format("{0}&apikey={1}", CreateUserQuerystring(username, email, page), this.ApiKey);
                try
                {
                    HttpClient.PostData(url, postStr);
                }
                catch (Exception)
                {
                    // ignore ex from post
                }
            }
        }

        #endregion

        #region Protected Methods

        protected void MergeConfigurationWithEvent(Event eventObj)
        {
            // merge in default tags from config file
            if (!string.IsNullOrEmpty(Utility.Configuration.Tags))
            {
                eventObj.Tags.AddRange(Utility.Tags.TokenizeAndFormat(Utility.Configuration.Tags));
            }

            // overwrite default source from config file
            if (!string.IsNullOrEmpty(Utility.Configuration.Source))
            {
                eventObj.Source = Utility.Configuration.Source;
            }

            // overwrite default user from config file
            if (!string.IsNullOrEmpty(Utility.Configuration.User))
            {
                eventObj.User = Utility.Configuration.User;
            }
        }

        protected string CreateEventQuerystring(Event eventObj)
        {
            var parameters = new NameValueCollection
            {
                { "text", Cap(eventObj.Text, 500) },
                { "link", Cap(eventObj.Link, 200) },
                { "tags", Cap(eventObj.Tags.ToArray(), 200) },
                { "source", Cap(eventObj.Source, 200) },
                { "user", Cap(eventObj.User, 200) },
            };

            if (eventObj.DataType == DataType.html)
            {
                parameters.Add("data", Cap(string.Concat("@html", Environment.NewLine, eventObj.Data), 5120));
            }
            else if (eventObj.DataType == DataType.json)
            {
                parameters.Add("data", Cap(string.Concat("@json", Environment.NewLine, eventObj.Data), 5120));
            }
            else
            {
                parameters.Add("data", Cap(eventObj.Data, 5120));
            }

            if (eventObj.Timestamp.HasValue)
            {
                parameters.Add("timestamp", Cap(DateToMilliseconds(eventObj.Timestamp.Value), 30));
            }

            if (eventObj.Value.HasValue)
            {
                parameters.Add("value", Cap(eventObj.Value.Value, 30));
            }

            if (eventObj.Geo != null)
            {
                parameters.Add("geo", Cap(eventObj.Geo, 30));
            }

            return ConvertToQueryString(parameters);
        }

        protected string CreateUserQuerystring(string username, string email, string page)
        {
            return ConvertToQueryString(new NameValueCollection
                {
                    {"user", Cap(username, 100) },
                    {"email", Cap(email, 100) },
                    {"page", Cap(page, 100) }
                });
        }

        protected static double DateToMilliseconds(DateTime input)
        {
            return (input - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
        }

        protected static string Cap(string input, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("Length", length, "Length must be > 0");
            else if (length == 0 || input == null || input.Length == 0)
                return string.Empty;
            else if (input.Length <= length)
                return input;
            else
                return input.Substring(0, length);
        }

        protected static string Cap(string[] input, int length)
        {
            return Cap(string.Join(" ", input), length);
        }

        protected static string Cap(double input, int length)
        {
            return Cap(input.ToString(CultureInfo.InvariantCulture), length);
        }

        protected static string ConvertToQueryString(NameValueCollection nvc)
        {
            return string.Join("&", Array.ConvertAll(nvc.AllKeys,
                key => string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(nvc[key]))));
        }

        #endregion

        #region Test Helpers

        private IHttpClient _httpClient;

        private IHttpClient HttpClient
        {
            get
            {
                if (_httpClient == null)
                    _httpClient = new HttpClient();
                return _httpClient;
            }
            set
            {
                _httpClient = value;
            }
        }

        public void SetHttpClient(IHttpClient client)
        {
            HttpClient = client;
        }

        #endregion

    }
}
