using System;

namespace MicroondasMVC.Models
{
    public class Microondas
    {
        public int TempoSegundos { get; set; }
        public int Potencia { get; set; }
        public bool Ligado { get; set; } = false;
        public bool Pausado { get; set; } = false; // Novo Estado
        public string MsgStatus { get; set; }
        public DateTime HoraInicio { get; set; }

        public int TempoRestante
        {
            get
            {
                // Se está pausado, o TempoSegundos já contém o valor travado no momento da pausa
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
                // --- MODO +30s (Já rodando) ---
                int atual = TempoRestante;
                TempoSegundos = Math.Min(atual + 30, 120);
                HoraInicio = DateTime.Now;
                MsgStatus = $"Aquecimento estendido. Faltam {FormatarTempo(TempoSegundos)}.";
            }
            else if (Pausado)
            {
                // --- MODO RETOMAR (Estava Pausado) ---
                // Não muda o tempo, apenas liga novamente
                Ligado = true;
                Pausado = false;
                HoraInicio = DateTime.Now;
                MsgStatus = $"Aquecimento retomado. Restam {FormatarTempo(TempoSegundos)}.";
            }
            else
            {
                // --- MODO INÍCIO DO ZERO ---
                Potencia = potenciaEntrada ?? 10;
                TempoSegundos = tempoEntrada ?? 30;

                ValidarLimites();

                Ligado = true;
                Pausado = false;
                HoraInicio = DateTime.Now;
                MsgStatus = $"Aquecendo por {FormatarTempo(TempoSegundos)} na potência {Potencia}.";
            }
        }

        public void PausarOuCancelar()
        {
            if (Ligado)
            {
                // REGRA 1: Se "Ligado" -> Pausa
                // Salvamos o tempo que falta agora como o novo TempoSegundos
                TempoSegundos = TempoRestante;
                Ligado = false;
                Pausado = true;
                MsgStatus = $"Pausado em {FormatarTempo(TempoSegundos)}. Pressione Iniciar para continuar ou Pausar para cancelar.";
            }
            else if (Pausado)
            {
                // REGRA 3: Se "Pausado" -> Cancela (Limpa tudo)
                Resetar();
                MsgStatus = "Operação cancelada.";
            }
            else
            {
                // REGRA 4: Se não iniciado -> Limpa inputs
                Resetar();
                MsgStatus = "Programação limpa.";
            }
        }

        public void Desligar()
        {
            Ligado = false;
            Pausado = false;
            MsgStatus = "Aquecimento concluído.";
        }

        private void Resetar()
        {
            Ligado = false;
            Pausado = false;
            TempoSegundos = 0;
            Potencia = 0;
        }

        private void ValidarLimites()
        {
            if (TempoSegundos < 1) TempoSegundos = 30;
            if (TempoSegundos > 120) TempoSegundos = 120;
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