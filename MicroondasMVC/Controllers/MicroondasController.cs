using MicroondasMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace MicroondasMVC.Controllers
{
    public class MicroondasController : Controller
    {
        private static Microondas micro = new Microondas();

        public IActionResult Index()
        {
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

        // Ação específica para programas
        [HttpPost]
        public IActionResult IniciarPrograma(int idPrograma)
        {
            micro.IniciarPrograma(idPrograma);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult PausarCancelar()
        {
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