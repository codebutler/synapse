//
// WebKitMessageView.cs: Display a chat conversation using WebKit and Adium 
//                       message styles.
// 
// Copyright (C) 2008 Eric Butler
// 
// Authors:
//   Eric Butler <eric@extremeboredom.net>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.IO;
using IO = System.IO;
using Qyoto;
using Synapse.Core;

namespace Synapse.QtClient
{
	public class ConversationWidget : QWebView
	{
		#region Private Variables
		string m_baseTemplateHtml       = null;
		string m_statusHtml             = null;
		PList  m_themeProperties        = null;
		DateTime m_timeOpened;

		string m_incomingContentHtml     = null;
		string m_incomingNextContentHtml = null;
		string m_incomingContextHtml     = null;
		string m_incomingNextContextHtml = null;
		
		string m_outgoingContentHtml     = null;
		string m_outgoingNextContentHtml = null;
		string m_outgoingContextHtml     = null;
		string m_outgoingNextContextHtml = null;
		
		string m_ThemeName              = null;
		string m_VariantName            = null;
		string m_ChatName               = null;
		string m_SourceName             = null;
		string m_DestinationName        = null;
		string m_DestinationDisplayName = null;
		
		CustomBackgroundType m_CustomBackgroundType  = CustomBackgroundType.BackgroundNormal;
		string               m_CustomBackgroundPath  = null;
		string               m_CustomBackgroundColor = null;
		
		static string s_ThemesDirectory = null;
		#endregion
		
		#region Public Static Properties
		public static string ThemesDirectory {
			get {
				return s_ThemesDirectory;
			}
			set {
				if (String.IsNullOrEmpty(value) || !Directory.Exists(value)) {
					throw new ArgumentException("Invalid themes directory (" + value + ").", "themesDirectory");
				}
				s_ThemesDirectory = value;
			}	
		}
		#endregion
		
		#region Constructor
		public ConversationWidget(QWidget parent) : base(parent)
		{
			QObject.Connect(this.Page().MainFrame(), Qt.SIGNAL("javaScriptWindowObjectCleared()"), this, Qt.SLOT("OnJavaScriptWindowObjectCleared()"));

			if (ConversationWidget.ThemesDirectory == null) {
				throw new Exception("Set ThemesDirectory first");
			}
			
			Assembly asm = Assembly.GetExecutingAssembly();
			using (StreamReader reader = new StreamReader(asm.GetManifestResourceStream("Template.html"))) {
				m_baseTemplateHtml = reader.ReadToEnd();
			}
			
			this.m_timeOpened = DateTime.Now;
		}
	#endregion
		
		#region Public Methods

		/*
		public void Initialize(string chatName, string sourceName, string destinationName, string destinationDisplayName)
		{			
			this.m_ChatName = chatName;
			this.m_SourceName = sourceName;
			this.m_DestinationName = destinationName;
			this.m_DestinationDisplayName = destinationDisplayName;
		}
		*/

		public void AppendMessage(bool incoming, bool next, string userIconPath, string senderScreenName, string sender,
		                          string senderColor, string senderStatusIcon, string senderDisplayName,
		                          string message)
		{
			string jsMethod = null;
			string template = null;
			
			if (next) {
				jsMethod = "appendNextMessage";
				if (incoming)
					template = this.m_incomingNextContentHtml;
				else
					template = this.m_outgoingNextContentHtml;
			} else {
				jsMethod = "appendMessage";
				if (incoming)
					template = this.m_incomingContentHtml;
				else
					template = this.m_outgoingContentHtml;
			}
			
			DateTime time = DateTime.Now;
			string html = FormatMessage(template, userIconPath, senderScreenName, sender, senderColor, senderStatusIcon,
			                            "ltr", senderDisplayName, String.Empty, message, time);
			html = Util.EscapeMessage(html);
			base.Page().MainFrame().EvaluateJavaScript(String.Format("{0}(\"{1}\")", jsMethod, html));
		}
		
		public void AppendStatus(string status, string message)
		{
			string html = FormatStatus(this.m_statusHtml, status, message, DateTime.Now);
			html = Util.EscapeMessage(html);
			base.Page().MainFrame().EvaluateJavaScript(String.Format("{0}(\"{1}\")", "appendMessage", html));
		}
				
