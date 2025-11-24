using System;
using System.Collections.Generic;
using System.Text;

namespace MicroondasMVC.Models
{
    internal class Microondas
    {
        //Atributos
        public int TempoSegundos { get; set; }
        public int Potencia { get; set; }
        public bool Ligado { get; set; } = false; //Inicia desligado
        public string MsgStatus { get; set; }
        public string IndicadorAquecimento { get; set; }


        //Métodos

        public void IniciarAquecimento()
        {
            if (!Ligado)
            {
                Ligado = true;
            }
            else
            {
                if (TempoSegundos + 30 > 120)
                {
                    TempoSegundos = 120;
                }
                else
                {
                    TempoSegundos += 30;
                }

            }

            // Msg Status
            if (TempoSegundos > 60 && TempoSegundos < 100)
            {
                int minutos = TempoSegundos / 60;
                int segundos = TempoSegundos % 60;

                MsgStatus = $"Aquecendo por {minutos}:{segundos:D2} minutos na potência {Potencia}.";
            }
            else
            {
                MsgStatus = $"Aquecendo por {TempoSegundos} segundos na potência {Potencia}.";
            }

            // ---------------------------------
            // Indicador visual de processamento 
            // ---------------------------------

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < TempoSegundos; i++)
            {
                sb.Append(new string('.', Potencia)); // potencia define quantos pontos por segundo
                sb.Append(" ");
            }

            // Ao término, adiciona a mensagem final
            sb.Append("Aquecimento concluído");

            IndicadorAquecimento = sb.ToString();
        }


    }
}
