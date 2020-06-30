using System;
using System.IO;
using System.Configuration;
using System.Web;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace MPN.Framework.Communication
{
	public class EmailHelper
	{
	    /// <summary>
	    /// Constructs a branded email, based upon the arguments supplied, and then delivers it.
	    /// </summary>
	    /// <param name="templateName">The name of the template to use for this email.</param>
	    /// <param name="isHtml">Is the mail HTML or TEXT format?</param>
	    /// <param name="recipient">The email address for the person to send this mail to</param>
	    /// <param name="arguments">The values to fill the mail with. Must be ordered according to the fill-order.</param>
	    /// <returns>A boolean, saying whether or not the delivery was successful.</returns>
	    public static bool SendMail(string templateName, bool isHtml, string recipient, string[] arguments)
		{
			/*
			// -------------------------------------------------------------------------------- //
			// CONFIGURATION
			// The web.config or app.config requires the following keys for SendMail() to work:
			// - MediaPanther.Framework.Email.TemplatePath 
			//   The path to where the template files are stored on a local or unc path.
			// - EmailTemplatePathIsWebPath
			//   Boolean; indicates whether EmailTemplatePath is a relative path for a web-app
			//   or a regular file path.
			// - MediaPanther.Framework.Email.FromAddress
			//   The email address the sent email should be from.
			// - MediaPanther.Framework.Email.SmtpServer
			//   The SMTP server to be used for sending the email. Must allow connections for
			//   the host this application runs on.
			//
			// Templates should have two versions, a text and html version. Name as {0}.txt or
			// {0}.html - where {0} is the template name. Only one has to be supplied, the one
			// that's called, but if a call is made for a format that a temnplate doesn't exit
			// for then the function will fail and return false.
			// -------------------------------------------------------------------------------- //
			*/

			if (arguments == null || arguments.Length == 0)
				return false;

			// retrieve the template.
			var templateExt = (isHtml) ? ".html" : ".txt";
			var configPath = ConfigurationManager.AppSettings["MediaPanther.Framework.Email.TemplatePath"];
			var templateRoot = (bool.Parse(ConfigurationManager.AppSettings["MediaPanther.Framework.Email.TemplatePathIsWebPath"])) ? HttpContext.Current.Server.MapPath(configPath) : configPath;
			var templatePath = string.Format("{0}{1}{2}", templateRoot, templateName, templateExt);
			var template = File.ReadAllText(templatePath);

			// template not readable.
			if (template == string.Empty)
				return false;

			// extract the mail subject.
            var subjectMatch = Regex.Match(template, "(##subject=(.*?)##)", RegexOptions.IgnoreCase);
			var subject = subjectMatch.Groups[2].Value;
			template = template.Replace(subjectMatch.Groups[1].Value, string.Empty);

			// merge the arguments with the template.
			for (var i = 0; i < arguments.Length; i++)
				template = template.Replace("##" + i + "##", arguments[i]);

			// construct and deliver the email.
			var m = new MailMessage();

			m.To.Add(recipient);
			m.Subject = subject;
			m.IsBodyHtml = isHtml;
			m.Body = template;
			m.From = new MailAddress(ConfigurationManager.AppSettings["MediaPanther.Framework.Email.FromAddress"]);

			try
			{
				var smtp = new SmtpClient(ConfigurationManager.AppSettings["MediaPanther.Framework.Email.SmtpServer"]);
				smtp.Send(m);
				return true;
			}
			catch (Exception ex)
			{
				Logger.LogError(ex.Message);
				return false;
			}
		}
	}
}