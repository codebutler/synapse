//
// WebKitMessageView.cs: Display a chat conversation using WebKit and Adium 
//                       message styles.
// 
// Copyright (C) 2008 Eric Butler
// 
// Authors:
//   Eric Butler <eric@extremeboredom.net>
//
// Based on code from the Adium project.
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using IO = System.IO;
using Qyoto;
using Synapse.Core;
using Synapse.UI;
using Synapse.UI.Chat;
using Synapse.Xmpp;
using Mono.Addins;

namespace Synapse.QtClient
{
	public class ConversationWidget : QWebView
	{
		#region Private Variables
		int    m_StyleVersion;
		string m_StylePath;
		PList m_StyleInfo;
		
		// Templates
		string m_HeaderHtml;
		string m_FooterHtml;
		string m_BaseHtml;
		string m_StatusHtml         = null;
		string m_ContentInHtml      = null;
		string m_NextContentInHtml  = null;
		string m_ContextInHtml      = null;
		string m_NextContextInHtml  = null;		
		string m_ContentOutHtml     = null;
		string m_NextContentOutHtml = null;
		string m_ContextOutHtml     = null;
		string m_NextContextOutHtml = null;
		string m_FileTransferHtml   = null;

		DateTime m_TimeOpened;
		
		bool m_ThemeLoaded = false;
		
		string m_ChatName               = null;
		string m_SourceName             = null;
		string m_DestinationName        = null;
		string m_DestinationDisplayName = null;
		
		// Style settings
		bool m_AllowsCustomBackground;
		bool m_TransparentDefaultBackground;
		bool m_AllowsUserIcons;
		bool m_UsingCustomTemplateHtml;
		
		// Behavior
		// FIXME: NSDateFormatter *timeStampFormatter;
		// FIXME: AINameFormat nameFormat;
		bool m_CombineConsecutive;
		bool m_AllowsColors;
		// FIXME: NSImage *userIconMask;
		
		SynapseJSObject m_JSWindowObject;
		
		static string s_ThemesDirectory = null;
		static Dictionary<string, PList> s_AllThemes;

		const string APPEND_MESSAGE_WITH_SCROLL      = "checkIfScrollToBottomIsNeeded(); appendMessage(\"{0}\"); scrollToBottomIfNeeded();";
		const string APPEND_NEXT_MESSAGE_WITH_SCROLL = "checkIfScrollToBottomIsNeeded(); appendNextMessage(\"{0}\"); scrollToBottomIfNeeded();";
		const string APPEND_MESSAGE                  = "appendMessage(\"{0}\");";
		const string APPEND_NEXT_MESSAGE             = "appendNextMessage(\"{0}\");";
		const string APPEND_MESSAGE_NO_SCROLL        = "appendMessageNoScroll(\"{0}\");";
		const string APPEND_NEXT_MESSAGE_NO_SCROLL	 = "appendNextMessageNoScroll(\"{0}\");";
		const string REPLACE_LAST_MESSAGE            = "replaceLastMessage(\"{0}\");";
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
		
		public static IDictionary<string, PList> AllThemes {
			get {
				// FIXME: Offer some way to reload themes... or better yet, watch the directory.
				if (s_AllThemes == null) {				
					s_AllThemes = new Dictionary<string, PList>();
					var dirInfo = new DirectoryInfo(s_ThemesDirectory);
					foreach (var subDirInfo in dirInfo.GetDirectories()) {
						if (subDirInfo.Name.EndsWith(".AdiumMessageStyle")) {
							string plistPath = Util.JoinPath(subDirInfo.FullName, "Contents", "Info.plist");
							if (File.Exists(plistPath))
								s_AllThemes.Add(subDirInfo.Name.Substring(0, subDirInfo.Name.Length - subDirInfo.Extension.Length), new PList(plistPath));
						}
					}
				}
				
				return s_AllThemes;
			}
		}
		
