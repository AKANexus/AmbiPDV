using CfeRecepcao_0007;
using PDV_WPF.Exceptions;
using System;
using System.Collections.Generic;

namespace PDV_WPF.Objetos
{
    internal class Devolucao
    {
        private bool _produtoRecebido = false;
        private CFe _cFe;
        private envCFeCFeInfCFe _infCfe;
        private envCFeCFeInfCFeDet _det;
        private List<envCFeCFeInfCFeDet> _listaDets;
        private envCFeCFeInfCFeDetProd _produto;
        private envCFeCFeInfCFeTotal _Total;
        public int nItemCupom = 1;

        public CFe RetornaCFe()
        {
            return _cFe;
        }

        public List<envCFeCFeInfCFeDet> RetornaListaDets()
        {
            return _listaDets;
        }

        public Devolucao()
        {
            _cFe = new CFe();
        }

        public void Clear()
        {
            _cFe = null;
            _infCfe = null;
            _det = null;
            _listaDets = null;
            _produto = null;
            _Total = null;
            _produtoRecebido = false;
            nItemCupom = 1;
        }

        public void AbrirNovaDevolucao()
        {
            if (!(_infCfe is null)) throw new ErroDeValidacaoDeConteudo("XML só pode conter um grupo de informações de cabeçalho");
            _infCfe = new envCFeCFeInfCFe() { versaoDadosEnt = "0.07" };
            _infCfe.ide = new envCFeCFeInfCFeIde();
            _cFe.infCFe = _infCfe;
            _listaDets = new List<envCFeCFeInfCFeDet>();
        }

        public void RecebeNovoProduto(int codProd,
                                      string descricao,
                                      string NCM,
                                      //string CFOP,
                                      decimal valorUnit,
                                      //string CEST,

                                      decimal valorOutros = 0,
                                      decimal valorDesconto = 0,
                                      string uniComercial = "UN",
                                      decimal quantidade = 1,
                                      string GTIN = null)
        {
            _det = new envCFeCFeInfCFeDet();
            _produto = new envCFeCFeInfCFeDetProd
            {
                cProd = codProd.ToString(),
                indRegra = "A"
            };

            if (!String.IsNullOrWhiteSpace(GTIN))
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
            if (String.IsNullOrWhiteSpace(descricao))
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
            if (!String.IsNullOrWhiteSpace(NCM))
            {
                _produto.NCM = NCM;
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
            if (valorUnit <= 0)
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
            else if (valorOutros > 0)
            {
                _produto.vOutro = valorOutros.ToString("0.00");
            }
            _produtoRecebido = true;
        }

        public bool AdicionaProduto()
        {
            if (!_produtoRecebido) throw new ErroDeValidacaoDeConteudo("Informações do produto estão faltando.");
            _det.nItem = nItemCupom.ToString();
            nItemCupom++;
            _det.prod = _produto;

            _listaDets.Add(_det);
            _produtoRecebido = false;
            return true;
        }

        public envCFeCFeInfCFeDetProd RemoveProduto(int numItem)
        {
            envCFeCFeInfCFeDet a_remover = _listaDets.Find(x => x.nItem == numItem.ToString());
            _listaDets.Remove(a_remover);
            return a_remover.prod;
        }

        public envCFeCFeInfCFeDetProd RemoveProduto(envCFeCFeInfCFeDet detARemover)
        {
            _listaDets.Remove(detARemover);
            return detARemover.prod;
        }

        public void TotalizaCupom()
        {
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
        }
    }
}