		public void LoadTheme(string themeName, string variantName)
		{
			Console.WriteLine("Loading Theme...");
			string themeDirectory = System.IO.Path.Combine(ThemesDirectory, themeName) + ".AdiumMessageStyle";
			if (!Directory.Exists(themeDirectory)) {
				throw new DirectoryNotFoundException(themeDirectory);
			}
			string resourcesPath = Util.JoinPath(themeDirectory, "Contents", "Resources");
			string plistPath     = Util.JoinPath(themeDirectory, "Contents", "Info.plist");
			
			// XXX: Add additional checks for other required files.
			if (!File.Exists(plistPath)) {
				throw new Exception("Missing required theme file: Info.plist");
			}
			
			PList themeProperties = new PList(plistPath);
			if (themeProperties.GetValue<long>("MessageViewVersion") < 3) {
			//	throw new Exception("Unsupported version.");
			}
			
			// Set up base template
			string headerHtml     = FormatHeaderOrFooter(File.ReadAllText(Util.JoinPath(resourcesPath, "Header.html")));
			string footerPath     = Util.JoinPath(resourcesPath, "Footer.html");
			string footerHtml     = String.Empty;
			if (File.Exists(footerPath)) {
				footerHtml = FormatHeaderOrFooter(File.ReadAllText(footerPath));
			}
			
			string baseUri        = "file://" + resourcesPath + "/";
			string mainCssPath    = "main.css";
			string variantCssPath = Util.JoinPath("Variants", variantName + ".css");
			
			string baseHtml       = FormatBaseTemplate(themeProperties, baseUri, mainCssPath, variantCssPath, headerHtml, footerHtml);
			base.Page().MainFrame().SetHtml(baseHtml, themeDirectory);
				
			string incomingPath = Util.JoinPath(resourcesPath, "Incoming");
			string outgoingPath = Util.JoinPath(resourcesPath, "Outgoing");
			
			// Load other templates
			string statusPath              = Util.JoinPath(resourcesPath, "Status.html");
			string incomingNextContentPath = Util.JoinPath(incomingPath, "NextContent.html");
			string outgoingContentPath     = Util.JoinPath(outgoingPath, "Content.html");
			string outgoingNextContentPath = Util.JoinPath(outgoingPath, "NextContent.html");
			string incomingContextPath     = Util.JoinPath(incomingPath, "Context.html");
			string incomingNextContextPath = Util.JoinPath(incomingPath, "NextContext.html");
			string outgoingContextPath     = Util.JoinPath(outgoingPath, "Context.html");
			string outgoingNextContextPath = Util.JoinPath(outgoingPath, "NextContext.html");
			
			/* From http://trac.adiumx.com/wiki/CreatingMessageStyles:
			 * If Incoming/NextContent.html isn't found, Incoming/Content.html will be used
			 * If Outgoing/Content.html isn't found, Incoming/Content.html will be used
			 * If Outgoing/NextContent.html isn't found, whatever was used for Outgoing/Content.html will be used
			 * If any of the Context files aren't found, whatever was used for their non-Context equivalent will be used
			 * If FileTransfer.html isn't found, a modified version of Status.html will be used 
			 */
			
			this.m_statusHtml              = File.ReadAllText(statusPath);
			this.m_incomingContentHtml     = File.ReadAllText(Util.JoinPath(incomingPath, "Content.html"));
			this.m_incomingNextContentHtml = File.Exists(incomingNextContentPath) ? File.ReadAllText(incomingNextContentPath) : this.m_incomingContentHtml;
			this.m_incomingContextHtml     = File.Exists(incomingContextPath)     ? File.ReadAllText(incomingContextPath)     : this.m_incomingContentHtml;
			this.m_incomingNextContextHtml = File.Exists(incomingNextContextPath) ? File.ReadAllText(incomingNextContextPath) : this.m_incomingNextContentHtml;
			this.m_outgoingContentHtml     = File.Exists(outgoingContentPath)     ? File.ReadAllText(outgoingContentPath)     : this.m_incomingContentHtml;
			this.m_outgoingNextContentHtml = File.Exists(outgoingNextContentPath) ? File.ReadAllText(outgoingNextContentPath) : this.m_outgoingContentHtml;
			this.m_outgoingContextHtml     = File.Exists(outgoingContextPath)     ? File.ReadAllText(outgoingContextPath)     : this.m_outgoingContentHtml;
			this.m_outgoingNextContextHtml = File.Exists(incomingContextPath)     ? File.ReadAllText(incomingContextPath)     : this.m_outgoingNextContentHtml;
			
			// Save theme info
			this.m_themeProperties     = themeProperties;
			this.m_ThemeName           = themeName;
			this.m_VariantName         = variantName;
			this.m_themeProperties     = new PList(plistPath);

			OnJavaScriptWindowObjectCleared();

			Console.WriteLine("Loaded!");
		}
		#endregion
		