		public static IEnumerable<string> GetVariants (string themeName)
		{
			var dirInfo = new DirectoryInfo(Util.JoinPath(s_ThemesDirectory, themeName + ".AdiumMessageStyle", "Contents", "Resources", "Variants"));
			if (dirInfo.Exists) {
				foreach (var fileInfo in dirInfo.GetFiles()) {
					if (fileInfo.Extension.ToLower() == ".css") {
						yield return fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
					}
				}
			} else {
				string noVariantName = s_AllThemes[themeName].ContainsKey("DisplayNameForNoVariant") ? s_AllThemes[themeName].Get<string>("DisplayNameForNoVariant") : "Normal";
				yield return noVariantName;
			}
		}
		#endregion
		
		#region Constructor
		public ConversationWidget(QWidget parent) : base(parent)
		{
			m_JSWindowObject = new SynapseJSObject(this);
			this.m_TimeOpened = DateTime.Now;
			
			this.DateFormat = "t";
			this.ShowUserIcons = true;
			this.ShowHeader = true;
			this.AllowTextBackgrounds = true;
			this.ShowIncomingFont = true;
			this.ShowIncomingColors = true;		
		}
		
		#endregion
		
		#region Public Methods
		public void LoadTheme(string themeName, string variantName)
		{
			if (this.ChatHandler == null) {
				throw new InvalidOperationException("Set ChatHandler first");
			}
			
			string themeDirectory = System.IO.Path.Combine(ThemesDirectory, themeName) + ".AdiumMessageStyle";
			if (!Directory.Exists(themeDirectory)) {
				throw new DirectoryNotFoundException(themeDirectory);
			}
			
			m_StylePath = Util.JoinPath(themeDirectory, "Contents", "Resources");
			
			string plistPath = Util.JoinPath(themeDirectory, "Contents", "Info.plist");
			
			// XXX: Add additional checks for other required files.
			if (!File.Exists(plistPath)) {
				throw new Exception("Missing required theme file: Info.plist");
			}
			
			m_StyleInfo = new PList(plistPath);
	
			// Default Behavior
			m_AllowsCustomBackground = true;
			m_AllowsUserIcons = true;	
			
			m_StyleVersion = m_StyleInfo.GetInt("MessageViewVersion");
			
			// Pre-fetch our templates			
			LoadTemplates();
			
			// Style flags
			m_AllowsCustomBackground = !m_StyleInfo.Get<bool>("DisableCustomBackground");
			m_TransparentDefaultBackground = m_StyleInfo.Get<bool>("DefaultBackgroundIsTransparent");
			if (m_TransparentDefaultBackground) {
				// FIXME:
				Console.WriteLine("Transparent background not supported");
			}
			
			m_CombineConsecutive = !m_StyleInfo.Get<bool>("DisableCombineConsecutive");
			
			if (m_StyleInfo.ContainsKey("ShowsUserIcons")) {
				m_AllowsUserIcons = m_StyleInfo.Get<bool>("ShowsUserIcons");
			}
			
			// User icon masking
			var maskName = m_StyleInfo.Get<string>("ImageMask");
			if (!String.IsNullOrEmpty(maskName)) {
				// FIXME:
				Console.WriteLine("ImageMask not supported");
			}
			
			m_AllowsColors = m_StyleInfo.Get<bool>("AllowTextColors");
			if (!m_AllowsColors) {
				Console.WriteLine("AllowTextColors not supported");
			}
			
			// FIXME: Need to selectively show certain actions depending on what's under the cursor.
			//this.AddAction(this.PageAction(QWebPage.WebAction.OpenLink));
			//this.AddAction(this.PageAction(QWebPage.WebAction.CopyLinkToClipboard));
			//this.AddAction(this.PageAction(QWebPage.WebAction.CopyImageToClipboard));
			//this.AddSeparator();
			QAction copyAction = this.PageAction(QWebPage.WebAction.Copy);
			copyAction.SetShortcuts(QKeySequence.StandardKey.Copy);
			this.AddAction(copyAction);
			this.AddAction(this.PageAction(QWebPage.WebAction.InspectElement));
			
			// Create the base template
			string baseUri = "file://" + m_StylePath + "/";
			string mainCssPath = "main.css";
			string variantCssPath = PathForVariant(variantName);
			var formattedBaseTemplate = FormatBaseTemplate(baseUri, mainCssPath, variantCssPath);
			base.Page().MainFrame().SetHtml(formattedBaseTemplate, themeDirectory);
			
			QObject.Connect(this.Page().MainFrame(), Qt.SIGNAL("javaScriptWindowObjectCleared()"), HandleJavaScriptWindowObjectCleared);

			if (ConversationWidget.ThemesDirectory == null) {
				throw new Exception("Set ThemesDirectory first");
			}

			this.ContextMenuPolicy = ContextMenuPolicy.ActionsContextMenu;
			
			this.Page().linkDelegationPolicy = QWebPage.LinkDelegationPolicy.DelegateAllLinks;
			QObject.Connect<QUrl>(this, Qt.SIGNAL("linkClicked(QUrl)"), HandleLinkClicked);
			
			m_ThemeLoaded = true;
			
			HandleJavaScriptWindowObjectCleared();
		}
	
