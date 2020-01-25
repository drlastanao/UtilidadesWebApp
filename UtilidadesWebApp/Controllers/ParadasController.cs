using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UtilidadesWebApp.Controllers
{
    public class ParadasController : Controller
    {
        // GET: Paradas
        public ActionResult Index()
        {
            return View();
        }

        // GET: Paradas/Details/5
        public ActionResult Cercanas(string posicion)
        {
            return View();
        }
    }
}