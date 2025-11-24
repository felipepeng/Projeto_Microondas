using MicroondasMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace MicroondasMVC.Controllers
{
    public class MicroondasController : Controller
    {
        private static Microondas micro = new Microondas();

        public IActionResult Index()
        {
            // Se o tempo estourou enquanto estava ligado (refresh de página tardio), desliga
            if (micro.Ligado && micro.TempoRestante <= 0)
            {
                micro.Desligar();
            }
            return View(micro);
        }

        [HttpPost]
        public IActionResult Iniciar(int? tempoSegundos, int? potencia)
        {
            micro.IniciarOuIncrementar(tempoSegundos, potencia);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult PausarCancelar()
        {
            // Chama a nova lógica do Model
            micro.PausarOuCancelar();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Desligar()
        {
            micro.Desligar();
            return Ok();
        }
    }
}