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
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.IO;
using IO = System.IO;
using Qyoto;
using Synapse.Core;
using Synapse.UI.Chat;
using Synapse.Xmpp;

namespace Synapse.QtClient
{
	public class ConversationWidget : QWebView
	{
		#region Private Variables
		string m_BaseTemplateHtml       = null;
		string m_StatusHtml             = null;
		DateTime m_TimeOpened;		
		PList  m_ThemeProperties         = null;
		bool   m_UsingCustomTemplateHtml = false;

		string m_ContentInHtml     = null;
		string m_NextContentInHtml = null;
		string m_ContextInHtml     = null;
		string m_NextContextInHtml = null;
		
		string m_ContentOutHtml     = null;
		string m_NextContentOutHtml = null;
		string m_ContextOutHtml     = null;
		string m_NextContextOutHtml = null;
		string m_FileTransferHtml   = null;

		bool m_ThemeLoaded = false;
		
		string m_ThemeName              = null;
		string m_VariantName            = null;
		string m_ChatName               = null;
		string m_SourceName             = null;
		string m_DestinationName        = null;
		string m_DestinationDisplayName = null;
		
		CustomBackgroundType m_CustomBackgroundType  = CustomBackgroundType.BackgroundNormal;
		string               m_CustomBackgroundPath  = null;
		string               m_CustomBackgroundColor = null;
		bool                 m_CombineConsecutive = true;
		
		QMenu m_Menu;
		
		static string s_ThemesDirectory = null;

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
		#endregion
		
		#region Constructor
		public ConversationWidget(QWidget parent) : base(parent)
		{
			QObject.Connect(this.Page().MainFrame(), Qt.SIGNAL("javaScriptWindowObjectCleared()"), this, Qt.SLOT("OnJavaScriptWindowObjectCleared()"));

			if (ConversationWidget.ThemesDirectory == null) {
				throw new Exception("Set ThemesDirectory first");
			}
			
			this.m_TimeOpened = DateTime.Now;
			
			m_Menu = new QMenu(this);
			// FIXME: Need to selectively show certain actions depending on what's under the cursor.
			//m_Menu.AddAction(this.PageAction(QWebPage.WebAction.OpenLink));
			//m_Menu.AddSeparator();
			m_Menu.AddAction(this.PageAction(QWebPage.WebAction.Copy));
			m_Menu.AddAction(this.PageAction(QWebPage.WebAction.InspectElement));
			//m_Menu.AddAction(this.PageAction(QWebPage.WebAction.CopyLinkToClipboard));
			//m_Menu.AddAction(this.PageAction(QWebPage.WebAction.CopyImageToClipboard));

			this.Page().linkDelegationPolicy = QWebPage.LinkDelegationPolicy.DelegateAllLinks;
			QObject.Connect(this, Qt.SIGNAL("linkClicked(QUrl)"), new OneArgDelegate<QUrl>(HandleLinkClicked));
		}
		#endregion
		
		#region Public Methods
		public void AppendContent(AbstractChatContent content, bool contentIsSimilar, bool willAddMoreContentObjects, 
		                          bool replaceLastContent)
		{
			if (content == null)
				throw new ArgumentNullException("content");
			
			if (!m_ThemeLoaded)
				throw new Exception("Call LoadTheme() first!");

			Page().MainFrame().EvaluateJavaScript(ScriptForAppendingContent(content,
			                                                                contentIsSimilar,
			                                                                willAddMoreContentObjects,
			                                                                replaceLastContent));
		}
				
