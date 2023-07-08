using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Web.Administration;
using System.Security.Cryptography.X509Certificates;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
public class SiteManagerController : ControllerBase
{
    [HttpPost("/CreateSite")]
    public ActionResult CreateSite(SiteVM site)
    {
        try
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certificates = store.Certificates.Find(
                X509FindType.FindBySubjectName, "localhost", validOnly: false);

            using (ServerManager serverManager = new ServerManager())
            {
                SiteGenerator siteGenerator = new SiteGenerator(serverManager);
                siteGenerator
                    .CreateAppPool(site.AppPoolName)
                    .CreateSite(site.SiteName, site.AppPoolName)
                    .AddHttpBinding("localhost")
                    .AddHttpsBinding("localhost", StoreName.My, certificates[0])
                    .AddApplication(site.AppPoolName, site.PhysicalPath)
                    .CompleteSite()
                    .SaveChanges();
            }
        }
        catch (Exception ex)
        {

        }

        return Ok();
    }

    [HttpDelete("/RemoveAppPool")]
    public ActionResult RemoveAppPool(string appPoolName)
    {
        try
        {
            using (ServerManager serverManager = new ServerManager())
            {
                SiteGenerator siteGenerator = new SiteGenerator(serverManager);
                siteGenerator
                    .RemoveAppPool(appPoolName)
                    .SaveChanges();
            }
        }
        catch (Exception ex)
        {

        }

        return Ok();
    }

    [HttpDelete("/RemoveSite")]
    public ActionResult RemoveSite(string siteName)
    {
        try
        {
            using (ServerManager serverManager = new ServerManager())
            {
                SiteGenerator siteGenerator = new SiteGenerator(serverManager);
                siteGenerator
                    .RemoveSite(siteName)
                    .SaveChanges();
            }
        }
        catch (Exception ex)
        {

        }

        return Ok();
    }
}