﻿using CfeRecepcao_0008;
using Clearcove.Logging;
using FirebirdSql.Data.FirebirdClient;
using PDV_WPF.Exceptions;
using PDV_WPF.Funcoes;
using PDV_WPF.Properties;
using PDV_WPF.Telas;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using PDV_WPF.REMENDOOOOO;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.Statics;
using PDV_WPF.DataSets;
using static PDV_WPF.REMENDOOOOO.FuncoesFirebird;
using System.Security.Cryptography;
using System.Windows.Media.Animation;
using System.Runtime.Remoting;
using System.Security.Policy;
using FluentValidation;
using System.Data;
using FluentValidation.Results;

namespace PDV_WPF.Objetos
{
    public class Venda
    {
        private bool _produtoRecebido = false;
        private bool _ICMSrecebido = false;
        private bool _PISrecebido = false;
        private bool _COFINSrecebido = false;
        private bool _ISSQNRecebido = false;
        private CFe _cFeRetornado;
        private CFe _cFe;
        private envCFeCFeInfCFe _infCfe;
        private envCFeCFeInfCFeDet _det;
        public List<envCFeCFeInfCFeDet> _listaDets;
        private envCFeCFeInfCFeDetProd _produto;
        private envCFeCFeInfCFeDetImposto _imposto;
        private envCFeCFeInfCFeDetImpostoICMS _ICMS;
        private envCFeCFeInfCFeDetImpostoICMSICMS00 _ICMS00;
        private envCFeCFeInfCFeDetImpostoICMSICMS40 _ICMS40;
        private envCFeCFeInfCFeDetImpostoICMSICMSSN102 _ICMS102;
        private envCFeCFeInfCFeDetImpostoICMSICMSSN900 _ICMS900;
        private envCFeCFeInfCFeDetImpostoPIS _PIS;
        private envCFeCFeInfCFeDetImpostoPISPISAliq _PISAliq;
        private envCFeCFeInfCFeDetImpostoPISPISQtde _PISQtde;
        private envCFeCFeInfCFeDetImpostoPISPISNT _PISNT;
        private envCFeCFeInfCFeDetImpostoPISPISSN _PISSN;
        private envCFeCFeInfCFeDetImpostoPISPISOutr _PISOutr;
        private envCFeCFeInfCFeDetImpostoPISST _PISST;
        private envCFeCFeInfCFeDetImpostoCOFINS _COFINS;
        private envCFeCFeInfCFeDetImpostoCOFINSCOFINSAliq _COFINSAliq;
        private envCFeCFeInfCFeDetImpostoCOFINSCOFINSQtde _COFINSQtde;
        private envCFeCFeInfCFeDetImpostoCOFINSCOFINSNT _COFINSNT;
        private envCFeCFeInfCFeDetImpostoCOFINSCOFINSSN _COFINSSN;
        private envCFeCFeInfCFeDetImpostoCOFINSCOFINSOutr _COFINSOutr;
        private envCFeCFeInfCFeDetImpostoCOFINSST _COFINSST;
        private envCFeCFeInfCFeDetImpostoISSQN _ISSQN;
        private envCFeCFeInfCFeTotal _Total;
        //private envCFeCFeInfCFeTotalDescAcrEntr _DescAcrEntr;
        private List<envCFeCFeInfCFePgtoMP> _listaPagamentos;
        //private envCFeCFeInfCFePgto _pagamento;
        private envCFeCFeInfCFePgtoMP _MP;
        private envCFeCFeInfCFeDetProdObsFiscoDet _obsFiscoDet;
        private List<envCFeCFeInfCFeDetProdObsFiscoDet> _listaObsFiscoDet;
        public int nItemCupom = 1;
        private readonly List<string> _listademetodos = new List<string>() { "01", "02", "03", "04", "05", "10", "11", "12", "13", "17", "19", "99" };
        private static readonly CultureInfo ptBR = CultureInfo.GetCultureInfo("pt-BR");
        public bool imprimeViaAssinar = false;
        private decimal _valTroco;
        public bool imprimeViaCliente = true;
        private enum TipoPromo { LEVA_PAGA, DESCONTO_VARIADO, PREÇO_FIXO }
        private ClienteDuePayDTO _clienteDuepay;

        public void RecebeCFeDoSAT(CFe cfeDeRetorno)
        {
            _cFeRetornado = cfeDeRetorno;
        }
        public CFe RetornaCFe()
        {
            //foreach (envCFeCFeInfCFeDet det in _cFe.infCFe.det)
            //{
            //    if (det.descAtacado > 0)
            //    {
            //        decimal disgraça = det.prod.vDesc.Safedecimal();
            //        if (det.prod.qCom.Safedecimal() == 1)
            //        {
            //            disgraça += (det.descAtacado);
            //        }
            //        else
            //        {
            //            disgraça += (det.descAtacado / det.prod.qCom.Safedecimal());

            //        } det.prod.vDesc = disgraça.ToString("0.00");
            //    }
            //}          
            return _cFe;
        }

        public Venda()
        {
            _cFe = new CFe();
        }

        public decimal DescontoAplicado()
        {
            if (_Total is null || _Total.DescAcrEntr.ItemElementName == ItemChoiceType1.vAcresSubtot)
                return 0;
            else
            {
                return decimal.Parse(_Total.DescAcrEntr.Item, CultureInfo.InvariantCulture);
            }
        }

        public void Clear()
        {
            _cFe = null;
            _infCfe = null;
            _det = null;
            _listaDets = null;
            _produto = null;
            _imposto = null;
            _ICMS = null;
            _ICMS00 = null;
            _ICMS40 = null;
            _ICMS102 = null;
            _ICMS900 = null;
            _PIS = null;
            _PISAliq = null;
            _PISQtde = null;
            _PISNT = null;
            _PISSN = null;
            _PISOutr = null;
            _PISST = null;
            _COFINS = null;
            _COFINSAliq = null;
            _COFINSQtde = null;
            _COFINSNT = null;
            _COFINSSN = null;
            _COFINSOutr = null;
            _COFINSST = null;
            _ISSQN = null;
            _Total = null;
            //_DescAcrEntr = null;
            _listaPagamentos = null;
            //_pagamento = null;
            _MP = null;
            _obsFiscoDet = null;
            _listaObsFiscoDet = null;
            _produtoRecebido = false;
            _ICMSrecebido = false;
            _PISrecebido = false;
            _COFINSrecebido = false;
            _ISSQNRecebido = false;
            nItemCupom = 1;
        }

        /// <summary>
        /// Instancia uma nova venda, com parâmetros iniciais (localizados no cabecalho do XML de venda).
        /// </summary>
        /// <param name="cNPJSH">CNPJ da Software House</param>
        /// <param name="assinatura">Assinatura do certificado para o XML</param>
        /// <param name="numeroCaixa">Número do Caixa</param>
        /// <param name="cNPJEmit">CNPJ do Emitente</param>
        /// <param name="iEEmit">Insc. Estadual do Emitente</param>
        /// <param name="iMEmit">Insc. Municipal do Emitente (op.)</param>
        /// <param name="pCRegTribISSQN">Código de Regime Especial de Tributação do ISSQN (op.)</param>
        /// <param name="pIndRatISSQN">Indicador de Rateio do ISSQN (op.)</param>
        public void AbrirNovaVenda(
                string cNPJSH, string assinatura, string numeroCaixa,
                string cNPJEmit, string iEEmit, string iMEmit = null, string pCRegTribISSQN = "1", string pIndRatISSQN = "N"
                )
        {
            if (!(_infCfe is null)) throw new ErroDeValidacaoDeConteudo("XML só pode conter um grupo de informações de cabeçalho");
            if (string.IsNullOrWhiteSpace(cNPJSH) || cNPJSH.Length != 14) throw new ErroDeValidacaoDeConteudo("CNPJ da Software House inválido. Não deve conter pontuações.");
            if (string.IsNullOrWhiteSpace(assinatura)) assinatura = "batata";
            if (string.IsNullOrWhiteSpace(numeroCaixa) || !int.TryParse(numeroCaixa, out int _x) || _x <= 0) throw new ErroDeValidacaoDeConteudo("Número do caixa inválido. Deve ser um número inteiro e maior que zero");
            if (string.IsNullOrWhiteSpace(cNPJEmit) || cNPJEmit.Length != 14) throw new ErroDeValidacaoDeConteudo("CNPJ do Emitente inválido. Não deve conter pontuação.");
            if (string.IsNullOrWhiteSpace(iEEmit)) throw new ErroDeValidacaoDeConteudo("Inscrição Estadual do Emitente Inválida.");
            _infCfe = new envCFeCFeInfCFe() { versaoDadosEnt = LAYOUT_SAT switch { "007" => "0.07", "008" => "0.08", _ => "0.08" } };
            _infCfe.ide = new envCFeCFeInfCFeIde() { CNPJ = cNPJSH, signAC = assinatura, numeroCaixa = numeroCaixa.PadLeft(3, '0') };
            if (string.IsNullOrWhiteSpace(iMEmit))
            {
                _infCfe.emit = new envCFeCFeInfCFeEmit()
                {
                    CNPJ = cNPJEmit,
                    IE = iEEmit.PadRight(12),
                    cRegTribISSQN = pCRegTribISSQN,
                    indRatISSQN = pIndRatISSQN
                };
            }
            else
            {
                _infCfe.emit = new envCFeCFeInfCFeEmit()
                {
                    CNPJ = cNPJEmit,
                    IE = iEEmit.PadRight(12),
                    IM = iMEmit,
                    cRegTribISSQN = pCRegTribISSQN,
                    indRatISSQN = pIndRatISSQN
                };
            }
            _cFe.infCFe = _infCfe;
            _listaDets = new List<envCFeCFeInfCFeDet>();
        }

        /// <summary>
        /// Informa dados referente ao cliente/destinatário.
        /// </summary>
        /// <param name="tipo">Tipo (CPF ou CNPJ)</param>
        /// <param name="texto">Informação</param>
        public void InformaCliente(ItemChoiceType tipo, string texto)
        {
            if (!(_infCfe.dest is null))
                _infCfe.dest = null;
            //throw new ErroDeValidacaoDeConteudo("Cliente só pode ser informado uma vez.");
            switch (tipo)
            {
                case ItemChoiceType.CNPJ:
                case ItemChoiceType.CPF:
                    if (texto == "1111111111111111") texto = "68316887053";
                    _infCfe.dest = new envCFeCFeInfCFeDest()
                    {
                        ItemElementName = tipo,
                        Item = texto != null ? texto.TiraPont() : texto
                    };
                    break;
                default:
                    _infCfe.dest = new envCFeCFeInfCFeDest();
                    break;
            }
            _cFe.infCFe = _infCfe;
        }

        /// <summary>
        /// Recebe informação de um produto a ser vendido.
        /// </summary>
        /// <param name="codProd">Código interno do produto (identificador)</param>
        /// <param name="descricao">Descrição do produto</param>
        /// <param name="NCM">NCM do produto</param>
        /// <param name="CFOP">CFOP do produto</param>
        /// <param name="valorUnit">Valor Unitário de Venda</param>
        /// <param name="CEST">CEST do Produto</param>
        /// <param name="valorOutros">Acréscimos extras</param>
        /// <param name="valorDesconto">Desconto em cima do valor do detalhamento</param>
        /// <param name="uniComercial">Unidade Comercial</param>
        /// <param name="quantidade">Quantidade a ser vendida</param>
        /// <param name="GTIN">Código EAN8 ou EAN13 identificador, de barras</param>
        public void RecebeNovoProduto(int codProd,
                                      string descricao,
                                      string NCM,
                                      string CFOP,
                                      decimal valorUnit,
                                      string CEST,
                                      decimal valorOutros = 0,
                                      decimal valorDesconto = 0,
                                      string uniComercial = "UN",
                                      decimal quantidade = 1,
                                      string GTIN = null,
                                      string familia = null,
                                      bool importadoKit = false,
                                      int? idScannTechh = null)
        {
            _det = new envCFeCFeInfCFeDet();
            _produto = new envCFeCFeInfCFeDetProd
            {
                cProd = codProd.ToString(),
                indRegra = "A"
            };

            if (!string.IsNullOrWhiteSpace(GTIN))
            {
                switch (GTIN.Length)
                {
                    case 8:
                    case 12:
                    case 13:
                    case 14:
                        _produto.cEAN = GTIN;
                        break;
                    default:
                        break;
                }
            }
            if (string.IsNullOrWhiteSpace(descricao))
            {
                throw new ErroDeValidacaoDeConteudo("Descrição inválida. A descrição não pode ser em branco.");
            }
            else
            {
                if (descricao.Length > 120)
                {
                    _produto.xProd = descricao.Substring(0, 120);
                }
                else
                {
                    _produto.xProd = descricao;
                }
            }
            if (!string.IsNullOrWhiteSpace(NCM))
            {
                _produto.NCM = NCM;
            }
            if (CFOP.Length != 4)
            {
                throw new ErroDeValidacaoDeConteudo("CFOP inválido.");
            }
            else
            {
                _produto.CFOP = string.IsNullOrWhiteSpace(CFOP) ? "5102" : CFOP;

                //switch (CFOP)
                //{
                //    case "5101":
                //    case "5102":
                //    case "5103":
                //    case "5405":
                //    case "5655":
                //        _produto.CFOP = CFOP;
                //        break;
                //    default:
                //        throw new ErroDeValidacaoDeConteudo("CFOPs válidos são \"5101, 5102, 5103, 5405, 5655\"");
                //}
            }
            _produto.uCom = uniComercial;
            if (quantidade <= 0)
            {
                throw new ErroDeValidacaoDeConteudo("Quantidade informada inválida. Quantidade deve ser maior que 0");
            }
            else
            {
                _produto.qCom = quantidade.ToString("0.0000");
            }
            if (valorUnit <= 0 && !(COD10PORCENTO == codProd))
            {
                throw new ErroDeValidacaoDeConteudo("Valor unitário inválido. Deve ser maior que zero.");
            }
            else
            {
                _produto.vUnCom = valorUnit.ToString("0.000");
            }
            if (valorDesconto < 0)
            {
                throw new ErroDeValidacaoDeConteudo("Desconto inválido. Desconto deve ser maior que zero.");
            }
            else if (valorDesconto > (quantidade * valorUnit))
            {
                throw new ErroDeValidacaoDeConteudo("Desconto inválido. Desconto não pode ser maior que o preço total do item.");
            }
            else if (true)
            {
                _produto.vDesc = valorDesconto.ToString("0.00");
            }
            if (valorOutros < 0)
            {
                throw new ErroDeValidacaoDeConteudo("Valor de acréscimo inválido. Valor não pode ser menor que zero.");
            }
            else if (true)
            {
                _produto.vOutro = valorOutros.ToString("0.00");
            }
            if (!string.IsNullOrWhiteSpace(CEST))
            {
                _listaObsFiscoDet = new List<envCFeCFeInfCFeDetProdObsFiscoDet>();
                _obsFiscoDet = new envCFeCFeInfCFeDetProdObsFiscoDet() { xCampoDet = "Cod. CEST", xTextoDet = CEST };
                _listaObsFiscoDet.Add(_obsFiscoDet);
                _produto.obsFiscoDet = _listaObsFiscoDet.ToArray();
            }

            _det.idScannTech = idScannTechh;
            _det.kit = importadoKit;
            _det.familia = familia;
            _produtoRecebido = true;
        }


