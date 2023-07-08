using Microsoft.Web.Administration;
using System.Security.Cryptography.X509Certificates;

namespace ApiGateway.Services;

public class SiteGenerator
{
    ServerManager _ServerManager;

    public SiteGenerator(ServerManager serverManager)
    {
        _ServerManager = serverManager;
    }

    public SiteGenerator RemoveAppPool(string appPoolName)
    {
        ApplicationPool? pool = _ServerManager.ApplicationPools
            .Where(x => x.Name == appPoolName)
            .SingleOrDefault();

        if (pool == null) 
            return this;

        _ServerManager.ApplicationPools.Remove(pool);
        return this;
    }

    public SiteGenerator CreateAppPool(string appPoolName, string runtime = "v4.0")
    {
        ApplicationPool appPool = _ServerManager.ApplicationPools.CreateElement();
        appPool.Name = appPoolName;
        appPool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
        appPool.ManagedRuntimeVersion = runtime;

        _ServerManager.ApplicationPools.Add(appPool);
        return this;
    }

    public SiteGenerator RemoveSite(string siteName)
    {
        Site? site = _ServerManager.Sites
            .Where(x => x.Name == siteName)
            .SingleOrDefault();

        if (site == null)
            return this;

        _ServerManager.Sites.Remove(site);
        return this;
    }

    public SiteUncompleted CreateSite(string siteName, string appPoolName)
    {
        Site newSite = _ServerManager.Sites.CreateElement();
        newSite.Id = 213231;
        newSite.Name = siteName;
        newSite.ServerAutoStart = true;
        newSite.ApplicationDefaults.ApplicationPoolName = appPoolName;
        return new SiteUncompleted(this, newSite);
    }

    public class SiteUncompleted
    {
        SiteGenerator _Generator;
        Site _Site;

        public SiteUncompleted(SiteGenerator generator, Site site)
        {
            _Generator = generator;
            _Site = site;
            _Site.Bindings.Clear();
        }

        public SiteUncompleted AddHttpBinding(string url, string domain = "*", string port = "80")
        {
            Binding http = _Site.Bindings.CreateElement();
            http.Protocol = "http";
            http.BindingInformation = $"{domain}:{port}:{url}";
            _Site.Bindings.Add(http);
            return this;
        }

        public SiteUncompleted AddHttpsBinding(string url, StoreName storeName, X509Certificate2 certificate, string domain = "*", string port = "443")
        {
            Binding https = _Site.Bindings.CreateElement();
            https.Protocol = "https";
            https.BindingInformation = $"{domain}:{port}:{url}";
            https.CertificateStoreName = storeName.ToString();
            https.CertificateHash = certificate.GetCertHash();
            _Site.Bindings.Add(https);
            return this;
        }

        public SiteUncompleted AddApplication(string appPoolName, string physicalPath)
        {
            Application rootApplication = _Site.Applications.CreateElement();
            rootApplication.Path = "/";
            rootApplication.ApplicationPoolName = appPoolName;
            rootApplication.VirtualDirectories.Add("/", physicalPath);
            _Site.Applications.Add(rootApplication);
            return this;
        }

        public SiteGenerator CompleteSite()
        {
            _Generator._ServerManager.Sites.Add(_Site);
            return _Generator;
        }
    }

    public void SaveChanges()
    {
        _ServerManager.CommitChanges();
    }
}
