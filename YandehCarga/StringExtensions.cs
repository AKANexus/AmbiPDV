using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YandehCarga
{
    public static class StringExtensions
    {
        public static string TiraPont(this string s, bool TiraEspaco = false)
        {
            if (s is null) return null;
            var sb = new StringBuilder();
            if (!TiraEspaco)
            {
                foreach (char c in s)
                {
                    if (!char.IsPunctuation(c))
                    {
                        sb.Append(c);
                    }
                }
            }
            else
            {
                foreach (char c in s)
                {
                    if (!char.IsPunctuation(c) && !char.IsWhiteSpace(c))
                    {
                        sb.Append(c);
                    }
                }
            }
            return sb.ToString();
        }
        public static bool IsCnpj(this string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj)) return false;

            int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma;
            int resto;
            string digito;
            string tempCnpj;
            cnpj = cnpj.Trim();
            cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "");
            if (cnpj.Length != 14)
            {
                return false;
            }

            tempCnpj = cnpj.Substring(0, 12);
            soma = 0;
            for (int i = 0; i < 12; i++)
            {
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];
            }

            resto = soma % 11;
            if (resto < 2)
            {
                resto = 0;
            }
            else
            {
                resto = 11 - resto;
            }

            digito = resto.ToString();
            tempCnpj += digito;
            soma = 0;
            for (int i = 0; i < 13; i++)
            {
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];
            }

            resto = soma % 11;
            if (resto < 2)
            {
                resto = 0;
            }
            else
            {
                resto = 11 - resto;
            }

            digito += resto.ToString();
            return cnpj.EndsWith(digito);
        }
        public static bool IsCpf(this string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return false;
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf;
            string digito;
            int soma;
            int resto;
            cpf = cpf.Trim();
            cpf = cpf.Replace(".", "").Replace("-", "");
            if (cpf.Length != 11)
            {
                return false;
            }

            tempCpf = cpf.Substring(0, 9);
            soma = 0;

            for (int i = 0; i < 9; i++)
            {
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            }

            resto = soma % 11;
            if (resto < 2)
            {
                resto = 0;
            }
            else
            {
                resto = 11 - resto;
            }

            digito = resto.ToString();
            tempCpf += digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
            {
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            }

            resto = soma % 11;
            if (resto < 2)
            {
                resto = 0;
            }
            else
            {
                resto = 11 - resto;
            }

            digito += resto.ToString();
            return cpf.EndsWith(digito);
        }
        public static bool IsTelefoneBR(this string telefone)
        {
            telefone = telefone.TiraPont();
            if (telefone.Length == 8 || telefone.Length == 9) return true;
            if (telefone.Length == 11 && telefone.StartsWith("0800")) return true;
            else return false;
        }
        public static int Safeint(this string vDado)
        {
            int vResult = 0;
            if (!string.IsNullOrWhiteSpace(vDado))
            {
                //if (vDado != "NaN") //TODO: Não é necessário, pois a conversão a seguir já pode estourar e resultar em zero.
                //{
                try
                {
                    vResult = Convert.ToInt32(vDado);
                }
                catch
                {
                    vResult = 0;
                }
                //}
            }
            return vResult;
        }
        public static decimal Safedecimal(this string vDado, CultureInfo culture = null)
        {
            culture ??= CultureInfo.InvariantCulture;
            decimal vResult = 0;
            if (!string.IsNullOrWhiteSpace(vDado))
            {
                try
                {
                    vResult = Convert.ToDecimal(vDado, culture);
                }
                catch (Exception)
                {
                    vResult = 0;
                }
            }
            return vResult;

        }
        public static string ParseToCPF(this string s)
        {
            if (s is null) return null;

            if (s.Length == 11 && s.IsCpf())
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(s.Substring(0, 3));
                sb.Append(".");
                sb.Append(s.Substring(3, 3));
                sb.Append(".");
                sb.Append(s.Substring(6, 3));
                sb.Append("-");
                sb.Append(s.Substring(9, 2));
                return sb.ToString();
            }

            if (s.Length == 14 && s.IsCpf())
            {
                return s;
            }

            return null;
        }
        public static string ParseToCNPJ(this string s)
        {
            if (s is null) return null;
            if (s.Length == 14 && s.IsCnpj())
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(s.Substring(0, 2));
                sb.Append(".");
                sb.Append(s.Substring(2, 3));
                sb.Append(".");
                sb.Append(s.Substring(5, 3));
                sb.Append("/");
                sb.Append(s.Substring(8, 4));
                sb.Append("-");
                sb.Append(s.Substring(12, 2));
                return sb.ToString();
            }

            if (s.Length == 18 && s.IsCnpj())
            {
                return s;
            }

            return null;
        }

        public static string ParseToCEP(this string s)
        {
            if (s is null) return null;
            if (s.Length == 8)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(s.Substring(0,5));
                sb.Append("-");
                sb.Append(s.Substring(5, 3));
                return sb.ToString();
            }

            if (s.Length == 9)
            {
                return s;
            }

            return null;
        }

        public static string ParseToRG(this string s)
        {
            if (s is null) return null;
            var sb = new StringBuilder();
            sb.Append(s.Substring(0, 2));

            sb.Append(".");
            sb.Append(s.Substring(2, 3));
            sb.Append(".");
            sb.Append(s.Substring(5, 3));
            sb.Append("-");
            sb.Append(s.Substring(8, 1));
            return sb.ToString();
        }
        public static string ParseToTelefone(this string s)
        {
            if (s is null) return null;
            StringBuilder sb = new StringBuilder();
            if (s.Length == 11 && s.StartsWith("0800"))
            {
                sb.Append(s.Substring(0, 4));
                sb.Append(" ");
                sb.Append(s.Substring(4, 3));
                sb.Append(" ");
                sb.Append(s.Substring(7, 2));
                sb.Append(" ");
                sb.Append(s.Substring(9, 2));
            }
            if (s.Length == 9)
            {
                sb.Append(s.Substring(0, 5));
                sb.Append("-");
                sb.Append(s.Substring(5, 4));
            }
            else if (s.Length == 8)
            {
                sb.Append(s.Substring(0, 4));
                sb.Append("-");
                sb.Append(s.Substring(4, 4));
            }
            return sb.ToString();
        }
        public static string ParseToCNAE(this string s)
        {
            if (s is null) return null;
            StringBuilder sb = new StringBuilder();
            if (s.Length == 7)
            {
                sb.Append(s.Substring(0, 4));
                sb.Append("-");
                sb.Append(s.Substring(4, 1));
                sb.Append("/");
                sb.Append(s.Substring(5, 2));
            }
            return sb.ToString();
        }
        public static bool IsNumbersOnly(this string str)
        {
            if (str.Length == 0) return false;
            foreach (char c in str)
            {
                if (!Char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }//Checa se a string possui apenas dígitos.
        public static string RemoveDiacritics(this string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
