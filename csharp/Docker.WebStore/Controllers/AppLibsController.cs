using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docker.WebStore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Docker.WebStore.Controllers
{
    [Route("")]
    public class AppLibsController : Controller
    {
        private readonly Store _store;

        public AppLibsController(Store store)
        {
            _store = store;
        }

        [Route("")]
        public IActionResult List()
        {
            var asms = _store.List().Select(asm=>new AssemblyModel(asm));
            return View(asms);
        }

        [Route("upload")]
        public async Task<IActionResult> UploadAssemblies(IEnumerable<IFormFile> files)
        {
            foreach(var f in files) {
                await _store.LoadAsync(f.OpenReadStream());
            }
            return RedirectToAction("list");
        }
    }
}