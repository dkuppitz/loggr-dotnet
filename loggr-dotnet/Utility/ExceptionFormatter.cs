using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Web;

namespace Loggr.Utility
{
    public class ExceptionFormatter
    {
        private ExceptionFormatter()
        {
        }

        public static string FormatType(string type)
        {
            if (type == null || type.Length == 0)
            {
                return string.Empty;
            }

            int lastDotIndex = CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(type, '.');

            if (lastDotIndex > 0)
            {
                type = type.Substring(lastDotIndex + 1);
            }

            const string conventionalSuffix = "Exception";

            if (type.Length > conventionalSuffix.Length)
            {
                int suffixIndex = type.Length - conventionalSuffix.Length;

                if (string.Compare(type, suffixIndex, conventionalSuffix, 0, conventionalSuffix.Length, true, CultureInfo.InvariantCulture) == 0)
                {
                    type = type.Substring(0, suffixIndex);
                }
            }

            return type;
        }

        public static string FormatType(Exception ex)
        {
            if (ex == null)
            {
                throw new System.ArgumentNullException("error");
            }

            return FormatType(ex.GetType().ToString());
        }

        public static string Format(Exception ex)
        {
            return Format(ex, null);
        }

        public static string Format(Exception ex, object traceObject)
        {
            var res = new StringBuilder()

                .AppendFormat("<b>Exception</b>: {0}<br />", ex.Message)
                .AppendFormat("<b>Type</b>: {0}<br />", ex.GetType())
                .AppendFormat("<b>Machine</b>: {0}<br />", System.Environment.MachineName)
                .Append("<br />")
                .Append(GetFormattedWebDetails(HttpContext.Current))
                .Append(GetFormattedTraceObject(traceObject))
                .Append("<br />")
                .Append("<b>Stack Trace</b><br />")
                .Append("<br />")
                .Append(GetFormattedStackTrace(ex));

            return res.ToString();
        }

        private static string GetFormattedWebDetails(HttpContext ctx)
        {
            if (ctx != null)
            {
                var res = new StringBuilder()

                    .AppendFormat("<b>Request URL</b>: {0}<br />", ctx.Request.Url)
                    .AppendFormat("<b>Is Authenticated</b>: {0}<br />", ctx.User.Identity.IsAuthenticated ? "True" : "False")
                    .AppendFormat("<b>User</b>: {0}<br />", ctx.User.Identity.IsAuthenticated ? ctx.User.Identity.Name : "anonymous")
                    .AppendFormat("<b>User host address</b>: {0}<br />", ctx.Request.ServerVariables["REMOTE_ADDR"])
                    .AppendFormat("<b>Request Method</b>: {0}<br />", ctx.Request.ServerVariables["REQUEST_METHOD"])
                    .AppendFormat("<b>User Agent</b>: {0}<br />", ctx.Request.ServerVariables["HTTP_USER_AGENT"])
                    .AppendFormat("<b>Referer</b>: {0}<br />", ctx.Request.ServerVariables["HTTP_REFERER"])
                    .AppendFormat("<b>Script Name</b>: {0}<br />", ctx.Request.ServerVariables["SCRIPT_NAME"]);

                return res.ToString();
            }
            else return null;
        }

        private static string GetFormattedTraceObject(object traceObject)
        {
            if (traceObject != null)
            {
                var res = new StringBuilder()

                    .Append("<br />")
                    .Append("<b>Traced Object(s)</b><br />")
                    .Append("<br />")
                    .Append(ObjectDumper.DumpObject(traceObject, 1));

                return res.ToString();
            }
            else return null;
        }

        private static string GetFormattedStackTrace(Exception ex)
        {
            var res = new StringBuilder();

            if (ex.InnerException != null)
            {
                res.Append(GetFormattedStackTrace(ex.InnerException));
            }

            res.AppendFormat("[{0}: {1}]<br />", ex.GetType(), ex.Message);

            if (ex.StackTrace != null)
            {
                res.Append(HttpUtility.HtmlEncode(ex.StackTrace).Replace(Environment.NewLine, "<br />"));
            }
            else
            {
                res.Append("No stack trace");
            }

            res.Append("<br/><br/>");

            return res.ToString();
        }
    }
}