using CfeRecepcao_0007;
using PayGo;
using PDV_WPF.Exceptions;
using PDV_WPF.Telas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Objetos
{
    public class OperTEF
    {
        private readonly DirectoryInfo diretorio = new DirectoryInfo(@"C:\PAYGO\OPER");
        private string transacaoAnterior;
        private string redeAdquirenteAnterior;
        public bool emVenda = false;
        public void PreparaNovaVenda()
        {
            Directory.CreateDirectory(@"C:\PAYGO\OPER");
            foreach (var file in diretorio.GetFiles())
            {
                file.Delete();
            }
        }

        public string RegistrarVendaDebito(decimal valor, int num_caixa, ItemChoiceType tipo, string registroCliente = null)
        {
            //011 - Tipo de transação: 20 - débito à vista (FIXO)
            //730 - Operação: 1 - Venda (com cartão) (FIXO)
            //003 - Valor da venda: valor
            //717 - DateTime da venda (FIXO)
            //718 - Número do Caixa
            //731 - Tipo de Cartão: 2 - Débito (FIXO)
            //006 - Tipo de Identificação
            //007 - Identificação do Cliente

            if (!String.IsNullOrWhiteSpace(transacaoAnterior))
            {
                ConfirmarUltimaTransacao();
            }
            var db = new TEFBox("Operação no TEF", "Pressione 'ENTER' e siga as instruções no TEF.", TEFBox.DialogBoxButtons.Yes, TEFBox.DialogBoxIcons.None);

            if (valor <= 0)
            {
                throw new ErroDeValidacaoTEF("Valor da venda não pode ser negativo");
            }
            if ((tipo == ItemChoiceType.CNPJ && tipo == ItemChoiceType.CPF) && String.IsNullOrWhiteSpace(registroCliente))
            {
                throw new ErroDeValidacaoTEF("Identificação do cliente inválida.");
            }

            var Venda = new CRT()
            {
                _003 = valor,
                _717 = DateTime.Now,
                _718 = num_caixa,
                _730 = "1",
                _731 = "2",
                _732 = "1"
            };

            switch (tipo)
            {
                case ItemChoiceType.CNPJ:
                    Venda._006 = CRT.cliente.CNPJ;
                    Venda._007 = registroCliente;
                    break;
                case ItemChoiceType.CPF:
                    Venda._006 = CRT.cliente.CPF;
                    Venda._007 = registroCliente;
                    break;
                default:
                    break;
            }
            Venda.Exec();
            if (db.ShowDialog() == false)
            {
                return null;
            }
            Dictionary<string, string> respostaCRT = General.LeResposta();
            Directory.CreateDirectory(@"C:\PAYGO\Oper");
            using (StreamWriter sw = File.CreateText(@"C:\PAYGO\Oper\" + respostaCRT["027-000"] + ".txt"))
            {
                foreach (var linha in respostaCRT)
                {
                    sw.WriteLine($"{linha.Key} = {linha.Value}");
                }
            }
            transacaoAnterior = respostaCRT["027-000"];
            redeAdquirenteAnterior = respostaCRT["010-000"];
            emVenda = true;
            return respostaCRT["030-000"];
        }

        public string RegistrarVendaCrédito(decimal valor, int num_caixa, ItemChoiceType tipo, int parcelas = 1, string registroCliente = null)
        {
            //730 - Operação: 1 - Venda (com cartão) (FIXO)
            //003 - Valor da venda: valor
            //717 - DateTime da venda (FIXO)
            //718 - Número do Caixa
            //731 - Tipo de Cartão: 2 - Débito (FIXO)
            //006 - Tipo de Identificação
            //007 - Identificação do Cliente

            if (!String.IsNullOrWhiteSpace(transacaoAnterior))
            {
                ConfirmarUltimaTransacao();
            }
            var db = new TEFBox("Operação no TEF", "Pressione 'ENTER' e siga as instruções no TEF.", TEFBox.DialogBoxButtons.Yes, TEFBox.DialogBoxIcons.None);


            if (valor <= 0)
            {
                throw new ErroDeValidacaoTEF("Valor da venda não pode ser negativo");
            }
            if ((tipo == ItemChoiceType.CNPJ || tipo == ItemChoiceType.CPF) && String.IsNullOrWhiteSpace(registroCliente))
            {
                throw new ErroDeValidacaoTEF("Identificação do cliente inválida.");
            }
            if (parcelas <= 0)
            {
                throw new ErroDeValidacaoTEF("Número de parcelas deve ser maior ou igual a 1");
            }
            var Venda = new CRT()
            {
                _003 = valor,
                _717 = DateTime.Now,
                _718 = num_caixa,
                _730 = "1",
                _731 = "1"

            };
            if (parcelas == 1)
            {
                Venda._732 = "1";
            }
            else if (parcelas > 1)
            {
                Venda._732 = "3";
                Venda._018 = parcelas;
            }

            //var Venda = new CRT()
            //{
            //    _003 = valor,
            //    _717 = DateTime.Now,
            //    _718 = num_caixa,
            //    _730 = "1",
            //    _731 = "2"
            //};
            switch (tipo)
            {
                case ItemChoiceType.CNPJ:
                    Venda._006 = CRT.cliente.CNPJ;
                    Venda._007 = registroCliente;
                    break;
                case ItemChoiceType.CPF:
                    Venda._006 = CRT.cliente.CPF;
                    Venda._007 = registroCliente;
                    break;
                default:
                    break;
            }
            Venda.Exec();
            if (db.ShowDialog() == false)
            {
                return null;
            }
            Dictionary<string, string> respostaCRT = General.LeResposta();
            Directory.CreateDirectory(@"C:\PAYGO\Oper");
            using (StreamWriter sw = File.CreateText(@"C:\PAYGO\Oper\" + respostaCRT["027-000"] + ".txt"))
            {
                foreach (var linha in respostaCRT)
                {
                    sw.WriteLine($"{linha.Key} = {linha.Value}");
                }
            }
            transacaoAnterior = respostaCRT["027-000"];
            redeAdquirenteAnterior = respostaCRT["010-000"];
            emVenda = true;
            return respostaCRT["030-000"];
        }

        public bool ConfirmarUltimaTransacao()
        {
            var Confirmacao = new CNF()
            {
                _010 = redeAdquirenteAnterior,
                _027 = transacaoAnterior,
                _717 = DateTime.Now
            };
            Confirmacao.Exec();
            Thread.Sleep(1000);
            return true;
        }

        public string CancelarVendaPorNSU(int cupom, decimal valorOperacao, string redeAdquirente, string NSU, string autorizacao = "0")
        {
            CNC Cancelamento = new CNC()
            {
                _002 = cupom.ToString(),
                _003 = (valorOperacao * 100).ToString("G29"),
                _739 = redeAdquirente,
                _012 = NSU,
                _013 = autorizacao
            };
            var db = new TEFBox("Operação no TEF", "Caso uma operação de TEF tenha sido feita, siga as instruções na tela.", TEFBox.DialogBoxButtons.Yes, TEFBox.DialogBoxIcons.None);
            Cancelamento.Exec();
            db.ShowDialog();
            if (db.DialogResult == false)
            {
                return "";
            }
            return "";
        }

        public string DesfazVendaAtual()
        {
            NCN nCN = new NCN()
            {
                _010 = redeAdquirenteAnterior,
                _027 = transacaoAnterior
            };
            nCN.Exec();
            if (DialogBox.Show("Operação no TEF", DialogBoxButtons.Yes, DialogBoxIcons.None, false, "Siga as instruções do TEF, e pressione ENTER quando terminar.") == false)
            {
                return "";
            }
            Thread.Sleep(1000);
            foreach (var file in diretorio.GetFiles())
            {
                if (file.Name.Contains(transacaoAnterior)) continue;
                Dictionary<string, string> operacaoSalva = General.LeResposta(file.FullName);
                CNC Cancelamento = new CNC()
                {
                    _003 = operacaoSalva["003-000"],
                    _739 = operacaoSalva["739-000"],
                    _012 = operacaoSalva["012-000"],
                    _013 = operacaoSalva["013-000"]
                };
                Cancelamento.Exec();
                var db1 = new TEFBox("Operação no TEF", "Pressione 'ENTER' e siga as instruções no TEF.", TEFBox.DialogBoxButtons.Yes, TEFBox.DialogBoxIcons.None);
                db1.ShowDialog();
                if (db1.DialogResult == false)
                {
                    return "";
                }
                ComprovanteTEF.ReciboTEF = General.LeResposta();
                ComprovanteTEF.IMPRIME(1, 0, 0, 0);
                ComprovanteTEF.IMPRIME(0, 1, 0, 0);
                Thread.Sleep(500);
                CNF confirmaCNC = new CNF()
                {
                    _010 = ComprovanteTEF.ReciboTEF["010-000"],
                    _027 = ComprovanteTEF.ReciboTEF["027-000"]
                };
                confirmaCNC.Exec();
                Thread.Sleep(500);
            }
            return "";
        }
    }
}