		void LoadTemplates ()
		{
			// Check if theme is version 1 and has its own Template.html
			string customTemplatePath = Util.JoinPath(m_StylePath, "Template.html");
			
			// Hack for files with bad case...
			foreach (var fileInfo in new DirectoryInfo(m_StylePath).GetFiles()) {
				if (fileInfo.Name.ToLower() == "template.html") {
					customTemplatePath = Util.JoinPath(m_StylePath, fileInfo.Name);
				}
			}
			
			if (!File.Exists(customTemplatePath) && m_StyleVersion >= 1) {
				Assembly asm = Assembly.GetExecutingAssembly();
				using (StreamReader reader = new StreamReader(asm.GetManifestResourceStream("Template.html"))) {
					m_BaseHtml = reader.ReadToEnd();
				}
				m_UsingCustomTemplateHtml = false;
			} else {
				m_BaseHtml = File.ReadAllText(customTemplatePath);
				m_UsingCustomTemplateHtml = true;
			}					
			
			// Set up base template
			string headerPath = Util.JoinPath(m_StylePath, "Header.html");
			m_HeaderHtml = (File.Exists(headerPath)) ? FormatHeaderOrFooter(File.ReadAllText(headerPath)) : String.Empty;
			string footerPath = Util.JoinPath(m_StylePath, "Footer.html");
			m_FooterHtml = (File.Exists(footerPath)) ? FormatHeaderOrFooter(File.ReadAllText(footerPath)) : String.Empty;
			
			string incomingPath = Util.JoinPath(m_StylePath, "Incoming");
			string outgoingPath = Util.JoinPath(m_StylePath, "Outgoing");
			
			// Load other templates
			string statusPath              = Util.JoinPath(m_StylePath, "Status.html");
			string incomingContentPath     = Util.JoinPath(incomingPath, "Content.html");
			string incomingNextContentPath = Util.JoinPath(incomingPath, "NextContent.html");
			string outgoingContentPath     = Util.JoinPath(outgoingPath, "Content.html");
			string outgoingNextContentPath = Util.JoinPath(outgoingPath, "NextContent.html");
			string incomingContextPath     = Util.JoinPath(incomingPath, "Context.html");
			string incomingNextContextPath = Util.JoinPath(incomingPath, "NextContext.html");
			string outgoingContextPath     = Util.JoinPath(outgoingPath, "Context.html");
			string outgoingNextContextPath = Util.JoinPath(outgoingPath, "NextContext.html");
			string filetransferRequestPath = Util.JoinPath(m_StylePath, "FileTransferRequest.html");
			
			/* From http://trac.adiumx.com/wiki/CreatingMessageStyles:
			 * If Incoming/NextContent.html isn't found, Incoming/Content.html will be used
			 * If Outgoing/Content.html isn't found, Incoming/Content.html will be used
			 * If Outgoing/NextContent.html isn't found, whatever was used for Outgoing/Content.html will be used
			 * If any of the Context files aren't found, whatever was used for their non-Context equivalent will be used
			 * If FileTransfer.html isn't found, a modified version of Status.html will be used 
			 */
			
			this.m_StatusHtml         = File.ReadAllText(statusPath);
			this.m_ContentInHtml      = File.ReadAllText(incomingContentPath);
			this.m_NextContentInHtml  = File.Exists(incomingNextContentPath) ? File.ReadAllText(incomingNextContentPath) : this.m_ContentInHtml;
			this.m_ContextInHtml      = File.Exists(incomingContextPath)     ? File.ReadAllText(incomingContextPath)     : this.m_ContentInHtml;
			this.m_NextContextInHtml  = File.Exists(incomingNextContextPath) ? File.ReadAllText(incomingNextContextPath) : this.m_NextContentInHtml;
			this.m_ContentOutHtml     = File.Exists(outgoingContentPath)     ? File.ReadAllText(outgoingContentPath)     : this.m_ContentInHtml;
			this.m_NextContentOutHtml = File.Exists(outgoingNextContentPath) ? File.ReadAllText(outgoingNextContentPath) : this.m_ContentOutHtml;
			this.m_ContextOutHtml     = File.Exists(outgoingContextPath)     ? File.ReadAllText(outgoingContextPath)     : this.m_ContentOutHtml;
			this.m_NextContextOutHtml = File.Exists(outgoingNextContextPath)     ? File.ReadAllText(outgoingNextContextPath)     : this.m_NextContentInHtml;
			
			if (File.Exists(filetransferRequestPath)) {
				this.m_FileTransferHtml = File.ReadAllText(filetransferRequestPath);
				// FIXME:
				/*
				[fileTransferHTMLTemplate replaceKeyword:@"%message%"
							 	   	          withString:@"<p><img src=\"%fileIconPath%\" style=\"width:32px; height:32px; vertical-align:middle;\"></img><input type=\"button\" onclick=\"%saveFileAsHandler%\" value=\"Download %fileName%\"></p>"];
				[fileTransferHTMLTemplate replaceKeyword:@"Download %fileName%"
						                      withString:[NSString stringWithFormat:AILocalizedString(@"Download %@", "%@ will be a file name"), @"%fileName%"]];
				*/
			}
		}
	
