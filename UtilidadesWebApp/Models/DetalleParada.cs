using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UtilidadesWebApp.Models
{
    public class DetalleParada
    {
        public string title { get; set; }

        public List<Destinos> destinos;

        public Geometry geometry;
  

    }

    public class Destinos
    {
        public string linea { get; set; }
        public string destino { get; set; }
        public string primero { get; set; }
        public string segundo { get; set; }
    }

    public class Geometry
    {
        public float[] coordinates { get; set; }=new float[2];
    }

}

