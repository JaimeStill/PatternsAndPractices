using Microsoft.AspNetCore.Mvc;
using Qxyz.Core.Banner;

namespace Qxyz.Web.Controllers
{
    [Route("api/[controller]")]
    public class BannerController : Controller
    {
        private BannerConfig config;

        public BannerController(BannerConfig config) => this.config = config;

        [HttpGet("[action]")]
        public BannerConfig GetConfig() => config;
    }
}