		public void LoadTheme(string themeName, string variantName)
		{
			m_ThemeLoaded = false;

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
			
			this.m_ThemeProperties = new PList(plistPath);
			this.m_ThemeName       = themeName;
			this.m_VariantName     = variantName;			

			// Check if theme is version 1 and has its own Template.html
			string customTemplatePath = Util.JoinPath(resourcesPath, "Template.html");
			if (!File.Exists(customTemplatePath) && StyleVersion >= 1) {
				Assembly asm = Assembly.GetExecutingAssembly();
				using (StreamReader reader = new StreamReader(asm.GetManifestResourceStream("Template.html"))) {
					m_BaseTemplateHtml = reader.ReadToEnd();
				}
				m_UsingCustomTemplateHtml = false;
			} else {
				m_BaseTemplateHtml = File.ReadAllText(customTemplatePath);
				m_UsingCustomTemplateHtml = true;
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
			
			string baseHtml       = FormatBaseTemplate(m_ThemeProperties, baseUri, mainCssPath, variantCssPath, headerHtml, footerHtml);
			base.Page().MainFrame().SetHtml(baseHtml, themeDirectory);
				
			string incomingPath = Util.JoinPath(resourcesPath, "Incoming");
			string outgoingPath = Util.JoinPath(resourcesPath, "Outgoing");
			
			// Load other templates
			string statusPath              = Util.JoinPath(resourcesPath, "Status.html");
			string incomingContentPath     = Util.JoinPath(incomingPath, "Content.html");
			string incomingNextContentPath = Util.JoinPath(incomingPath, "NextContent.html");
			string outgoingContentPath     = Util.JoinPath(outgoingPath, "Content.html");
			string outgoingNextContentPath = Util.JoinPath(outgoingPath, "NextContent.html");
			string incomingContextPath     = Util.JoinPath(incomingPath, "Context.html");
			string incomingNextContextPath = Util.JoinPath(incomingPath, "NextContext.html");
			string outgoingContextPath     = Util.JoinPath(outgoingPath, "Context.html");
			string outgoingNextContextPath = Util.JoinPath(outgoingPath, "NextContext.html");
			string filetransferRequestPath = Util.JoinPath(resourcesPath, "FileTransferRequest.html");
			
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
			this.m_NextContextOutHtml = File.Exists(incomingContextPath)     ? File.ReadAllText(incomingContextPath)     : this.m_NextContentOutHtml;
			
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
			
			OnJavaScriptWindowObjectCleared();

			m_ThemeLoaded = true;
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

		protected override void ContextMenuEvent (Qyoto.QContextMenuEvent arg1)
		{			
			m_Menu.Popup(arg1.GlobalPos());
		}

		int StyleVersion {
			get {
				return Convert.ToInt32(m_ThemeProperties.GetValue<long>("MessageViewVersion"));
			}
		}
		
		#region Private Methods
		string ScriptForAppendingContent(AbstractChatContent content, bool contentIsSimilar, bool willAddMoreContentObjects, bool replaceLastContent)
		{
			string newHTML = null;
			string script;

			if (!m_CombineConsecutive) contentIsSimilar = false;

			newHTML = TemplateForContent(content, contentIsSimilar);
			newHTML = FillKeywords(newHTML, content, contentIsSimilar);

			if (!m_UsingCustomTemplateHtml || StyleVersion >= 4) {
				if (replaceLastContent)
					script = REPLACE_LAST_MESSAGE;
				else if (willAddMoreContentObjects) {
					script = (contentIsSimilar ? APPEND_NEXT_MESSAGE_NO_SCROLL : APPEND_MESSAGE_NO_SCROLL);
				} else {
					script = (contentIsSimilar ? APPEND_NEXT_MESSAGE : APPEND_MESSAGE);
				}
			} else if (StyleVersion >= 3) {
				if (willAddMoreContentObjects) {
					script = (contentIsSimilar ? APPEND_NEXT_MESSAGE_NO_SCROLL : APPEND_MESSAGE_NO_SCROLL);
				} else {
					script = (contentIsSimilar ? APPEND_NEXT_MESSAGE : APPEND_MESSAGE);
				}
			} else if (StyleVersion >= 1) {
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
				string userStatusIcon = String.Format("avatar:/{0}", AvatarManager.GetAvatarHash(content.Source.Bare));
				
				inString = inString.Replace("%userIconPath%", userStatusIcon);
				inString = inString.Replace("%senderScreenName%", content.Source.ToString());
				inString = inString.Replace("%sender%", content.Account.GetDisplayName(content.Source));
				inString = inString.Replace("%senderDisplayName%", content.Account.GetDisplayName(content.Source));
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
			Gui.Open(url);
		}
		
		string FormatBaseTemplate(PList themeProperties, string basePath, string mainPath, string variantPath, string headerHtml, string footerHtml)
		{
			mainPath = String.Format("@import url(\"{0}\");", mainPath);
			
			string html = this.m_BaseTemplateHtml;
			string[] substitutions = null;
			if (StyleVersion < 3 && m_UsingCustomTemplateHtml) {
				substitutions = new string[] { basePath, variantPath, headerHtml, footerHtml };
			} else {
				substitutions = new string[] { basePath, mainPath, variantPath, headerHtml, footerHtml };
			}
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
		              
		string FormatHeaderOrFooter(string headerTemplateHtml)
		{
			headerTemplateHtml = headerTemplateHtml.Replace("%chatName%", this.m_ChatName);
			headerTemplateHtml = headerTemplateHtml.Replace("%sourceName%", this.m_SourceName);
			headerTemplateHtml = headerTemplateHtml.Replace("%destinationName%", this.m_DestinationName);
			headerTemplateHtml = headerTemplateHtml.Replace("%destinationDisplayName%", this.m_DestinationDisplayName);
			headerTemplateHtml = headerTemplateHtml.Replace("%timeOpened%", this.m_TimeOpened.ToString());
			
			Regex regex = new Regex(@"%timeOpened\{(.*)\}%");
			headerTemplateHtml = regex.Replace(headerTemplateHtml, delegate (Match match) {
				string pattern = match.Groups[1].Value;
				return Util.Strftime(pattern, this.m_TimeOpened);
			});
		
			return headerTemplateHtml;
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
