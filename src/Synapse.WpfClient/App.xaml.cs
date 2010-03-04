using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Synapse.WpfClient.Views;
using Synapse.WpfClient.ViewModels;

using Synapse.ServiceStack;
using System.IO;
using System.Drawing;
using Synapse.Xmpp.Services;

namespace Synapse.WpfClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application, IClient
    {
        bool m_IsStarted;

        public App()
        {
            Synapse.ServiceStack.Application.Initialize(this);

            // FIXME: I don't like all of these being here.
            ServiceManager.RegisterService<XmppService>();
            ServiceManager.RegisterService<AccountService>();
            ServiceManager.RegisterService<ShoutService>();
            ServiceManager.RegisterService<GeoService>();
            // OctyService, GuiService
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Synapse.ServiceStack.Application.Run();

            new MainWindow(new MainWindowVM()).Show();

            m_IsStarted = true;

            if (Started != null)
                Started(this);
        }

        public event Action<IClient> Started;

        public string ClientId
        {
            get { return "WpfClient"; }
        }

        public bool IsStarted
        {
            get { return m_IsStarted; }
        }

        public object CreateImage(byte[] data)
        {
            using (var stream = new MemoryStream(data)) 
            {
                return new Bitmap(stream);
            }
        }

        public object CreateImage(string fileName)
        {
            return new Bitmap(fileName);
        }

        public object CreateImageFromResource(string resourceName)
        {
            return new Bitmap(null, resourceName);
        }

        public void ShowErrorWindow(string title, string errorMessage, string errorDetail)
        {
            // FIXME: System.Windows.MessageBox.Show(
        }

        public void ShowErrorWindow(string errorTitle, Exception error)
        {
            throw new NotImplementedException();
        }

        public void DesktopNotify(Services.ActivityFeedItemTemplate template, Services.IActivityFeedItem item, string text)
        {
            throw new NotImplementedException();
        }

        public new void Exit()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