		#region Public Properties
		public string ChatName {
			set {
				this.m_ChatName = value;
			}
		}
		
		public string ThemeName {
			get {
				return m_ThemeName;
			}
		}
		
		public string VariantName {
			get {
				return m_VariantName;
			}
		}
		
		public CustomBackgroundType CustomBackgroundType {
			get {
				return m_CustomBackgroundType;
			}
			set {
				m_CustomBackgroundType = value;
			}
		}
		
		public string CustomBackgroundPath {
			get {
				return m_CustomBackgroundPath;
			}
			set {
				m_CustomBackgroundPath = value;
			}
		}
		
		public string CustomBackgroundColor {
			get {
				return m_CustomBackgroundColor;
			}
			set {
				m_CustomBackgroundColor = value;
			}
		}
#endregion
		
		#region Private Methods
		private string FormatBaseTemplate(PList themeProperties, string basePath, string mainPath, string variantPath, string headerHtml, string footerHtml)
		{
			mainPath = "@import url(\"" + mainPath + "\");";
			
			string html = this.m_baseTemplateHtml;
			string[] substitutions = new string[] { basePath, mainPath, variantPath, headerHtml, footerHtml };
			for (int i = 0; i < substitutions.Length; i++) {
				int index = html.IndexOf("%@");
				html = html.Remove(index, 2);
				html = html.Insert(index, substitutions[i]);
			}
			
			bool allowsCustomBackground         = !themeProperties.GetValue<bool>("DisableCustomBackground");
			bool defaultBackgroundIsTransparent = themeProperties.GetValue<bool>("DefaultBackgroundIsTransparent");
			string defaultBackgroundColor       = themeProperties.GetValue<string>("DefaultBackgroundColor");
		
			string bgStyle = String.Empty;
			
			if (allowsCustomBackground && (!String.IsNullOrEmpty(m_CustomBackgroundPath) || !String.IsNullOrEmpty(m_CustomBackgroundColor))) {
				if (!String.IsNullOrEmpty(m_CustomBackgroundPath)) {
					switch (m_CustomBackgroundType) {
					case CustomBackgroundType.BackgroundNormal:
						bgStyle = String.Format(@"background-image: url('{0}'); background-repeat: no-repeat; background-attachment: fixed", 
						                        m_CustomBackgroundPath);
						break;
					case CustomBackgroundType.BackgroundCenter:
						bgStyle = String.Format("background-image: url('{0}'); background-position: center; background-repeat: no-repeat; background-attachment:fixed;",
						                        m_CustomBackgroundPath);
						break;
					case CustomBackgroundType.BackgroundTile:
						bgStyle = String.Format("background-image: url('{0}'); background-repeat: repeat;",
						                        m_CustomBackgroundPath);
						break;
					case CustomBackgroundType.BackgroundTileCenter:
						bgStyle = String.Format("background-image: url('{0}'); background-repeat: repeat; background-position: center;", 
						                        m_CustomBackgroundPath);
						break;
					case CustomBackgroundType.BackgroundScale:
						bgStyle = String.Format("background-image: url('{0}'); -webkit-background-size: 100% 100%; background-size: 100% 100%; background-attachment: fixed;", 
						                        m_CustomBackgroundPath);
						break;
					}
				} else {
						bgStyle = "background-image: none;";
				}
				if (String.IsNullOrEmpty(m_CustomBackgroundColor)) {
					/*
					float red, green, blue, alpha;
					[customBackgroundColor getRed:&red green:&green blue:&blue alpha:&alpha];
					[bodyTag appendString:[NSString stringWithFormat:@"background-color: rgba(%i, %i, %i, %f); ", (int)(red * 255.0), (int)(green * 255.0), (int)(blue * 255.0), alpha]];
					 */
					throw new NotImplementedException();
				}
			} else {
				// XXX: I don't see this in the Adium source, but themes seem 
				// to expect it. Weird.
				bgStyle = "background-attachment: fixed;";
			}
			
			html = html.Replace("==bodyBackground==", bgStyle);
			
			return html;
		}
		              