		public void AppendContent(AbstractChatContent content, bool contentIsSimilar, bool willAddMoreContentObjects, 
		                          bool replaceLastContent)
		{
			if (content == null)
				throw new ArgumentNullException("content");
			
			if (!m_ThemeLoaded)
				throw new Exception("Call LoadTheme() first!");

			var js = ScriptForAppendingContent(content, contentIsSimilar, willAddMoreContentObjects, replaceLastContent);
			Page().MainFrame().EvaluateJavaScript(js);
		}
		#endregion
		
		#region Public Properties
		public IChatHandler ChatHandler {
			get;
			set;
		}
		
		// FIXME: Almost all of these public r/w properties aren't used...
		// Need to integrate with preferences...
		
		
		public string DateFormat {
			get;
			set;
		}
		
		public bool UseCustomNameFormat {
			get;
			set;
		}
		
		public bool ShowUserIcons {
			get;
			set;
		}
		
		public bool ShowHeader {
			get;
			set;
		}
		
		public bool AllowTextBackgrounds {
			get;
			set;
		}
		
		public bool ShowIncomingFont {
			get;
			set;
		}
		
		public bool ShowIncomingColors {
			get;
			set;
		}
		
		public CustomBackgroundType CustomBackgroundType {
			get;
			set;
		}
		
		public string CustomBackgroundPath {
			get;
			set;
		}
		
		public string CustomBackgroundColor {
			get;
			set;
		}
		
		public bool AllowsUserIcons {
			get {
				return m_AllowsUserIcons;
			}
		}
		#endregion
		
