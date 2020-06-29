using PDV_WPF.Properties;
using System;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;
using static PDV_WPF.Funcoes.Extensions;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;



namespace Balancas
{
    class Balanca
    {
        private class Toledo : IDisposable
        {
            [DllImport(@"P05.dll", CallingConvention = CallingConvention.Winapi)]
            public static extern int AbrePorta(int porta, int velocidade, int dataBits, int paridade);

            [DllImport(@"P05.dll", CallingConvention = CallingConvention.Winapi)]
            public static extern int FechaPorta();

            [DllImport(@"P05.dll", CallingConvention = CallingConvention.Winapi)]
            public static extern int PegaPeso(int tipoEscrita, StringBuilder peso, string diretorio);

            public decimal ProcessaPeso()
            {
                FechaPorta();
                Int32.TryParse(BALPORTA.ToString(), out int porta);
                var baud = BALBAUD switch
                {
                    2400 => 0,
                    4800 => 1,
                    9600 => 2,
                    _ => 1,
                };
                int parid = BALPARITY;
                long retorno = AbrePorta(porta, baud, 1, parid);
                //long retorno = AbrePorta(1, 1, 1, 0);
                /*Paridade:
                 * 0 = Nenhuma
                 * 1 = Impar
                 * 2 = Par
                 * 3 = Espaço
                 */

                if (retorno == 1)
                {
                    StringBuilder pesoStr = new StringBuilder();
                    retorno = PegaPeso(0, pesoStr, "");
                    Decimal.TryParse(pesoStr.ToString(), out decimal peso);
                    retorno = FechaPorta();

                    if (retorno == 1)
                    {
                        return peso / 1000;
                    }
                    else
                    {
                        return -100;
                        //Erro ao fechar porta
                    }
                }
                else
                {
                    return -200;
                    //Erro ao abrir a porta
                }
            }
            public decimal RetornaPeso()
            {
                decimal peso;
                for (int i = 0; i < 6; i++)
                {
                    peso = ProcessaPeso();
                    if (peso != 0)
                    {
                        //MessageBox.Show(peso.ToString());
                        return peso;
                    }
                    System.Threading.Thread.Sleep(500);
                }
                return -100;
            }
            private const string LOCAL_ESCRITA = ""; //Diretorio onde será gravado o arquivo. Se vazio significa o diretorio local do programa
            private const int OPCAO_ESCRITA = 1; //Disponibilizar em     => 0 = Arq Texto, 1 = Área de Transferência
            public void Dispose()
            {
                FechaPorta();
            }
        }
        private class Protocolo2 : IDisposable
        {
            decimal peso = 0;

            SerialPort PORTA = new SerialPort($"COM{BALPORTA}", BALBAUD.Safeint(), ((Parity)Enum.ToObject(typeof(Parity), BALPARITY)), BALBITS);
            /*Paridade:
             * 0 = Nenhuma
             * 1 = Impar
             * 2 = Par
             * 3 = Espaço
             */
            public decimal RetornaPeso()
            {
                PORTA.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);

                PORTA.Open();

                for (int i = 0; i < 6; i++)
                {
                    if (peso != 0)
                    {
                        PORTA.Close();
                        return peso / 1000;
                    }
                    else { System.Threading.Thread.Sleep(500); }
                }
                PORTA.Close();
                return -100;
            }
            private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
            {
                // Show all the incoming data in the port's buffer
                string receivedString = PORTA.ReadExisting();
                if (receivedString.Length < 5) return;
                try
                {
                    peso = receivedString.Substring(1, 5).Safedecimal();
                }
                catch (Exception)
                {
                    return;
                }
            }
            public void Dispose()
            {
                if (PORTA.IsOpen)
                { PORTA.Close(); }
            }

        }
        public static decimal RetornaPeso()
        {
            switch (BALMODELO)
            {
                case 2:
                    using (Toledo tol = new Toledo())
                    {
                        return tol.RetornaPeso();
                    }
                case 3:
                    return -100;
                case 1:
                    using (Protocolo2 Prt2 = new Protocolo2())
                    {
                        return Prt2.RetornaPeso();
                    }
                default:
                    return -200;
            }
        }
    }
}
