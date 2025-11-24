using MicroondasMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroondasMVC.Controllers
{
    public class MicroondasController : Controller
    {
        private static Microondas micro = new Microondas();

        public IActionResult Index()
        {
            return View(micro);
        }


        [HttpPost]
        public IActionResult Iniciar(int? tempoSegundos, int? potencia)
        {
            // Se o micro já está ligado, NÃO muda nenhum valor
            // Apenas deixa o model adicionar +30s
            if (micro.Ligado)
            {
                micro.IniciarAquecimento();
                return RedirectToAction("Index");
            }

            // Micro desligado → configurar tempo normalmente

            micro.Potencia = potencia ?? 10;
            micro.TempoSegundos = tempoSegundos ?? 30;

            micro.IniciarAquecimento();

            return RedirectToAction("Index");
        }



    }
}
