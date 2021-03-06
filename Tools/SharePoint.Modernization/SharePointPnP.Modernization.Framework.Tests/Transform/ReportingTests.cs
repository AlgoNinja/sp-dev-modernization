﻿using Microsoft.SharePoint.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharePointPnP.Modernization.Framework.Telemetry;
using SharePointPnP.Modernization.Framework.Telemetry.Observers;
using SharePointPnP.Modernization.Framework.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.Modernization.Framework.Tests.Transform
{
    [TestClass]
    public class ReportingTests
    {

        private List<LogEntry> GetExampleInfoLogs()
        {
            List<LogEntry> logs = new List<LogEntry>();

            //Example Logs
            logs.Add(new LogEntry() { Heading = "SharePoint Connection", Message = "EXAMPLE:Overriding 'TargetPageTakesSourcePageName' to ensure that the newly created page in the other site collection gets the same name as the source page" });

            logs.Add(new LogEntry() { Heading = LogStrings.Heading_Summary, Message = "This is an example summary message " });
            logs.Add(new LogEntry() { Heading = LogStrings.Heading_Summary, Message = "THIS IS GENERATED BY UNIT TEST LINKS ARE NOT WORKING" });

            logs.Add(new LogEntry() { Heading = "Page Creation", Message = "EXAMPLE:Modern page created" });
            logs.Add(new LogEntry() { Heading = "Article page handling", Message = "EXAMPLE:Transforming source page as Article page" });

            logs.Add(new LogEntry() { Heading = LogStrings.Heading_PageTransformationInfomation, Message = "Transforming site ;#; https://tenant.sharepoint.com/sites/PnPTransformationSource-TeamSite" });
            logs.Add(new LogEntry() { Heading = LogStrings.Heading_PageTransformationInfomation, Message = "Cross-Site transfer mode to site ;#; https://tenant.sharepoint.com/sites/PnPTransformationTarget" });
            logs.Add(new LogEntry() { Heading = LogStrings.Heading_PageTransformationInfomation, Message = "An referenced asset was found and copied to ;#; /sites/PnPTransformationTarget/SiteAssets/SitePages/WPP_Microsoft-publishes-guidance-to-boost-public-sector-cloud-security/EDU18_Teachers_013-960x640.jpg" });
            logs.Add(new LogEntry() { Heading = LogStrings.Heading_PageTransformationInfomation, Message = "Transformed Page ;#; /sites/PnPTransformationTarget/SitePages/WPP_Microsoft-publishes-guidance-to-boost-public-sector-cloud-security.aspx" });

            return logs;
        }

        private List<LogEntry> GetExampleWarnLogs()
        {
            List<LogEntry> logs = new List<LogEntry>();
            logs.Add(new LogEntry() { Heading = "Asset Transfer", Message = "An issue occurred in transferring an asset, falling back to original reference." });

            return logs;
        }

        private List<LogEntry> GetExampleErrorLogs()
        {
            List<LogEntry> logs = new List<LogEntry>();
            logs.Add(new LogEntry() { Heading = "Article page handling", Message = "Setting version stamp on page error Error: You can only publish, unpublish  documents in a minor version enabled list" });

            return logs;
        }



        [TestMethod]
        public void Reporting_SameSite_WebPartPageTest()
        {
            using (var sourceClientContext = TestCommon.CreateClientContext())
            {
                var pageTransformator = new PageTransformator(sourceClientContext);
                pageTransformator.RegisterObserver(new UnitTestLogObserver()); //Registers the unit test observer to log details for testing

                var pages = sourceClientContext.Web.GetPages("wpp");

                foreach (var page in pages)
                {
                    PageTransformationInformation pti = new PageTransformationInformation(page)
                    {
                        // If target page exists, then overwrite it
                        Overwrite = true,

                        // Don't log test runs
                        SkipTelemetry = true,

                        // ModernizationCenter options
                        ModernizationCenterInformation = new ModernizationCenterInformation()
                        {
                            AddPageAcceptBanner = true
                        },

                        // Give the migrated page a specific prefix, default is Migrated_
                        TargetPagePrefix = "Converted_",

                        // Replace embedded images and iframes with a placeholder and add respective images and video web parts at the bottom of the page
                        HandleWikiImagesAndVideos = false,

                    };

                    pageTransformator.Transform(pti);
                    pageTransformator.FlushObservers();
                }

            }

            Assert.Inconclusive(TestCommon.InconclusiveNoAutomatedChecksMessage);

        }

        [TestMethod]
        public void Reporting_SameSite_WebPartPage_SingleObserverTest()
        {
            using (var sourceClientContext = TestCommon.CreateClientContext())
            {
                var pageTransformator = new PageTransformator(sourceClientContext);
                // This test will use only the default observer

                var pages = sourceClientContext.Web.GetPages("wpp").Take(1);

                foreach (var page in pages)
                {
                    PageTransformationInformation pti = new PageTransformationInformation(page)
                    {
                        // If target page exists, then overwrite it
                        Overwrite = true,

                        // Don't log test runs
                        SkipTelemetry = true,

                        // ModernizationCenter options
                        ModernizationCenterInformation = new ModernizationCenterInformation()
                        {
                            AddPageAcceptBanner = true
                        },

                        // Give the migrated page a specific prefix, default is Migrated_
                        TargetPagePrefix = "Converted_",

                        // Replace embedded images and iframes with a placeholder and add respective images and video web parts at the bottom of the page
                        HandleWikiImagesAndVideos = false,

                    };

                    pageTransformator.Transform(pti);
                    pageTransformator.FlushObservers();
                }

            }

            Assert.Inconclusive(TestCommon.InconclusiveNoAutomatedChecksMessage);

        }

        [TestMethod]
        public void Reporting_CrossSite_WebPartPage_MarkdownObserverTest()
        {
            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext())
                {
                    var pageTransformator = new PageTransformator(sourceClientContext, targetClientContext);
                    pageTransformator.RegisterObserver(new MarkdownObserver());

                    var pages = sourceClientContext.Web.GetPages("wpp").Take(1);

                    foreach (var page in pages)
                    {
                        PageTransformationInformation pti = new PageTransformationInformation(page)
                        {
                            // If target page exists, then overwrite it
                            Overwrite = true,

                            // Don't log test runs
                            SkipTelemetry = true,

                            // Replace embedded images and iframes with a placeholder and add respective images and video web parts at the bottom of the page
                            HandleWikiImagesAndVideos = false,
                            
                        };

                        pageTransformator.Transform(pti);
                        pageTransformator.FlushSpecificObserver<MarkdownObserver>();
                    }

                }
            }

            Assert.Inconclusive(TestCommon.InconclusiveNoAutomatedChecksMessage);

        }

        [TestMethod]
        public void Reporting_CrossSite_WebPartPage_MarkdownObserverToSharePointTest()
        {
            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext())
                {
                    var pageTransformator = new PageTransformator(sourceClientContext, targetClientContext);
                    pageTransformator.RegisterObserver(new MarkdownToSharePointObserver(targetClientContext));

                    var pages = sourceClientContext.Web.GetPages("wpp").Take(1);

                    foreach (var page in pages)
                    {
                        PageTransformationInformation pti = new PageTransformationInformation(page)
                        {
                            // If target page exists, then overwrite it
                            Overwrite = true,

                            // Don't log test runs
                            SkipTelemetry = true,

                            // Replace embedded images and iframes with a placeholder and add respective images and video web parts at the bottom of the page
                            HandleWikiImagesAndVideos = false,
                            
                        };

                        pageTransformator.Transform(pti);
                        pageTransformator.FlushObservers();
                    }

                }
            }

            Assert.Inconclusive(TestCommon.InconclusiveNoAutomatedChecksMessage);

        }


        [TestMethod]
        public void Reporting_CrossSite_WebPartPage_ConsoleObserverTest()
        {
            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext())
                {
                    var pageTransformator = new PageTransformator(sourceClientContext, targetClientContext);
                    pageTransformator.RegisterObserver(new ConsoleObserver());

                    var pages = sourceClientContext.Web.GetPages("wpp").Take(1);

                    foreach (var page in pages)
                    {
                        PageTransformationInformation pti = new PageTransformationInformation(page)
                        {
                            // If target page exists, then overwrite it
                            Overwrite = true,

                            // Don't log test runs
                            SkipTelemetry = true,

                            // Replace embedded images and iframes with a placeholder and add respective images and video web parts at the bottom of the page
                            HandleWikiImagesAndVideos = false,
                                                       

                        };

                        pageTransformator.Transform(pti);
                        pageTransformator.FlushObservers();
                    }

                }
            }

            Assert.Inconclusive(TestCommon.InconclusiveNoAutomatedChecksMessage);

        }

        [TestMethod]
        public void Reporting_MarkDownToSharePointObserverTest()
        {
            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                List<LogEntry> infoLogs = GetExampleInfoLogs();
                List<LogEntry> warnLogs = GetExampleWarnLogs();
                List<LogEntry> errorLogs = GetExampleErrorLogs();

                var markdownObserver = new MarkdownToSharePointObserver(targetClientContext);
                infoLogs.ForEach(o => { markdownObserver.Info(o); });
                warnLogs.ForEach(o => { markdownObserver.Info(o); });
                errorLogs.ForEach(o => { markdownObserver.Info(o); });

                markdownObserver.Flush();
            }

            Assert.Inconclusive(TestCommon.InconclusiveNoAutomatedChecksMessage);

        }

        [TestMethod]
        public void Reporting_MarkDownObserverTest()
        {

            List<LogEntry> infoLogs = GetExampleInfoLogs();
            List<LogEntry> warnLogs = GetExampleWarnLogs();
            List<LogEntry> errorLogs = GetExampleErrorLogs();

            var markdownObserver = new MarkdownObserver();
            infoLogs.ForEach(o => { markdownObserver.Info(o); });
            warnLogs.ForEach(o => { markdownObserver.Info(o); });
            errorLogs.ForEach(o => { markdownObserver.Info(o); });

            markdownObserver.Flush();

            //Note: This generates a file at the location of the assembly bin/debug folder

            Assert.Inconclusive(TestCommon.InconclusiveNoAutomatedChecksMessage);

        }

        [TestMethod]
        public void Reporting_ConsoleObserverTest()
        {

            List<LogEntry> infoLogs = GetExampleInfoLogs();
            List<LogEntry> warnLogs = GetExampleWarnLogs();
            List<LogEntry> errorLogs = GetExampleErrorLogs();

            var observer = new ConsoleObserver();
            infoLogs.ForEach(o => { observer.Info(o); });
            warnLogs.ForEach(o => { observer.Info(o); });
            errorLogs.ForEach(o => { observer.Info(o); });

            observer.Flush(); //Does nothing

            //Note: Look for the Output link on the unit test

            Assert.Inconclusive(TestCommon.InconclusiveNoAutomatedChecksMessage);

        }
    }
}
