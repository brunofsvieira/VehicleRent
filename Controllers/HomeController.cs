using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VehicleRent.Models;

namespace VehicleRent.Controllers
{
    /// <summary>
    /// Represents the HomeController component.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Executes the Index operation.
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Executes the Privacy operation.
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        /// <summary>
        /// Executes the Error operation.
        /// </summary>
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
