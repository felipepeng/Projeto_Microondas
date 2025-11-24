using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroondasMVC.Models
{
    public class ProgramaAquecimento
    {
        public string Nome { get; set; }
        public string Alimento { get; set; }
        public int Tempo { get; set; }
        public int Potencia { get; set; }
        public char CaractereAquecimento { get; set; }
        public string Instrucoes { get; set; }

        // Identificador simples para a View
        public int Id { get; set; }
    }

    public class Microondas
    {
        public int TempoSegundos { get; set; }
        public int Potencia { get; set; }
        public bool Ligado { get; set; } = false;
        public bool Pausado { get; set; } = false;
        public string MsgStatus { get; set; }
        public DateTime HoraInicio { get; set; }

        // Novo: Armazena o programa atual (se houver)
        public ProgramaAquecimento ProgramaAtivo { get; set; }

        // Lista estática dos programas (Singleton de dados)
        public static List<ProgramaAquecimento> Programas = new List<ProgramaAquecimento>
        {
            new ProgramaAquecimento { Id=1, Nome="Pipoca", Alimento="Pipoca (de micro-ondas)", Tempo=180, Potencia=7, CaractereAquecimento='*', Instrucoes="Observar o barulho de estouros de milho, caso houver intervalo de mais de 10 segundos entre um estouro e outro, interrompa o aquecimento." },
            new ProgramaAquecimento { Id=2, Nome="Leite", Alimento="Leite", Tempo=300, Potencia=5, CaractereAquecimento='%', Instrucoes="Cuidado com aquecimento de líquidos, o choque térmico aliado ao movimento do recipiente pode causar fervura imediata causando risco de queimaduras." },
            new ProgramaAquecimento { Id=3, Nome="Carnes de boi", Alimento="Carne em pedaços ou fatias", Tempo=840, Potencia=4, CaractereAquecimento='#', Instrucoes="Interrompa o processo na metade e vire o conteúdo com a parte de baixo para cima para o descongelamento uniforme." },
            new ProgramaAquecimento { Id=4, Nome="Frango", Alimento="Frango (qualquer corte)", Tempo=480, Potencia=7, CaractereAquecimento='@', Instrucoes="Interrompa o processo na metade e vire o conteúdo com a parte de baixo para cima para o descongelamento uniforme." },
            new ProgramaAquecimento { Id=5, Nome="Feijão", Alimento="Feijão congelado", Tempo=480, Potencia=9, CaractereAquecimento='&', Instrucoes="Deixe o recipiente destampado e em casos de plástico, cuidado ao retirar o recipiente pois o mesmo pode perder resistência em altas temperaturas." }
        };

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
            // Se está LIGADO
            if (Ligado)
            {
                // REGRA NOVA: Programas pré-definidos NÃO permitem acréscimo de tempo
                if (ProgramaAtivo != null)
                {
                    MsgStatus = "Não é permitido acrescentar tempo a programas pré-definidos.";
                    return;
                }

                // Modo +30s (Manual)
                int atual = TempoRestante;
                TempoSegundos = Math.Min(atual + 30, 120); // Limite hardcoded para manual
                HoraInicio = DateTime.Now;
                MsgStatus = $"Aquecimento estendido. Faltam {FormatarTempo(TempoSegundos)}.";
            }
            // Se está PAUSADO
            else if (Pausado)
            {
                Ligado = true;
                Pausado = false;
                HoraInicio = DateTime.Now;
                MsgStatus = $"Aquecimento retomado. Restam {FormatarTempo(TempoSegundos)}.";
            }
            // MODO INÍCIO DO ZERO (Manual)
            else
            {
                ProgramaAtivo = null; // Garante que é manual
                Potencia = potenciaEntrada ?? 10;
                TempoSegundos = tempoEntrada ?? 30;

                ValidarLimitesManual();

                Ligado = true;
                Pausado = false;
                HoraInicio = DateTime.Now;
                MsgStatus = $"Aquecendo por {FormatarTempo(TempoSegundos)} na potência {Potencia}.";
            }
        }

        public void IniciarPrograma(int idPrograma)
        {
            var prog = Programas.FirstOrDefault(p => p.Id == idPrograma);
            if (prog == null) return;

            Resetar(); // Limpa estados anteriores

            ProgramaAtivo = prog;
            TempoSegundos = prog.Tempo;
            Potencia = prog.Potencia;

            // Programas ignoram validação de 2 minutos (ex: Carnes tem 14min)

            Ligado = true;
            HoraInicio = DateTime.Now;
            MsgStatus = $"Programa {prog.Nome}: {prog.Instrucoes}";
        }

        public void PausarOuCancelar()
        {
            if (Ligado)
            {
                TempoSegundos = TempoRestante;
                Ligado = false;
                Pausado = true;
                MsgStatus = $"Pausado em {FormatarTempo(TempoSegundos)}.";
            }
            else if (Pausado)
            {
                Resetar();
                MsgStatus = "Operação cancelada.";
            }
            else
            {
                Resetar();
                MsgStatus = "Programação limpa.";
            }
        }

        public void Desligar()
        {
            Ligado = false;
            Pausado = false;
            ProgramaAtivo = null; // Reseta programa ao fim
            MsgStatus = "Aquecimento concluído.";
        }

        private void Resetar()
        {
            Ligado = false;
            Pausado = false;
            TempoSegundos = 0;
            Potencia = 0;
            ProgramaAtivo = null;
        }

        private void ValidarLimitesManual()
        {
            // Validações aplicáveis APENAS para entrada manual
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