		#region Private Methods
		string ScriptForAppendingContent(AbstractChatContent content, bool contentIsSimilar, bool willAddMoreContentObjects, bool replaceLastContent)
		{
			string newHTML = null;
			string script;

			if (!m_CombineConsecutive) contentIsSimilar = false;

			newHTML = TemplateForContent(content, contentIsSimilar);
			newHTML = FillKeywords(newHTML, content, contentIsSimilar);

			if (!m_UsingCustomTemplateHtml || m_StyleVersion >= 4) {
				if (replaceLastContent)
					script = REPLACE_LAST_MESSAGE;
				else if (willAddMoreContentObjects) {
					script = (contentIsSimilar ? APPEND_NEXT_MESSAGE_NO_SCROLL : APPEND_MESSAGE_NO_SCROLL);
				} else {
					script = (contentIsSimilar ? APPEND_NEXT_MESSAGE : APPEND_MESSAGE);
				}
			} else if (m_StyleVersion >= 3) {
				if (willAddMoreContentObjects) {
					script = (contentIsSimilar ? APPEND_NEXT_MESSAGE_NO_SCROLL : APPEND_MESSAGE_NO_SCROLL);
				} else {
					script = (contentIsSimilar ? APPEND_NEXT_MESSAGE : APPEND_MESSAGE);
				}
			} else if (m_StyleVersion >= 1) {
				script = (contentIsSimilar ? APPEND_NEXT_MESSAGE : APPEND_MESSAGE);
			} else {
				if (m_UsingCustomTemplateHtml && content is ChatContentStatus) {
					script = APPEND_MESSAGE_WITH_SCROLL;
				} else {
					script = (contentIsSimilar ? APPEND_NEXT_MESSAGE_WITH_SCROLL : APPEND_MESSAGE_WITH_SCROLL);
				}
			}

			return String.Format(script, Util.EscapeJavascript(newHTML));
		}

		string TemplateForContent (AbstractChatContent content, bool contentIsSimilar)
		{
			string template = null;
			if (content.Type == ChatContentType.Message || content.Type == ChatContentType.Notification) {
				if (content.IsOutgoing) {
					template = (contentIsSimilar ? m_NextContentOutHtml : m_ContentOutHtml);
				} else {
					template = (contentIsSimilar ? m_NextContentInHtml : m_ContentInHtml);
				}
			} else if (content.Type == ChatContentType.Context) {
				if (content.IsOutgoing) {
					template = (contentIsSimilar ? m_NextContextOutHtml : m_ContextOutHtml);
				} else {
					template = (contentIsSimilar ? m_NextContextInHtml : m_ContextInHtml);
				}
			} else if (content.Type == ChatContentType.FileTransfer) {
				template = m_FileTransferHtml;
			} else {
				template = m_StatusHtml;
			}

			if (String.IsNullOrEmpty(template)) {
				throw new Exception("Failed to get template for content: " + content.Type);
			}

			return template;
		}

		string FillKeywords (string inString, AbstractChatContent content, bool contentIsSimilar)
		{
			if (inString == null)
				throw new ArgumentNullException("inString");

			if (content == null)
				throw new ArgumentNullException("content");
			
			inString = inString.Replace("%time%", content.Date.ToShortTimeString());
			inString = inString.Replace("%shortTime%", content.Date.ToString("%h:%m"));

			string senderStatusIcon = (content.Source != null) ? String.Format("avatar:/{0}", AvatarManager.GetAvatarHash(content.Source.Bare)) : String.Empty;
			inString = inString.Replace("%senderStatusIcon%", senderStatusIcon);

			// FIXME do %localized{x}% replacements

			inString = inString.Replace("%messageClasses%", (contentIsSimilar ? "consecutive " : "") + String.Join(" ", content.DisplayClasses));

			// FIXME: sender colors

			inString = inString.Replace("%messageDirection%", inString.Contains("<DIV dir=\"rtl\">") ? "rtl" : "ltr");

			Regex regex = new Regex(@"%time\{(.*)\}%");
			inString = regex.Replace(inString, delegate (Match match) {
				string pattern = match.Groups[1].Value;
				return Util.Strftime(pattern, content.Date);
			});
			
			if (content is ChatContentMessage) {
				
				string userIconPath = null;
				if (ShowUserIcons) {
					userIconPath = String.Format("avatar:/{0}", AvatarManager.GetAvatarHash(content.Source.Bare));
				} else {
					userIconPath = (content.IsOutgoing) ? "Outgoing/buddy_icon.png" : "Incoming/buddy_icon.png";
				}
				
				inString = inString.Replace("%userIconPath%", userIconPath);
				inString = inString.Replace("%senderScreenName%", content.Source.ToString());
				inString = inString.Replace("%sender%", content.SourceDisplayName);
				inString = inString.Replace("%senderDisplayName%", content.SourceDisplayName);
				inString = inString.Replace("%service%", String.Empty);

				// FIXME: %textbackgroundcolor{

				// FIXME: File transfers
								
			} else if (content is ChatContentStatus) {
				inString = inString.Replace("%status%", ((ChatContentStatus)content).StatusType);
				inString = inString.Replace("%statusSender%", (content.Source != null) ? content.Source.ToString() : String.Empty);

				// FIXME: %statusPhrase%
			}
			
			inString = inString.Replace("%message%", content.MessageHtml);
			
			return inString;
		}
		