        /// <summary>
        /// Recebe Informações sobre a tarifação ICMS.
        /// </summary>
        /// <param name="tipo">Tipo da empresa (SN - Simples Nacional, ou RPA - Regime Periódico de Apuração)</param>
        /// <param name="cSOSN">CSOSN do produto</param>
        /// <param name="cST">CST do produto</param>
        /// <param name="origem">Origem do produto (0 - Nacional, 1 - Importação Direta, 2 - Importação Terceiros)</param>
        /// <param name="vpICMS">Alguma coisa importante. Só não sei o quê...</param>
        public void RecebeInfoICMS(TipoDeEmpresa tipo,
                                   string cSOSN = null,
                                   string cST = null,
                                   string vpICMS = null,
                                   string vBASE_ICMS = "100")
        {
            if (_ICMSrecebido || _ISSQNRecebido)
            {
                throw new ErroDeValidacaoDeConteudo("ICMS/ISSQN já informado. Impossível informar ISSQN");
            }
            _ICMS = new envCFeCFeInfCFeDetImpostoICMS();
            if (string.IsNullOrWhiteSpace(cST) || cST.Substring(0, 1).Length != 1)
            {
                //throw new ErroDeValidacaoDeConteudo("Código de origem (CST) inválido.");
                cST = "000";
            }
            switch (tipo)
            {
                case TipoDeEmpresa.RPA:
                    if (string.IsNullOrWhiteSpace(cST))
                    {
                        throw new ErroDeValidacaoDeConteudo("CST informado é inválido.");
                    }
                    //_imposto.CST = cST;
                    //_imposto.CSOSN = cSOSN;
                    switch (cST.Substring(1))
                    {
                        case "00":
                        case "20":
                        case "90":
                            if (string.IsNullOrWhiteSpace(vpICMS))
                            {
                                throw new ErroDeValidacaoDeConteudo("Alíquota Efetiva do ICMS (pICMS) inválida");
                            }
                            else
                            {
                                _ICMS00 = new envCFeCFeInfCFeDetImpostoICMSICMS00 { Orig = cST.Substring(0, 1), pICMS = (decimal.Parse(vpICMS) * decimal.Parse(vBASE_ICMS) / 100).ToString("0.00"), CST = cST.Substring(1) };
                                _ICMS.Item = _ICMS00;
                                _ICMSrecebido = true;
                            }
                            break;
                        case "40":
                        case "41":
                        case "60":
                            _ICMS40 = new envCFeCFeInfCFeDetImpostoICMSICMS40 { Orig = cST.Substring(0, 1), CST = cST.Substring(1) };
                            _ICMS.Item = _ICMS40;
                            _ICMSrecebido = true;
                            break;
                        default:
                            _ICMS00 = new envCFeCFeInfCFeDetImpostoICMSICMS00 { Orig = "0", pICMS = (decimal.Parse(vpICMS) * decimal.Parse(vBASE_ICMS) / 100).ToString("0.00"), CST = cST.Substring(1) };
                            _ICMS.Item = _ICMS00;
                            _ICMSrecebido = true;
                            break;
                    }
                    break;
                case TipoDeEmpresa.SN:
                    switch (cSOSN)
                    {
                        case "102":
                        case "300":
                        case "400":
                        case "500":
                            _ICMS102 = new envCFeCFeInfCFeDetImpostoICMSICMSSN102() { Orig = cST.Substring(0, 1), CSOSN = cSOSN };
                            _ICMS.Item = _ICMS102;
                            _ICMSrecebido = true;
                            break;
                        case "900":
                            _ICMS900 = new envCFeCFeInfCFeDetImpostoICMSICMSSN900() { Orig = cST.Substring(0, 1), CSOSN = cSOSN, pICMS = (decimal.Parse(vpICMS) * decimal.Parse(vBASE_ICMS) / 100).ToString("0.00") };
                            _ICMS.Item = _ICMS900;
                            _ICMSrecebido = true;
                            break;
                        default:
                            _ICMS102 = new envCFeCFeInfCFeDetImpostoICMSICMSSN102() { Orig = "0", CSOSN = "102" };
                            _ICMS.Item = _ICMS102;
                            _ICMSrecebido = true;
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Recebe informações do PIS para o item.
        /// </summary>
        /// <param name="cST">CST do produto</param>
        /// <param name="valorBaseCalculo">Valor de Base de Cálculo PIS (preencher também 'aliquotaPorcento')</param>
        /// <param name="aliquotaPorcento">Alíquota porcentual ICMS (preencher também 'valorBaseCalculo')</param>
        /// <param name="qtdVendida">Quantidade Vendida (preencher também 'aliquotaValor')</param>
        /// <param name="aliquotaValor">Valor absoluto da alíquota PIS (preencher também 'qtdVendida')</param>
        public void RecebePIS(string cST = null,
                              decimal valorBaseCalculo = 0,
                              decimal aliquotaPorcento = 0,
                              decimal qtdVendida = 0,
                              decimal aliquotaValor = 0)
        {
            if (_PISrecebido)
            {
                throw new ErroDeValidacaoDeConteudo("PIS não pode ser informado mais de uma vez");
            }

            _PIS = new envCFeCFeInfCFeDetImpostoPIS();
            switch (cST)
            {
                case "05":
                    _PISAliq = new envCFeCFeInfCFeDetImpostoPISPISAliq() { CST = cST, vBC = (valorBaseCalculo * qtdVendida).ToString("0.00"), pPIS = (aliquotaPorcento / 100).ToString("0.0000") };
                    _PIS.Item = _PISAliq;
                    _PISST = new envCFeCFeInfCFeDetImpostoPISST
                    {
                        ItemsElementName = new ItemsChoiceType1[] { ItemsChoiceType1.vBC, ItemsChoiceType1.pPIS },
                        Items = new string[] { valorBaseCalculo.ToString("0.00"), (aliquotaPorcento).ToString("0.0000") }
                    };
                    _PISrecebido = true;
                    break;
                case "01":
                case "02":
                    _PISAliq = new envCFeCFeInfCFeDetImpostoPISPISAliq() { CST = cST, vBC = (valorBaseCalculo * qtdVendida).ToString("0.00"), pPIS = (aliquotaPorcento / 100).ToString("0.0000") };
                    _PIS.Item = _PISAliq;
                    _PISrecebido = true;
                    break;
                case "03":
                    _PISQtde = new envCFeCFeInfCFeDetImpostoPISPISQtde() { CST = cST, vAliqProd = aliquotaValor.ToString("0.0000"), qBCProd = qtdVendida.ToString("0.000") };
                    _PIS.Item = _PISQtde;
                    _PISrecebido = true;
                    break;
                case "04":
                case "06":
                case "07":
                case "08":
                case "09":
                    _PISNT = new envCFeCFeInfCFeDetImpostoPISPISNT() { CST = cST };
                    _PIS.Item = _PISNT;
                    _PISrecebido = true;
                    break;
                case "49":
                    _PISSN = new envCFeCFeInfCFeDetImpostoPISPISSN() { CST = cST };
                    _PIS.Item = _PISSN;
                    _PISrecebido = true;
                    break;
                case "99":
                    _PISOutr = new envCFeCFeInfCFeDetImpostoPISPISOutr
                    {
                        CST = cST
                    };

                    if ((valorBaseCalculo != 0 || aliquotaPorcento != 0) && (aliquotaValor != 0 || qtdVendida != 0))
                    {
                        _PISOutr.ItemsElementName = new ItemsChoiceType[] { ItemsChoiceType.vBC, ItemsChoiceType.pPIS };
                        _PISOutr.Items = new string[] { (valorBaseCalculo * qtdVendida).ToString("0.00"), (aliquotaPorcento / 100).ToString("0.0000") };
                        _PIS.Item = _PISOutr;
                        _PISrecebido = true;
                        //throw new ErroDeValidacaoDeConteudo("PIS Outros deve ser informado apenas com base de cálculo ou com alíquota em valor");
                    }
                    else if (valorBaseCalculo < 0 && aliquotaPorcento < 0)
                    {
                        _PISOutr.ItemsElementName = new ItemsChoiceType[] { ItemsChoiceType.vBC, ItemsChoiceType.pPIS };
                        _PISOutr.Items = new string[] { (valorBaseCalculo * qtdVendida).ToString("0.00"), (aliquotaPorcento / 100).ToString("0.0000") };
                        _PIS.Item = _PISOutr;
                        _PISrecebido = true;
                    }
                    else if (aliquotaValor < 0 && qtdVendida < 0)
                    {
                        _PISOutr.ItemsElementName = new ItemsChoiceType[] { ItemsChoiceType.qBCProd, ItemsChoiceType.vAliqProd };
                        _PISOutr.Items = new string[] { (valorBaseCalculo * qtdVendida).ToString("0.0000"), qtdVendida.ToString("0.000") };
                        _PIS.Item = _PISOutr;
                        _PISrecebido = true;
                    }
                    else
                    {
                        throw new ErroDeValidacaoDeConteudo("PIS Outros incorretamente informado.");
                    }
                    break;
                default:
                    _PISOutr = new envCFeCFeInfCFeDetImpostoPISPISOutr
                    {
                        CST = "99",
                        ItemsElementName = new ItemsChoiceType[] { ItemsChoiceType.vBC, ItemsChoiceType.pPIS },
                        Items = new string[] { 0.ToString("0.00"), 0.ToString("0.0000") }
                    };
                    _PIS.Item = _PISOutr;
                    _PISrecebido = true;
                    break;
            }
        }

        /// <summary>
        /// Recebe informações do COFINS para o item.
        /// </summary>
        /// <param name="cST">CST do produto</param>
        /// <param name="valorBaseCalculo">Valor de Base de Cálculo PIS (preencher também 'aliquotaPorcento')</param>
        /// <param name="aliquotaPorcento">Alíquota porcentual PIS (preencher também 'valorBaseCalculo')</param>
        /// <param name="qtdVendida">Quantidade Vendida (preencher também 'aliquotaValor')</param>
        /// <param name="aliquotaValor">Valor absoluto da alíquota PIS (preencher também 'qtdVendida')</param>
        public void RecebeCOFINS(string cST = null,
                                 decimal valorBaseCalculo = 0,
                                 decimal aliquotaPorcento = 0,
                                 decimal qtdVendida = 0,
                                 decimal aliquotaValor = 0)
        {
            if (_COFINSrecebido)
            {
                throw new ErroDeValidacaoDeConteudo("COFINS não pode ser informado mais de uma vez");
            }

            _COFINS = new envCFeCFeInfCFeDetImpostoCOFINS();
            switch (cST)
            {
                case "05":
                    _COFINSAliq = new envCFeCFeInfCFeDetImpostoCOFINSCOFINSAliq() { CST = cST, vBC = (valorBaseCalculo * qtdVendida).ToString("0.00"), pCOFINS = (aliquotaPorcento / 100).ToString("0.0000") };
                    _COFINS.Item = _COFINSAliq;
                    _COFINSST = new envCFeCFeInfCFeDetImpostoCOFINSST
                    {
                        ItemsElementName = new ItemsChoiceType3[] { ItemsChoiceType3.vBC, ItemsChoiceType3.pCOFINS },
                        Items = new string[] { valorBaseCalculo.ToString("0.00"), (aliquotaPorcento).ToString("0.0000") }
                    };
                    _COFINSrecebido = true;
                    break;
                case "01":
                case "02":
                    _COFINSAliq = new envCFeCFeInfCFeDetImpostoCOFINSCOFINSAliq() { CST = cST, vBC = (valorBaseCalculo * qtdVendida).ToString("0.00"), pCOFINS = (aliquotaPorcento / 100).ToString("0.0000") };
                    _COFINS.Item = _COFINSAliq;
                    _COFINSrecebido = true;
                    break;
                case "03":
                    _COFINSQtde = new envCFeCFeInfCFeDetImpostoCOFINSCOFINSQtde() { CST = cST, vAliqProd = (aliquotaPorcento * valorBaseCalculo / 100).ToString("0.0000"), qBCProd = qtdVendida.ToString("0.000") };
                    _COFINS.Item = _COFINSQtde;
                    _COFINSrecebido = true;
                    break;
                case "04":
                case "06":
                case "07":
                case "08":
                case "09":
                    _COFINSNT = new envCFeCFeInfCFeDetImpostoCOFINSCOFINSNT() { CST = cST };
                    _COFINS.Item = _COFINSNT;
                    _COFINSrecebido = true;
                    break;
                case "49":
                    _COFINSSN = new envCFeCFeInfCFeDetImpostoCOFINSCOFINSSN() { CST = cST };
                    _COFINS.Item = _COFINSSN;
                    _COFINSrecebido = true;
                    break;
                case "99":
                    _COFINSOutr = new envCFeCFeInfCFeDetImpostoCOFINSCOFINSOutr
                    {
                        CST = cST
                    };

                    if ((valorBaseCalculo != 0 || aliquotaPorcento != 0) && (aliquotaValor != 0 || qtdVendida != 0))
                    {
                        _COFINSOutr.ItemsElementName = new ItemsChoiceType2[] { ItemsChoiceType2.vBC, ItemsChoiceType2.pCOFINS };
                        _COFINSOutr.Items = new string[] { (valorBaseCalculo * qtdVendida).ToString("0.00"), (aliquotaPorcento / 100).ToString("0.0000") };
                        _COFINS.Item = _COFINSOutr;
                        _COFINSrecebido = true;
                        //throw new ErroDeValidacaoDeConteudo("COFINS Outros deve ser informado apenas com base de cálculo ou com alíquota em valor");
                    }
                    else if (valorBaseCalculo < 0 && aliquotaPorcento < 0)
                    {
                        _COFINSOutr.ItemsElementName = new ItemsChoiceType2[] { ItemsChoiceType2.vBC, ItemsChoiceType2.pCOFINS };
                        _COFINSOutr.Items = new string[] { (valorBaseCalculo * qtdVendida).ToString("0.00"), (aliquotaPorcento / 100).ToString("0.0000") };
                        _COFINS.Item = _COFINSOutr;
                        _COFINSrecebido = true;
                    }
                    else if (aliquotaValor < 0 && qtdVendida < 0)
                    {
                        _COFINSOutr.ItemsElementName = new ItemsChoiceType2[] { ItemsChoiceType2.qBCProd, ItemsChoiceType2.vAliqProd };
                        _COFINSOutr.Items = new string[] { (valorBaseCalculo * qtdVendida).ToString("0.0000"), qtdVendida.ToString("0.000") };
                        _COFINS.Item = _COFINSOutr;
                        _COFINSrecebido = true;
                    }
                    else
                    {
                        throw new ErroDeValidacaoDeConteudo("COFINS Outros incorretamente informado.");
                    }
                    break;
                default:
                    _COFINSOutr = new envCFeCFeInfCFeDetImpostoCOFINSCOFINSOutr
                    {
                        CST = "99",
                        ItemsElementName = new ItemsChoiceType2[] { ItemsChoiceType2.vBC, ItemsChoiceType2.pCOFINS },
                        Items = new string[] { 0.ToString("0.00"), 0.ToString("0.0000") }
                    };
                    _COFINS.Item = _COFINSOutr;
                    _COFINSrecebido = true;
                    break;
            }
        }

        /// <summary>
        /// Recebe informações sobre impostos sobre serviços.
        /// </summary>
        /// <param name="valorDeducaoISSQN"></param>
        /// <param name="valorAliquota"></param>
        /// <param name="codMunicipio"></param>
        /// <param name="codListaServ"></param>
        /// <param name="codTribISSQN"></param>
        /// <param name="codNatOp"></param>
        /// <param name="incentivoFiscal"></param>
        public void RecebeInfoISSQN(decimal valorAliquota = 0,
                                    decimal valorDeducaoISSQN = 0,
                                    string codMunicipio = null,
                                    string codListaServ = null,
                                    string codTribISSQN = null,
                                    string codNatOp = null,
                                    string incentivoFiscal = "2")
        {
            if (_ICMSrecebido || _ISSQNRecebido)
            {
                throw new ErroDeValidacaoDeConteudo("ICMS/ISSQN já informado. Impossível informar ISSQN");
            }
            //if (!string.IsNullOrWhiteSpace(codMunicipio) && codMunicipio.Length != 7)
            //{
            //    throw new ErroDeValidacaoDeConteudo("Código de Município Inválido.");
            //}

            //if (!string.IsNullOrWhiteSpace(codListaServ) && codListaServ.Length != 5)
            //{
            //    throw new ErroDeValidacaoDeConteudo("Código LC 116/03 Inválido.");
            //}

            //if (!string.IsNullOrWhiteSpace(codTribISSQN) && codTribISSQN.Length != 20)
            //{
            //    throw new ErroDeValidacaoDeConteudo("Código do Serviço Prestado Inválido.");
            //}

            //if (!string.IsNullOrWhiteSpace(codNatOp) && codNatOp.Length != 2)
            //{
            //    throw new ErroDeValidacaoDeConteudo("Código da Natureza de Operação Inválido.");
            //}

            //if (incentivoFiscal != "1" && incentivoFiscal != "0")
            //{
            //    throw new ErroDeValidacaoDeConteudo("Indicador de Incentivo Fiscal Inválido.");
            //}

            _ISSQN = new envCFeCFeInfCFeDetImpostoISSQN()
            {
                vDeducISSQN = 0.ToString("0.00"),
                vAliq = (2.1).ToString("000.00"),
                cNatOp = "01",
                indIncFisc = "2"
                //cMunFG = codMunicipio,
                //cListServ = codListaServ,
            };
            _ISSQNRecebido = true;
        }

        public List<ValidationFailure> SetClienteDuepay(ClienteDuePayDTO cliente)
        {
            if (cliente is not null)
            {
                ValidationResult result = cliente.Validador.Validate(cliente);
                if (result.IsValid)
                {
                    _clienteDuepay = cliente;
                    return null;
                }
                return result.Errors;
            }
            return null;
        }
        public ClienteDuePayDTO GetClienteDuepay()
        {
            return _clienteDuepay;
        }
        /// <summary>
        /// Adiciona o produto com todas as informações tributárias que foram preenchidas.
        /// </summary>
        /// <param name="cST">CST do produto</param>
        /// <returns></returns>
        public bool AdicionaProduto(string cST)
        {
            if (!_produtoRecebido) throw new ErroDeValidacaoDeConteudo("Informações do produto estão faltando.");
            if (!(_ICMSrecebido || _ISSQNRecebido)) throw new ErroDeValidacaoDeConteudo("Informações do ICMS/ISSQN estão faltando.");
            if (_ICMSrecebido && !_PISrecebido) throw new ErroDeValidacaoDeConteudo("Informações do PIS estão faltando.");
            if (_ICMSrecebido && !_COFINSrecebido) throw new ErroDeValidacaoDeConteudo("Informações do COFINS estão faltando.");
            _det.nItem = nItemCupom.ToString();
            nItemCupom++;
            _det.prod = _produto;
            _imposto = new envCFeCFeInfCFeDetImposto();
            if (_ICMSrecebido)
            {
                //_imposto.CST = cST;
                _imposto.Item = _ICMS;
                _imposto.PIS = _PIS;
                _imposto.COFINS = _COFINS;
                if (!(_PISST is null))
                    _imposto.PISST = _PISST;
                if (!(_COFINSST is null))
                    _imposto.COFINSST = _COFINSST;
                _det.imposto = _imposto;
            }
            else if (_ISSQNRecebido)
            {
                //_imposto.CST = cST;
                _imposto.Item = _ISSQN;
                _imposto.PIS = _PIS;
                _imposto.COFINS = _COFINS;
                if (!(_PISST is null))
                    _imposto.PISST = _PISST;
                if (!(_COFINSST is null))
                    _imposto.COFINSST = _COFINSST;
                _det.imposto = _imposto;
            }
            _det.prod.vUnComOri = _det.prod.vUnCom;
            _listaDets.Add(_det);
            _produtoRecebido = _ICMSrecebido = _PISrecebido = _COFINSrecebido = _ISSQNRecebido = false;
            return true;
        }

        /// <summary>
        /// Remove um produto da venda atual.
        /// </summary>
        /// <param name="numItem">Número sequencial referente ao produto a ser retirado</param>
        /// <returns></returns>
        public List<envCFeCFeInfCFeDetProd> RemoveProduto(int numItemINT = 0, string numItemSTRING = null, int qtdDevolver = 0)
        {
            try
            {
                if (numItemINT != 0)
                {
                    List<envCFeCFeInfCFeDet> listCanc = _listaDets.FindAll(x => x.nItem == numItemINT.ToString());
                    List<envCFeCFeInfCFeDetProd> listProdCanc = new List<envCFeCFeInfCFeDetProd>();
                    foreach (var list in listCanc)
                    {
                        if (list is null) return null;
                        listProdCanc.Add(list.prod);
                        _listaDets.Remove(list);
                        //for (int i = 0; i < _listaDets.Count; i++)
                        //{
                        //    _listaDets[i].nItem = (i + 1).ToString();
                        //}                    
                    }
                    return listProdCanc.Count == 0 ? null : listProdCanc;
                }
                if (numItemSTRING != null)
                {
                    List<CfeRecepcao_0008.envCFeCFeInfCFeDet> listCanc = _listaDets.FindAll(s => s.prod.cEAN == numItemSTRING);
                    List<envCFeCFeInfCFeDetProd> listProdCanc = new List<envCFeCFeInfCFeDetProd>();
                    int iteracao = 0;
                    foreach (var list in listCanc)
                    {
                        if (list is null) return null;
                        listProdCanc.Add(list.prod);
                        _listaDets.Remove(list);
                        iteracao++;
                        if (iteracao == qtdDevolver) break;
                    }
                    return listProdCanc.Count == 0 ? null : listProdCanc;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao tentar estornar item.\n\nSe o problema persistir entre em contato com o suporte.", "Estorno de item", MessageBoxButton.OK, MessageBoxImage.Error);
                log.Debug("Erro ao tentar estornar item, segue erro: " + ex);
                return null;
            }
        }
        /// <summary>
        /// Recebe informações sobre um método de pagamento.
        /// </summary>
        /// <param name="codigoMetodo">Código CFE do método de pagamento</param>
        /// <param name="valorMetodo">Valor do método de pagamento</param>
        public void RecebePagamento(string codigoMetodo, decimal valorMetodo, InfoAdministradora infoAdmins, decimal valorTroco = 0)
        {
            if (_listaPagamentos is null) _listaPagamentos = new List<envCFeCFeInfCFePgtoMP>();
            if (!_listademetodos.Contains(codigoMetodo)) throw new ErroDeValidacaoDeConteudo("Código do Método de Pagamento informado inválido.");
            _MP = new envCFeCFeInfCFePgtoMP() { cMP = codigoMetodo.PadLeft(2, '0'), vMP = valorMetodo.ToString("0.00"), InfoAdmin = infoAdmins };
            _listaPagamentos.Add(_MP);
            _valTroco = valorTroco;
        }

        /// <summary>
        /// Recebe informações sobre um método de pagamento a prazo com validade.
        /// </summary>
        /// <param name="codigoMetodo">Código CFE do método de pagamento</param>
        /// <param name="valorMetodo">Valor do método de pagamento</param>
        /// <param name="vencimento">Vencimento da forma de pagamento</param>
        /// <param name="id_cliente">ID do cliente</param>
        public void RecebePagamento(string codigoMetodo, decimal valorMetodo, InfoAdministradora infoAdmin, DateTime vencimento, int id_cliente)
        {
            if (_listaPagamentos is null) _listaPagamentos = new List<envCFeCFeInfCFePgtoMP>();
            if (!_listademetodos.Contains(codigoMetodo)) throw new ErroDeValidacaoDeConteudo("Código do Método de Pagamento informado inválido.");
            _MP = new envCFeCFeInfCFePgtoMP() { cMP = codigoMetodo.PadLeft(2, '0'), vMP = valorMetodo.ToString("0.00"), idCliente = id_cliente, vencimento = vencimento, InfoAdmin = infoAdmin };
            imprimeViaAssinar = true;
            _listaPagamentos.Add(_MP);
        }

        /// <summary>
        /// Recebe informações sobre acréscimo ou desconto. Valores negativos indicam desconto, enquanto valores positivos indicam acréscimo.
        /// </summary>
        /// <param name="porcentagem">Valor porcentual do desconto/acréscimo</param>
        /// <param name="ajusteAbsoluto">Valor absoluto do desconto/acréscimo</param>
        public void RecebeAjuste(decimal porcentagem = 0, decimal ajusteAbsoluto = 0)
        {


            if ((ajusteAbsoluto != 0) && (porcentagem != 0))
            {
                throw new ErroDeValidacaoDeConteudo("Desconto absoluto e relativo ambos informados.");
            }
            if (ajusteAbsoluto > 0)
            {
                _Total = new envCFeCFeInfCFeTotal() { DescAcrEntr = new envCFeCFeInfCFeTotalDescAcrEntr() { ItemElementName = ItemChoiceType1.vAcresSubtot, Item = ajusteAbsoluto.ToString("0.00", CultureInfo.InvariantCulture) } };

            }
            if (ajusteAbsoluto < 0)
            {
                _Total = new envCFeCFeInfCFeTotal() { DescAcrEntr = new envCFeCFeInfCFeTotalDescAcrEntr() { ItemElementName = ItemChoiceType1.vDescSubtot, Item = (-ajusteAbsoluto).ToString("0.00", CultureInfo.InvariantCulture) } };
            }
        }

        /// <summary>
        /// Desfaz/apaga quaisquer informações sobre descontos/acréscimos sobre a venda atual.
        /// </summary>
        public void LimpaAjuste()
        {
            _Total = null;
        }

        /// <summary>
        /// Preenche os campos dos métodos de pagamento no XML de venda.
        /// </summary>
        /// <param name="troco"></param>
        public bool TotalizaCupom()
        {
            if (_listaDets is null || _listaDets.Count < 1)
            {
                return false;
            }
            #region Re-enumera os produtos para uma ordem sequencial ininterrupta.
            for (int i = 0; i < _listaDets.Count; i++)
            {
                _listaDets[i].nItem = (i + 1).ToString();
            }
            #endregion

            if (_infCfe.det is null)
                _infCfe.det = _listaDets.ToArray();
            if (_Total is null)
            {
                _infCfe.total = new envCFeCFeInfCFeTotal();
            }
            else
            {
                _infCfe.total = _Total;
            }
            if (_infCfe.pgto is null && !(_listaPagamentos is null))
                _infCfe.pgto = new envCFeCFeInfCFePgto() { MP = _listaPagamentos.ToArray(), dTroco = _valTroco };
            return true;
        }
        Logger log = new Logger("Venda");
        /// <summary>
        /// Grava o XML de retorno do SAT informado na base de dados.
        /// </summary>
        /// <param name="cfeDeRetorno">XML retornado</param>
        /// <param name="noCaixa">Série do Cupom Fiscal</param>
        /// <param name="idVendedor">ID do Vendedor que efetuou a venda</param>
        /// <returns></returns>
        public int GravaVendaNaBase(int noCaixa, short idVendedor, int nCFe = -1, int dav = 0)
        {
            if (_cFeRetornado is null)
            {
                throw new ErroDeValidacaoDeConteudo("CFe retornado pelo SAT era inválido");
            }
            CFe cfeDeRetorno = _cFeRetornado;
            int ID_NFVENDA, ID_SAT, nItemCup, NF_NUMERO, ID_CLIENTE = 0;
            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            using (var OPER_TA = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter())
            using (var VENDA_TA = new DataSets.FDBDataSetVendaTableAdapters.SP_TRI_GRAVANFVENDATableAdapter())
            {
                FuncoesFirebird remendo = new();
                OPER_TA.Connection = LOCAL_FB_CONN;
                VENDA_TA.Connection = LOCAL_FB_CONN;
                try
                {
                    if (DateTime.TryParseExact(cfeDeRetorno.infCFe.ide.dEmi + cfeDeRetorno.infCFe.ide.hEmi, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime tsEmissao))
                    {
                        tsEmissao = DateTime.Now;
                    }
                    if (!decimal.TryParse(cfeDeRetorno.infCFe.pgto.vTroco, NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal vTroco))
                    {
                        throw new Exception("vTroco não pode ser obtido do XML de retorno");
                    }
                    if (!decimal.TryParse(cfeDeRetorno.infCFe.total.vCFe, NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal vCFE))
                    {
                        throw new Exception("vCFE não pode ser obtido do XML de retorno");
                    }
                    if ((!(cfeDeRetorno.infCFe.total.DescAcrEntr is null)) && cfeDeRetorno.infCFe.total.DescAcrEntr.ItemElementName is ItemChoiceType1.vDescSubtot)
                    {
                        if (!decimal.TryParse(cfeDeRetorno.infCFe.total.DescAcrEntr.Item, NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal vDesc))
                            throw new Exception("vDesc não pode ser obtido do XML de retorno");
                    }
                    DataSets.FDBDataSetVenda.SP_TRI_GRAVANFVENDARow nFVendaRow = VENDA_TA.SP_TRI_GRAVANFVENDA(idVendedor, noCaixa.ToString(), tsEmissao, tsEmissao, 2, vTroco, nCFe, dav > 0 ? $"DAV Nº {dav}" : null)[0];
                    ID_NFVENDA = nFVendaRow.RID_NFVENDA;
                    NF_NUMERO = nFVendaRow.RNF_NUMERO;
                    log.Debug($"ID_NFVENDA = (int)OPER_TA.SP_TRI_GRAVANFVENDA(0, \"1\", {tsEmissao}, {tsEmissao}, 2, {vTroco});");
                    ID_SAT = (int)OPER_TA.SP_TRI_GRAVASAT(ID_NFVENDA, cfeDeRetorno.infCFe.Id.Substring(3), int.Parse(cfeDeRetorno.infCFe.ide.nCFe), cfeDeRetorno.infCFe.ide.nserieSAT);
                }
                catch (Exception ex)
                {
                    log.Error("Erro durante a gravação na base", ex);
                    MessageBox.Show("Erro ao gravar cupom de venda. \nSe o problema persistir, entre em contato com a equipe de suporte.");
                    return -1;
                }

                foreach (envCFeCFeInfCFeDet detalhamento in cfeDeRetorno.infCFe.det)
                {
                    int ID_NFV_ITEM;
                    nItemCup = int.Parse(detalhamento.nItem);
                    try
                    {
                        string pCSOSN = null;
                        if (detalhamento.imposto.Item is envCFeCFeInfCFeDetImpostoICMS)
                        {
                            envCFeCFeInfCFeDetImpostoICMS iCMS = (envCFeCFeInfCFeDetImpostoICMS)detalhamento.imposto.Item;
                            if (iCMS.Item is envCFeCFeInfCFeDetImpostoICMSICMSSN102)
                            {
                                pCSOSN = ((envCFeCFeInfCFeDetImpostoICMSICMSSN102)iCMS.Item).CSOSN;
                            }
                            else if (iCMS.Item is envCFeCFeInfCFeDetImpostoICMSICMSSN900)
                            {
                                pCSOSN = ((envCFeCFeInfCFeDetImpostoICMSICMSSN900)iCMS.Item).CSOSN;
                            }
                        }
                        else
                        {
                            pCSOSN = null;
                        }

                        ID_NFV_ITEM = (int)OPER_TA.SP_TRI_GRAVANFVITEM(ID_NFVENDA,
                            Convert.ToInt32(detalhamento.prod.cProd),
                            Convert.ToInt16(detalhamento.nItem),
                            Convert.ToDecimal(detalhamento.prod.qCom, CultureInfo.InvariantCulture),
                            Convert.ToDecimal(detalhamento.prod.vDesc, CultureInfo.InvariantCulture) + Convert.ToDecimal(detalhamento.prod.vRatDesc, CultureInfo.InvariantCulture),
                            pCSOSN, 0, 0, Convert.ToDecimal(detalhamento.prod.vUnCom, CultureInfo.InvariantCulture),
                            detalhamento.idScannTech,
                            Convert.ToDecimal(detalhamento.prod.vOutro, CultureInfo.InvariantCulture));

                        if (detalhamento.imposto.Item is envCFeCFeInfCFeDetImpostoICMS)
                        {
                            envCFeCFeInfCFeDetImpostoICMS iCMS = (envCFeCFeInfCFeDetImpostoICMS)detalhamento.imposto.Item;
                            using var TB_NFV_ITEM_ICMS = new DataSets.FDBDataSetVendaTableAdapters.TB_NFV_ITEM_ICMSTableAdapter { Connection = LOCAL_FB_CONN };
                            if (iCMS.Item is envCFeCFeInfCFeDetImpostoICMSICMSSN102 ICMSSN102) //SIMPLES NACIONAL = CSOSN 102, 300, 400, 500 E OUTROS
                            {
                                TB_NFV_ITEM_ICMS.Insert(ID_NFV_ITEM, 0, 0, "000", 0, 0);
                            }
                            else if (iCMS.Item is envCFeCFeInfCFeDetImpostoICMSICMSSN900 ICMSSN900) //SIMPLES NACIONAL = CSOSN 900
                            {
                                TB_NFV_ITEM_ICMS.Insert(ID_NFV_ITEM, decimal.Parse(ICMSSN900.vICMS, CultureInfo.InvariantCulture), decimal.Parse(ICMSSN900.pICMS, CultureInfo.InvariantCulture), "000", decimal.Parse(ICMSSN900.pICMS, CultureInfo.InvariantCulture), decimal.Parse(ICMSSN900.vICMS, CultureInfo.InvariantCulture));
                            }
                            else if (iCMS.Item is envCFeCFeInfCFeDetImpostoICMSICMS40 ICMS40) //REGIME NORMAL = CST 40, 41 E 60
                            {
                                TB_NFV_ITEM_ICMS.Insert(ID_NFV_ITEM, 0, 0, ICMS40.Orig + ICMS40.CST, 0, 0);
                            }
                            else if (iCMS.Item is envCFeCFeInfCFeDetImpostoICMSICMS00 ICMS00) //REGIME NORMAL = CST 00, 20 E 90
                            {
                                if (ICMS00.CST is "20") //Redução BC
                                {
                                    int.TryParse(detalhamento.prod.cProd, out int codProd);


                                    using var TaxaProd = new DataSets.FDBDataSetOperSeedTableAdapters.TB_ESTOQUETableAdapter { Connection = LOCAL_FB_CONN };
                                    using var AliqTaxa = new DataSets.FDBDataSetOperSeedTableAdapters.TB_TAXA_UFTableAdapter { Connection = LOCAL_FB_CONN };

                                    var taxa = TaxaProd.TaxaPorID(codProd);
                                    if (taxa is null) taxa = "III";

                                    decimal ALIQ_ICMS = Convert.ToDecimal(AliqTaxa.AliqPorID(taxa.ToString()), CultureInfo.InvariantCulture);
                                    decimal POR_BC_ICMS = Convert.ToDecimal(AliqTaxa.BCPorID(taxa.ToString()), CultureInfo.InvariantCulture);

                                    decimal.TryParse(detalhamento.prod.vProd.Replace('.', ','), out decimal vProd);
                                    decimal VLR_BC_ICMS = Math.Round(POR_BC_ICMS / 100 * vProd, 2);

                                    TB_NFV_ITEM_ICMS.Insert(ID_NFV_ITEM, VLR_BC_ICMS, POR_BC_ICMS, ICMS00.Orig + ICMS00.CST, ALIQ_ICMS, decimal.Parse(ICMS00.vICMS, CultureInfo.InvariantCulture));
                                }
                                else //Cobrado integralmente
                                {
                                    TB_NFV_ITEM_ICMS.Insert(ID_NFV_ITEM, decimal.Parse(detalhamento.prod.vItem, CultureInfo.InvariantCulture), 100, ICMS00.Orig + ICMS00.CST, decimal.Parse(ICMS00.pICMS, CultureInfo.InvariantCulture), decimal.Parse(ICMS00.vICMS, CultureInfo.InvariantCulture)); ;
                                }
                            }
                        }
                        else if (detalhamento.imposto.Item is envCFeCFeInfCFeDetImpostoISSQN)
                        {

                        }

                        if (detalhamento.imposto.COFINS is not null)
                        {
                            using var TB_NFV_ITEM_COFINS = new DataSets.FDBDataSetVendaTableAdapters.TB_NFV_ITEM_COFINSTableAdapter()
                            {
                                Connection = LOCAL_FB_CONN
                            };
                            if (detalhamento.imposto.COFINS.Item is envCFeCFeInfCFeDetImpostoCOFINSCOFINSAliq
                                COFINSAliq) //CST 01, 02 e 05
                            {
                                TB_NFV_ITEM_COFINS.Insert(ID_NFV_ITEM, 100, COFINSAliq.CST, decimal.Parse(COFINSAliq.pCOFINS, CultureInfo.InvariantCulture) * 100, decimal.Parse(COFINSAliq.vCOFINS, CultureInfo.InvariantCulture), decimal.Parse(COFINSAliq.vBC, CultureInfo.InvariantCulture));
                            }
                            if (detalhamento.imposto.COFINS.Item is envCFeCFeInfCFeDetImpostoCOFINSCOFINSNT
                                COFINSNT) //CST 04, 06, 07, 08 e 09
                            {
                                TB_NFV_ITEM_COFINS.Insert(ID_NFV_ITEM, 0, COFINSNT.CST, 0, 0, 0);
                            }
                            if (detalhamento.imposto.COFINS.Item is envCFeCFeInfCFeDetImpostoCOFINSCOFINSOutr
                                COFINSOutr) //CST 99
                            {
                                TB_NFV_ITEM_COFINS.Insert(ID_NFV_ITEM, 100, COFINSOutr.CST, decimal.Parse(COFINSOutr.Items[1], CultureInfo.InvariantCulture) * 100, decimal.Parse(COFINSOutr.vCOFINS, CultureInfo.InvariantCulture), decimal.Parse(COFINSOutr.Items[0], CultureInfo.InvariantCulture));
                            }
                            if (detalhamento.imposto.COFINS.Item is envCFeCFeInfCFeDetImpostoCOFINSCOFINSQtde
                                COFINSQtde)
                            {

                            }
                            if (detalhamento.imposto.COFINS.Item is envCFeCFeInfCFeDetImpostoCOFINSCOFINSSN
                                COFINSSN) //CST 49
                            {
                                TB_NFV_ITEM_COFINS.Insert(ID_NFV_ITEM, 0, COFINSSN.CST, 0, 0, 0);
                            }
                        }

                        if (detalhamento.imposto.PIS is not null)
                        {
                            using var TB_NFV_ITEM_PIS = new DataSets.FDBDataSetVendaTableAdapters.TB_NFV_ITEM_PISTableAdapter()
                            {
                                Connection = LOCAL_FB_CONN
                            };
                            if (detalhamento.imposto.PIS.Item is envCFeCFeInfCFeDetImpostoPISPISAliq
                                PISAliq) //CST 01, 02 E 05
                            {
                                TB_NFV_ITEM_PIS.Insert(ID_NFV_ITEM, PISAliq.CST, 100, decimal.Parse(PISAliq.pPIS, CultureInfo.InvariantCulture) * 100, decimal.Parse(PISAliq.vPIS, CultureInfo.InvariantCulture), decimal.Parse(PISAliq.vBC, CultureInfo.InvariantCulture));
                            }
                            if (detalhamento.imposto.PIS.Item is envCFeCFeInfCFeDetImpostoPISPISNT
                                PISNT) //CST 04, 06, 07, 08 E 09
                            {
                                TB_NFV_ITEM_PIS.Insert(ID_NFV_ITEM, PISNT.CST, 0, 0, 0, 0);
                            }
                            if (detalhamento.imposto.PIS.Item is envCFeCFeInfCFeDetImpostoPISPISOutr
                                PISOutr) //CST 99
                            {
                                TB_NFV_ITEM_PIS.Insert(ID_NFV_ITEM, PISOutr.CST, 100, decimal.Parse(PISOutr.Items[1], CultureInfo.InvariantCulture) * 100, decimal.Parse(PISOutr.vPIS, CultureInfo.InvariantCulture), decimal.Parse(PISOutr.Items[0], CultureInfo.InvariantCulture));
                            }
                            if (detalhamento.imposto.PIS.Item is envCFeCFeInfCFeDetImpostoPISPISQtde
                                PISQtde)
                            {

                            }
                            if (detalhamento.imposto.PIS.Item is envCFeCFeInfCFeDetImpostoPISPISSN
                                PISSN) //CST 49
                            {
                                TB_NFV_ITEM_PIS.Insert(ID_NFV_ITEM, PISSN.CST, 0, 0, 0, 0);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        log.Error("Erro ao gravar a venda na base", ex);
                        DialogBox.Show(strings.VENDA,
                                       DialogBoxButtons.No,
                                       DialogBoxIcons.Error, true,
                                       strings.ERRO_DURANTE_VENDA,
                                       RetornarMensagemErro(ex, false));
                        return -1;
                    }
                    if (nItemCup <= 0)
                    {
                        throw new Exception("O ID de retorno do item de cupom é menor ou igual a zero: " + nItemCup.ToString());
                    }
                }

                foreach (var x in cfeDeRetorno.infCFe.pgto.MP.Select(selector: (pagamento, index) => new { pagamento, index }))
                {
                    int ID_NUMPAG;
                    try
                    {
                        using var CONTAREC_TA = new FDBDataSetTableAdapters.TB_CONTA_RECEBERTableAdapter();
                        using var TB_NFV_FMAPAGTO_TA = new DataSets.FDBDataSetVendaTableAdapters.TB_NFVENDA_FMAPAGTO_NFCETableAdapter();
                        using var TB_NFVENDA_FMAPAGTO = new DataSets.FDBDataSetVendaTableAdapters.TB_FORMA_PAGTO_NFCETableAdapter();
                        TB_NFV_FMAPAGTO_TA.Connection = CONTAREC_TA.Connection = TB_NFVENDA_FMAPAGTO.Connection = LOCAL_FB_CONN;
                        int idNfce = (short)TB_NFVENDA_FMAPAGTO.GetDataByIdNFCE(x.pagamento.cMP)[0]["ID_FMANFCE"];
                        InfoAdministradora infoAdmin = RetornaCFe().infCFe.pgto.MP[x.index].InfoAdmin; // Pegando InfoAdmin do objeto de venda em memoria pois o retornado pelo SAT não possui essa informação.

                        ID_NUMPAG = idNfce switch
                        {
                            1 => (int)TB_NFV_FMAPAGTO_TA.SP_TRI_NFVFMAPGTO_INSERT(decimal.Parse(x.pagamento.vMP, CultureInfo.InvariantCulture) - decimal.Parse(cfeDeRetorno.infCFe.pgto.vTroco, CultureInfo.InvariantCulture), ID_NFVENDA, idNfce, 2, infoAdmin?.IdAdmin == 0 ? null : infoAdmin?.IdAdmin),
                            3 => (int)TB_NFV_FMAPAGTO_TA.SP_TRI_NFVFMAPGTO_INSERT(decimal.Parse(x.pagamento.vMP, CultureInfo.InvariantCulture), ID_NFVENDA, idNfce, 3, infoAdmin?.IdAdmin == 0 ? null : infoAdmin?.IdAdmin),
                            _ => (int)TB_NFV_FMAPAGTO_TA.SP_TRI_NFVFMAPGTO_INSERT(decimal.Parse(x.pagamento.vMP, CultureInfo.InvariantCulture), ID_NFVENDA, idNfce, 2, infoAdmin?.IdAdmin == 0 ? null : infoAdmin?.IdAdmin)
                        };
                       
                        if ((x.pagamento.cMP == "03" || x.pagamento.cMP == "04") && INFORMA_MAQUININHA && VINCULA_MAQ_CTA && infoAdmin != null)
                        {
                            // Para de gerar contas a receber para cartões                             

                            int resultIdConta = 0;
                            int resultIdMovto = 0;
                            string strMensagemLog = "";
                            (string metodo, decimal valorDescontadoTaxa) = x.pagamento.cMP switch
                            {
                                "03" => ("Crédito", x.pagamento.vMP.Safedecimal() - (infoAdmin.TaxaCredito / 100 * x.pagamento.vMP.Safedecimal()).RoundABNT()),
                                "04" => ("Débito", x.pagamento.vMP.Safedecimal() - (infoAdmin.TaxaDebito / 100 * x.pagamento.vMP.Safedecimal()).RoundABNT()),
                                _ => ("(Pagamento não identificado)", 0)
                            };

                            try
                            {
                                log.Debug("Criando conta a receber para cartão...");

                                DateTime vencimento = DateTime.Now.AddDays(infoAdmin.DiasParaVencimento);

                                strMensagemLog = String.Format("SP_TRI_LANCACONTAREC(IdNfVenda: {0}, Vecimento: {1}, Valor: {2}, IdCliente: {3}, Descricao: {4}, NumCaixa: {5}, IdConta: {6}, IdNumPag: {7})",
                                                               ID_NFVENDA,
                                                               vencimento,
                                                               valorDescontadoTaxa,
                                                               infoAdmin.IdCliente == 0 ? null : infoAdmin.IdCliente,
                                                               $"Venda POS cartão de {metodo} AmbiPDV {DateTime.Now.ToString("HH:mm")}".ToUpper(),
                                                               (short)NO_CAIXA,
                                                               infoAdmin.IdConta == 0 ? null : infoAdmin.IdConta,
                                                               ID_NUMPAG);

                                resultIdConta = (int)CONTAREC_TA.SP_TRI_LANCACONTAREC(ID_NFVENDA,
                                                                                      vencimento,
                                                                                      valorDescontadoTaxa,
                                                                                      infoAdmin.IdCliente,
                                                                                      $"Venda POS cartão de {metodo} AmbiPDV {DateTime.Now.ToString("HH:mm")}".ToUpper(),
                                                                                      (short)NO_CAIXA,
                                                                                      infoAdmin.IdConta == 0 ? null : infoAdmin.IdConta,
                                                                                      ID_NUMPAG);
                            }
                            catch (Exception ex)
                            {
                                log.Error($"Erro ao gravar conta a receber de cartões: {ex.InnerException.Message ?? ex.Message}");
                                log.Error($"Procedure utilizada: {strMensagemLog}");
                                MessageBox.Show("Erro ao gravar conta a receber do cartão utilizado na venda.\n\n" +
                                                "Nenhuma conta foi criada mas o fluxo de finalização da venda prosseguirá, por favor entre em contato com a equipe de suporte.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                                goto EndFluxCtaRecCartao;
                            }

                            if (resultIdConta <= 0)
                            {
                                // Erro grave que detona com o fluxo da venda.
                                // Pelo menos deixar algumas dicas...

                                log.Error(string.Format("Erro na: {0}\nRetorno: {1}", strMensagemLog, resultIdConta));
                                MessageBox.Show("Erro ao gravar conta a receber do cartão utilizado na venda.\n\n" +
                                                "Nenhuma conta foi criada mas o fluxo de finalização da venda prosseguirá, por favor entre em contato com a equipe de suporte.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                goto EndFluxCtaRecCartao;
                            }

                            try
                            {
                                log.Debug("Conta a receber criada com sucesso.");
                                log.Debug("Lançando na movimento diario...");

                                strMensagemLog = String.Format("SP_TRI_LANCAMOVDIARIO(NUMCAIXA: {0}, VALOR: {1}, DESCRICAO_MOV: {2}, CONTA_ORIGEM: {3}, CONTA_DESTINO: {4})",
                                                               NO_CAIXA,
                                                               valorDescontadoTaxa,
                                                               $"VENDA NO CARTÃO DE {metodo.ToUpper()} AMBIPDV - CUPOM {NF_NUMERO} {DateTime.Now.ToString("HH:mm")}",
                                                               22, 30);


                                resultIdMovto = (int)OPER_TA.SP_TRI_LANCAMOVDIARIO(NO_CAIXA.ToString(),
                                                                                   valorDescontadoTaxa,
                                                                                   $"VENDA NO CARTÃO DE {metodo.ToUpper()} AMBIPDV - CUPOM {NF_NUMERO} {DateTime.Now.ToString("HH:mm")}",
                                                                                   22, 30);
                            }
                            catch (Exception ex)
                            {
                                log.Error($"Erro ao gravar movimento diario referente a conta receber: {ex.InnerException.Message ?? ex.Message}");
                                log.Error($"Procedure utilizada: {strMensagemLog}");

                                MessageBox.Show("Erro ao gravar movimentação referente à conta a receber.\n\n" +
                                                "Nenhuma movimentação foi criada mas o fluxo de finalização da venda prosseguirá, por favor entre em contato com a equipe de suporte.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                                goto EndFluxCtaRecCartao;
                            }

                            if (resultIdMovto <= 0)
                            {
                                // Erro grave que detona com o fluxo da venda.
                                // Pelo menos deixar algumas dicas...

                                log.Error(string.Format("Erro na: {0}\nRetorno: {1}", strMensagemLog, resultIdMovto));
                                MessageBox.Show("Erro ao gravar movimentação referente à conta a receber.\n\n" +
                                                "Nenhuma movimentação foi criada mas o fluxo de finalização da venda prosseguirá, por favor entre em contato com a equipe de suporte.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                                goto EndFluxCtaRecCartao;
                            }

                            try
                            {
                                log.Debug("Movimento diario criado com sucesso.");
                                log.Debug("Lançando vinculo de movimento com conta a receber (TB_CTAREC_MOVTO)...");

                                strMensagemLog = string.Format("SP_TRI_CTAREC_MOVTO(PID_MOVTO: {0}, PID_CTAREC: {1})", resultIdMovto, resultIdConta);

                                int registrosInseridos = (int)OPER_TA.SP_TRI_CTAREC_MOVTO(resultIdMovto, resultIdConta);
                                registrosInseridos += (int)OPER_TA.SP_TRI_CTAREC_MOVTO(resultIdMovto + 1, resultIdConta); // Chama novamente para vincular o movimento de credito.
                                                                                                                          // Normalmente o ID do credito é um depois do debito por isso "+ 1" deve resolver.
                                                                                                                          // Se não for, fodeu.

                                if (registrosInseridos <= 0)
                                {
                                    log.Error("Não foi inserido corretamente vinculo da movDiario com a Conta a receber");
                                    log.Error($"Procedure utilizada: {strMensagemLog}");
                                    MessageBox.Show("Erro ao gravar vinculo da movimentação com conta a receber.\n\n" +
                                                    "Nenhum vinculo foi criado mas o fluxo de finalização da venda prosseguirá, por favor entre em contato com a equipe de suporte.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                                    goto EndFluxCtaRecCartao;
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error($"Erro ao gravar vinculo movDiario com conta receber: {ex.InnerException.Message ?? ex.Message}");
                                log.Error($"Procedure utilizada: {strMensagemLog}");

                                MessageBox.Show("Erro ao gravar movimentação/conta a receber." +
                                                "\n\nO fluxo da finalização da venda prosseguirá mas por favor entre em contato com a equipe de suporte.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                                goto EndFluxCtaRecCartao;
                            }

                            try
                            {
                                log.Debug("Vinculo na (TB_CTAREC_MOVTO) criado com sucesso.");
                                log.Debug("Lançando vinculo de venda com pagamento e conta a receber (TB_NFV_CTAREC)...");

                                using (var fbCommNfvCtarecInsertServ = new FbCommand())
                                {
                                    strMensagemLog = string.Format("INSERT INTO TB_NFV_CTAREC VALUES (ID_NFVENDA: {0}, ID_CTAREC: {1}, ID_NUMPAG: {2})",
                                                                   ID_NFVENDA,
                                                                   resultIdConta,
                                                                   ID_NUMPAG);


                                    fbCommNfvCtarecInsertServ.Connection = LOCAL_FB_CONN;

                                    if (fbCommNfvCtarecInsertServ.Connection.State != ConnectionState.Open)
                                        fbCommNfvCtarecInsertServ.Connection.Open();

                                    fbCommNfvCtarecInsertServ.CommandText = "INSERT INTO TB_NFV_CTAREC (ID_NFVENDA, ID_CTAREC, ID_NUMPAG) VALUES (@ID_NFVENDA, @ID_CTAREC, @ID_NUMPAG); ";
                                    fbCommNfvCtarecInsertServ.CommandType = CommandType.Text;

                                    fbCommNfvCtarecInsertServ.Parameters.Add("@ID_NFVENDA", ID_NFVENDA);
                                    fbCommNfvCtarecInsertServ.Parameters.Add("@ID_CTAREC", resultIdConta);
                                    fbCommNfvCtarecInsertServ.Parameters.Add("@ID_NUMPAG", ID_NUMPAG);

                                    // Executa a sproc
                                    int inseridos = fbCommNfvCtarecInsertServ.ExecuteNonQuery();

                                    if (inseridos <= 0)
                                    {
                                        log.Error("Não foi inserido corretamente vinculo do ID da venda com ID da conta a receber e ID do pagamento.");
                                        log.Error($"SQL utilizado: {strMensagemLog}");
                                        MessageBox.Show("Erro ao gravar vinculo de venda com conta a receber e pagamento.\n\n" +
                                                        "Nenhum vinculo foi criado mas o fluxo de finalização da venda prosseguirá. Por favor entre em contato com a equipe de suporte.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                                        goto EndFluxCtaRecCartao;
                                    }

                                    log.Debug("Vinculo na (TB_NFV_CTAREC) criado com sucesso.");
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error($"Erro ao gravar vinculo ID_NFVENDA com ID_CTAREC e ID_NUMPAG: {ex.InnerException.Message ?? ex.Message}");
                                log.Error($"Comando utilizado: {strMensagemLog}");

                                MessageBox.Show("Erro ao gravar vinculo de venda com conta a receber e pagamento." +
                                                "\n\nNenhum vinculo foi criado mas o fluxo da finalização da venda prosseguirá. Por favor entre em contato com a equipe de suporte.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                                goto EndFluxCtaRecCartao;
                            }
                        }
                        EndFluxCtaRecCartao:

                        if (x.pagamento.cMP == "05" && !x.pagamento.desconto)
                        {
                            string strMensagemErro = "";
                            log.Debug("Venda à prazo AmbiPDV");
                            int ID_CTAREC = 0;
                            int ID_MOVTO = 0;
                            try
                            {
                                ID_CTAREC = (int)CONTAREC_TA.SP_TRI_NFVCTAREC(ID_NFVENDA,
                                                                              x.pagamento.vencimento,
                                                                              ("Venda à prazo AmbiPDV " + DateTime.Now.ToShortTimeString()).ToUpper(),
                                                                              x.pagamento.idCliente,
                                                                              ID_NUMPAG);

                                ID_CLIENTE = x.pagamento.idCliente;
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao gravar venda na base", ex);
                                MessageBox.Show("Erro ao gravar conta a receber. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                                return -1; //deuruim();
                            }
                            try
                            {
                                strMensagemErro = string.Format("NFISCAL");
                                log.Debug(strMensagemErro);
                                ID_MOVTO = (int)OPER_TA.SP_TRI_LANCAMOVDIARIO("x",
                                                                              Convert.ToDecimal(x.pagamento.vMP, CultureInfo.InvariantCulture),
                                                                              ("Venda à prazo AmbiPDV - Cupom " + ID_NFVENDA.ToString() + " " + DateTime.Now.ToShortTimeString()).ToUpper(),
                                                                              147, 5);
                            }
                            catch (Exception ex)
                            {
                                log.Error("Erro ao gravar venda na base", ex);
                                MessageBox.Show("Erro ao gravar movimentação referente à conta a receber. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                                return -1; //deuruim(); ColocarTransacao();
                            }
                            try
                            {
                                log.Debug($"SP_TRI_CTAREC_MOVTO (coo: {ID_NFVENDA})");
                                //OPER_TA.SP_TRI_CTAREC_MOVTO((short)NO_CAIXA, coo);
                                using var TB_CTAREC_MOVTO = new DataSets.FDBDataSetVendaTableAdapters.TB_CTAREC_MOVTOTableAdapter
                                {
                                    Connection = LOCAL_FB_CONN
                                };
                                TB_CTAREC_MOVTO.Insert(ID_MOVTO, ID_CTAREC);
                            }
                            catch (Exception ex)
                            {
                                log.Error("Falha ao gravar venda na base", ex);
                                MessageBox.Show("Erro ao gravar movimentação/conta a receber. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                                return -1; //deuruim(); ColocarTransacao();
                            }
                        }
                        if (x.pagamento.cMP == "19")
                        {
                            ID_CLIENTE = x.pagamento.idCliente;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Falha ao gravar venda na base", ex);
                        if (ex.InnerException is not null)
                            log.Error("Falha ao gravar venda na base - inner", ex.InnerException);

                        MessageBox.Show("Erro ao gravar forma de pagamento. \nSe o problema persistir, entre em contato com a equipe de suporte.");
                        return -1;
                    }

                    Administradora.infoAdministradora = null; //Terminei de gravar as administradora, desinstancia. (será arrumado posteriormente)               
                }
                OPER_TA.SP_TRI_ATUALIZANFVENDA(ID_NFVENDA, ID_CLIENTE);
            }
            return ID_NFVENDA;
        }

        /// <summary>
        /// Grava o XML de venda gerado pelo sistema na base de dados.
        /// </summary>
        /// <param name="cfeDeRetorno">XML gerado</param>
        /// <param name="vTroco">Troco da venda</param>
        /// <param name="noCaixa">Série do Cupom Fiscal</param>
        /// <param name="idVendedor">ID do Vendedor que efetuou a venda</param>
        /// <param name="ECF">Determina se foi usado ECF para fazer a venda</param>
        /// <returns></returns>
        public virtual (int NF_NUMERO, int ID_NFVENDA) GravaNaoFiscalBase(decimal vTroco, int noCaixa, short idVendedor, bool ECF = false, int dav = 0)
        {
            CFe cfeDeRetorno = RetornaCFe();
            int NF_NUMERO, nItemCup, ID_NFVENDA, ID_CLIENTE = 0;
            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            using (var OPER_TA = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter())
            using (var VENDA_TA = new DataSets.FDBDataSetVendaTableAdapters.SP_TRI_GRAVANFVENDATableAdapter())
            {
                OPER_TA.Connection = LOCAL_FB_CONN;
                VENDA_TA.Connection = LOCAL_FB_CONN;
                try
                {
                    decimal totItem = 0;
                    foreach (var detalhamento in cfeDeRetorno.infCFe.det)
                    {
                        totItem += decimal.Parse(detalhamento.prod.vUnCom, ptBR) * decimal.Parse(detalhamento.prod.qCom, ptBR);
                    }
                    decimal valorDescAcr, porcentDescAcr = 0;
                    if (!(cfeDeRetorno.infCFe.total.DescAcrEntr is null))
                    {
                        valorDescAcr = decimal.Parse(cfeDeRetorno.infCFe.total.DescAcrEntr.Item, CultureInfo.InvariantCulture);
                        porcentDescAcr = valorDescAcr / totItem;

                        foreach (var detalhamento in cfeDeRetorno.infCFe.det)
                        {
                            switch (cfeDeRetorno.infCFe.total.DescAcrEntr.ItemElementName)
                            {
                                case ItemChoiceType1.vAcresSubtot:
                                    detalhamento.prod.vRatAcr =
                                        (decimal.Parse(detalhamento.prod.vUnCom, ptBR) * decimal.Parse(detalhamento.prod.qCom, ptBR) * porcentDescAcr).ToString();
                                    break;
                                case ItemChoiceType1.vDescSubtot:
                                    detalhamento.prod.vRatDesc =
                                        (decimal.Parse(detalhamento.prod.vUnCom, ptBR) * decimal.Parse(detalhamento.prod.qCom, ptBR) * porcentDescAcr).ToString();
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    DateTime tsEmissao = DateTime.Now;
                    OPER_TA.Connection = LOCAL_FB_CONN;
                    DataSets.FDBDataSetVenda.SP_TRI_GRAVANFVENDARow nFVendaRow;
                    switch (ECF)
                    {
                        case true:
                            nFVendaRow = VENDA_TA.SP_TRI_GRAVANFVENDA(idVendedor, "E" + noCaixa.ToString(), tsEmissao, tsEmissao, 2, vTroco, -1, dav > 0 ? $"DAV Nº {dav}" : null)[0];
                            break;
                        case false:
                        default:
                            nFVendaRow = VENDA_TA.SP_TRI_GRAVANFVENDA(idVendedor, "N" + noCaixa.ToString(), tsEmissao, tsEmissao, 2, vTroco, -1, dav > 0 ? $"DAV Nº {dav}" : null)[0];
                            break;
                    }
                    ID_NFVENDA = nFVendaRow.RID_NFVENDA;
                    NF_NUMERO = nFVendaRow.RNF_NUMERO;
                    log.Debug($"ID_NFVENDA = (int)OPER_TA.SP_TRI_GRAVANF(0, \"1\", {tsEmissao}, {tsEmissao}, 2, {vTroco});");
                }
                catch (Exception ex)
                {
                    log.Error("Falha ao gravar venda não fiscal na base", ex);
                    MessageBox.Show("Erro ao gravar cupom de venda. \nSe o problema persistir, entre em contato com a equipe de suporte.");
                    return (-1, -1);
                }
                foreach (envCFeCFeInfCFeDet detalhamento in cfeDeRetorno.infCFe.det)
                {
                    int ID_NFV_ITEM;
                    nItemCup = int.Parse(detalhamento.nItem);
                    try
                    {
                        string pCSOSN = null;
                        ID_NFV_ITEM = (int)OPER_TA.SP_TRI_GRAVANFVITEM(ID_NFVENDA,
                            Convert.ToInt32(detalhamento.prod.cProd),
                            Convert.ToInt16(detalhamento.nItem),
                            Convert.ToDecimal(detalhamento.prod.qCom, ptBR),
                            Convert.ToDecimal(detalhamento.prod.vDesc, ptBR) + Convert.ToDecimal(detalhamento.prod.vRatDesc, ptBR),
                            pCSOSN, 0, 0, Convert.ToDecimal(detalhamento.prod.vUnCom, ptBR),
                            detalhamento.idScannTech,
                            Convert.ToDecimal(detalhamento.prod.vOutro, ptBR));
                    }
                    catch (Exception ex)
                    {
                        log.Error("Falha ao gravar venda não fiscal", ex);
                        DialogBox.Show(strings.VENDA,
                                       DialogBoxButtons.No,
                                       DialogBoxIcons.Error, true,
                                       strings.ERRO_DURANTE_VENDA,
                                       RetornarMensagemErro(ex, false));
                        return (-1, -1);
                    }
                    #region CONTROLA_LOTE_DESATIVADO
                    //try
                    //{
                    //    if (Caixa._contingencia == false)
                    //    {
                    //        using var remendo1 = new DataSets.FDBDataSetVendaTableAdapters.TB_LOTETableAdapter();
                    //        using var SERVER_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) };
                    //        remendo1.Connection = SERVER_FB_CONN;
                    //        remendo1.SP_REM_CONTROLALOTE(Convert.ToInt32(detalhamento.prod.cProd), Convert.ToDecimal(detalhamento.prod.qCom, ptBR));
                    //    }
                    //    else
                    //    {
                    //        log.Debug("Caixa em contigencia, será pulado a procedure SP_REM_CONTROLALOTE");
                    //    }
                    //}
                    //catch (Exception ex)
                    //{                        
                    //    log.Debug("Erro ao tentar rodar a procedure SP_REM_CONTROLALOTE, ERRO: " + ex);
                    //}
                    #endregion
                    if (nItemCup <= 0)
                    {
                        throw new Exception("O ID de retorno do item de cupom é menor ou igual a zero: " + nItemCup.ToString());
                    }
                }
                if (!(cfeDeRetorno.infCFe.pgto is null))
                {
                    foreach (var pagamento in cfeDeRetorno.infCFe.pgto.MP)
                    {
                        int ID_NUMPAG;
                        try
                        {
                            using var CONTAREC_TA = new FDBDataSetTableAdapters.TB_CONTA_RECEBERTableAdapter();
                            using var TB_NFV_FMAPAGTO_TA = new DataSets.FDBDataSetVendaTableAdapters.TB_NFVENDA_FMAPAGTO_NFCETableAdapter();
                            using var TB_NFVENDA_FMAPAGTO = new DataSets.FDBDataSetVendaTableAdapters.TB_FORMA_PAGTO_NFCETableAdapter();
                            TB_NFV_FMAPAGTO_TA.Connection = CONTAREC_TA.Connection = TB_NFVENDA_FMAPAGTO.Connection = LOCAL_FB_CONN;
                            int idNfce = (short)TB_NFVENDA_FMAPAGTO.GetDataByIdNFCE(pagamento.cMP)[0]["ID_FMANFCE"];
                            if (pagamento.InfoAdmin.IdAdmin <= 0)
                            {
                                ID_NUMPAG = idNfce switch
                                {
                                    1 => (int)TB_NFV_FMAPAGTO_TA.SP_TRI_NFVFMAPGTO_INSERT(decimal.Parse(pagamento.vMP, ptBR) - cfeDeRetorno.infCFe.pgto.dTroco, ID_NFVENDA, idNfce, 2, null), //Agora essa procedure preenche tanto a TB_NFVENDA_FMAPAGTO_NFCE como a TB_NFCE_BANDEIRA, preenchimento OK.
                                    3 => (int)TB_NFV_FMAPAGTO_TA.SP_TRI_NFVFMAPGTO_INSERT(decimal.Parse(pagamento.vMP, ptBR), ID_NFVENDA, idNfce, 3, null),
                                    _ => (int)TB_NFV_FMAPAGTO_TA.SP_TRI_NFVFMAPGTO_INSERT(decimal.Parse(pagamento.vMP, ptBR), ID_NFVENDA, idNfce, 2, null)
                                };
                            }
                            else
                            {
                                ID_NUMPAG = idNfce switch
                                {
                                    1 => (int)TB_NFV_FMAPAGTO_TA.SP_TRI_NFVFMAPGTO_INSERT(decimal.Parse(pagamento.vMP, ptBR) - cfeDeRetorno.infCFe.pgto.dTroco, ID_NFVENDA, idNfce, 2, pagamento.InfoAdmin.IdAdmin), //Agora essa procedure preenche tanto a TB_NFVENDA_FMAPAGTO_NFCE como a TB_NFCE_BANDEIRA, preenchimento OK.
                                    3 => (int)TB_NFV_FMAPAGTO_TA.SP_TRI_NFVFMAPGTO_INSERT(decimal.Parse(pagamento.vMP, ptBR), ID_NFVENDA, idNfce, 3, pagamento.InfoAdmin.IdAdmin),
                                    _ => (int)TB_NFV_FMAPAGTO_TA.SP_TRI_NFVFMAPGTO_INSERT(decimal.Parse(pagamento.vMP, ptBR), ID_NFVENDA, idNfce, 2, pagamento.InfoAdmin.IdAdmin)
                                };
                            }

                            //ID_NUMPAG = (idNfce == 3) ?
                            //    (int)TB_NFV_FMAPAGTO_TA.SP_TRI_NFVFMAPGTO_INSERT(decimal.Parse(pagamento.vMP, ptBR), ID_NFVENDA, idNfce, 3) :
                            //    (int)TB_NFV_FMAPAGTO_TA.SP_TRI_NFVFMAPGTO_INSERT(decimal.Parse(pagamento.vMP, ptBR), ID_NFVENDA, idNfce, 2);


                            if ((pagamento.cMP == "03" || pagamento.cMP == "04") && INFORMA_MAQUININHA && VINCULA_MAQ_CTA)
                            {
                                // Para de gerar contas a receber para cartões                             

                                int resultIdConta = 0;
                                int resultIdMovto = 0;
                                string strMensagemLog = "";
                                (string metodo, decimal valorDescontadoTaxa) = pagamento.cMP switch
                                {
                                    "03" => ("Crédito", pagamento.vMP.Safedecimal() - (pagamento.InfoAdmin.TaxaCredito / 100 * pagamento.vMP.Safedecimal()).RoundABNT()),
                                    "04" => ("Débito", pagamento.vMP.Safedecimal() - (pagamento.InfoAdmin.TaxaDebito / 100 * pagamento.vMP.Safedecimal()).RoundABNT()),
                                    _ => ("(Pagamento não identificado)", 0)
                                };

                                try
                                {
                                    log.Debug("Criando conta a receber para cartão...");

                                    DateTime vencimento = DateTime.Now.AddDays(pagamento.InfoAdmin.DiasParaVencimento);

                                    strMensagemLog = String.Format("SP_TRI_LANCACONTAREC(IdNfVenda: {0}, Vecimento: {1}, Valor: {2}, IdCliente: {3}, Descricao: {4}, NumCaixa: {5}, IdConta: {6}, IdNumPag: {7})",
                                                                   ID_NFVENDA,
                                                                   vencimento,
                                                                   valorDescontadoTaxa,
                                                                   pagamento.InfoAdmin.IdCliente == 0 ? null : pagamento.InfoAdmin.IdCliente,
                                                                   $"Venda POS cartão de {metodo} AmbiPDV {DateTime.Now.ToString("HH:mm")}".ToUpper(),
                                                                   (short)NO_CAIXA,
                                                                   pagamento.InfoAdmin.IdConta == 0 ? null : pagamento.InfoAdmin.IdConta,
                                                                   ID_NUMPAG);

                                    resultIdConta = (int)CONTAREC_TA.SP_TRI_LANCACONTAREC(ID_NFVENDA,
                                                                                          vencimento,
                                                                                          valorDescontadoTaxa,
                                                                                          pagamento.InfoAdmin.IdCliente,
                                                                                          $"Venda POS cartão de {metodo} AmbiPDV {DateTime.Now.ToString("HH:mm")}".ToUpper(),
                                                                                          (short)NO_CAIXA,
                                                                                          pagamento.InfoAdmin.IdConta == 0 ? null : pagamento.InfoAdmin.IdConta,
                                                                                          ID_NUMPAG);
                                }
                                catch (Exception ex)
                                {
                                    log.Error($"Erro ao gravar conta a receber de cartões: {ex.InnerException.Message ?? ex.Message}");
                                    log.Error($"Procedure utilizada: {strMensagemLog}");
                                    MessageBox.Show("Erro ao gravar conta a receber do cartão utilizado na venda.\n\n" +
                                                    "Nenhuma conta foi criada mas o fluxo de finalização da venda prosseguirá, por favor entre em contato com a equipe de suporte.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                                    goto EndFluxCtaRecCartao;
                                }

                                if (resultIdConta <= 0)
                                {
                                    // Erro grave que detona com o fluxo da venda.
                                    // Pelo menos deixar algumas dicas...

                                    log.Error(string.Format("Erro na: {0}\nRetorno: {1}", strMensagemLog, resultIdConta));
                                    MessageBox.Show("Erro ao gravar conta a receber do cartão utilizado na venda.\n\n" +
                                                    "Nenhuma conta foi criada mas o fluxo de finalização da venda prosseguirá, por favor entre em contato com a equipe de suporte.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    goto EndFluxCtaRecCartao;
                                }

                                try
                                {
                                    log.Debug("Conta a receber criada com sucesso.");
                                    log.Debug("Lançando na movimento diario...");

                                    strMensagemLog = String.Format("SP_TRI_LANCAMOVDIARIO(NUMCAIXA: {0}, VALOR: {1}, DESCRICAO_MOV: {2}, CONTA_ORIGEM: {3}, CONTA_DESTINO: {4})",
                                                                   NO_CAIXA,
                                                                   valorDescontadoTaxa,
                                                                   $"VENDA NO CARTÃO DE {metodo.ToUpper()} AMBIPDV - CUPOM {NF_NUMERO} {DateTime.Now.ToString("HH:mm")}",
                                                                   22, 30);


                                    resultIdMovto = (int)OPER_TA.SP_TRI_LANCAMOVDIARIO(NO_CAIXA.ToString(),
                                                                                       valorDescontadoTaxa,
                                                                                       $"VENDA NO CARTÃO DE {metodo.ToUpper()} AMBIPDV - CUPOM {NF_NUMERO} {DateTime.Now.ToString("HH:mm")}",
                                                                                       22, 30);
                                }
                                catch (Exception ex)
                                {
                                    log.Error($"Erro ao gravar movimento diario referente a conta receber: {ex.InnerException.Message ?? ex.Message}");
                                    log.Error($"Procedure utilizada: {strMensagemLog}");

                                    MessageBox.Show("Erro ao gravar movimentação referente à conta a receber.\n\n" +
                                                    "Nenhuma movimentação foi criada mas o fluxo de finalização da venda prosseguirá, por favor entre em contato com a equipe de suporte.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                                    goto EndFluxCtaRecCartao;
                                }

                                if (resultIdMovto <= 0)
                                {
                                    // Erro grave que detona com o fluxo da venda.
                                    // Pelo menos deixar algumas dicas...

                                    log.Error(string.Format("Erro na: {0}\nRetorno: {1}", strMensagemLog, resultIdMovto));
                                    MessageBox.Show("Erro ao gravar movimentação referente à conta a receber.\n\n" +
                                                    "Nenhuma movimentação foi criada mas o fluxo de finalização da venda prosseguirá, por favor entre em contato com a equipe de suporte.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                                    goto EndFluxCtaRecCartao;
                                }

                                try
                                {
                                    log.Debug("Movimento diario criado com sucesso.");
                                    log.Debug("Lançando vinculo de movimento com conta a receber (TB_CTAREC_MOVTO)...");

                                    strMensagemLog = string.Format("SP_TRI_CTAREC_MOVTO(PID_MOVTO: {0}, PID_CTAREC: {1})", resultIdMovto, resultIdConta);

                                    int registrosInseridos = (int)OPER_TA.SP_TRI_CTAREC_MOVTO(resultIdMovto, resultIdConta);
                                    registrosInseridos += (int)OPER_TA.SP_TRI_CTAREC_MOVTO(resultIdMovto + 1, resultIdConta); // Chama novamente para vincular o movimento de credito.
                                                                                                                              // Normalmente o ID do credito é um depois do debito por isso "+ 1" deve resolver.
                                                                                                                              // Se não for, fodeu.

                                    if (registrosInseridos <= 0)
                                    {
                                        log.Error("Não foi inserido corretamente vinculo da movDiario com a Conta a receber");
                                        log.Error($"Procedure utilizada: {strMensagemLog}");
                                        MessageBox.Show("Erro ao gravar vinculo da movimentação com conta a receber.\n\n" +
                                                        "Nenhum vinculo foi criado mas o fluxo de finalização da venda prosseguirá, por favor entre em contato com a equipe de suporte.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                                        goto EndFluxCtaRecCartao;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.Error($"Erro ao gravar vinculo movDiario com conta receber: {ex.InnerException.Message ?? ex.Message}");
                                    log.Error($"Procedure utilizada: {strMensagemLog}");

                                    MessageBox.Show("Erro ao gravar movimentação/conta a receber." +
                                                    "\n\nO fluxo da finalização da venda prosseguirá mas por favor entre em contato com a equipe de suporte.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                                    goto EndFluxCtaRecCartao;
                                }

                                try
                                {
                                    log.Debug("Vinculo na (TB_CTAREC_MOVTO) criado com sucesso.");
                                    log.Debug("Lançando vinculo de venda com pagamento e conta a receber (TB_NFV_CTAREC)...");

                                    using (var fbCommNfvCtarecInsertServ = new FbCommand())
                                    {
                                        strMensagemLog = string.Format("INSERT INTO TB_NFV_CTAREC VALUES (ID_NFVENDA: {0}, ID_CTAREC: {1}, ID_NUMPAG: {2})",
                                                                       ID_NFVENDA,
                                                                       resultIdConta,
                                                                       ID_NUMPAG);


                                        fbCommNfvCtarecInsertServ.Connection = LOCAL_FB_CONN;

                                        if (fbCommNfvCtarecInsertServ.Connection.State != ConnectionState.Open)
                                            fbCommNfvCtarecInsertServ.Connection.Open();

                                        fbCommNfvCtarecInsertServ.CommandText = "INSERT INTO TB_NFV_CTAREC (ID_NFVENDA, ID_CTAREC, ID_NUMPAG) VALUES (@ID_NFVENDA, @ID_CTAREC, @ID_NUMPAG); ";
                                        fbCommNfvCtarecInsertServ.CommandType = CommandType.Text;

                                        fbCommNfvCtarecInsertServ.Parameters.Add("@ID_NFVENDA", ID_NFVENDA);
                                        fbCommNfvCtarecInsertServ.Parameters.Add("@ID_CTAREC", resultIdConta);
                                        fbCommNfvCtarecInsertServ.Parameters.Add("@ID_NUMPAG", ID_NUMPAG);

                                        // Executa a sproc
                                        int inseridos = fbCommNfvCtarecInsertServ.ExecuteNonQuery();

                                        if (inseridos <= 0)
                                        {
                                            log.Error("Não foi inserido corretamente vinculo do ID da venda com ID da conta a receber e ID do pagamento.");
                                            log.Error($"SQL utilizado: {strMensagemLog}");
                                            MessageBox.Show("Erro ao gravar vinculo de venda com conta a receber e pagamento.\n\n" +
                                                            "Nenhum vinculo foi criado mas o fluxo de finalização da venda prosseguirá. Por favor entre em contato com a equipe de suporte.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                                            goto EndFluxCtaRecCartao;
                                        }

                                        log.Debug("Vinculo na (TB_NFV_CTAREC) criado com sucesso.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.Error($"Erro ao gravar vinculo ID_NFVENDA com ID_CTAREC e ID_NUMPAG: {ex.InnerException.Message ?? ex.Message}");
                                    log.Error($"Comando utilizado: {strMensagemLog}");

                                    MessageBox.Show("Erro ao gravar vinculo de venda com conta a receber e pagamento." +
                                                    "\n\nNenhum vinculo foi criado mas o fluxo da finalização da venda prosseguirá. Por favor entre em contato com a equipe de suporte.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                                    goto EndFluxCtaRecCartao;
                                }
                            }
                        EndFluxCtaRecCartao:


                            if (pagamento.cMP == "05" && !pagamento.desconto)
                            {
                                string strMensagemErro = "";
                                log.Debug("Venda à prazo AmbiPDV");
                                int ID_CTAREC = 0;
                                int ID_MOVTO = 0;
                                try
                                {
                                    ID_CTAREC = (int)CONTAREC_TA.SP_TRI_NFVCTAREC(ID_NFVENDA,
                                                                                        pagamento.vencimento,
                                                                                        ("Venda à prazo AmbiPDV " + DateTime.Now.ToShortTimeString()).ToUpper(),
                                                                                        pagamento.idCliente,
                                                                                        ID_NUMPAG
                                                                                        );
                                    ID_CLIENTE = pagamento.idCliente;
                                }
                                catch (Exception ex)
                                {
                                    log.Error("Falha ao gravar venda não fiscal", ex);
                                    MessageBox.Show("Erro ao gravar conta a receber. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                                    return (-1, -1);
                                }
                                try
                                {
                                    strMensagemErro = string.Format("NFISCAL SP_TRI_LANCAMOVDIARIO({0}, vMP: {1}, Descrição: {2}, {3}, {4}", "x",
                                                                                 Convert.ToDecimal(pagamento.vMP, ptBR),
                                                                                 ("Venda à prazo AmbiPDV - Cupom " + NF_NUMERO.ToString() + " " + DateTime.Now.ToShortTimeString()).ToUpper(),
                                                                                 147, 5);
                                    log.Debug(strMensagemErro);
                                    ID_MOVTO = (int)OPER_TA.SP_TRI_LANCAMOVDIARIO("x",
                                                                                            Convert.ToDecimal(pagamento.vMP, ptBR),
                                                                                            ("Venda à prazo AmbiPDV - Cupom " + NF_NUMERO.ToString() + " " + DateTime.Now.ToShortTimeString()).ToUpper(),
                                                                                            147, 5);
                                }
                                catch (Exception ex)
                                {
                                    log.Error("Falha ao gravar venda não fiscal", ex);
                                    MessageBox.Show("Erro ao gravar movimentação referente à conta a receber. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                                    return (-1, -1);
                                }
                                try
                                {
                                    log.Debug($"SP_TRI_CTAREC_MOVTO (coo: {NF_NUMERO})");
                                    //OPER_TA.SP_TRI_CTAREC_MOVTO((short)NO_CAIXA, coo);
                                    using var TB_CTAREC_MOVTO = new DataSets.FDBDataSetVendaTableAdapters.TB_CTAREC_MOVTOTableAdapter
                                    {
                                        Connection = LOCAL_FB_CONN
                                    };
                                    TB_CTAREC_MOVTO.Insert(ID_MOVTO, ID_CTAREC);
                                }
                                catch (Exception ex)
                                {
                                    log.Error("", ex);
                                    MessageBox.Show("Erro ao gravar movimentação/conta a receber. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                                    return (-1, -1);
                                }
                            }
                            if (pagamento.cMP == "19")
                            {
                                ID_CLIENTE = pagamento.idCliente;
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("", ex);
                            MessageBox.Show("Erro ao gravar forma de pagamento. \nSe o problema persistir, entre em contato com a equipe de suporte.");
                            return (-1, -1);
                        }
                    }

                    Administradora.infoAdministradora = null; // Terminei de gravar as informações da administradora, desinstancia.
                }
                OPER_TA.SP_TRI_ATUALIZANFVENDA(ID_NFVENDA, ID_CLIENTE);

            }
            return (NF_NUMERO, ID_NFVENDA);
        }

        private FuncoesFirebird _funcoes = new();

        public decimal AplicaPrecoAtacado()
        {
            decimal vlrTotalDescAtacado = 0;
            using FbConnection LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
            var quantsCupom =
                from det in _listaDets
                where det.kit == false && det.scannTech == false
                group det by det.prod.cProd into newGroup
                select new
                {
                    cod = newGroup.Key,
                    qtdTotal = newGroup.Sum(x => decimal.Parse(x.prod.qCom))
                };

            var familiasCupom =
                from det in _listaDets
                where det.kit == false && det.scannTech == false
                group det by det.familia
                into newGroup
                select new
                {
                    familia = newGroup.Key,
                    qtdTotal = newGroup.Sum(x => decimal.Parse(x.prod.qCom))
                };

            #region Atacado por item

            foreach (var item in quantsCupom)
            {
                var info = _funcoes.GetInfoAtacado(int.Parse(item.cod), LOCAL_FB_CONN);
                switch (info.TipoAtacado)
                {
                    case CondicaoAtacado.Todas:
                        if (info.PrcAtacado > 0 && info.QtdAtacado > 0 && item.qtdTotal >= info.QtdAtacado)
                        {
                            foreach (var det in _listaDets)
                            {
                                if (det.prod.cProd == item.cod)
                                {
                                    if (det.prod.vUnComOri is null or "" or "0.000") det.prod.vUnComOri = det.prod.vUnCom;
                                    det.prod.vUnCom = info.PrcAtacado.ToString("0.000");
                                    vlrTotalDescAtacado += (det.prod.qCom.Safedecimal() * det.prod.vUnComOri.Safedecimal()) - (det.prod.qCom.Safedecimal() * det.prod.vUnCom.Safedecimal());
                                    det.atacado = true;
                                }
                            }
                        }
                        break;
                    case CondicaoAtacado.SomenteAcima:
                        if (info.PrcAtacado > 0 && info.QtdAtacado > 0 && item.qtdTotal >= info.QtdAtacado)
                        {
                            decimal qtdItensComAtacado = item.qtdTotal - info.QtdAtacado;
                            foreach (var itemProd in _listaDets.Where(x => x.prod.cProd == item.cod).OrderByDescending(x => x.nItem))
                            {
                                decimal qtdCompradaItemAtual = itemProd.prod.qCom.Safedecimal();
                                decimal descItemAtual;

                                if (qtdItensComAtacado > 0)
                                {
                                    if (qtdCompradaItemAtual >= qtdItensComAtacado)
                                    {
                                        descItemAtual = (itemProd.prod.vUnCom.Safedecimal() - info.PrcAtacado) * qtdItensComAtacado;
                                        qtdItensComAtacado -= qtdItensComAtacado;
                                    }
                                    else
                                    {
                                        descItemAtual = (itemProd.prod.vUnCom.Safedecimal() - info.PrcAtacado) * qtdCompradaItemAtual;
                                        qtdItensComAtacado -= qtdCompradaItemAtual;
                                    }
                                    itemProd.prod.vDesc = descItemAtual.ToString("#0.00");
                                    vlrTotalDescAtacado += descItemAtual;
                                    itemProd.atacado = true;
                                }
                                else break;
                            }
                        }
                        break;
                    case CondicaoAtacado.Multiplos:
                        if (info.PrcAtacado > 0 && info.QtdAtacado > 0 && item.qtdTotal >= info.QtdAtacado)
                        {
                            int combosPassados = (int)(item.qtdTotal / info.QtdAtacado);
                            decimal qtdItensComAtacado = combosPassados * info.QtdAtacado;
                            foreach (var itemProd in _listaDets.Where(x => x.prod.cProd == item.cod))
                            {
                                decimal descItemAtual;
                                if (qtdItensComAtacado > 0)
                                {
                                    if (itemProd.prod.qCom.Safedecimal() is decimal qtdCompradaItemAtual && qtdCompradaItemAtual >= qtdItensComAtacado)
                                    {
                                        descItemAtual = (itemProd.prod.vUnCom.Safedecimal() - info.PrcAtacado) * qtdItensComAtacado;
                                        qtdItensComAtacado -= qtdItensComAtacado;
                                    }
                                    else
                                    {
                                        descItemAtual = (itemProd.prod.vUnCom.Safedecimal() - info.PrcAtacado) * qtdCompradaItemAtual;
                                        qtdItensComAtacado -= qtdCompradaItemAtual;
                                    }
                                    itemProd.prod.vDesc = descItemAtual.ToString("#0.00");
                                    vlrTotalDescAtacado += descItemAtual;
                                    itemProd.atacado = true;
                                }
                                else break;
                            }
                        }
                        break;
                    case CondicaoAtacado.ApenasIgual:
                        if (info.PrcAtacado > 0 && info.QtdAtacado > 0 && item.qtdTotal >= info.QtdAtacado)
                        {
                            decimal qtdItensIterados = 0;
                            foreach (var itemProd in _listaDets.Where(x => x.prod.cProd == item.cod))
                            {
                                qtdItensIterados += itemProd.prod.qCom.Safedecimal();
                                if (qtdItensIterados >= info.QtdAtacado)
                                {
                                    decimal descItemAtual = itemProd.prod.vUnCom.Safedecimal() - info.PrcAtacado;
                                    itemProd.prod.vDesc = descItemAtual.ToString("#0.00");
                                    vlrTotalDescAtacado += descItemAtual;
                                    itemProd.atacado = true;

                                    qtdItensIterados += qtdItensIterados - info.QtdAtacado - qtdItensIterados;
                                    // Solução para aplicar o desconto nos itens corretos.
                                    // Exemplo: Quantidade para atacado é 4 ou seja sera aplicado desconto no 4º item depois no 8º e assim por diante....                                    
                                }
                            }
                        }
                        break;
                    default:
                        log.Debug("Condição do atacado não esperada, pulando verificação e aplicação de preço.");
                        break;
                }
            }

            #endregion Atacado por item

            #region Atacado por familia

            List<string> familiasVerificadas = new();
            foreach (var det in _listaDets)
            {
                var info = _funcoes.GetInfoAtacado(int.Parse(det.prod.cProd), LOCAL_FB_CONN);
                string familia = det.familia;
                if (familia == null || familia == "")
                {
                    continue;
                }
                else
                {
                    if (familiasVerificadas.Contains(familia))
                    {
                        continue;
                    }
                    familiasVerificadas.Add(familia);
                    foreach (var item in familiasCupom.Where(x => x.familia == familia && info.QtdAtacado > 0 && info.PrcAtacado > 0 && x.qtdTotal >= info.QtdAtacado))
                    {
                        switch (info.TipoAtacado)
                        {
                            case CondicaoAtacado.Todas:
                                foreach (var det1 in _listaDets.Where(x => x.familia == familia && x.kit is false && x.scannTech is false))
                                {
                                    if (det1.atacado) continue;

                                    var info1 = _funcoes.GetInfoAtacado(int.Parse(det1.prod.cProd), LOCAL_FB_CONN);
                                    if (det1.prod.vUnComOri is null or "" or "0.000") det1.prod.vUnComOri = det1.prod.vUnCom;
                                    det1.prod.vUnCom = info1.PrcAtacado.ToString("0.000");
                                    vlrTotalDescAtacado += (det1.prod.qCom.Safedecimal() * det1.prod.vUnComOri.Safedecimal()) - (det1.prod.qCom.Safedecimal() * det1.prod.vUnCom.Safedecimal());
                                    det1.atacado = true;
                                }
                                break;
                            case CondicaoAtacado.SomenteAcima:
                                if (item.qtdTotal > info.QtdAtacado)
                                {
                                    decimal qtdItensComAtacado = item.qtdTotal - info.QtdAtacado;
                                    foreach (var itemProd in _listaDets.Where(x => x.familia == familia && x.kit is false && x.scannTech is false).OrderByDescending(x => x.nItem))
                                    {
                                        decimal qtdCompradaItemAtual = itemProd.prod.qCom.Safedecimal();
                                        if (itemProd.atacado) { qtdItensComAtacado -= qtdCompradaItemAtual; continue; }

                                        decimal descItemAtual;
                                        if (qtdItensComAtacado > 0)
                                        {
                                            if (qtdCompradaItemAtual >= qtdItensComAtacado)
                                            {
                                                descItemAtual = (itemProd.prod.vUnCom.Safedecimal() - info.PrcAtacado) * qtdItensComAtacado;
                                                qtdItensComAtacado -= qtdItensComAtacado;
                                            }
                                            else
                                            {
                                                descItemAtual = (itemProd.prod.vUnCom.Safedecimal() - info.PrcAtacado) * qtdCompradaItemAtual;
                                                qtdItensComAtacado -= qtdCompradaItemAtual;
                                            }
                                            itemProd.prod.vDesc = descItemAtual.ToString("#0.00");
                                            vlrTotalDescAtacado += descItemAtual;
                                            itemProd.atacado = true;
                                        }
                                        else break;
                                    }
                                }
                                break;
                            case CondicaoAtacado.Multiplos:
                                {
                                    int combosPassados = (int)(item.qtdTotal / info.QtdAtacado);
                                    decimal qtdItensComAtacado = combosPassados * info.QtdAtacado;
                                    foreach (var itemProd in _listaDets.Where(x => x.familia == familia && x.kit is false && x.scannTech is false).OrderBy(x => !x.atacado))
                                    {
                                        decimal qtdCompradaItemAtual = itemProd.prod.qCom.Safedecimal();

                                        if (itemProd.atacado) { qtdItensComAtacado -= qtdCompradaItemAtual; continue; }

                                        decimal descItemAtual;
                                        if (qtdItensComAtacado > 0)
                                        {
                                            if (qtdCompradaItemAtual >= qtdItensComAtacado)
                                            {
                                                descItemAtual = (itemProd.prod.vUnCom.Safedecimal() - info.PrcAtacado) * qtdItensComAtacado;
                                                qtdItensComAtacado -= qtdItensComAtacado;
                                            }
                                            else
                                            {
                                                descItemAtual = (itemProd.prod.vUnCom.Safedecimal() - info.PrcAtacado) * qtdCompradaItemAtual;
                                                qtdItensComAtacado -= qtdCompradaItemAtual;
                                            }
                                            itemProd.prod.vDesc = descItemAtual.ToString("#0.00");
                                            vlrTotalDescAtacado += descItemAtual;
                                            itemProd.atacado = true;
                                        }
                                        else break;
                                    }
                                }
                                break;
                            case CondicaoAtacado.ApenasIgual:
                                decimal qtdItensIterados = 0;
                                foreach (var itemProd in _listaDets.Where(x => x.familia == familia && x.kit is false && x.scannTech is false))
                                {
                                    qtdItensIterados += itemProd.prod.qCom.Safedecimal();
                                    if (qtdItensIterados >= info.QtdAtacado)
                                    {
                                        if (itemProd.atacado) { qtdItensIterados += qtdItensIterados - info.QtdAtacado - qtdItensIterados; continue; } // Se já tiver aplicado desconto atacado
                                                                                                                                                       // decrementa a qtdItensIterados e continua sem aplicar desconto.
                                                                                                                                                       // Se não fizer isso será aplicado desconto no proximo item (o que é incorreto).
                                        decimal descItemAtual = itemProd.prod.vUnCom.Safedecimal() - info.PrcAtacado;
                                        itemProd.prod.vDesc = descItemAtual.ToString("#0.00");
                                        vlrTotalDescAtacado += descItemAtual;
                                        itemProd.atacado = true;

                                        qtdItensIterados += qtdItensIterados - info.QtdAtacado - qtdItensIterados;
                                        // Solução para aplicar o desconto nos itens corretos.
                                        // Exemplo: Quantidade para atacado é 4 ou seja sera aplicado desconto no 4º item depois no 8º e assim por diante....                                    
                                    }
                                }
                                break;
                            default:
                                log.Debug("Condição atacado não esperada, pulando verificação e aplicação de preço");
                                break;
                        }
                    }
                }
            }

            #endregion Atacado por familia

            return vlrTotalDescAtacado;
        }

        public decimal VerificaScannTech()
        {
            try
            {
                decimal vlrTotDescScannTech = 0;
                using (var tblPromoServ = new FDBDataSetOperSeed.SP_TRI_OBTEMPROMOSCANNTECHDataTable())
                {
                    var prodCodBarras = from produtos in _listaDets
                                        where produtos.idScannTech is not null
                                        group produtos by produtos.idScannTech into newGroup
                                        select new
                                        {
                                            ID = newGroup.Key,
                                            QTD_COMPRADA = newGroup.Sum(x => decimal.Parse(x.prod.qCom))
                                        };

                    using (var taPromoItens = new DataSets.FDBDataSetOperSeedTableAdapters.SP_TRI_OBTEMPROMOSCANNTECHTableAdapter())
                    {
                        taPromoItens.Connection = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
                        foreach (var itens in prodCodBarras)
                        {
                            taPromoItens.Fill(tblPromoServ, itens.ID);
                            if (tblPromoServ.Count <= 0) return 0;

                            if (itens.QTD_COMPRADA >= tblPromoServ[0].QTD)
                            {
                                int combosPassados = 0;
                                decimal totProdCalcularDesc = 0; // ----> Calcular desconto para essa quantidade de itens.
                                decimal qtdBaseDesconto = 0;     // ----> Usado como base para obter o valor da variavel acima (totProdCalcularDesc).
                                decimal resto = 0;
                                string tipoPromocao = tblPromoServ[0].TIPO;

                                if (tipoPromocao.Equals("LLEVA_PAGA")) qtdBaseDesconto = tblPromoServ[0].QTD - tblPromoServ[0].DET;
                                else qtdBaseDesconto = tblPromoServ[0].QTD;

                                if (itens.QTD_COMPRADA > tblPromoServ[0].QTD)
                                {
                                    combosPassados = (int)(itens.QTD_COMPRADA / tblPromoServ[0].QTD);
                                    combosPassados = tblPromoServ[0].LIMITE > 0 && combosPassados > tblPromoServ[0].LIMITE ? tblPromoServ[0].LIMITE : combosPassados;
                                    totProdCalcularDesc = combosPassados * qtdBaseDesconto;
                                }
                                else
                                {
                                    combosPassados = 1;
                                    totProdCalcularDesc = qtdBaseDesconto;
                                }

                                if (tipoPromocao.StartsWith("ADICIONAL")) ScannTechAdicional();
                                else ScannTechPromos();

                                void ScannTechPromos()
                                {
                                    List<envCFeCFeInfCFeDet> prodScanntech = _listaDets.Where(x => x.idScannTech == itens.ID).ToList();
                                    decimal qtdProdQueReceberãoDesconto, qtdProdQueFaltaDarDesconto;
                                    qtdProdQueReceberãoDesconto = qtdProdQueFaltaDarDesconto = tblPromoServ[0].QTD * combosPassados;

                                    foreach (var prod in prodScanntech)
                                    {
                                        decimal qtd = decimal.Parse(prod.prod.qCom);
                                        decimal vlrUnit = decimal.Parse(prod.prod.vUnCom);

                                        switch (tipoPromocao)
                                        {
                                            case "LLEVA_PAGA":
                                                {
                                                    decimal vlrDescBruto;
                                                    if (qtd >= qtdProdQueFaltaDarDesconto)
                                                    {
                                                        vlrDescBruto = vlrUnit * totProdCalcularDesc / qtdProdQueReceberãoDesconto * qtdProdQueFaltaDarDesconto;
                                                        qtdProdQueFaltaDarDesconto -= qtdProdQueFaltaDarDesconto;
                                                    }
                                                    else
                                                    {
                                                        vlrDescBruto = vlrUnit * totProdCalcularDesc / qtdProdQueReceberãoDesconto * qtd;
                                                        qtdProdQueFaltaDarDesconto -= qtd;
                                                    }
                                                    decimal vlrDescRound = vlrDescBruto.RoundABNT();
                                                    resto += vlrDescRound - vlrDescBruto;
                                                    prod.prod.vDesc = vlrDescRound.ToString("#0.00");
                                                    vlrTotDescScannTech += vlrDescBruto;
                                                }
                                                break;

                                            case "DESCUENTO_VARIABLE":
                                                {
                                                    decimal vlrDescBruto;
                                                    if (qtd >= qtdProdQueFaltaDarDesconto)
                                                    {
                                                        vlrDescBruto = tblPromoServ[0].DET / 100 * vlrUnit * qtdProdQueFaltaDarDesconto;
                                                        qtdProdQueFaltaDarDesconto -= qtdProdQueFaltaDarDesconto;
                                                    }
                                                    else
                                                    {
                                                        vlrDescBruto = tblPromoServ[0].DET / 100 * vlrUnit * qtd;
                                                        qtdProdQueFaltaDarDesconto -= qtd;
                                                    }
                                                    decimal vlrDescRound = vlrDescBruto.RoundABNT();
                                                    resto += vlrDescRound - vlrDescBruto;
                                                    prod.prod.vDesc = vlrDescRound.ToString("#0.00");
                                                    vlrTotDescScannTech += vlrDescBruto;
                                                }
                                                break;

                                            case "PRECIO_FIJO":
                                                {
                                                    decimal vlrDescBruto;
                                                    if (qtd >= qtdProdQueFaltaDarDesconto)
                                                    {
                                                        vlrDescBruto = (vlrUnit - (tblPromoServ[0].DET / tblPromoServ[0].QTD)) * qtdProdQueFaltaDarDesconto;
                                                        qtdProdQueFaltaDarDesconto -= qtdProdQueFaltaDarDesconto;
                                                    }
                                                    else
                                                    {
                                                        vlrDescBruto = (vlrUnit - (tblPromoServ[0].DET / tblPromoServ[0].QTD)) * qtd;
                                                        qtdProdQueFaltaDarDesconto -= qtd;
                                                    }

                                                    decimal vlrDescRound = vlrDescBruto.RoundABNT();
                                                    resto += vlrDescRound - vlrDescBruto;
                                                    prod.prod.vDesc = vlrDescRound.ToString("#0.00");
                                                    vlrTotDescScannTech += vlrDescBruto;
                                                }
                                                break;

                                            case "DESCUENTO_FIJO":
                                                {
                                                    decimal vlrDescBruto;
                                                    if (qtd >= qtdProdQueFaltaDarDesconto)
                                                    {
                                                        vlrDescBruto = tblPromoServ[0].DET * combosPassados / qtdProdQueReceberãoDesconto * qtdProdQueFaltaDarDesconto;
                                                        qtdProdQueFaltaDarDesconto -= qtdProdQueFaltaDarDesconto;
                                                    }
                                                    else
                                                    {
                                                        vlrDescBruto = tblPromoServ[0].DET * combosPassados / qtdProdQueReceberãoDesconto * qtd;
                                                        qtdProdQueFaltaDarDesconto -= qtd;
                                                    }
                                                    decimal vlrDescRound = vlrDescBruto.RoundABNT();
                                                    resto += vlrDescRound - vlrDescBruto;
                                                    prod.prod.vDesc = vlrDescRound.ToString("#0.00");
                                                    vlrTotDescScannTech += vlrDescBruto;
                                                }
                                                break;
                                        }

                                        #region desativado por enquanto
                                        //iterador++;
                                        //if (iterador == iteracoes && resto != 0) //iterando pelo ultimo item da promoção e tem resto, vamos ajustar nele.
                                        //{
                                        //    prod.prod.vDesc = resto > 0 ?
                                        //    (decimal.Parse(prod.prod.vDesc) - Math.Abs(resto)).ToString("F2") :
                                        //    (decimal.Parse(prod.prod.vDesc) + Math.Abs(resto)).ToString("F2") ;
                                        //}
                                        #endregion desativado por enquanto

                                        if (qtdProdQueFaltaDarDesconto == 0)
                                        {
                                            prod.prod.vDesc = resto > 0 ?
                                            (decimal.Parse(prod.prod.vDesc) - Math.Abs(resto)).ToString("F2") :
                                            (decimal.Parse(prod.prod.vDesc) + Math.Abs(resto)).ToString("F2");
                                            break;
                                        }
                                    }
                                }
                                void ScannTechAdicional()
                                {
                                    using (var tblScannAdicional = new FDBDataSetOperSeed.TRI_SCANN_ADICIONALDataTable())
                                    using (var taScannAdicional = new DataSets.FDBDataSetOperSeedTableAdapters.TRI_SCANN_ADICIONALTableAdapter())
                                    {
                                        taScannAdicional.Connection = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
                                        taScannAdicional.FillByIdPromo(tblScannAdicional, itens.ID.Safeint());

                                        decimal qtdProdutosParaDarDesconto = combosPassados;

                                        if (tipoPromocao == "ADICIONAL_REGALO") qtdProdutosParaDarDesconto = combosPassados * tblPromoServ[0].DET;

                                        if (tblScannAdicional != null && tblScannAdicional.Rows.Count > 0)
                                        {
                                            foreach (var itemAdicional in tblScannAdicional)
                                            {
                                                foreach (var itemComDesconto in _listaDets.Where(x => x.prod.cEAN == itemAdicional.CODIGOBARRAS && qtdProdutosParaDarDesconto > 0))
                                                {
                                                    decimal vlrUnit = itemComDesconto.prod.vUnCom.Safedecimal();
                                                    decimal qtdCompra = itemComDesconto.prod.qCom.Safedecimal();
                                                    decimal vlrDesc = default;

                                                    if (qtdCompra >= qtdProdutosParaDarDesconto)
                                                    {
                                                        vlrDesc = tipoPromocao == "ADICIONAL_DESCUENTO" ? (tblPromoServ[0].DET / 100 * vlrUnit).RoundABNT() * qtdProdutosParaDarDesconto : vlrUnit * qtdProdutosParaDarDesconto;
                                                        qtdProdutosParaDarDesconto -= qtdProdutosParaDarDesconto;
                                                    }
                                                    else
                                                    {
                                                        vlrDesc = tipoPromocao == "ADICIONAL_DESCUENTO" ? (tblPromoServ[0].DET / 100 * vlrUnit).RoundABNT() * qtdCompra : vlrUnit * qtdCompra;
                                                        qtdProdutosParaDarDesconto -= qtdCompra;
                                                    }

                                                    itemComDesconto.prod.vDesc = vlrDesc.ToString("#0.00");
                                                    vlrTotDescScannTech += vlrDesc;

                                                    if (qtdProdutosParaDarDesconto == 0) break;
                                                }
                                            }
                                        }
                                        else log.Debug("Não foi encontrado item para receber desconto da promoção `ADICIONAL` da ScannTech.");
                                    }
                                }

                                log.Debug($"Aplicação de descontos ScannTech, código da promoção: {itens.ID}");
                            }
                        }
                    }
                    return vlrTotDescScannTech;
                }
            }
            catch (Exception ex)
            {
                log.Error("Erro ao verificar/aplicar promoções ScannTech: " + ex.Message);
                MessageBox.Show("Erro ao verificar promoção ScannTech. Entre em contato com o suporte técnico", "Atenção", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }
        public decimal ValorDaVenda()
        {
            decimal valVenda = 0;
            foreach (envCFeCFeInfCFeDet det in _listaDets)
            {
                valVenda += (det.prod.vUnCom.Safedecimal() * det.prod.qCom.Safedecimal() - det.prod.vDesc.Safedecimal()).RoundABNT() + det.prod.vOutro.Safedecimal().RoundABNT() - (det.descAtacado.Safedecimal().RoundABNT());
            }
            return valVenda;
        }
    }

    public class ClienteDuePayDTO
    {
        public int IdCliente { get; set; }
        public string Nome { get; set; }
        public string CpfOrCnpj { get; set; }
        public string Telefone { get; set; }
        public int NumeroDaSorte { get; set; }
        public ClienteDuePayDTOValidator Validador { get; set; } = new();

        public ClienteDuePayDTO(int id, string nome, string cpfOrCnpj, string telefone, int numeroDaSorte)
        {
            IdCliente = id;
            Nome = nome;
            CpfOrCnpj = cpfOrCnpj;
            Telefone = telefone;
            NumeroDaSorte = numeroDaSorte;
        }
    }

    public class ClienteDuePayDTOValidator : AbstractValidator<ClienteDuePayDTO>
    {
        public ClienteDuePayDTOValidator()
        {
            RuleFor(x => x.IdCliente)
                .NotEqual(0)
                .WithMessage("Id do cliente não pode ser igual a zero");

            RuleFor(x => x.Nome)
                .NotNull()
                .NotEmpty()
                .WithMessage("Nome do cliente não pode estar em branco");

            RuleFor(x => x.CpfOrCnpj)
                .NotNull()
                .NotEmpty()
                .WithMessage("CPF/CNPJ do cliente não pode estar em branco");

            RuleFor(x => x.Telefone)
                .NotNull()
                .NotEmpty()
                .WithMessage("Telefone do cliente não pode estar em branco");

            RuleFor(x => x.NumeroDaSorte)
                .NotEqual(0)
                .WithMessage("Número da sorte não pode estar em branco");

        }
    }
}
