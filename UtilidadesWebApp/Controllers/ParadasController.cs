using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UtilidadesWebApp.Models;

namespace UtilidadesWebApp.Controllers
{
    public class ParadasController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        public ParadasController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

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
                //return new RedirectResult("http://zaragoza.avanzagrupo.com/frm_esquemaparadatime.php?poste=" + parada);
                var obj = OnGet(parada);
                if (obj != null)
                {
                    var resultado = obj?.Result?.Result;
                    if (resultado == null)
                        return NotFound();

                    var infoparada = JsonConvert.DeserializeObject<DetalleParada>(resultado);

                    TratarCookies(infoparada.geometry.coordinates[1].ToString() + "|" + infoparada.geometry.coordinates[0].ToString(), parada, guardar, cookie);

                    return View("Mostrar", infoparada);
                }
                else
                    return NotFound();
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
            posicion=posicion.Replace(".", ",");

            double latitud = (posicion != null) ? double.Parse(posicion.Split('|')[0]) : 0;
            double longitud = (posicion != null) ? float.Parse(posicion.Split('|')[1]) : 0;


            foreach (var parada in cookie)
            {

                var R = 6371; // Radius of the earth in km
                var dLat = deg2rad(parada.latitud - latitud);  // deg2rad below
                var dLon = deg2rad(parada.longitud - longitud);
                var a =
                    Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(deg2rad(latitud)) * Math.Cos(deg2rad(parada.latitud)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                var d = R * c; // Distance in km

                parada.distancia = (float)d;

            }


            var aux = cookie.OrderBy(x => x.distancia);
            return aux;
        }
        public double deg2rad(double deg)
        {
            return deg * (Math.PI / 180);
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



        public async Task<Task<string>> OnGet(string parada)
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                "https://www.zaragoza.es/sede/servicio/urbanismo-infraestructuras/transporte-urbano/poste-autobus/tuzsa-" + parada + ".json");

            var client = _clientFactory.CreateClient();

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = response.Content.ReadAsStringAsync();
                return responseStream;
            }
            else
                return null;

        }

    }

}