		void HandleLinkClicked (QUrl url)
		{
			// We don't open arbitrary links for security reasons.
			var validSchemes = new [] { "http", "https", "ftp", "xmpp" };
			if (validSchemes.Contains(url.Scheme().ToLower())) {
				Util.Open(url);
			} else if (url.Scheme().ToLower() == "xmpp") {
				// FIXME: Add xmpp: uri handler.
				QMessageBox.Information(this.TopLevelWidget(), "Not implenented", "xmpp: uris not yet supported.");
				
			// Ignore # urls.
			} else if (!url.HasFragment()) {
				QMessageBox.Information(this.TopLevelWidget(), "Link Fragment", url.HasFragment() + " " + url.Fragment());
				QMessageBox.Information(this.TopLevelWidget(), "Link URL", url.ToString());
			}
		}
		
		string FormatBaseTemplate(string basePath, string mainPath, string variantPath)
		{
			mainPath = String.Format("@import url(\"{0}\");", mainPath);
			
			string html = this.m_BaseHtml;
			string headerHtml = ShowHeader ? m_HeaderHtml : String.Empty;
			string[] substitutions = null;
			if (m_StyleVersion < 3 && m_UsingCustomTemplateHtml) {
				substitutions = new string[] { basePath, variantPath, headerHtml, m_FooterHtml };
			} else {
				substitutions = new string[] { basePath, mainPath, variantPath, headerHtml, m_FooterHtml };
			}
			for (int i = 0; i < substitutions.Length; i++) {
				int index = html.IndexOf("%@");
				html = html.Remove(index, 2);
				html = html.Insert(index, substitutions[i]);
			}
			
			string bgStyle = String.Empty;
			
			if (m_AllowsCustomBackground && (!String.IsNullOrEmpty(this.CustomBackgroundPath) || !String.IsNullOrEmpty(this.CustomBackgroundColor))) {
				if (!String.IsNullOrEmpty(this.CustomBackgroundPath)) {
					switch (this.CustomBackgroundType) {
					case CustomBackgroundType.BackgroundNormal:
						bgStyle = String.Format(@"background-image: url('{0}'); background-repeat: no-repeat; background-attachment: fixed", 
						                        this.CustomBackgroundPath);
						break;
					case CustomBackgroundType.BackgroundCenter:
						bgStyle = String.Format("background-image: url('{0}'); background-position: center; background-repeat: no-repeat; background-attachment:fixed;",
						                        this.CustomBackgroundPath);
						break;
					case CustomBackgroundType.BackgroundTile:
						bgStyle = String.Format("background-image: url('{0}'); background-repeat: repeat;",
						                        this.CustomBackgroundPath);
						break;
					case CustomBackgroundType.BackgroundTileCenter:
						bgStyle = String.Format("background-image: url('{0}'); background-repeat: repeat; background-position: center;", 
						                        this.CustomBackgroundPath);
						break;
					case CustomBackgroundType.BackgroundScale:
						bgStyle = String.Format("background-image: url('{0}'); -webkit-background-size: 100% 100%; background-size: 100% 100%; background-attachment: fixed;", 
						                        this.CustomBackgroundPath);
						break;
					}
				} else {
						bgStyle = "background-image: none;";
				}
				if (String.IsNullOrEmpty(this.CustomBackgroundColor)) {
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

			// FIXME: This is not so great.
			if (html.IndexOf("<head>") < 0)
				throw new Exception("<head> not found!");

			StringBuilder extraHeaders = new StringBuilder();

			extraHeaders.Append("<script type=\"text/javascript\" src=\"resource:/prototype.js\"></script>");
			extraHeaders.Append("<script type=\"text/javascript\" src=\"resource:/effects.js\"></script>");
			
			foreach (ResourceCodon node in AddinManager.GetExtensionNodes("/Synapse/UI/ConversationHtmlHeaders")) {
				extraHeaders.Append(node.GetResourceString());
			}
			
			html = html.Insert(html.IndexOf("</head>"), extraHeaders.ToString());

			return html;
		}
		              
		string FormatHeaderOrFooter(string headerTemplateHtml)
		{
			headerTemplateHtml = headerTemplateHtml.Replace("%chatName%", this.m_ChatName);
			headerTemplateHtml = headerTemplateHtml.Replace("%sourceName%", this.m_SourceName);
			headerTemplateHtml = headerTemplateHtml.Replace("%destinationName%", this.m_DestinationName);
			headerTemplateHtml = headerTemplateHtml.Replace("%destinationDisplayName%", this.m_DestinationDisplayName);
			headerTemplateHtml = headerTemplateHtml.Replace("%timeOpened%", this.m_TimeOpened.ToString(this.DateFormat));
			
			string iconPath = null;
			if (this.ChatHandler is ChatHandler) {
				var incomingJid = ((ChatHandler)this.ChatHandler).Jid;
				if (AvatarManager.HasAvatarHash(incomingJid)) {
					iconPath = String.Format("avatar:/{0}", AvatarManager.GetAvatarHash(incomingJid));
				}
			}
			headerTemplateHtml = headerTemplateHtml.Replace("%incomingIconPath%", (iconPath != null) ? iconPath : "incoming_icon.png");
			
			var outgoingJid = this.ChatHandler.Account.Jid;
			if (AvatarManager.HasAvatarHash(outgoingJid)) {
				iconPath = AvatarManager.GetAvatarHash(outgoingJid);
			}
			headerTemplateHtml = headerTemplateHtml.Replace("%outgoingIconPath%", (iconPath != null) ? iconPath : "outgoing_icon.png");
			
			// FIXME: %serviceIconImg%
			
			Regex regex = new Regex(@"%timeOpened\{(.*)\}%");
			headerTemplateHtml = regex.Replace(headerTemplateHtml, delegate (Match match) {
				string pattern = match.Groups[1].Value;
				return Util.Strftime(pattern, this.m_TimeOpened);
			});
		
			return headerTemplateHtml;
		}
		
		string PathForVariant (string variant)
		{
			// Styles before version 3 stored the default variant in main.css, and not the variants folder.
			if (m_StyleVersion < 3 && variant == NoVariantName)
				return "main.css";
			else
				return Path.Combine("Variants", String.Format("{0}.css", variant));
		}
		
		string NoVariantName {
			get {
				string noVariantName = m_StyleInfo.Get<string>("DisplayNameForNoVariant");
				return (!String.IsNullOrEmpty(noVariantName)) ? noVariantName : "Normal";
			}
		}
		#endregion

		#region Signal Handlers
		void HandleJavaScriptWindowObjectCleared ()
		{
			base.Page().MainFrame().AddToJavaScriptWindowObject("Synapse", m_JSWindowObject);
		}
		#endregion
		
		protected override void ResizeEvent (Qyoto.QResizeEvent e)
		{
			base.ResizeEvent (e);
			
			// FIXME: Seems "window.onresize" doesn't work, so we need to fake it.
			// Qt bug?
			base.Page().MainFrame().EvaluateJavaScript("windowDidResize()");
		}
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
