using MicroondasMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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

        // --- Cadastro ---

        [HttpGet]
        public IActionResult Cadastro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SalvarPrograma(ProgramaAquecimento novoPrograma)
        {
            if (!ModelState.IsValid)
            {
                return View("Cadastro", novoPrograma);
            }

            // Validação: Caractere '.' é reservado
            if (novoPrograma.CaractereAquecimento == '.')
            {
                ModelState.AddModelError("CaractereAquecimento", "O caractere '.' é reservado para o modo padrão.");
                return View("Cadastro", novoPrograma);
            }

            // Validação: Caractere único em relação a TODOS os programas
            var todos = Microondas.TodosProgramas;
            if (todos.Any(p => p.CaractereAquecimento == novoPrograma.CaractereAquecimento))
            {
                ModelState.AddModelError("CaractereAquecimento", $"O caractere '{novoPrograma.CaractereAquecimento}' já está em uso.");
                return View("Cadastro", novoPrograma);
            }

            Microondas.AdicionarCustomizado(novoPrograma);
            return RedirectToAction("Index");
        }

        // --- Operações ---

        [HttpPost]
        public IActionResult Iniciar(int? tempoSegundos, int? potencia)
        {
            micro.IniciarOuIncrementar(tempoSegundos, potencia);
            return RedirectToAction("Index");
        }

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