using CfeRecepcao_0008;
using Clearcove.Logging;
using LocalDarumaFrameworkDLL;
using PDV_WPF.Properties;
using PDV_WPF.Telas;
using System;
using System.Text;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF
{
    public static class FuncoesECF
    {
        private static StringBuilder sbRetornoVerificaRetorno = new StringBuilder();
        private static StringBuilder sbRetornoVerificaErro = new StringBuilder();


        /// <summary>
        /// Determina se a ECF está com uma redução Z pendente.
        /// </summary>
        /// <returns></returns>
        static public bool? ChecaStatusReducaoZ()
        {
            // Verificar se o caixa usa ECF:
            // Verificar se uma redução z está pendente:
            StringBuilder sbRetornoStatusRedZ = new StringBuilder();
            int intRetornoDaruma = UnsafeNativeMethods.rVerificarReducaoZ_ECF_Daruma(sbRetornoStatusRedZ);
            switch (intRetornoDaruma)
            {
                case 0:
                    // Erro de comunicação, não foi possível enviar o método. 
                    //logErroAntigo("(ECF) Erro de comunicação, não foi possível enviar o método (rVerificarReducaoZ_ECF_Daruma retornou 0)");
                        DialogBox.Show(strings.REDUCAO_Z, DialogBoxButtons.No, DialogBoxIcons.Error, false, "Verificação de redução Z não retornou o status!", "Não será possível abrir uma venda.", "Por favor entre em contato com o suporte técnico.");
                    return null;
                case 1:
                    // OK, Sucesso ao enviar o método. 
                    // Pega o status do retorno (sbRetornoStatusRedZ):
                    switch (sbRetornoStatusRedZ.ToString())
                    {
                        case "0":
                            // Não há redução Z
                            // Segue o jogo
                            return true;
                        case "1":
                            // Redução Z pendente
                            // Interromper e realizar a redução
                            return false;
                        default:
                            logErroAntigo("(ECF) Erro ao obter pendência de redução Z (sbRetornoStatusRedZ retornou " + sbRetornoStatusRedZ.ToString() + ")");
                            DialogBox.Show(strings.REDUCAO_Z, DialogBoxButtons.No, DialogBoxIcons.Error, false, "Erro ao obter pendência de redução Z!", "Não será possível abrir uma venda.", "Por favor entre em contato com o suporte técnico.");
                            return null;
                    }
                case -6:
                    logErroAntigo("(ECF) Erro ao obter pendência de redução Z (Impressora está desligada)");
                    DialogBox.Show("IMPRESSORA FISCAL DESLIGADA", DialogBoxButtons.No, DialogBoxIcons.Error, false, "Verifique, ou reinicie a impressora e tente novamente.");
                    return null;
                default:
                    // Retorno não esperado
                    logErroAntigo("(ECF) Retorno não esperado (rVerificarReducaoZ_ECF_Daruma retornou " + intRetornoDaruma.ToString() + ")");
                    DialogBox.Show(strings.REDUCAO_Z, DialogBoxButtons.No, DialogBoxIcons.Error, false, "Verificação de redução Z retornou um status não esperado!", "Não será possível abrir uma venda.", "Por favor entre em contato com o suporte técnico.");
                    return null;
            }
        }

        /// <summary>
        /// Efetua a redução Z
        /// </summary>
        /// <returns>True se a redução Z foi efetuada com sucesso, False por qualquer outro motivo</returns>
        static public bool EfetuaReducaoZ()
        {
            Logger log = new Logger("Redução Z");
            if (!PedeSenhaGerencial("Efetuando Redução Z", Objetos.Enums.Permissoes.Nenhum))
            { return false; }



            switch (DialogBox.Show(strings.REDUCAO_Z, DialogBoxButtons.YesNo, DialogBoxIcons.None, false, "Deseja efetuar a redução Z?", "ATENÇÃO - O sistema poderá ficar bloqueado para vendas até amanhã!!"))
            {
                case true:
                    int retorno = UnsafeNativeMethods.iReducaoZ_ECF_Daruma("", "");
                    switch (retorno)
                    {
                        case 1:
                            int erro = UnsafeNativeMethods.eRetornarErro_ECF_Daruma();
                            switch (erro)
                            {
                                case 0:
                                    DialogBox.Show("Redução Z", DialogBoxButtons.Yes, DialogBoxIcons.None, false, "Redução concluída.");
                                    log.Debug("Redução Z efetuada");
                                    return true;
                                case 144:
                                    DialogBox.Show("Falha na redução Z", DialogBoxButtons.No, DialogBoxIcons.Info, false, "ECF desconectada");
                                    log.Debug("ECF RETORNOU ERRO 144");
                                    break;
                                case 78:
                                    UnsafeNativeMethods.iCFCancelar_ECF_Daruma();
                                    DialogBox.Show("Falha na redução Z", DialogBoxButtons.No, DialogBoxIcons.Info, false, "Havia um cupom anerto, que foi cancelado.", "Por favor, tente executar a redução Z novamente");
                                    break;
                                default:
                                    DialogBox.Show("Falha na redução Z", DialogBoxButtons.No, DialogBoxIcons.Info, false, $"Erro: {erro}");
                                    log.Debug($"ECF RETORNOU ERRO {erro}");
                                    break;
                            }
                            break;

                    }
                    break;
                default:
                    break;
            }
            return false;
        }

        static public bool? VendeProdutoECF(envCFeCFeInfCFeDet item)
        {
            //string _NCM = string.Empty;

            //_NCM = (item.prod.NCM is null) ? "" : item.prod.NCM;

            //int retornoNCM = Declaracoes.confCFNCM_ECF_Daruma(_NCM, "0");
            //switch (retornoNCM)
            //{
            //    case 0://Aqui a impressora não respondeu nada, e a DLL não soube lidar com a exceção.
            //        return null;
            //    case 1://Aqui a impressora respondeu alguma coisa.
            //        switch (ValidaOperacao(out string msgErro))
            //        {
            //            case true:
            //                break;
            //            default:
            //                DialogBox.Show("VendeProdutoECF(NCM)", $"Houve um erro ({retornoNCM})" + msgErro, DialogBoxButtons.Yes, DialogBoxIcons.None);
            //                return false;
            //        }
            //        break;
            //    default://Aqui a impressora não respondeu, mas a DLL soube lidar com a exceção.
            //        DialogBox.Show("VendeProdutoECF(NCM)", $"Houve um erro ({retornoNCM})" + InterpretaRetorno(retornoNCM), DialogBoxButtons.Yes, DialogBoxIcons.None);
            //        return false;
            //}


            int retorno = UnsafeNativeMethods.iCFVender_ECF_Daruma("T1800", item.prod.qCom.Substring(0, item.prod.qCom.Length - 1), item.prod.vUnCom.Substring(0, item.prod.vUnCom.Length - 1), "D$", item.prod.vDesc, item.prod.cProd, item.prod.uCom, item.prod.xProd);

            switch (retorno)
            {
                case 0://Aqui a impressora não respondeu nada, e a DLL não soube lidar com a exceção.
                    return null;
                case 1://Aqui a impressora respondeu alguma coisa.
                    switch (ValidaOperacao(out string msgErro))
                    {
                        case true:
                            break;
                        default:
                            DialogBox.Show("VendeProdutoECF", DialogBoxButtons.Yes, DialogBoxIcons.None, false, $"Houve um erro ({retorno})" + msgErro);
                            return false;
                    }
                    break;
                default://Aqui a impressora não respondeu, mas a DLL soube lidar com a exceção.
                    DialogBox.Show("VendeProdutoECF", DialogBoxButtons.Yes, DialogBoxIcons.None, true, $"Houve um erro ({retorno})" + InterpretaRetorno(retorno));
                    return false;
            }

            return true;
        }

        static string InterpretaRetorno(int retorno)
        {
            UnsafeNativeMethods.eInterpretarRetorno_ECF_Daruma(retorno, sbRetornoVerificaRetorno);
            return sbRetornoVerificaRetorno.ToString();
        }

        static bool ValidaOperacao(out string msgErro)
        {
            int a = UnsafeNativeMethods.eRetornarErro_ECF_Daruma();
            if (a == 0)
            {
                msgErro = String.Empty;
                return true;
            }
            else
            {
                UnsafeNativeMethods.eInterpretarErro_ECF_Daruma(a, sbRetornoVerificaErro);
                msgErro = sbRetornoVerificaErro.ToString();
                return false;
            }
        }

        /// <summary>
        /// Abre a gaveta usando a ECF
        /// </summary>
        /// <returns></returns>
        static public bool AbreGaveta()
        {
            UnsafeNativeMethods.eAbrirGaveta_ECF_Daruma();
            return true;
        }

        static public bool? AlteraValorXML(string tag, string valor)
        {
            int retorno = UnsafeNativeMethods.regAlterarValor_Daruma(tag, valor);

            switch (retorno)
            {
                case 0://Aqui a impressora não respondeu nada, e a DLL não soube lidar com a exceção.
                    return null;
                case 1://Aqui a impressora respondeu alguma coisa.
                    switch (ValidaOperacao(out string msgErro))
                    {
                        case true:
                            break;
                        default:
                            DialogBox.Show("", DialogBoxButtons.Yes, DialogBoxIcons.None, false, $"Houve um erro ({retorno})" + msgErro);
                            return false;
                    }
                    break;
                default://Aqui a impressora não respondeu, mas a DLL soube lidar com a exceção.
                    DialogBox.Show("VendeProdutoECF", DialogBoxButtons.Yes, DialogBoxIcons.None, false, $"Houve um erro ({retorno})" + InterpretaRetorno(retorno));
                    return false;
            }

            return true;
        }

    }
}
