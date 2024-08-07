using PDV_WPF.Properties;
using System;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Clearcove.Logging;
using static PDV_WPF.Funcoes.Extensions;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;



namespace Balancas
{
    class Balanca
    {
        private class Toledo : IDisposable
        {
            private Logger _log = new Logger(typeof(Balanca));

            [DllImport(@"P05.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern int AbrePorta(int porta, int velocidade, int dataBits, int paridade);

            [DllImport(@"P05.dll", CallingConvention = CallingConvention.Winapi)]
            public static extern int FechaPorta();

            [DllImport(@"P05.dll", CallingConvention = CallingConvention.Winapi)]
            public static extern int PegaPeso(int tipoEscrita, StringBuilder peso, string diretorio);

            public decimal ProcessaPeso()
            {
                _log.Info(">> ProcessaPeso chamado!");

                _log.Info("Fechando porta (só pra garantir)");
                _log.Info($"FechaPorta retornou {FechaPorta()}");
                Int32.TryParse(BALPORTA.ToString(), out int porta);
                _log.Info($"A porta da balança é {porta}");
                var baud = BALBAUD switch
                {
                    2400 => 0,
                    4800 => 1,
                    9600 => 2,
                    _ => 1,
                };
                _log.Info($"A velocidade é {BALBAUD}, que converte para {baud}");
                int parid = BALPARITY;
                _log.Info($"A paridade é {BALPARITY}, que converte para {parid}");
                _log.Info($"Abrindo a porta.");
                long retorno = AbrePorta(porta, baud, 1, parid);
                //long retorno = AbrePorta(1, 1, 1, 0);
                /*Paridade:
                 * 0 = Nenhuma
                 * 1 = Impar
                 * 2 = Par
                 * 3 = Espaço
                 */
                _log.Info($"AbrePorta retornou {retorno}");
                if (retorno == 1)
                {
                    StringBuilder pesoStr = new StringBuilder();
                    _log.Info("A porta abriu, bora pegar o preço");
                    try
                    {
                        PegaPeso(0, pesoStr, "");
                        _log.Info($"PegaPeso retornou e preencheu pesoStr com {pesoStr}");
                        Decimal.TryParse(pesoStr.ToString(), out decimal peso);
                        _log.Info($"O peso obtido foi {peso} (que corresponde ao peso {peso / 1000}. Só fechar a porta agora.");
                        retorno = FechaPorta();
                        _log.Info($"FechaPorta retornou {retorno}");
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
                    catch (Exception e)
                    {
                        _log.Error("Deu rui ao chamar PegaPeso", e);
                        FechaPorta();
                        return 0;
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
                _log.Info(">> RetornaPeso chamado!");
                try
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
                catch (Exception e)
                {

                    throw;
                }
                finally
                {
                    FechaPorta();
                }

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

            private Logger _log = new Logger(typeof(Balanca));

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
                byte[] receivedBuffer = Encoding.ASCII.GetBytes(receivedString);
                _log.Debug(receivedString);
                var indexStart = Array.IndexOf(receivedBuffer, 0x02);
                var indexEnd = Array.IndexOf(receivedBuffer, 0x03);
                byte[] processedBytes = receivedBuffer.Skip(indexStart).Take(indexEnd - indexStart).ToArray();
                try
                {
                    if (processedBytes[0] == 'N' || processedBytes[0] == 'I' || processedBytes[0] == 'S')
                        return;
                    peso = Encoding.ASCII.GetString(processedBytes).Safedecimal();
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
