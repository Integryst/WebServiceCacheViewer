// Copyright (c) 2012 Integryst, LLC, http://www.integryst.com/
// See LICENSE.txt for licensing information

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using com.plumtree.server;
using com.plumtree.openkernel.config;
using com.plumtree.openkernel.factory;
using com.plumtree.server.impl.directory;
using com.plumtree.openfoundation.util;

namespace WebServiceCacheViewer
{
    public partial class Form1 : Form
    {
        private IPTSession session = null;
//        private Hashtable PROP_BAG_PROPS;
//        private int URL_PROP = 106;
        private bool getSession()
        {
            try
            {
                if (session != null)
                    return true;

                IOKContext okContext = OKConfigFactory.createInstance(txtConfig.Text, "portal");
                PortalObjectsFactory.Init(okContext);
                session = PortalObjectsFactory.CreateSession();

                session.Connect(txtUser.Text, txtPass.Text, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception getting session: " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
            return true;

        }

        public Form1()
        {
            InitializeComponent();
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            if (btnGo.Text.Equals("Go"))
            {
                btnGo.Text = "Stop";
                getSession();
                IPTObjectManager wsManager = session.GetWebServices();
                IPTQueryResult res = wsManager.SimpleQuery(-1, PT_PROPIDS.PT_PROPID_NAME);
                setStatus("Loading Web Services - " + res.RowCount() + " results ..." + Environment.NewLine);
                setStatus("name\tid\tpath\tcreated\tmodified\tdescription\tmin cache\tmax cache" + Environment.NewLine);

                string propString = "";
                progressBar1.Maximum = res.RowCount();

                // iterate through all web services
                int wsID;
                string wsName;
                for (int x = 0; x < res.RowCount(); x++)
                {
                    wsID=-1;
                    wsName="";
                    propString = "";
                    Application.DoEvents();
                    if (btnGo.Text.Equals("Stop"))
                    {
                        progressBar1.Value = x;
                        wsID = res.ItemAsInt(x, PT_PROPIDS.PT_PROPID_OBJECTID);
                        wsName = res.ItemAsString(x, PT_PROPIDS.PT_PROPID_NAME);
                        IPTWebService webService = (IPTWebService)wsManager.Open(wsID, false);
                        IXPPropertyBag propBag = webService.GetProviderInfo();
                        IXPEnumerator enumerator = propBag.GetEnumerator();
                        int folderID = webService.GetAdminFolderID();
                        IPTAdminFolder folder = session.GetAdminCatalog().OpenAdminFolder(folderID, false);

                        propString += wsName + "\t" + wsID + "\t" + 
                            /* webService.GetAbsoluteURL(PlumtreeExtensibility.PT_PROPBAG_HTTPGADGET_URL) + "\t" + */ 
                            folder.GetPath() + "\t" + webService.GetCreated() + "\t" + 
                            webService.GetLastModified() + "\t" + webService.GetDescription() + "\t";
                        string mincache="-";
                        string maxcache="-";
                        while (enumerator.MoveNext())
                        {
                            string name = (string)enumerator.GetCurrent();
                            string val = propBag.ReadAsString(name);
                            if ("PTC_HTTPGADGET_MAXC".Equals(name))
                                maxcache = val;
                            else if ("PTC_HTTPGADGET_MINC".Equals(name))
                                mincache = val;
//                            else 
//                                tempprop += name + ": " + val + ",";
                        }
                        propString += mincache + "\t" + maxcache + /* "\t" + tempprop + */ Environment.NewLine;
                        setStatus(propString);
                    }
                }
            }
            else
            {
                btnGo.Text = "Go";
            }
                progressBar1.Value = 0;
        }

        private void setStatus(string text)
        {
            txtStatus.Text += text;
            Application.DoEvents();
        }

        private void txtStatus_TextChanged(object sender, EventArgs e)
        {

        }

    }
}
