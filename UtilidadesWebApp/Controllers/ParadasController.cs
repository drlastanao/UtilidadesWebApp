using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UtilidadesWebApp.Models;

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
        public ActionResult Cercanas(string posicion, string parada, string guardar)
        {
            var cookie = Request.Cookies["Paradas"] != null ? JsonConvert.DeserializeObject<List<Parada>>(Request.Cookies["Paradas"]) : null;

            if (!string.IsNullOrEmpty(parada))
            {
                TratarCookies(posicion, parada, guardar, cookie);
                return new RedirectResult("http://zaragoza.avanzagrupo.com/frm_esquemaparadatime.php?poste=" + parada);
            }

            if (cookie != null)
            {
                var cookies = OrdenarCookies(cookie, posicion);

                return View("Cercanas", cookies);
            }
            else
                return NotFound();
        }

        private IOrderedEnumerable<Parada> OrdenarCookies(List<Parada> cookie, string posicion)
        {
            float latitud = (posicion != null) ? float.Parse(posicion.Split('|')[0]) : 0;
            float longitud = (posicion != null) ? float.Parse(posicion.Split('|')[1]) : 0;


            foreach (var parada in cookie)
            {

                float difLatitud = parada.latitud - latitud;
                float difLongitud = parada.longitud - longitud;


                double a = Math.Sin(difLatitud / 2) * Math.Sin(difLatitud / 2) +
                          Math.Cos(EnRadianes(latitud)) *
                          Math.Cos(EnRadianes(parada.latitud)) *
                          Math.Sin(difLongitud / 2) * Math.Sin(difLongitud / 2);
                double diferencia = 1 - a;

                double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(diferencia));


                parada.distancia = (float)c;

            }


            var aux = cookie.OrderBy(x => x.distancia);
            return aux;
        }

        static float EnRadianes(float valor)
        {
            return Convert.ToSingle(Math.PI / 180) * valor;
        }
        private void TratarCookies(string posicion, string parada, string guardar, List<Parada> cookie)
        {
            if (!string.IsNullOrEmpty(guardar))
            {

                var aux2 = new Parada()
                {
                    numero = parada,
                    latitud = (posicion != null) ? float.Parse(posicion.Split('|')[0]) : 0,
                    longitud = (posicion != null) ? float.Parse(posicion.Split('|')[1]) : 0,
                    distancia = 9999999

                };

                if (cookie == null)
                {
                    var aux = new List<Parada>();
                    aux.Add(aux2);
                    Set("Paradas", JsonConvert.SerializeObject(aux));

                }
                else
                {
                    if (cookie.Where(x => x.numero == parada).FirstOrDefault() == null)
                    {
                        cookie.Add(aux2);
                        Set("Paradas", JsonConvert.SerializeObject(cookie));
                    }
                }

            }
        }


        public void Set(string key, string value, int? expireTime = null)
        {
            CookieOptions option = new CookieOptions();

            if (expireTime.HasValue)
                option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            else
                option.Expires = DateTime.Now.AddMilliseconds(1000 * 60 * 60 * 24 * 10);

            Response.Cookies.Append(key, value, option);
        }
    }


}