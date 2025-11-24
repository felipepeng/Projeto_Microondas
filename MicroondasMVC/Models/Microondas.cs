using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MicroondasMVC.Models
{
    public class ProgramaAquecimento
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O Nome é obrigatório.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O Alimento é obrigatório.")]
        public string Alimento { get; set; }

        [Required(ErrorMessage = "O Tempo é obrigatório.")]
        [Range(1, 9999, ErrorMessage = "Tempo deve ser maior que 0.")]
        public int Tempo { get; set; }

        [Required(ErrorMessage = "A Potência é obrigatória.")]
        [Range(1, 10, ErrorMessage = "A Potência deve ser entre 1 e 10.")]
        public int Potencia { get; set; }

        [Required(ErrorMessage = "O Caractere de aquecimento é obrigatório.")]
        public char CaractereAquecimento { get; set; }

        public string Instrucoes { get; set; } // Opcional

        public bool EhCustomizado { get; set; } = false;
    }

    public class Microondas
    {
        //  Estados do Microondas
        public int TempoSegundos { get; set; }
        public int Potencia { get; set; }
        public bool Ligado { get; set; } = false;
        public bool Pausado { get; set; } = false;
        public string MsgStatus { get; set; }
        public DateTime HoraInicio { get; set; }
        public ProgramaAquecimento ProgramaAtivo { get; set; }

        //  Persistência e Listas 
        private static string CaminhoArquivo = "programas_customizados.json";

        public static List<ProgramaAquecimento> ProgramasPreDefinidos = new List<ProgramaAquecimento>
        {
            new ProgramaAquecimento { Id=1, Nome="Pipoca", Alimento="Pipoca (de micro-ondas)", Tempo=180, Potencia=7, CaractereAquecimento='*', Instrucoes="Observar o barulho de estouros de milho, caso houver intervalo de mais de 10 segundos entre um estouro e outro, interrompa o aquecimento.", EhCustomizado=false },
            new ProgramaAquecimento { Id=2, Nome="Leite", Alimento="Leite", Tempo=300, Potencia=5, CaractereAquecimento='%', Instrucoes="Cuidado com aquecimento de líquidos, o choque térmico aliado ao movimento do recipiente pode causar fervura imediata causando risco de queimaduras.", EhCustomizado=false },
            new ProgramaAquecimento { Id=3, Nome="Carnes de boi", Alimento="Carne em pedaços ou fatias", Tempo=840, Potencia=4, CaractereAquecimento='#', Instrucoes="Interrompa o processo na metade e vire o conteúdo com a parte de baixo para cima para o descongelamento uniforme.", EhCustomizado=false },
            new ProgramaAquecimento { Id=4, Nome="Frango", Alimento="Frango (qualquer corte)", Tempo=480, Potencia=7, CaractereAquecimento='@', Instrucoes="Interrompa o processo na metade e vire o conteúdo com a parte de baixo para cima para o descongelamento uniforme.", EhCustomizado=false },
            new ProgramaAquecimento { Id=5, Nome="Feijão", Alimento="Feijão congelado", Tempo=480, Potencia=9, CaractereAquecimento='&', Instrucoes="Deixe o recipiente destampado e em casos de plástico, cuidado ao retirar o recipiente pois o mesmo pode perder resistência em altas temperaturas.", EhCustomizado=false }
        };

        public static List<ProgramaAquecimento> TodosProgramas
        {
            get
            {
                var customizados = CarregarCustomizados();
                var todos = new List<ProgramaAquecimento>();
                todos.AddRange(ProgramasPreDefinidos);
                todos.AddRange(customizados);
                return todos;
            }
        }

        public static List<ProgramaAquecimento> CarregarCustomizados()
        {
            if (!File.Exists(CaminhoArquivo)) return new List<ProgramaAquecimento>();
            try
            {
                var json = File.ReadAllText(CaminhoArquivo);
                return JsonSerializer.Deserialize<List<ProgramaAquecimento>>(json) ?? new List<ProgramaAquecimento>();
            }
            catch
            {
                return new List<ProgramaAquecimento>();
            }
        }

        public static void AdicionarCustomizado(ProgramaAquecimento novo)
        {
            var lista = CarregarCustomizados();

            // Gera ID único
            int maiorId = 0;
            if (ProgramasPreDefinidos.Any()) maiorId = ProgramasPreDefinidos.Max(p => p.Id);
            if (lista.Any() && lista.Max(p => p.Id) > maiorId) maiorId = lista.Max(p => p.Id);

            novo.Id = maiorId + 1;
            novo.EhCustomizado = true;

            lista.Add(novo);

            var json = JsonSerializer.Serialize(lista);
            File.WriteAllText(CaminhoArquivo, json);
        }

        //  Lógica da Operação 
        public char CaractereAtual => ProgramaAtivo != null ? ProgramaAtivo.CaractereAquecimento : '.';

        public int TempoRestante
        {
            get
            {
                if (Pausado) return TempoSegundos;
                if (!Ligado) return 0;
                var tempoPassado = (int)(DateTime.Now - HoraInicio).TotalSeconds;
                var restante = TempoSegundos - tempoPassado;
                return restante > 0 ? restante : 0;
            }
        }

        public void IniciarOuIncrementar(int? tempoEntrada, int? potenciaEntrada)
        {
            if (Ligado)
            {
                if (ProgramaAtivo != null)
                {
                    MsgStatus = "Não é permitido acrescentar tempo a programas pré-definidos.";
                    return;
                }
                // Modo Manual (+30s)
                int atual = TempoRestante;
                TempoSegundos = Math.Min(atual + 30, 120);
                HoraInicio = DateTime.Now;
                MsgStatus = $"Aquecimento estendido. Faltam {FormatarTempo(TempoSegundos)}.";
            }
            else if (Pausado)
            {
                Ligado = true; Pausado = false; HoraInicio = DateTime.Now;
                MsgStatus = $"Aquecimento retomado. Restam {FormatarTempo(TempoSegundos)}.";
            }
            else
            {
                // Início Manual do Zero
                ProgramaAtivo = null;
                Potencia = potenciaEntrada ?? 10;
                TempoSegundos = tempoEntrada ?? 30;
                ValidarLimitesManual();

                Ligado = true; Pausado = false; HoraInicio = DateTime.Now;
                MsgStatus = $"Aquecendo por {FormatarTempo(TempoSegundos)} na potência {Potencia}.";
            }
        }

        public void IniciarPrograma(int idPrograma)
        {
            var prog = TodosProgramas.FirstOrDefault(p => p.Id == idPrograma);
            if (prog == null) return;

            Resetar();
            ProgramaAtivo = prog;
            TempoSegundos = prog.Tempo;
            Potencia = prog.Potencia;

            Ligado = true;
            HoraInicio = DateTime.Now;
            MsgStatus = $"Programa {prog.Nome}: {prog.Instrucoes}";
        }

        public void PausarOuCancelar()
        {
            if (Ligado)
            {
                TempoSegundos = TempoRestante;
                Ligado = false; Pausado = true;
                MsgStatus = $"Pausado em {FormatarTempo(TempoSegundos)}.";
            }
            else if (Pausado)
            {
                Resetar(); MsgStatus = "Operação cancelada.";
            }
            else
            {
                Resetar(); MsgStatus = "Programação limpa.";
            }
        }

        public void Desligar()
        {
            Ligado = false; Pausado = false; ProgramaAtivo = null;
            MsgStatus = "Aquecimento concluído.";
        }

        private void Resetar()
        {
            Ligado = false; Pausado = false; TempoSegundos = 0; Potencia = 0; ProgramaAtivo = null;
        }

        private void ValidarLimitesManual()
        {
            if (TempoSegundos < 1) TempoSegundos = 30;
            if (TempoSegundos > 120)
            {
                TempoSegundos = 120;
                MsgStatus = "Tempo manual limitado a 2 minutos.";
            }
            if (Potencia < 1) Potencia = 1;
            if (Potencia > 10) Potencia = 10;
        }

        private string FormatarTempo(int segundosTotal)
        {
            if (segundosTotal >= 60)
            {
                int min = segundosTotal / 60;
                int seg = segundosTotal % 60;
                return $"{min}:{seg:D2} min";
            }
            return $"{segundosTotal} seg";
        }
    }
}