		private string FormatHeaderOrFooter(string headerTemplateHtml)
		{
			headerTemplateHtml = headerTemplateHtml.Replace("%chatName%", this.m_ChatName);
			headerTemplateHtml = headerTemplateHtml.Replace("%sourceName%", this.m_SourceName);
			headerTemplateHtml = headerTemplateHtml.Replace("%destinationName%", this.m_DestinationName);
			headerTemplateHtml = headerTemplateHtml.Replace("%destinationDisplayName%", this.m_DestinationDisplayName);
			headerTemplateHtml = headerTemplateHtml.Replace("%timeOpened%", this.m_timeOpened.ToString());
			
			Regex regex = new Regex(@"%timeOpened\{(.*)\}%");
			headerTemplateHtml = regex.Replace(headerTemplateHtml, delegate (Match match) {
				string pattern = match.Groups[1].Value;
				return Util.Strftime(pattern, this.m_timeOpened);
			});
		
			return headerTemplateHtml;
		}
		
		private string FormatMessage(string contentTemplate, string userIconPath,
		                             string senderScreenName, string sender, 
		                             string senderColor, string senderStatusIcon, 
		                             string messageDirection, string senderDisplayName, 
		                             string service, string message, DateTime time)
		{
			if (String.IsNullOrEmpty(contentTemplate))
				throw new ArgumentNullException("contentTemplate");
			
			contentTemplate = contentTemplate.Replace("%userIconPath%", userIconPath);
			contentTemplate = contentTemplate.Replace("%senderScreenName%", senderScreenName);
			contentTemplate = contentTemplate.Replace("%sender%", sender);
			contentTemplate = contentTemplate.Replace("%senderColor%", senderColor);
			contentTemplate = contentTemplate.Replace("%senderStatusIcon%", senderStatusIcon);
			contentTemplate = contentTemplate.Replace("%messageDirection%", messageDirection);
			contentTemplate = contentTemplate.Replace("%senderDisplayName%", senderDisplayName);
			contentTemplate = contentTemplate.Replace("%service%", service);
			
			Regex regex = new Regex(@"%textbackgroundcolor\{(.*)\}%");
			contentTemplate = regex.Replace(contentTemplate, delegate (Match match) {
				//Console.WriteLine(match.Groups[1].Value);
				//throw new NotImplementedException();
				return String.Empty;
			});
			
			return FormatStatusOrMessage(contentTemplate, message, time, String.Empty); // XXX
		}
		
		private string FormatStatus(string statusTemplate, string status, string message, DateTime time)
		{
			if (String.IsNullOrEmpty(statusTemplate))
				throw new ArgumentNullException("statusTemplate");
			
			statusTemplate = statusTemplate.Replace("%status%", status);
			return FormatStatusOrMessage(statusTemplate, message, time, String.Empty); // XXX
		}
		
		private string FormatStatusOrMessage(string template, string message, DateTime time, string messageClasses)
		{
			template = template.Replace("%message%", message);
			template = template.Replace("%time%", time.ToShortTimeString());
			template = template.Replace("%shortTime%", time.ToString("%h:%m"));
			template = template.Replace("%messageClasses%", messageClasses);
			
			Regex regex = new Regex(@"%time\{(.*)\}%");
			template = regex.Replace(template, delegate (Match match) {
				string pattern = match.Groups[1].Value;
				return Util.Strftime(pattern, time);
			});
			
			return template;
		}
		#endregion

		#region Signal Handlers
		[Q_SLOT]
		private void OnJavaScriptWindowObjectCleared ()
		{
			if (m_ThemeName != null) {
				// XXX: Do anything here?
			}
		}
		#endregion
	}
	
	public enum CustomBackgroundType
	{
		BackgroundNormal,
		BackgroundCenter,
		BackgroundTile,
		BackgroundTileCenter,
		BackgroundScale
	}
}